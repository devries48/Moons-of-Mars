using UnityEditor;
using UnityEngine;

namespace Game.Astroids
{
    [CustomEditor(typeof(HudManager))]
    public class HudManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("Test", EditorStyles.boldLabel);

            if (GUILayout.Button("Add Hyper-Jump"))
                (target as HudManager).AddHyperJump();

            if (GUILayout.Button("Remove Hyper-Jump"))
                (target as HudManager).RemoveHyperJump();

            if (GUILayout.Button("Add Fuel"))
                (target as HudManager).hudController.SetFuelPercentage(100);

            if (GUILayout.Button("Remove Fuel"))
                (target as HudManager).hudController.SetFuelPercentage(9.9f);

            if (GUILayout.Button("Empty Fuel"))
                (target as HudManager).hudController.SetFuelPercentage(0);

            if (GUILayout.Button("Shield"))
                (target as HudManager).ActivateShield(10f);
            if (GUILayout.Button("fire-rate"))
                (target as HudManager).ActivateWeapon(10f, PowerupManager.PowerupWeapon.firerate);
            if (GUILayout.Button("shot-spread"))
                (target as HudManager).ActivateWeapon(10f, PowerupManager.PowerupWeapon.shotSpread);

        }
    }
}