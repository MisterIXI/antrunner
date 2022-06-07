using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : MonoBehaviour
{
    private Vector3 headPos;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        headPos = transform.position + transform.up * 0.3f;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        float angle = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg - 90f;
        //cap rotation at 15 to limit the ant's rotation
        Debug.Log(angle + " | " + (transform.rotation.eulerAngles.z));
        Debug.Log(Quaternion.Angle(transform.rotation, Quaternion.Euler(0, 0, angle)));
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 0.1f);

        // transform.rotation = Quaternion.Euler(0, 0, angle + 90f);

        if (Vector3.Distance(mousePos, transform.position) > 0.3f)
        {
            transform.Translate(transform.up * 1.1f, Space.World);
            // transform.Translate(transform.right * 0.1f);
        }
    }

    void OnDrawGizmos()
    {
        // // Draw a red sphere at the ant head
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(transform.position + transform.up * 0.3f, 0.1f);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(mousePos, 0.1f);

        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.up * 0.1f);
    }
}
