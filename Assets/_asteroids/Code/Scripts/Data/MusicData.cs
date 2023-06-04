using UnityEngine;

[CreateAssetMenu(fileName = "Music data", menuName = "Asteroids/Music Data")]

public class MusicData : ScriptableObject
{
    [SerializeField] AudioClip[] menuMusic;
    [SerializeField] AudioClip[] pauseMusic;
    [SerializeField] AudioClip[] stageCompleteMusic;
    [SerializeField] AudioClip[] lowIntenseMusic;
    [SerializeField] AudioClip[] mediunIntenseMusic;
    [SerializeField] AudioClip[] highIntenseMusic;
    [SerializeField, Tooltip("The game is over")] AudioClip[] defeatMusic;
    [SerializeField, Tooltip("Game completed and/or high score reached")] AudioClip[] victoryMusic;

    public enum MusicLevel { none, menu, pause, stage, victory, defeat, low, medium, high }

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
            MusicLevel.defeat => RandomClip(defeatMusic),
            MusicLevel.victory => RandomClip(victoryMusic),
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
