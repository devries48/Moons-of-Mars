using System.Collections;
using UnityEngine.Audio;
using UnityEngine;

//todo: move to shared include documentation
public static class FadeMixerGroup
{
    public static IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume, float startVolume = -1f)
    {
        if (startVolume >= 0f)
            SetVolume(audioMixer, exposedParam, startVolume);

        audioMixer.GetFloat(exposedParam, out float currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);

        float currentTime = 0;
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);
        
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            SetVolume(audioMixer, exposedParam, newVol);
            yield return null;
        }
        yield break;
    }

    static void SetVolume(AudioMixer audioMixer, string exposedParam, float vol)
    {
        audioMixer.SetFloat(exposedParam, Mathf.Log10(vol) * 20);
    }
}