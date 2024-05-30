using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacingCameraCanvas : MonoBehaviour
{
    private void Start()
    {
        Quaternion cameraRotation = Camera.main.transform.rotation;
        transform.LookAt(transform.position + cameraRotation * Vector3.forward, cameraRotation * Vector3.up);
    }

    void Update()
    {
        Quaternion cameraRotation = Camera.main.transform.rotation;
        transform.LookAt(transform.position + cameraRotation * Vector3.forward, cameraRotation * Vector3.up);
    }

}
