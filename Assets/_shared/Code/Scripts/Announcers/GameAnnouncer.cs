using MoonsOfMars.Shared.Announcers;

namespace MoonsOfMars.Shared
{
    public class GameAnnouncer : TextAnnouncerBase
    {
        public const string Ready = "Ready";
        public const string LevelStart = "Level {0}"; // "Level 1" etc
        public const string LevelCleared = "Level Cleared!";
        public const string StageCompleted = "Stage Completed!";
        public const string Gameover = "GAME OVER";

        public TextAnnouncerBase strategy;

        public static GameAnnouncer AnnounceTo(TextAnnouncerBase strategy)
        {
            var instance = CreateInstance<GameAnnouncer>();
            instance.strategy = strategy;
            return instance;
        }

        //public virtual void LevelPlaying() => ClearAnnouncements();
        //public virtual void LevelCleared() => Announce(cleared);
        //public virtual void StageCleared() => Announce(stageCleared);
        //public virtual void GameOver() => Announce(gameover);
        public override void Announce(string message) => strategy.Announce(message);
        public override void Announce(string format, object arg0) => strategy.Announce(string.Format(format, arg0));

        //public virtual void GameStart() => Announce(gamestart);

    }
}