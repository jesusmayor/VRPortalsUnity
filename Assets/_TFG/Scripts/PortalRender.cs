using UnityEngine;
using Settings = UnityEngine.XR.XRSettings;
using UnityEngine.XR;

namespace TFG
{
    //clase que se encarga de la parte funcional y visual de los portales
    [AddComponentMenu("TFG/PortalRender")]
    public class PortalRender : MonoBehaviour
    {
        #region ATTRIBUTES
        //Nodo al que pertenece el portal
        public GameObject parentObject;

        public bool collidable;
        //destino al que se teletransporta. Puede ser cualquier Transform, pero en el caso de los portales suele ser otro portal
        public Transform connectedPortal;
        //referencia local al material para acelerar las operaciones
        private Renderer portalRenderer;
        //texturas para que el otro lado del portal pueda ser visto
        private RenderTexture rTextureLeft, rTextureRight;
        //referencia a la cámara auxiliar
        private BoxCollider boxCollider;
        
        //[HideInInspector] //cámaras auxiliares que serán la visión de nuestros portales
        public Camera auxCameraleft, auxCameraright;
        //referencia al GameManager para tener acceso rápido a los objetos principales 
        GameManager gmref;

        private int doTenTimes;

        public float offsetPortal = 0.01f;
        #endregion

        public void Start()
        {
            doTenTimes = 10;
            if(parentObject != null)
            {
                if (parentObject.name == "Entry Portal")
                    collidable = false;
                else
                    collidable = true;
            }
        }

        public void OnWillRenderObject()
        {
            // se le asigna a cada cámara su textura correspondiente y tras colocarlas en la posición deseada
            // se modifica su matriz de proyección de manera que su plano de corte cercano sea el portal de
            // destino en su vista de cámara.
            if (Camera.current == gmref.lefteye)
            {
                //Debug.Log("Left");
                //Calculamos la posición y rotación relativa de la cámara auxiliar respecto a la cámara principal del ojo izquierdo.
                if (TryGetXRNodeOffsetEyeVector(out Vector3 leftEyePos,out Vector3 rightEyePos)) //La posición debe ser obtenida mediante el API de Unity.
                {
                    auxCameraleft.transform.position = GetRelativePos(gmref.leftMovement.TransformPoint(leftEyePos));
                }
                auxCameraleft.transform.rotation = GetRelativeDir(gmref.lefteye.transform.rotation); //La rotación si vale la de la cámara.
                //Aplicamos la matriz de proyección ublicua en el ojo izquierdo
                //Vector4 clipPlaneB_L = CameraSpacePlane(auxCameraleft, connectedPortal.transform.position, connectedPortal.transform.forward, 1.0f);
                //Matrix4x4 projectionB_L = gmref.lefteye.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
                //CalculateObliqueMatrix(ref projectionB_L, clipPlaneB_L);
                //auxCameraleft.projectionMatrix = gmref.lefteye.CalculateObliqueMatrix(clipPlaneB_L);

                //Comentado para sustituir a la proyección ublicua en el caso de que sea necesario.
                auxCameraleft.projectionMatrix = gmref.lefteye.projectionMatrix;

                //Indicamos a la cámara izquierda que renderice sobre su textura derecha (Que es la que ahora tiene asociado el portal).
                auxCameraleft.Render();
            }
            else if (Camera.current == gmref.righteye)
            {
                //Debug.Log("Right");
                //Calculamos la posición y rotación relativa de la cámara auxiliar respecto a la cámara principal del ojo derecho.
                if (TryGetXRNodeOffsetEyeVector(out Vector3 leftEyePos, out Vector3 rightEyePos)) //La posición debe ser obtenida mediante el API de Unity.
                {
                    auxCameraright.transform.position = GetRelativePos(gmref.rightMovement.TransformPoint(rightEyePos));
                }
                auxCameraright.transform.rotation = GetRelativeDir(gmref.righteye.transform.rotation); //La rotación si vale la de la cámara.
                //Aplicamos la matriz de proyección ublicua del ojo derecho.
                //Vector4 clipPlaneB_R = CameraSpacePlane(auxCameraright, connectedPortal.transform.position, connectedPortal.transform.forward, 1.0f);
                //Matrix4x4 projectionB_R = gmref.righteye.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
                //CalculateObliqueMatrix(ref projectionB_R, clipPlaneB_R);
                //auxCameraright.projectionMatrix = gmref.righteye.CalculateObliqueMatrix(clipPlaneB_R);

                //Comentado para sustituir a la proyección ublicua en el caso de que sea necesario.
                auxCameraright.projectionMatrix = gmref.righteye.projectionMatrix;

                //Indicamos a la cámara derecha que renderice sobre su textura derecha (Que es la que ahora tiene asociado el portal).
                auxCameraright.Render();
            }
        }
        
        // Dada la posicion y la normal del portal de destino, calcula su posición dentro del espacio de la cámara
        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            //Vector3 offsetPos = pos + normal * portalOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(pos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        // Ajusta la matriz de proyección de la cámara auxiliar para que su plano de corte de cerca sea la posición
        // del portal dentro del espacio de la cámara
        private static void CalculateObliqueMatrix(ref Matrix4x4 projection, Vector4 clipPlane)
        {
            Vector4 q = projection.inverse * new Vector4(
                Mathf.Sign(clipPlane.x),
                Mathf.Sign(clipPlane.y),
                1.0f,
                1.0f
            ); //Q
            Vector4 M4 = new Vector4(projection[3], projection[7], projection[11], projection[15]);

            Vector4 c = (Vector4.Dot(2.0f * M4, q) / (Vector4.Dot(clipPlane, q))) * clipPlane;

            // third row = clip plane - fourth row
            projection[2] = c.x - M4.x;
            projection[6] = c.y - M4.y;
            projection[10] = c.z - M4.z;
            projection[14] = c.w - M4.w;
        }

        public void Update()
        {
            if (doTenTimes > 0)//Se hacen 10 bucles vacíos para esperar a que se inicialize el módulo XR y poder cargar las texturas de las cámaras
            {
                doTenTimes--;
            }
            else if(doTenTimes == 0)
            {
                setCameraTextures();
                doTenTimes--;
            }

            


            Vector3 leftEyeWorldPosition = Vector3.zero; 
            Vector3 rightEyeWorldPosition = Vector3.zero;
            if (TryGetXRNodeOffsetEyeVector(out Vector3 leftEyePos, out Vector3 rightEyePos)) //La posición debe ser obtenida mediante el API de Unity.
            {
                leftEyeWorldPosition = gmref.leftMovement.TransformPoint(leftEyePos);
                rightEyeWorldPosition = gmref.rightMovement.TransformPoint(rightEyePos);

                if (boxCollider.bounds.Contains(leftEyeWorldPosition))
                {
                    if (Vector3.Dot((leftEyeWorldPosition - (transform.forward * offsetPortal)) - transform.position, transform.forward) <= 0)
                    {
                        gmref.leftMovement.position = GetRelativePos(gmref.leftMovement.position - transform.forward * offsetPortal * 1.5f);
                        gmref.leftMovement.rotation = GetRelativeDir(gmref.leftMovement.rotation);
                    }
                }
                if (boxCollider.bounds.Contains(rightEyeWorldPosition))
                {
                    if (Vector3.Dot((rightEyeWorldPosition - (transform.forward * offsetPortal)) - transform.position, transform.forward) <= 0)
                    {
                        gmref.rightMovement.position = GetRelativePos(gmref.rightMovement.position - transform.forward * offsetPortal * 1.5f);
                        gmref.rightMovement.rotation = GetRelativeDir(gmref.rightMovement.rotation);
                    }
                }
            }
        }

        #region PRIVATE METHODS

        //crea las cámaras auxiliares
        private Camera CreateAuxCamera(Camera mainCamera)
        {
            GameObject anchorGameObject = new GameObject();
            anchorGameObject.name = gameObject.name + "Aux" + mainCamera.name;
            Camera auxCam = anchorGameObject.AddComponent<Camera>();
            

            //las cámaras auxiliares tendrán las mismas características que
            //la cámara principal pero su textura será mostrada en el portal
            auxCam.CopyFrom(mainCamera);
            auxCam.tag = "auxCamera";
            auxCam.stereoTargetEye = StereoTargetEyeMask.None;
            auxCam.cullingMask = ~(1 << LayerMask.NameToLayer("Portal"));

            //desactivamos la cámara para evitar bucles
            auxCam.enabled = false;
            return auxCam;
        }

        //calcula la posición de las cámaras auxiliares
        private Vector3 GetRelativePos(Vector3 mainCameraPos)
        {
            //Calculando la posición relativa respecto al plano de entrada: Vector: centro del plano-cámara.
            //Esto se consigue con InverseTransformPoint: dando un objeto y una posición de mundo, decirte donde debería estar en coordenadas Locales al objeto (Afecta la escala).
            Vector3 relativePosition = transform.InverseTransformPoint(mainCameraPos);
            //Reflejamos este vector respecto al up del PORTAL (no de mundo), para que aparezca en el otro lado, pero manteniendo su eje superior.
            Vector3 reflectedRespectOtherPlane = Vector3.Reflect(-relativePosition, Vector3.up);
            //Calculamos de vuelta las coordenadas locales que teníamos a coordenadas de mundo respecto al otro portal.
            //Quizás hay que tocar escalas por aquí cuando queramos hacer al jugador más grande:
            return connectedPortal.TransformPoint(reflectedRespectOtherPlane);            
        }

        //calcula la rotación de las cámaras auxiliares
        private Quaternion GetRelativeDir(Quaternion mainCameraRot)
        {
            //Voy a tratar de seguir la misma lógica con las rotaciones:
            //Al no indicar rotaciones con grados, evitamos el uso de grados euler, Unity calculará internamente con cuaterniones.
            Vector3 relativeDirection = transform.InverseTransformDirection(mainCameraRot * Vector3.forward);
            Vector3 reflectedRespectOtherPlaneDir = Vector3.Reflect(-relativeDirection, Vector3.up);
            //Ya que tenemos hacia dónde debe mirar en coordenadas locales, le decimos que mire hacia allí, con la cámara apuntando hacia el up del nuevo portal.
            Vector3 relativeDirectionUp = transform.InverseTransformDirection(mainCameraRot * Vector3.up);
            Vector3 reflectedRespectOtherPlaneDirUp = Vector3.Reflect(-relativeDirectionUp, Vector3.up);
            return Quaternion.LookRotation(connectedPortal.TransformDirection(reflectedRespectOtherPlaneDir), connectedPortal.TransformDirection(reflectedRespectOtherPlaneDirUp));
        }

        private bool TryGetXRNodeOffsetEyeVector(out Vector3 leftEyePos, out Vector3 rightEyePos)
        {
            leftEyePos = Vector3.zero;
            rightEyePos = Vector3.zero;
            InputDevice deviceLeft = InputDevices.GetDeviceAtXRNode(XRNode.LeftEye);
            InputDevice deviceRight = InputDevices.GetDeviceAtXRNode(XRNode.RightEye);
            if (deviceLeft.isValid && deviceRight.isValid)
            {
                if (deviceLeft.TryGetFeatureValue(CommonUsages.leftEyePosition, out leftEyePos) &&
                    deviceRight.TryGetFeatureValue(CommonUsages.rightEyePosition, out rightEyePos))
                {
                    return true;
                }
                else
                {
                    Debug.LogWarning("Feature device Position or Rotation not found");
                    return false;
                }
            }
            Debug.LogWarning("Doesn't detect the eyes");
            return false;
        }

        private  void setCameraTextures()
        {
            //creación de la textura de los portales
            rTextureLeft = new RenderTexture(XRSettings.eyeTextureWidth * 2, XRSettings.eyeTextureHeight * 2, 32); //Increasing *2 render quality to avoid aliasing.
            rTextureRight = new RenderTexture(XRSettings.eyeTextureWidth * 2, XRSettings.eyeTextureWidth * 2, 32);

            rTextureLeft.name = gameObject.name + "RTextureLeft";
            rTextureRight.name = gameObject.name + "RTextureRight";

            portalRenderer = GetComponent<MeshRenderer>();
            portalRenderer.material = new Material(Shader.Find("TFG/portal"));
            portalRenderer.material.hideFlags = HideFlags.DontSave;

            //atajos/referencias a otras clases
            gmref = GameManager.reference;

            boxCollider = GetComponent<BoxCollider>();


            //creación de las cámaras que nos aportarán la textura de los portales
            auxCameraleft = CreateAuxCamera(gmref.lefteye);
            auxCameraright = CreateAuxCamera(gmref.righteye);

            if (parentObject != null)
            {
                auxCameraleft.transform.parent = parentObject.transform;
                auxCameraright.transform.parent = parentObject.transform;
            }

            //Asociamos las RenderTextures a los ojos asociados
            auxCameraleft.targetTexture = rTextureLeft;
            auxCameraright.targetTexture = rTextureRight;

            //Asignamos al portal la textura asociada a ambos ojos del portal.
            portalRenderer.material.SetTexture("_LeftEyeTexture", rTextureLeft);
            portalRenderer.material.SetTexture("_RightEyeTexture", rTextureRight);
        }

        #endregion
    }
}