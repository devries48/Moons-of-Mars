using UnityEngine;
using static UnityEditor.PlayerSettings;

// todo: documentation
// https://www.youtube.com/watch?v=NYysvuyivc4
namespace Game.Astroids
{
    public class LightCheckController : MonoBehaviour
    {
        [SerializeField] RenderTexture lightCheckTexture;

        float _nextActionTime = 0.0f;
        readonly float _checkPeriod = 1f;
       
        //TODO: public event like Action<int> EventThrustChanged = delegate { };
        public delegate void LightLevelChanged(int level);
        public event LightLevelChanged OnLevelChanged;

        readonly System.Func<Color, float> luminance = c => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;

        void Update()
        {
            if (Time.time <= _nextActionTime)
                return;

            _nextActionTime += _checkPeriod;

            var tmpTexture = RenderTexture.GetTemporary(lightCheckTexture.width, lightCheckTexture.height, 0);
            Graphics.Blit(lightCheckTexture, tmpTexture);
            var prev = RenderTexture.active;
            RenderTexture.active = tmpTexture;

            var temp2DTexture = new Texture2D(lightCheckTexture.width, lightCheckTexture.height);
            temp2DTexture.ReadPixels(new Rect(0, 0, tmpTexture.width, tmpTexture.height), 0, 0);
            temp2DTexture.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(tmpTexture);

            var colors = temp2DTexture.GetPixels32();

            var LightLevel = 0f;
            for (int i = 0; i < colors.Length; i++)
                LightLevel += luminance(colors[i]);

            OnLevelChanged?.Invoke((int)LightLevel);
        }
    }
}