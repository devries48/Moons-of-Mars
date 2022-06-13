using UnityEngine;
using UnityEngine.Rendering;

namespace SolarSystem
{
	public class StarRenderer : MonoBehaviour
	{
		public Shader starInstanceShader;
		public float size;

		[Tooltip("Display stars in the editor.")]
        [SerializeField()]
		bool _renderInEditor = false;

		Material starMaterial;
		Mesh quadMesh;
		ComputeBuffer argsBuffer;
		ComputeBuffer starDataBuffer;
		Camera _mainCamera;
		Bounds bounds;

		public float brightnessMultiplier;

		public StarData starData;

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

				ComputeHelper.Release(argsBuffer, starDataBuffer);

				starMaterial = new Material(starInstanceShader);
				bounds = new Bounds(Vector3.zero, Vector3.one * 10);
				argsBuffer = ComputeHelper.CreateArgsBuffer(quadMesh, starData.NumStars);
				starDataBuffer = ComputeHelper.CreateStructuredBuffer(starData.Stars);

				SetBuffer();

				cmd.DrawMeshInstancedIndirect(quadMesh, 0, starMaterial, 0, argsBuffer, 0);
			}
		}

		void SetBuffer()
		{
			starMaterial.SetBuffer("StarData", starDataBuffer);
		}

		public void UpdateFixedStars() //  (EarthOrbit earth, bool geocentric)
		{
			if (Application.isPlaying)
			{
				starMaterial.SetFloat("size", size);
				starMaterial.SetVector("centre", _mainCamera.transform.position);
				starMaterial.SetFloat("brightnessMultiplier", brightnessMultiplier);

				Matrix4x4 rotMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);

				// Earth remains stationary and without rotation, so rotate the stars instead
				//if (geocentric)
				//{
				//	rotMatrix = Matrix4x4.Rotate(Quaternion.Inverse(earth.earthRot));
				//}

				starMaterial.SetMatrix("rotationMatrix", rotMatrix);

				if (_renderInEditor)
				{
					bounds.center = _mainCamera.transform.position;
					Graphics.DrawMeshInstancedIndirect(quadMesh, 0, starMaterial, bounds, argsBuffer, castShadows: ShadowCastingMode.Off, receiveShadows: false);
				}
			}
		}

		void CreateQuadMesh()
		{
			quadMesh = new Mesh();

			Vector3[] vertices = {
			new Vector3(-1,-1), // bottom left
			new Vector3(1,-1), // bottom right
			new Vector3(1,1), // top left
			new Vector3(-1, 1) // top right
		};

			int[] triangles = { 0, 2, 1, 0, 3, 2 };

			quadMesh.SetVertices(vertices);
			quadMesh.SetTriangles(triangles, 0, true);
		}

		void OnDestroy()
		{
			ComputeHelper.Release(argsBuffer, starDataBuffer);
		}

		void EditorOnlyInit()
		{
#if UNITY_EDITOR
			EditorShaderHelper.onRebindRequired += () => SetBuffer();
#endif
		}
	}
}