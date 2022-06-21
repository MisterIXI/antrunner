Shader "Custom/AntInstanceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        
        CGPROGRAM
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _MetallicGlossMap;
        struct appdata_custom {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            float4 tangent : TANGENT;
            
            uint id : SV_VertexID;
            uint inst : SV_InstanceID;

            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        struct Input {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float3 worldPos;
        };
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        #pragma multi_compile __ FRAME_INTERPOLATION
        #pragma surface surf Standard vertex:vert addshadow nolightmap
        #pragma instancing_options procedural:setup

        float2 _antPos;
        float2 _antDir;
        float4x4 _antRotMatrix;

        int _CurrentFrame;
        int _NextFrame;
        float _FrameInterpolation;
        int NbFrames;

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            struct Ant
            {
                float2 position;
                float2 direction;
                float2 rotMatrix;
                uint pheromone;
                float _debug;
                float frame;
                float next_frame;
                float frame_interpolation;
            };

            StructuredBuffer<Ant> ants; 
            StructuredBuffer<float4> vertexAnimation; 

        #endif

        void setup(){
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                _CurrentFrame = ants[unity_InstanceID].frame;
                _NextFrame = ants[unity_InstanceID].next_frame;
                _FrameInterpolation = ants[unity_InstanceID].frame_interpolation;
                _antPos = ants[unity_InstanceID].position;
                _antDir = ants[unity_InstanceID].direction;
                float2 tempMatrix = ants[unity_InstanceID].rotMatrix;
                _antRotMatrix = float4x4(
                tempMatrix.y, 0, -tempMatrix.x,0,
                0,  1,  0,  0,
                tempMatrix.x, 0, tempMatrix.y,0,
                0,  0,  0,  1
                );

 
            #endif
        }

        void vert(inout appdata_custom v)
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                v.vertex = lerp(vertexAnimation[v.id * NbFrames + _CurrentFrame], vertexAnimation[v.id * NbFrames + _NextFrame], _FrameInterpolation);
                // v.vertex *= 5;
                v.vertex = mul( _antRotMatrix, v.vertex);
                // move vertex to ant position
                // v.vertex.xyz += float3(_antPos.x, 1, _antPos.y);
                // (visualisation) change vertical position so ants are in a 3D cube
                v.vertex.xyz += float3(_antPos.x,unity_InstanceID * 0.01f , _antPos.y);
                //move head to antPos
                v.vertex.xyz -= float3(_antDir.x*3, 0, _antDir.y*3);

                
            #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            // fixed4 m = tex2D (_MetallicGlossMap, IN.uv_MainTex); 
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            // o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap)) * 0.000001;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
}
