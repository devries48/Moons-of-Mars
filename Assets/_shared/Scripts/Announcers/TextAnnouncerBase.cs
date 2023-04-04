using TMPro;
using UnityEngine;

namespace Announcers
{
    public abstract class TextAnnouncerBase : ScriptableObject
    {
        public abstract void Announce(string message);
        public virtual void ClearAnnouncements() { Announce(""); }
        public virtual void Announce(string format, object arg0) => Announce(string.Format(format, arg0));
        public static TextAnnouncerBase TextComponent(TextMeshProUGUI text) => TextComponentAnnouncer.New(text);
        public static TextAnnouncerBase Log(Object context) => ContextualLogAnnouncer.New(context);
    }
}