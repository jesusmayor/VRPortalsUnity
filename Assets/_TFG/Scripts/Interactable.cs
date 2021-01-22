using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public Material green;
    new Light light;
    MeshRenderer mr;

    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
        light.color = Color.red;
        mr = GetComponent<MeshRenderer>();
    }

    public void ActivateThis()
    {
        light.color = Color.green;
        mr.material = green;
    }
}
