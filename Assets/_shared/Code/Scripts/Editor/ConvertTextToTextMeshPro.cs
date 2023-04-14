/// Revision info:
/// Updated initial project from Naphier to make it work for Unity 2019.4
/// 
/// Changes:
/// 1) Include change from levilansing because TMP is now part of Unity.
/// 2) Added using needed for BindingFlags.

using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using TMPro.EditorUtilities;

namespace MoonsOfMars.Shared
{
    public class ConvertTextToTextMeshPro : Editor
    {
        [MenuItem("GameObject/UI/Convert To Text Mesh Pro", false, 4000)]
        static void DoIt()
        {
            if (!Selection.activeGameObject.TryGetComponent<Text>(out var uiText))
            {
                EditorUtility.DisplayDialog(
                    "ERROR!", "You must select a Unity UI Text Object to convert.", "OK", "");
                return;
            }

            MenuCommand command = new(uiText);

            var method = typeof(TMPro_CreateObjectMenu).GetMethod("CreateTextMeshProGuiObjectPerform", BindingFlags.Static | BindingFlags.NonPublic);
            method.Invoke(null, new object[] { command });

            if (!Selection.activeGameObject.TryGetComponent<TextMeshProUGUI>(out var tmp))
            {
                EditorUtility.DisplayDialog(
                    "ERROR!",
                    "Something went wrong! Text Mesh Pro did not select the newly created object.",
                    "OK",
                    "");
                return;
            }

            tmp.fontStyle = GetTmpFontStyle(uiText.fontStyle);

            tmp.fontSize = uiText.fontSize;
            tmp.fontSizeMin = uiText.resizeTextMinSize;
            tmp.fontSizeMax = uiText.resizeTextMaxSize;
            tmp.enableAutoSizing = uiText.resizeTextForBestFit;
            tmp.alignment = GetTmpAlignment(uiText.alignment);
            tmp.text = uiText.text;
            tmp.color = uiText.color;

            tmp.transform.SetParent(uiText.transform.parent);
            tmp.name = uiText.name;

            tmp.rectTransform.anchoredPosition3D = uiText.rectTransform.anchoredPosition3D;
            tmp.rectTransform.anchorMax = uiText.rectTransform.anchorMax;
            tmp.rectTransform.anchorMin = uiText.rectTransform.anchorMin;
            tmp.rectTransform.localPosition = uiText.rectTransform.localPosition;
            tmp.rectTransform.localRotation = uiText.rectTransform.localRotation;
            tmp.rectTransform.localScale = uiText.rectTransform.localScale;
            tmp.rectTransform.pivot = uiText.rectTransform.pivot;
            tmp.rectTransform.sizeDelta = uiText.rectTransform.sizeDelta;

            tmp.transform.SetSiblingIndex(uiText.transform.GetSiblingIndex());

            // Copy all other components
            Component[] components = uiText.GetComponents<Component>();
            int componentsCopied = 0;
            for (int i = 0; i < components.Length; i++)
            {
                var thisType = components[i].GetType();
                if (thisType == typeof(Text) ||
                    thisType == typeof(RectTransform) ||
                    thisType == typeof(Transform) ||
                    thisType == typeof(CanvasRenderer))
                    continue;

                UnityEditorInternal.ComponentUtility.CopyComponent(components[i]);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(tmp.gameObject);

                componentsCopied++;
            }

            if (componentsCopied == 0)
                Undo.DestroyObjectImmediate(uiText.gameObject);
            else
            {
                EditorUtility.DisplayDialog(
                    "uGUI to TextMesh Pro",
                    string.Format(
                        "{0} components copied. Please check for accuracy as some references may not transfer properly.",
                        componentsCopied),
                    "OK",
                    "");
                uiText.name += " OLD";
                uiText.gameObject.SetActive(false);
            }
        }


        private static FontStyles GetTmpFontStyle(FontStyle uGuiFontStyle)
        {
            var tmp = uGuiFontStyle switch
            {
                FontStyle.Bold => FontStyles.Bold,
                FontStyle.Italic => FontStyles.Italic,
                FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic,
                _ => FontStyles.Normal,
            };
            return tmp;
        }


        private static TextAlignmentOptions GetTmpAlignment(TextAnchor uGuiAlignment)
        {
            var alignment = uGuiAlignment switch
            {
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.MidlineLeft,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Midline,
                TextAnchor.MiddleRight => TextAlignmentOptions.MidlineRight,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.TopLeft,
            };
            return alignment;
        }
    }
}