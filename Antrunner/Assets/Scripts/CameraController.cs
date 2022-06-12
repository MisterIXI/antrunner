using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour
{
    private bool movementEnabled;
    public float cameraScrollSpeed = 0.1f;
    public float zoomSensitivity = 0.05f;

    void Start()
    {
        movementEnabled = false;
    }

    void MoveCamera()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        transform.Translate(x, 0, z);
    }

    public void MoveCameraEnabled(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Debug.Log("Enabled camera movement");
            movementEnabled = true;
        }
        else if (context.canceled)
        {
            // Debug.Log("Disabled camera movement");
            movementEnabled = false;
        }
    }

    public void MoveCamera(InputAction.CallbackContext context)
    {
        if (movementEnabled)
        {
            Vector2 mouseDelta = context.ReadValue<Vector2>();
            // Debug.Log("Mouse Movement: " + mouseDelta);
            transform.Translate(-mouseDelta.x * cameraScrollSpeed, -mouseDelta.y * cameraScrollSpeed, 0);
        }
    }


    public void Zoom(InputAction.CallbackContext context)
    {
        float scroll = context.ReadValue<float>();
        float newZoom = Camera.main.orthographicSize - scroll * zoomSensitivity;
        if (newZoom > 0)
        {
            GetComponent<Camera>().orthographicSize = newZoom;
        }
    }

}
