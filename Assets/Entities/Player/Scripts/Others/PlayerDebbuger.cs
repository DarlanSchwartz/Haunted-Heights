using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDebbuger : MonoBehaviour
{
    [SerializeField] Camera debugCamera;
    [SerializeField] KeyCode showDebugCameraKey = KeyCode.H;

    private void Awake()
    {
        if (debugCamera == null)
        {
            debugCamera = GetComponent<Camera>();
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(showDebugCameraKey))
        {
            debugCamera.enabled = !debugCamera.enabled;
        }
    }
}
