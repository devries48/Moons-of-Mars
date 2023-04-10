using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace MoonsOfMars.SolarSystem
{
    [ExecuteAlways]
    public class CustomFalloff : MonoBehaviour
    {

        public AnimationCurve lightFalloffCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public FalloffType bakedLightsFalloff = FalloffType.InverseSquared;
        public int falloffLookupTextureSize = 8;
        public TextureFormat textureFormat = TextureFormat.ARGB32;
        public FilterMode textureFilterMode = FilterMode.Trilinear;

        public void AdjustFalloffCurve()
        {

            int pixelCount = falloffLookupTextureSize;
            Texture2D m_AttenTex = new(pixelCount, 1, textureFormat, false, true)
            {
                filterMode = textureFilterMode,
                wrapMode = TextureWrapMode.Clamp
            };
            Color[] pixels = new Color[pixelCount * pixelCount];
            //Vector2 center = new(0, pixelCount / 2);
            int blackLimit = pixelCount - 1;
            int maxDistance = 10;

            for (int i = 1; i <= pixelCount; i++)
            {
                float v;

                if (i < blackLimit)
                {
                    float normalizedIntensity = lightFalloffCurve.Evaluate(1f / pixelCount * i);
                    float linearIntensity = normalizedIntensity * maxDistance;
                    v = 1.0f / (linearIntensity * linearIntensity);
                }
                else
                    v = 0.0f;

                pixels[i - 1] = new Color(v, v, v, v);
            }

            m_AttenTex.SetPixels(pixels);
            m_AttenTex.Apply();
            Shader.SetGlobalTexture("_customFalloffTexture", m_AttenTex);
        }

        void Update()
        {
            AdjustFalloffCurve();
        }

        // Inverse Square Falloff for the progressive lightmapper - Code from https://docs.unity3d.com/Manual/ProgressiveLightmapper-CustomFallOff.html
        public void OnEnable()
        {
            void testDel(Light[] requests, Unity.Collections.NativeArray<LightDataGI> lightsOutput)
            {
                DirectionalLight dLight = new();
                PointLight point = new();
                SpotLight spot = new();
                RectangleLight rect = new();
                LightDataGI ld = new();

                for (int i = 0; i < requests.Length; i++)
                {
                    Light l = requests[i];
                    switch (l.type)
                    {
                        case UnityEngine.LightType.Directional:
                            LightmapperUtils.Extract(l, ref dLight);
                            ld.Init(ref dLight);
                            break;
                        case UnityEngine.LightType.Point:
                            LightmapperUtils.Extract(l, ref point);
                            ld.Init(ref point);
                            break;
                        case UnityEngine.LightType.Spot:
                            LightmapperUtils.Extract(l, ref spot);
                            ld.Init(ref spot);
                            break;
                        case UnityEngine.LightType.Area:
                            LightmapperUtils.Extract(l, ref rect);
                            ld.Init(ref rect);
                            break;
                        default:
                            ld.InitNoBake(l.GetInstanceID());
                            break;
                    }

                    ld.falloff = bakedLightsFalloff;
                    lightsOutput[i] = ld;
                }
            }

            Lightmapping.SetDelegate(testDel);
        }

        void OnDisable()
        {
            Lightmapping.ResetDelegate();
        }

    }
}