using UnityEngine;

public class Movement : MonoBehaviour
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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided");
        grid_generator.collisionDetected = true;
    }
}