using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos(){
        int resolution = 1;
        float ySize= 80f * 2 * resolution;
        float xSize = 150f * 2 * resolution;
        Vector2 startV = new Vector2(-150f, -80f);

        Gizmos.color = Color.red;
        for(int i = 0; i < ySize; i++){
            for(int j = 0; j < xSize; j++){
                // Gizmos.DrawSphere(new Vector3(startV.x + i* (1/resolution), startV.y + j * (1/resolution), 0), 0.1f);
            }
        }
    }
}
