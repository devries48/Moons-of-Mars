using System.Collections;
using UnityEngine.Audio;
using UnityEngine;

namespace MoonsOfMars.Shared
{
    public static class FadeMixerGroup
    {
        public const string s_BACKGROUND_VOL = "Volume_Background";

        // https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/#:~:text=You%20can%20fade%20audio%20in,script%20will%20do%20the%20rest.
        // StartCoroutine(FadeMixerGroup.StartFade(AudioMixer audioMixer, String exposedParameter, float duration, float targetVolume));
        public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
        {
            audioMixer.GetFloat(exposedParam, out float currentVol);
            currentVol = Mathf.Pow(10, currentVol / 20);

            float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);
            float currentTime = 0;

            while (currentTime < duration)
            {
                currentTime += Time.unscaledDeltaTime;
                float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
                audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
                yield return null;
            }

            yield break;
        }

        public static float GetCurrentVolume(AudioMixer audioMixer, string exposedParam)
        {
            audioMixer.GetFloat(exposedParam, out float currentVol);
            return Mathf.Pow(10, currentVol / 20);
        }
    }
}