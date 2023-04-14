using MoonsOfMars.Shared.Announcers;
using System;

namespace Game.Asteroids
{
    public class GameAnnouncer : TextAnnouncerBase
    {
        const string gamestart = "Ready";
        const string cleared = "Level Cleared!";
        const string stageCleared = "Stage Completed!";
        const string gameover = "GAME OVER";

        const string fmtLevel = "Level {0}"; // "Level 1" etc

        public TextAnnouncerBase strategy;

        public static GameAnnouncer AnnounceTo(TextAnnouncerBase strategy)
        {
            var instance = CreateInstance<GameAnnouncer>();
            instance.strategy = strategy;
            return instance;
        }

        public virtual void LevelPlaying() => ClearAnnouncements();
        public virtual void LevelCleared() => Announce(cleared);
        public virtual void StageCleared() => Announce(stageCleared);
        public virtual void LevelStarts(int level) => Announce(fmtLevel, level);
        public virtual void GameOver() => Announce(gameover);
        public override void Announce(string message) => strategy.Announce(message);
        public virtual void GameStart() => Announce(gamestart);

    }
}