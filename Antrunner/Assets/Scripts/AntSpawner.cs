using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntSpawner : MonoBehaviour
{
    private const float rightBounds = 150f;
    private const float upperBounds = 80f;
    private const int MAX_ANTS = 10000;
    public GameObject ant;
    public Ant[] ants;

    private ComputeShader shader;
    // Start is called before the first frame update
    void Start()
    {
        ants = new Ant[MAX_ANTS];
        for (int i = 0; i < MAX_ANTS; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-80f, 80f), Random.Range(-60f, 60f), 0);
            float randomRot = Random.Range(0, 360);
            GameObject antObj = GameObject.Instantiate(ant, randomPos, new Quaternion(0, 0, randomRot, 0));
            ants[i].pos = randomPos;
            ants[i].rot = randomRot;
            ants[i].forward = antObj.transform.up * 0.3f;
        }
        shader = Resources.Load<ComputeShader>("ShadyAnt");
    }

    void Update()
    {

    }
    void FixedUpdate()
    {
        Compute();
        // Debug.Log("Ant0: " + ants[0].pos + " | " + ants[0].rot);
    }

    public struct Ant
    {
        public Vector2 pos;
        public float rot;
        public Vector2 forward;
    }

    void Compute()
    {
        ComputeBuffer antsBuffer = new ComputeBuffer(MAX_ANTS, sizeof(float) * 2 + sizeof(float) + sizeof(float) * 2);
        antsBuffer.SetData(ants);
        shader.SetFloat("rightBounds", rightBounds);
        shader.SetFloat("upperBounds", upperBounds);
        shader.SetBuffer(0, "ants", antsBuffer);
        shader.Dispatch(0, MAX_ANTS / 8, 1, 1);
        antsBuffer.GetData(ants);
        antsBuffer.Dispose();

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-rightBounds, upperBounds,0), new Vector3(-rightBounds, -upperBounds,0));
        Gizmos.DrawLine(new Vector3(-rightBounds, -upperBounds,0), new Vector3(rightBounds, -upperBounds,0));
        Gizmos.DrawLine(new Vector3(rightBounds, -upperBounds,0), new Vector3(rightBounds, upperBounds,0));
        Gizmos.DrawLine(new Vector3(rightBounds, upperBounds,0), new Vector3(-rightBounds, upperBounds,0));
    }
}
