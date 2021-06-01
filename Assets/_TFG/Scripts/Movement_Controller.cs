using TFG;
using UnityEngine;

public class Movement_Controller : MonoBehaviour
{

    #region "Variables"
    public Grid_Generator grid_generator;
    public Transform XRRig_Transform;
    public float MouseSensitivity;
    public float MoveSpeed;
    #endregion

    private void Start()
    {
        Cursor.visible = false;
    }
    void Update()
    {
        if(Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            forward.y = 0;
            forward.Normalize();
            right.y = 0;
            right.Normalize();
            XRRig_Transform.position += (forward * Input.GetAxis("Vertical") * MoveSpeed) + (right * Input.GetAxis("Horizontal") * MoveSpeed);
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.name == "Leave Portal")
        {
            if(col.GetComponent<PortalRender>().collidable == true)
            {
                Debug.Log("Collided with a leave portal");
                col.GetComponent<PortalRender>().collidable = false;
                grid_generator.leaveCollisionDetected = true;
                grid_generator.currentPortalCollided = col.transform;
            }
            else
            {
                col.GetComponent<PortalRender>().collidable = true;
            }
        }
        else if(col.gameObject.name == "Entry Portal")
        {
            if(col.GetComponent<PortalRender>().collidable == true)//To avoid entry portal to read collision at the teleport time
            {
                Debug.Log("Portal collidable, updating nodes...");
                col.GetComponent<PortalRender>().collidable = false;
                grid_generator.entryCollisionDetected = true;
                grid_generator.currentPortalCollided = col.transform;
            }
            else
            {
                Debug.Log("Portal not collidable yet.");
                col.GetComponent<PortalRender>().collidable = true;
            }
        }
    }
}