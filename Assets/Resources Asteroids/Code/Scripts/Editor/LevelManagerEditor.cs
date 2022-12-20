using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Astroids
{
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Create path"))
            {
                var level = (target as LevelManager);
                level.m_EarthPath = level.controlPoints.ToList().Select(t => t.position).ToArray();

                var c = level.controlPoints;
                Debug.Log($"new Vector3({c[0].position.x}f, {c[0].position.y}f, {c[0].position.z}f)".ToString(new CultureInfo("en-US", false)));
                Debug.Log($"new Vector3({c[1].position.x}f, {c[1].position.y}f, {c[1].position.z}f)".ToString(new CultureInfo("en-US", false)));
                Debug.Log($"new Vector3({c[2].position.x}f, {c[2].position.y}f, {c[2].position.z}f)".ToString(new CultureInfo("en-US", false)));
                Debug.Log($"new Vector3({c[3].position.x}f, {c[3].position.y}f, {c[3].position.z}f)".ToString(new CultureInfo("en-US", false)));
            }

        }
 
    }
}