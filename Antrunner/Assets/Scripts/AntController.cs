using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : MonoBehaviour
{
    public GameObject ant;
    // static int antCount = 100;
    long lastSpawn;
    private Vector3 headPos;

    private static int intHelper = 0;
    private int id;
    // Start is called before the first frame update
    void Start()
    {
        lastSpawn = System.DateTime.Now.Ticks;
        id = intHelper++;
    }

    // Update is called once per frame
    void Update()
    {
        // long time = System.DateTime.Now.Ticks;
        // Debug.Log(time - lastSpawn);
        // if (time - lastSpawn > 10000000)
        // {
        //     lastSpawn = time;
        //     Instantiate(ant, headPos, Quaternion.identity);
        // }
        // //time.deltaTime
    }
    void FixedUpdate()
    {
        if (true)
        {

            headPos = transform.position + transform.up * 0.3f;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            float angle = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x) * Mathf.Rad2Deg - 90f;
            //cap rotation at 15 to limit the ant's rotation
            // Debug.Log(angle + " | " + (transform.rotation.eulerAngles.z));
            // Debug.Log(Quaternion.Angle(transform.rotation, Quaternion.Euler(0, 0, angle)));
            if (Random.value > 0.5f)
            {
                transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 0.1f);
            }

            Jiggle();

            if (Vector3.Distance(mousePos, transform.position) > 0.3f)
            {
                transform.Translate(transform.up * 0.1f, Space.World);
            }
        }
    }

    void Jiggle()
    {
        transform.Rotate(0, 0, Random.value * 10);
    }


    void Compute()
    {
        
    }

    void OnDrawGizmos()
    {
        // // Draw a red sphere at the ant head
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(transform.position + transform.up * 0.3f, 0.1f);
       
        // Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Gizmos.color = Color.green;
        // Gizmos.DrawSphere(mousePos, 0.1f);

        // Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.3f);
        // Gizmos.color = Color.red;
        // Gizmos.DrawLine(transform.position, transform.up * 0.1f);
    }
}
