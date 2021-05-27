using UnityEngine;

public class Movement_Controller : MonoBehaviour
{

    #region "Variables"
    public Rigidbody Rigid;
    public Grid_Generator grid_generator;
    public float MouseSensitivity;
    public float MoveSpeed;
    #endregion

    private void Start()
    {
        Cursor.visible = false;
    }
    void Update()
    {
        Rigid.MoveRotation(Rigid.rotation * Quaternion.Euler(new Vector3(0, Input.GetAxis("Mouse X") * MouseSensitivity, 0)));
        Rigid.MovePosition(transform.position + (transform.forward * Input.GetAxis("Vertical") * MoveSpeed) + (transform.right * Input.GetAxis("Horizontal") * MoveSpeed));
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.name == "Leave Portal")
        {
            Debug.Log("Collided with a leave portal");
            grid_generator.leaveCollisionDetected = true;
            grid_generator.currentPortalCollided = col.transform;
        }
        else if(col.gameObject.name == "Entry Portal")
        {
            grid_generator.entryCollisionDetected = true;
            grid_generator.currentPortalCollided = col.transform;
        }
    }
}