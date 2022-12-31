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

            var level = (target as LevelManager);

            if (GUILayout.Button("Gizmo Earth path"))
            {
                level._gizmoStageIndex = 0;
            }
            if (GUILayout.Button("Gizmo Mars path"))
            {
                level._gizmoStageIndex = 1;
            }

        }

    }
}