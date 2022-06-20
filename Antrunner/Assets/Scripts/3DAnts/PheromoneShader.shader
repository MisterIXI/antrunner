Shader "Custom/PheromoneShader"
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

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            struct Pheromone{
                float nMone;
                float pMone;
                float eMone;
            };

            StructuredBuffer<Pheromone> pheromones; 
            StructuredBuffer<float4> vertices;
            uint ant_count;
            uint width;
            uint height;
            uint width_scaled;
            uint height_scaled;
            uint downscale_factor;
        #endif
        float3 _pheromone;
        float _strength;

        void setup(){
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                _pheromone = float3(pheromones[unity_InstanceID].nMone, pheromones[unity_InstanceID].pMone, pheromones[unity_InstanceID].eMone);
                _strength = (_pheromone.x + _pheromone.y + _pheromone.z) / ant_count;
                _strength = _strength >1 ? 1 : _strength;
                width_scaled = width / downscale_factor;
                height_scaled = height / downscale_factor;
            #endif
        }

        void vert(inout appdata_custom v)
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                v.vertex = vertices[v.id];
                //scale plane to fit 1x1 square
                v.vertex *= 0.1;
                //scale plane by strength
                v.vertex *= (downscale_factor/2) * _strength;
                uint x = (unity_InstanceID % width_scaled)*downscale_factor;
                uint y = (unity_InstanceID/width_scaled)*downscale_factor;
                v.vertex.xyz += float3(x, 3,y);
                // move to middle of downscaled position
                v.vertex.xyz += float3(downscale_factor/2, 0, downscale_factor/2);
            #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            // fixed4 m = tex2D (_MetallicGlossMap, IN.uv_MainTex); 
            // o.Albedo = float4(_pheromone.x, _pheromone.y, _pheromone.z, 1);
            o.Albedo = c.rgb;
            // o.Alpha = 0.5;
            o.Alpha = c.a;
            // o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap)) * 0.000001;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
}
