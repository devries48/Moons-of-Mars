using UnityEngine;

namespace MoonsOfMars.Shared
{
    [ExecuteInEditMode()]
    [System.Serializable]
    public class FlexibleUIBase : MonoBehaviour
    {
        public FlexibleUIData themeController;

        protected virtual void OnSkinUI()
        {

        }

        public virtual void Awake()
        {
           if (themeController)
                OnSkinUI();
        }

        public virtual void Update()
        {
            if (themeController)
                OnSkinUI();
        }
    }
}
