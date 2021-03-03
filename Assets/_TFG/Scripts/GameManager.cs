using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TFG;
using UnityEngine.XR;
using UnityEngine.InputSystem.XR;
using UnityEditor.Experimental.GraphView;

namespace TFG
{
    /// clase con patrón Singleton que se encarga de almacenar referencias 
    /// a objetos importantes como la cabeza o el jugador

    [AddComponentMenu("TFG/Manager")]
        public class GameManager : MonoBehaviour
        {
            #region ATTRIBUTES
            // referencia a la referencia Singleton
            [HideInInspector]
            public static GameManager reference;

            /// referencia a las cámaras de la escena
            public Camera lefteye;
            public Camera righteye;
            public Camera center;
        
            /// referencia a los Transform padres de estas cámaras
            public Transform leftMovement;
            public Transform rightMovement;
            #endregion
        private void Start()
        {

        }
        void Awake()
        {
            // asociación Singleton
            if (reference == null)
                reference = this;
            else
                this.enabled = false;

            lefteye = GameObject.FindGameObjectWithTag("Left eye").GetComponent<Camera>();
            righteye = GameObject.FindGameObjectWithTag("Right eye").GetComponent<Camera>();
            center = GameObject.FindGameObjectWithTag("centerCamera").GetComponent<Camera>();
            leftMovement = GameObject.FindGameObjectWithTag("Lefteyetransform").GetComponent<Transform>();
            rightMovement= GameObject.FindGameObjectWithTag("Righteyetransform").GetComponent<Transform>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        }
    }
}