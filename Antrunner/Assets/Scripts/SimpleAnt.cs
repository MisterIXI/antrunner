using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAnt : MonoBehaviour
{
    private static int idHelper = 0;
    public int id;
    private AntSpawner.Ant[] ants;
    // Start is called before the first frame update
    void Start()
    {
        this.ants = GameObject.Find("AntControl").GetComponent<AntSpawner>().ants;
        this.id = idHelper++;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = new Vector3(ants[id].pos.x, ants[id].pos.y, 0);
        transform.rotation = Quaternion.Euler(0, 0, ants[id].rot);
        ants[id].forward = transform.up * 0.3f;
        ants[id].rot = transform.rotation.eulerAngles.z;
        //implicit conversion to Vector2 (Z is ignored)
        // ants[id].pos = transform.position;
        // ants[id].rot = transform.rotation.eulerAngles.z;
    }

    //     void OnDrawGizmos()
    // {
    //     // // Draw a red sphere at the ant head
    //     // Gizmos.color = Color.red;
    //     // Gizmos.DrawSphere(transform.position + transform.up * 0.3f, 0.1f);
       
    //     // Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //     // Gizmos.color = Color.green;
    //     // Gizmos.DrawSphere(mousePos, 0.1f);

    //     // Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.3f);
    //     // Gizmos.color = Color.red;
    //     // Gizmos.DrawLine(transform.position, Vector3.zero);
    // }

    public static void ResetIdHelper()
    {
        idHelper = 0;
    }
}
