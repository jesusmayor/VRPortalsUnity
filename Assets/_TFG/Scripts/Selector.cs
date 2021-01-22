using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    // Update is called once per frame
    void FixedUpdate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (var hitCollider in hitColliders)
        {
            Interactable inter = hitCollider.gameObject.GetComponent<Interactable>();
            if (inter != null)
            {
                inter.ActivateThis();
            }
        }

    }
}
