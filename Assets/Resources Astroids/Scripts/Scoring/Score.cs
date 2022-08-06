namespace Game.Astroids
{
    public static class Score
    {
        public delegate void PointsAdded(int points);

        public static event PointsAdded OnEarn;

        public static int Earned { get; private set; }

        public static void Reset()
        {
            Earned = 0;
            Invoke_onEarn(0);
        }

        public static void Earn(int points)
        {
            Earned += points;
            Invoke_onEarn(points);
        }

        // The idea is that UI can have a way to 
        // present a breakdown of the tally of 
        // all earnings. For now, let's just 
        // trick the listeners we got zero points
        // so they can display feedback to user.
        public static void Tally()
        {
            Invoke_onEarn(0);
        }

        static void Invoke_onEarn(int points)
        {
            OnEarn?.Invoke(points);
        }

        public static void LevelCleared(int level)
        {
            Earn(level * 100);
        }
    }
}