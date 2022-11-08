using TMPro;

public class GameAnnouncer : Announcer
{
    const string gamestart = "Ready";
    const string cleared = "Level Cleared!";
    const string gameover = "GAME OVER";

    const string fmtLevel = "Level {0}"; // "Level 1" etc

    public Announcer strategy;

    public static GameAnnouncer AnnounceTo(TextMeshProUGUI text)
    {
        return AnnounceTo(TextComponent(text));
    }

    public static GameAnnouncer AnnounceTo(params Announcer[] strategies)
    {
        return AnnounceTo(Many(strategies));
    }

    public static GameAnnouncer AnnounceTo(Announcer strategy)
    {
        var instance = CreateInstance<GameAnnouncer>();
        instance.strategy = strategy;
        return instance;
    }

    public virtual void LevelPlaying()
    {
        Announce("");
    }

    public virtual void LevelCleared()
    {
        Announce(cleared);
    }

    public virtual void LevelStarts(int level)
    {
        Announce(fmtLevel, level);
    }

    public virtual void GameOver()
    {
        Announce(gameover);
    }

    public override void Announce(string message)
    {
        strategy.Announce(message);
    }

    public virtual void GameStart()
    {
        Announce(gamestart);
    }
}
