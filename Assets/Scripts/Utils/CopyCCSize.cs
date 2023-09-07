using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyCCSize : MonoBehaviour
{
    // Start is called before the first frame update
    private CharacterController controller;
    private CapsuleCollider col;
    void Awake()
    {
        controller = GetComponentInParent<CharacterController>();
        col= GetComponentInParent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        col.height = controller.height;
        col.center = controller.center;
        col.radius = controller.radius;
    }
}
