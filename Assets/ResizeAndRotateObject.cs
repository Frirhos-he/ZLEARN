using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ResizeAndRotateObject : MonoBehaviour
{

    [FormerlySerializedAs("scale")]
    public float Scale = 1;
    // Start is called before the first frame update
    void Start()
    {
        Quaternion cameraRotation = Camera.main.transform.rotation;
        transform.localScale = new Vector3(Scale,Scale,Scale);
        //modifica la rotazione lungo y per ruotare tutto verso la camera
        var rotation = transform.rotation.eulerAngles;
        rotation.y = cameraRotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(rotation);
    }
}
