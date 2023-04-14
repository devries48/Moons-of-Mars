using System;
using System.Collections;
using UnityEngine;

namespace MoonsOfMars.Shared
{
    public class Utils
    {
        public enum OjectLayer
        {
            Default = 0,
            Game = 9,
            Background = 11,
        }

        public static void SetGameObjectLayer(GameObject obj, OjectLayer layer)
        {
            int val = (int)layer;
            if (val == obj.layer) return;

            obj.layer = val;
            SetGameObjectLayerRecursive(obj, val);
        }

        static void SetGameObjectLayerRecursive(GameObject obj, int layer)
        {

            foreach (Transform child in obj.transform)
            {
                child.gameObject.layer = layer;

                var hasChild = child.GetComponentInChildren<Transform>();
                if (hasChild != null)
                    SetGameObjectLayerRecursive(child.gameObject, layer);
            }
        }

        public static IEnumerator WaitUntilTrue(Func<bool> checkMethod)
        {
            while (checkMethod() == false)
            {
                yield return null;
            }
        }
    }
}