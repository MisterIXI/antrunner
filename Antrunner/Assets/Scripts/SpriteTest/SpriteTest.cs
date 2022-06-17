using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;


public class SpriteTest : MonoBehaviour
{

    private const int WIDTH = 3840;
    private const int HEIGHT = 2160;
    MeshRenderer spriteRenderer;
    public ComputeShader shader;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<MeshRenderer>();
        RenderTexture rt = new RenderTexture(3840, 2160, 0, GraphicsFormat.R16G16B16A16_SFloat);
        rt.enableRandomWrite = true;
        rt.autoGenerateMips = false;
        rt.Create();
        rt.wrapMode = TextureWrapMode.Clamp;
        rt.filterMode = FilterMode.Point;
        shader.SetTexture(0, "Result", rt);
        shader.SetInt("width", 3840);
        shader.SetInt("height", 2160);

        Vector3Int threadGroupSizes = GetThreadGroupSizes(shader, 0);
        int numGroupsX = Mathf.CeilToInt(WIDTH / (float)threadGroupSizes.x);
        int numGroupsY = Mathf.CeilToInt(HEIGHT / (float)threadGroupSizes.y);
        shader.Dispatch(0, numGroupsX, numGroupsY, 1);
        spriteRenderer.material.SetTexture("_MainTex", rt);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static Vector3Int GetThreadGroupSizes(ComputeShader compute, int kernelIndex = 0)
    {
        uint x, y, z;
        compute.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        return new Vector3Int((int)x, (int)y, (int)z);
    }
}
