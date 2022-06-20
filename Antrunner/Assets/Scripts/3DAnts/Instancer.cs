using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;

public class Instancer : MonoBehaviour
{

    private const int WIDTH = 300;
    private const int HEIGHT = 300;
    private const int DOWNSCALE_FACTOR = 2;
    private readonly int ANT_COUNT = 1;
    private readonly int ANT_MULT = 1;
    public ComputeShader computeShader;
    public Material antMat;
    public Material pheromoneMat;
    public Mesh antMesh;
    public Mesh planeMesh;
    public AnimationClip antAnimClip;
    public GameObject antSample;
    private MeshRenderer meshRenderer;
    private ComputeBuffer antsBuffer;
    private ComputeBuffer vertexBuffer;
    private ComputeBuffer antArgsBuffer;
    private ComputeBuffer pheromoneBuffer;
    private ComputeBuffer pheromoneVertexBuffer;
    private ComputeBuffer pheromoneArgsBuffer;
    private Ant[] ants;
    private Pheromone[] pheromones;

    private Bounds InfiniteBounds = new Bounds(Vector3.zero, Vector3.one * 9999);

    // Start is called before the first frame update
    void Start()
    {
        // Initialize Rendertexture
        meshRenderer = GetComponentInChildren<MeshRenderer>();


        // Initialize Buffers
        ants = SpawnAnts(ANT_COUNT * ANT_MULT);
        antsBuffer = new ComputeBuffer(ANT_COUNT * ANT_MULT, sizeof(uint) + sizeof(float) * 10);
        antsBuffer.SetData(ants);
        pheromones = new Pheromone[WIDTH * HEIGHT / DOWNSCALE_FACTOR];
        pheromoneBuffer = new ComputeBuffer(HEIGHT * WIDTH / DOWNSCALE_FACTOR, Marshal.SizeOf(typeof(Pheromone)));
        pheromoneBuffer.SetData(pheromones);

        //configure Shader
        computeShader.SetInt("width", WIDTH);
        computeShader.SetInt("height", HEIGHT);
        computeShader.SetInt("downscaleFactor", DOWNSCALE_FACTOR);
        computeShader.SetFloat("decay", 0.001f);
        computeShader.SetBuffer(0, "ants", antsBuffer);
        computeShader.SetBuffer(0, "pheromones", pheromoneBuffer);
        computeShader.SetBuffer(1, "ants", antsBuffer);
        computeShader.SetBuffer(1, "pheromones", pheromoneBuffer);
        computeShader.SetBuffer(2, "pheromones", pheromoneBuffer);
        Vector3Int threadGroupSizes = GetThreadGroupSizes(computeShader, 0);
        antMat.SetBuffer("ants", antsBuffer);
        antMat.SetInteger("downscale_factor", DOWNSCALE_FACTOR);
        //  Compute();

        antArgsBuffer = new ComputeBuffer(
            1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments
            );
        antArgsBuffer.SetData(new uint[5] {
            antMesh.GetIndexCount(0), (uint) (ANT_COUNT * ANT_MULT), 0, 0, 0
        });
        pheromoneArgsBuffer = new ComputeBuffer(
            1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments
            );
        pheromoneArgsBuffer.SetData(new uint[5] {
            planeMesh.GetIndexCount(0), (uint) (WIDTH* HEIGHT/2), 0, 0, 0
        });
        GenerateSkinnedAnimationForGPUBuffer();

    }

    struct Pheromone
    {
        public float nMone;
        public float pMone;
        public float eMone;
    };

    struct Ant
    {
        public Vector2 position;
        public Vector2 direction;
        public Vector2 rotMatrix;
        public uint pheromone;
        public float _debug;
        public float frame;
        public float next_frame;
        public float frame_interpolation;
    }
    void Update()
    {
        Compute();
        // Debug.Log("Ant0: " + ants[0].position + " | " + ants[0].direction + " | " + ants[0]._debug + " | " + ants[0].rotMatrix);
        // Debug.Log("Ant1: " + ants[1].position + " | " + ants[1].direction + " | " + ants[1]._debug + " | " + ants[1].rotMatrix);
        // Debug.Log("Ant2: " + ants[2].position + " | " + ants[2].direction + " | " + ants[2]._debug + " | " + ants[2].rotMatrix);

    }

    public void DebugStep(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Compute();
            Debug.Log("Ant0: " + ants[0].position + " | " + ants[0].direction + " | " + ants[0]._debug + " | " + ants[0].rotMatrix);

        }

    }

    void LateUpdate()
    {

        Graphics.DrawMeshInstancedIndirect(antMesh, 0, antMat, InfiniteBounds, antArgsBuffer, 0);
        Graphics.DrawMeshInstancedIndirect(planeMesh, 0, pheromoneMat, InfiniteBounds, pheromoneArgsBuffer, 0);

    }

    private void Compute()
    {
        computeShader.Dispatch(0, ANT_COUNT, ANT_MULT, 1);
        computeShader.Dispatch(1, ANT_COUNT, ANT_MULT, 1);
        pheromoneBuffer.GetData(pheromones);
        computeShader.Dispatch(2, WIDTH/2, HEIGHT/2, 1);
        pheromoneBuffer.GetData(pheromones);
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
            ants[i].pheromone = 1;
            // Debug.Log("Ant" + i + ": " + ants[i].position + " | " + ants[i].direction);
        }
        ants[0].pheromone = 2;
        return ants;
    }
    void OnDestroy()
    {
        //TODO: Release all computeBuffers
        antsBuffer.Release();
        antArgsBuffer.Release();
        vertexBuffer.Release();
        pheromoneBuffer.Release();
        pheromoneVertexBuffer.Release();
        pheromoneArgsBuffer.Release();

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
        antMat.SetInteger("NbFrames", FramesInAnimation);
        vertexBuffer.SetData(vertexAnimationData);
        antMat.SetBuffer("vertexAnimation", vertexBuffer);
        antSample.SetActive(false);

        pheromoneVertexBuffer = new ComputeBuffer(planeMesh.vertexCount, Marshal.SizeOf(typeof(Vector4)));
        Vector4[] pheromoneVertices = new Vector4[planeMesh.vertexCount];
        for (int i = 0; i < planeMesh.vertexCount; i++)
        {
            pheromoneVertices[i] = planeMesh.vertices[i];
        }
        pheromoneVertexBuffer.SetData(pheromoneVertices);
        pheromoneMat.SetInteger("height", HEIGHT);
        pheromoneMat.SetInteger("width", WIDTH);
        pheromoneMat.SetBuffer("vertices", pheromoneVertexBuffer);
        pheromoneMat.SetBuffer("pheromones", pheromoneBuffer);
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
    public int debugAngle = 45;
    public int debugDist = 2;
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            // draw ant[0] direction as line
            Vector3 antPos = new Vector3(ants[0].position.x, 0, ants[0].position.y);
            Vector3 antDir = new Vector3(ants[0].direction.x, 0, ants[0].direction.y);
            // Vector3 debugDir = 
            Gizmos.DrawLine(antPos, antPos + antDir * 10);
            Gizmos.DrawLine(antPos, antPos + Vector3.up * 10);

            float s = Mathf.Sin(debugAngle / 180f * Mathf.PI);
            float c = Mathf.Cos(debugAngle / 180f * Mathf.PI);
            Vector3 leftCheck = new Vector3(c * antDir.x - s * antDir.z, 0, s * antDir.x + c * antDir.z);
            Gizmos.DrawCube(antPos + leftCheck * debugDist, Vector3.one * 1f);
        }

    }
}
