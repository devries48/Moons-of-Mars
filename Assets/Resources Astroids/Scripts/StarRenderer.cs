using UnityEngine;
using UnityEngine.Rendering;

namespace SolarSystem
{
    public class StarRenderer : MonoBehaviour
    {
        #region editor fields

        [SerializeField()]Shader starInstanceShader;
        [SerializeField()] float size;

        [Tooltip("Display stars in the editor.")]
        [SerializeField()] bool renderInEditor = false;

        [SerializeField()] float brightnessMultiplier;
        [SerializeField()] StarData starData;

        #endregion


        #region fields

        Material _starMaterial;
        Mesh _quadMesh;
        ComputeBuffer _argsBuffer;
        ComputeBuffer _starDataBuffer;
        Camera _mainCamera;
        Bounds _bounds;
        #endregion

        private void Update()
        {
            UpdateFixedStars();
        }

        public void SetUpStarRenderingCommand(CommandBuffer cmd, Camera cam)
        {
            if (Application.isPlaying)
            {
                _mainCamera = cam;

                //stars = loader.LoadStars();
                CreateQuadMesh();
                EditorOnlyInit();

                ComputeHelper.Release(_argsBuffer, _starDataBuffer);

                _starMaterial = new Material(starInstanceShader);
                _bounds = new Bounds(Vector3.zero, Vector3.one * 10);
                _argsBuffer = ComputeHelper.CreateArgsBuffer(_quadMesh, starData.NumStars);
                _starDataBuffer = ComputeHelper.CreateStructuredBuffer(starData.Stars);

                SetBuffer();

                cmd.DrawMeshInstancedIndirect(_quadMesh, 0, _starMaterial, 0, _argsBuffer, 0);
            }
        }

        void SetBuffer()
        {
            _starMaterial.SetBuffer("StarData", _starDataBuffer);
        }

        public void UpdateFixedStars() //  (EarthOrbit earth, bool geocentric)
        {
            if (Application.isPlaying)
            {
                _starMaterial.SetFloat("size", size);
                _starMaterial.SetVector("centre", _mainCamera.transform.position);
                _starMaterial.SetFloat("brightnessMultiplier", brightnessMultiplier);

                Matrix4x4 rotMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

                _starMaterial.SetMatrix("rotationMatrix", rotMatrix);

                if (renderInEditor)
                {
                    _bounds.center = _mainCamera.transform.position;
                    Graphics.DrawMeshInstancedIndirect(_quadMesh, 0, _starMaterial, _bounds, _argsBuffer, castShadows: ShadowCastingMode.Off, receiveShadows: false);
                }
            }
        }

        void CreateQuadMesh()
        {
            _quadMesh = new Mesh();

            Vector3[] vertices = {
            new Vector3(-1,-1), // bottom left
			new Vector3(1,-1),	// bottom right
			new Vector3(1,1),	// top left
			new Vector3(-1, 1)	// top right
		};

            int[] triangles = { 0, 2, 1, 0, 3, 2 };

            _quadMesh.SetVertices(vertices);
            _quadMesh.SetTriangles(triangles, 0, true);
        }

        void OnDestroy()
        {
            ComputeHelper.Release(_argsBuffer, _starDataBuffer);
        }

        void EditorOnlyInit()
        {
#if UNITY_EDITOR
            EditorShaderHelper.onRebindRequired += () => SetBuffer();
#endif
        }
    }
}