using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalColliderManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Portal"))
        {
            Debug.Log("Portal collision detected");
        }
    }
}
