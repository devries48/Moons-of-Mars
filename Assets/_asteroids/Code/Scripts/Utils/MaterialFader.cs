using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    public class MaterialFader
    {
        readonly List<MatInfo> _matOpaqueList;
        readonly List<MatInfo> _matTranspList;
        readonly float _targetAlpha = 0;

        public float m_fadeSpeed = 1f;

        public MaterialFader(GameObject model, float targetAlpha = 0)
        {
            _targetAlpha = targetAlpha;

            _matOpaqueList = new();
            _matTranspList = new();

            InitMaterials(model);
        }

        void InitMaterials(GameObject model)
        {
            foreach (var rend in model.GetComponentsInChildren<Renderer>())
            {
                var m = rend.materials;
                if (m != null)
                    foreach (var mat in m)
                        if (MatIsOpaque(mat))
                            _matOpaqueList.Add(new MatInfo(mat));
                        else
                            _matTranspList.Add(new MatInfo(mat));

            }
        }

        /// <summary>
        /// Restore material alpha to original alpha
        /// </summary>
        public IEnumerator FadeIn(bool isOpaque, float startalpha = -1)
        {
            float time = 0;
            var matList = isOpaque ? _matOpaqueList : _matTranspList;

            if (isOpaque)
                SetOpaqueMaterialsToTransparent();

            if (startalpha != -1)
            {
                foreach (var mat in matList)
                    mat.SetAlpha(startalpha);
            }

            yield return new WaitForSeconds(.5f);

            while (true)
            {
                var materials = matList.Where(m => m.Mat.color.a < m.OrgAlpha);
                if (!materials.Any())
                    break;

                foreach (var mat in materials)
                    mat.SetAlpha(Mathf.Lerp(_targetAlpha, mat.OrgAlpha, time * m_fadeSpeed));

                time += Time.deltaTime;
                yield return null;
            }

            if (isOpaque)
                RestoreOpaqueMaterials();
        }

        /// <summary>
        /// Set material alpha to target alpha
        /// </summary>
        public IEnumerator FadeOut(bool isOpaque)
        {
            var matList = isOpaque ? _matOpaqueList : _matTranspList;
            float time = 0;

            if (isOpaque)
                SetOpaqueMaterialsToTransparent();

            while (true)
            {
                var materials = matList.Where(m => m.Mat.color.a > _targetAlpha);
                if (!materials.Any())
                    break;

                foreach (var mat in materials)
                    mat.SetAlpha(Mathf.Lerp(mat.OrgAlpha, _targetAlpha, time * m_fadeSpeed));

                time += Time.deltaTime;
                yield return null;
            }
        }

        bool MatIsOpaque(Material mat) => mat.GetInt("_Surface") == 0;

        void SetOpaqueMaterialsToTransparent()
        {
            foreach (var mat in _matOpaqueList)
            {
                var m = mat.Mat;

                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetInt("_ZWrite", 0);
                m.SetInt("_Surface", 1);

                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                m.SetShaderPassEnabled("DepthOnly", false);
                m.SetShaderPassEnabled("SHADOWCASTER", true);

                m.SetOverrideTag("RenderType", "Transparent");

                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }
        }

        void RestoreOpaqueMaterials()
        {
            foreach (var mat in _matOpaqueList)
            {
                var m = mat.Mat;

                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                m.SetInt("_ZWrite", 1);
                m.SetInt("_Surface", 0);

                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

                m.SetShaderPassEnabled("DepthOnly", true);
                m.SetShaderPassEnabled("SHADOWCASTER", true);

                m.SetOverrideTag("RenderType", "Opaque");

                m.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }
        }

        class MatInfo
        {
            public MatInfo(Material mat)
            {
                Mat = mat;
                OrgAlpha = mat.color.a;
                OrgMetallic = mat.GetFloat("_Metallic");
            }

            public string Name => Mat != null ? Mat.name : null;
            public Material Mat;
            public readonly float OrgAlpha;
            public readonly float OrgMetallic;

            public void SetAlpha(float alpha)
            {
                Mat.color = new Color(
                    Mat.color.r,
                    Mat.color.g,
                    Mat.color.b,
                    alpha);

                if (OrgMetallic > 0)
                {
                    if (alpha == 0)
                        SetMetallic(0);
                    else if (alpha == OrgAlpha)
                        SetMetallic(OrgMetallic);

                }
            }

            void SetMetallic(float metallic) => Mat.SetFloat("_Metallic", metallic);
        }
    }
}