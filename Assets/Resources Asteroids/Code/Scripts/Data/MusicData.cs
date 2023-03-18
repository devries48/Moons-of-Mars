using UnityEngine;

[CreateAssetMenu(fileName = "Music data", menuName = "Asteroids/Music Data")]

public class MusicData : ScriptableObject
{
    public AudioClip[] menuMusic;
    public AudioClip[] pauseMusic;
    public AudioClip[] stageCompleteMusic;
    public AudioClip[] lowIntenseMusic;
    public AudioClip[] mediunIntenseMusic;
    public AudioClip[] highIntenseMusic;

    public enum MusicLevel { none, menu, pause, stage, low, medium, high }

    public AudioClip GetMusicClip(MusicLevel level)
    {
        var clip = level switch
        {
            MusicLevel.menu => RandomClip(menuMusic),
            MusicLevel.pause => RandomClip(pauseMusic),
            MusicLevel.stage => RandomClip(stageCompleteMusic),
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
