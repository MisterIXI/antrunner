using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;

public class Instancer : MonoBehaviour
{

    private const int WIDTH = 1000;
    private const int HEIGHT = 1000;
    private int MAX_ANTS = 65000;
    public ComputeShader computeShader;
    public Material antMat;
    public Mesh antMesh;
    public AnimationClip antAnimClip;
    public GameObject antSample;
    private MeshRenderer meshRenderer;
    private ComputeBuffer antsBuffer;
    private ComputeBuffer vertexBuffer;
    private ComputeBuffer argsBuffer;
    private Ant[] ants;
    private int numGroupsX, numGroupsY;

    private Bounds InfiniteBounds = new Bounds(Vector3.zero, Vector3.one * 9999);

    // Start is called before the first frame update
    void Start()
    {
        // Initialize Rendertexture
        meshRenderer = GetComponentInChildren<MeshRenderer>();


        // Initialize Buffers
        ants = SpawnAnts(MAX_ANTS);
        antsBuffer = new ComputeBuffer(MAX_ANTS, sizeof(float) * 10);
        antsBuffer.SetData(ants);

        //configure Shader
        computeShader.SetInt("width", WIDTH);
        computeShader.SetInt("height", HEIGHT);
        computeShader.SetBuffer(0, "ants", antsBuffer);
        Vector3Int threadGroupSizes = GetThreadGroupSizes(computeShader, 0);
        numGroupsX = Mathf.CeilToInt(WIDTH / (float)threadGroupSizes.x);
        numGroupsY = Mathf.CeilToInt(HEIGHT / (float)threadGroupSizes.y);
        antMat.SetBuffer("ants", antsBuffer);
        //  Compute();

        argsBuffer = new ComputeBuffer(
    1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments
);

        argsBuffer.SetData(new uint[5] {
            antMesh.GetIndexCount(0), (uint) MAX_ANTS, 0, 0, 0
        });

        GenerateSkinnedAnimationForGPUBuffer();

    }

    struct Ant
    {
        public Vector2 position;
        public Vector2 direction;
        public Vector2 rotMatrix;
        public float _debug;
        public float frame;
        public float next_frame;
        public float frame_interpolation;
    }
    void Update()
    {
        Compute();
        Debug.Log("Ant0: " + ants[0].position + " | " + ants[0].direction + " | " + ants[0]._debug + " | " + ants[0].rotMatrix);
    }

    public void DebugStep(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Compute();
            // Debug.Log("Ant0: " + ants[0].position + " | " + ants[0].direction);
        }

    }

    void LateUpdate()
    {
        Graphics.DrawMeshInstancedIndirect(antMesh, 0, antMat, InfiniteBounds, argsBuffer, 0);

    }

    private void Compute()
    {
        computeShader.Dispatch(0, MAX_ANTS, 1, 1);
        antsBuffer.GetData(ants);
    }


    private Ant[] SpawnAnts(int antCount)
    {
        Ant[] ants = new Ant[antCount];
        for (int i = 0; i < antCount; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(0, WIDTH), Random.Range(0, HEIGHT), 0);
            float randomAngle = Random.Range(0, 360);
            Vector3 randomRot = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
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
        argsBuffer.Release();
    }
    public static Vector3Int GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0)
    {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        return new Vector3Int((int)x, (int)y, (int)z);
    }

    private void GenerateSkinnedAnimationForGPUBuffer()
    {
        if (antAnimClip == null)
        {
            CreateOneFrameAnimationData();
            return;
        }

        SkinnedMeshRenderer antSMR = antSample.GetComponentInChildren<SkinnedMeshRenderer>();
        Animator animator = antSample.GetComponentInChildren<Animator>();
        int iLayer = 0;
        AnimatorStateInfo aniStateInfo = animator.GetCurrentAnimatorStateInfo(iLayer);

        Mesh bakedMesh = new Mesh();
        float sampleTime = 0;
        float perFrameTime = 0;

        int FramesInAnimation = Mathf.ClosestPowerOfTwo((int)(antAnimClip.frameRate * antAnimClip.length));
        perFrameTime = antAnimClip.length / FramesInAnimation;

        var vertexCount = antSMR.sharedMesh.vertexCount;
        vertexBuffer = new ComputeBuffer(vertexCount * FramesInAnimation, Marshal.SizeOf(typeof(Vector4)));
        Vector4[] vertexAnimationData = new Vector4[vertexCount * FramesInAnimation];
        for (int i = 0; i < FramesInAnimation; i++)
        {
            animator.Play(aniStateInfo.shortNameHash, iLayer, sampleTime);
            animator.Update(0f);

            antSMR.BakeMesh(bakedMesh);

            for (int j = 0; j < vertexCount; j++)
            {
                Vector3 vertex = bakedMesh.vertices[j];
                vertexAnimationData[(j * FramesInAnimation) + i] = vertex;
            }

            sampleTime += perFrameTime;
        }
        computeShader.SetInt("NbFrames", FramesInAnimation);
        antMat.SetInt("NbFrames", FramesInAnimation);
        vertexBuffer.SetData(vertexAnimationData);
        antMat.SetBuffer("vertexAnimation", vertexBuffer);
        antSample.SetActive(false);
    }

    private void CreateOneFrameAnimationData()
    {
        var vertexCount = antMesh.vertexCount;
        int FramesInAnimation = 1;
        Vector4[] vertexAnimationData = new Vector4[vertexCount * FramesInAnimation];
        vertexBuffer = new ComputeBuffer(vertexCount * FramesInAnimation, Marshal.SizeOf(typeof(Vector4)));
        for (int j = 0; j < vertexCount; j++)
            vertexAnimationData[(j * FramesInAnimation)] = antMesh.vertices[j];

        vertexBuffer.SetData(vertexAnimationData);
        antMat.SetBuffer("vertexAnimation", vertexBuffer);
        antSample.SetActive(false);
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            // draw ant[0] direction as line
            Vector3 antPos = new Vector3(ants[0].position.x, 0, ants[0].position.y);
            Vector3 antDir = new Vector3(ants[0].direction.x, 0, ants[0].direction.y);
            Gizmos.DrawLine(antPos, antPos + antDir * 10);
        }

    }
}
