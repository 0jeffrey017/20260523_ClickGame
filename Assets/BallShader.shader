Shader "Custom/BallProceduralURP"
{
    Properties
    {
        _BaseColor("Color", Color) = (1, 1, 1, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                uint instanceID     : SV_InstanceID; // Crucial for procedural rendering
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 normalWS     : TEXCOORD0;
            };

            // Global buffers populated by the C# BallSimulationManager script
            StructuredBuffer<float3> _PositionsBuffer;
            float _Radius;

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Smoothness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                // 1. Fetch the center position of this specific ball instance
                float3 ballCenterWS = _PositionsBuffer[input.instanceID];

                // 2. Scale the original vertex coordinates by the radius and add the center offset
                float3 positionWS = (input.positionOS.xyz * _Radius) + ballCenterWS;

                // 3. Transform the calculated world space coordinates into clip space for screen output
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Simple lambert lighting so the spheres look 3D inside the box
                float3 normal = normalize(input.normalWS);
                float3 lightDir = _MainLightPosition.xyz;
                float lightIntensity = saturate(dot(normal, lightDir)) * 0.6 + 0.4;

                return _BaseColor * lightIntensity;
            }
            ENDHLSL
        }
    }
}