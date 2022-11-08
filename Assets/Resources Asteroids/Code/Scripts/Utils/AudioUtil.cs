using System;
using System.Collections;
using UnityEngine;

public static class AudioUtil
{
    //Usage: StartCoroutine(AudioUtil.FadeOut(spawnAudio, duration));
    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime, Action action = default)
    {
        if (audioSource != null)
        {
            float startVolume = audioSource.volume;
            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
                yield return null;
            }
            audioSource.Stop();

            if (action != default)
                action();
        }

    }

    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        if (audioSource != null)
        {
            audioSource.Play();
            audioSource.volume = 0f;
            while (audioSource.volume < 1)
            {
                audioSource.volume += Time.deltaTime / FadeTime;
                yield return null;
            }
        }
    }

}