using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoveCamera();
        Zoom();
    }

    void MoveCamera()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        transform.Translate(x, 0, z);
    }

    void Zoom(){
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(0, scroll, 0, Space.World);
    }

}
