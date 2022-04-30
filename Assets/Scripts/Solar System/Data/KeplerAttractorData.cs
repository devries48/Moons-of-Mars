using UnityEngine;

	/// <summary>
	/// Attractor data, necessary for calculation orbit.
	/// </summary>
	[System.Serializable]
	public class KeplerAttractorData
{
	public Transform AttractorObject;
	public float AttractorMass = 1000;
	internal float GravityConstant = 0.1f;
}
