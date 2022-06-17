using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class RenderControl : MonoBehaviour
{

    private const int WIDTH = 3840;
    private const int HEIGHT = 2160;
    private int MAX_ANTS = 1000000;
    public ComputeShader computeShader;
    private MeshRenderer meshRenderer;
    private RenderTexture renderTexture;
    private RenderTexture defaultTexture;
    private ComputeBuffer antsBuffer;
    private Ant[] ants;
    private int numGroupsX, numGroupsY;
    // Start is called before the first frame update
    void Start()
    {
        // Initialize Rendertexture
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        renderTexture = new RenderTexture(WIDTH, HEIGHT, 0, GraphicsFormat.R16G16B16A16_SFloat);
        renderTexture.enableRandomWrite = true;
        renderTexture.autoGenerateMips = false;
        renderTexture.Create();
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.filterMode = FilterMode.Point;
        defaultTexture = new RenderTexture(WIDTH, HEIGHT, 0, GraphicsFormat.R16G16B16A16_SFloat);
        defaultTexture.Create();
        Graphics.CopyTexture(renderTexture, defaultTexture);

        // Initialize Buffers
        ants = SpawnAnts(MAX_ANTS);
        antsBuffer = new ComputeBuffer(MAX_ANTS, sizeof(float) * 4);
        antsBuffer.SetData(ants);

        //configure Shader
        computeShader.SetTexture(0, "outputTexture", renderTexture);
        computeShader.SetInt("width", WIDTH);
        computeShader.SetInt("height", HEIGHT);
        computeShader.SetBuffer(0, "ants", antsBuffer);
        Vector3Int threadGroupSizes = GetThreadGroupSizes(computeShader, 0);
        numGroupsX = Mathf.CeilToInt(WIDTH / (float)threadGroupSizes.x);
        numGroupsY = Mathf.CeilToInt(HEIGHT / (float)threadGroupSizes.y);

        //  Compute();
    }

    struct Ant
    {
        public Vector2 position;
        public Vector2 direction;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Compute();
        Debug.Log("Ant0: " + ants[0].position + " | " + ants[0].direction);
    }

    private void Compute()
    {
        resetTexture();
        computeShader.Dispatch(0, numGroupsX, numGroupsY, 1);
        meshRenderer.material.SetTexture("_MainTex", renderTexture);
        antsBuffer.GetData(ants);
    }

    private void resetTexture()
    {
        Graphics.CopyTexture(defaultTexture, renderTexture);
    }

    private Ant[] SpawnAnts(int antCount)
    {
        Ant[] ants = new Ant[antCount];
        for (int i = 0; i < antCount; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(0, WIDTH), Random.Range(0, HEIGHT), 0);
            Vector3 randomRot = new Vector3(Random.Range(-1f,1f), Random.Range(-1,1), 0).normalized;
            ants[i].position = randomPos;
            ants[i].direction = randomRot;
            // Debug.Log("Ant" + i + ": " + ants[i].position + " | " + ants[i].direction);
        }
        return ants;
    }
    void OnDestroy()
    {
        //TODO: Release all computeBuffers
        antsBuffer.Release();
    }
    public static Vector3Int GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0)
    {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        return new Vector3Int((int)x, (int)y, (int)z);
    }
}
