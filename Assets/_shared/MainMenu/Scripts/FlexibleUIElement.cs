using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace MoonsOfMars.Shared
{
    [System.Serializable]
    public class FlexibleUIElement : FlexibleUIBase
    {
        [SerializeField] bool _textIsThemeColor;
        [SerializeField] bool _dimElement;
        protected override void OnSkinUI()
        {
            base.OnSkinUI();

            gameObject.TryGetComponent<Image>(out var image);
            if (image != null)
            {
                image = GetComponent<Image>();
                image.color = GetImageColor();
            }

            gameObject.TryGetComponent<TextMeshPro>(out var tmp);
            if (tmp != null)
                tmp.color = GetTextColor();
            else
            {
                gameObject.TryGetComponent<TextMeshProUGUI>(out var tmpUi);
                if (tmpUi != null)
                    tmpUi.color = GetTextColor();
            }

        }

        Color GetImageColor()
        {
            Color color= themeController.currentColor;
            if (_dimElement)
                color.a = .2f;

            return color;
        }

        Color GetTextColor()
        {
            Color color= _textIsThemeColor ? themeController.currentColor : themeController.textColor;
            if (_dimElement)
                color.a = .2f;

            return color;
        }



    }
}