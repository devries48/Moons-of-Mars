using UnityEngine;

[CreateAssetMenu(fileName = "Music data", menuName = "Astroids/Music Data")]

public class MusicData : ScriptableObject
{
    public AudioClip[] menuMusic;
    public AudioClip[] lowIntenseMusic;
    public AudioClip[] mediunIntenseMusic;
    public AudioClip[] highIntenseMusic;

    public enum MusicLevel {none, menu, low, medium, high }

    public AudioClip GetMusicClip(MusicLevel level)
    {
        var clip = level switch
        {
            MusicLevel.menu => RandomClip(menuMusic),
            MusicLevel.low => RandomClip(lowIntenseMusic),
            MusicLevel.medium => RandomClip(mediunIntenseMusic),
            MusicLevel.high => RandomClip(highIntenseMusic),
            _ => null
        };

        return clip;
    }

    AudioClip RandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return null;

        return clips[Random.Range(0, clips.Length)];
    }

}
