using UnityEngine;
using TMPro;

namespace Announcers
{
    public class TextComponentAnnouncer : TextAnnouncerBase
    {
        public TextMeshProUGUI m_TmPro;

        static Vector3 _minScale;
        static Vector3 _defScale;

        public static TextComponentAnnouncer New(TextMeshProUGUI tmpro)
        {
            var instance = CreateInstance<TextComponentAnnouncer>();
            instance.m_TmPro = tmpro;

            _minScale = tmpro.rectTransform.localScale / 10;
            _defScale = tmpro.rectTransform.localScale;

            return instance;
        }

        public override void Announce(string message)
        {
            if (m_TmPro == null)
                return;

            if (string.IsNullOrEmpty(message))
            {
                m_TmPro.rectTransform.localScale = _defScale * 1.5f;
                LeanTween.scale(m_TmPro.rectTransform, _minScale, .5f).setDelay(.5f).setOnComplete(() =>
                {
                    m_TmPro.text = "";
                });
            }
            else
            {
                m_TmPro.rectTransform.localScale = _minScale;
                LeanTween.scale(m_TmPro.rectTransform, _defScale * 1.5f, 2f).setEaseOutElastic();
                m_TmPro.text = message;
            }

        }
    }
}