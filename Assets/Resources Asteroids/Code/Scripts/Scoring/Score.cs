using UnityEngine;

namespace Game.Asteroids
{
    public static class Score
    {
        public delegate void PointsAdded(int points, Vector3 pos);

        public static event PointsAdded OnEarn;

        public static int Earned { get; private set; }

        public static void Reset()
        {
            Earned = 0;
            Invoke_onEarn(0, Vector3.zero);
        }

        public static void Earn(int points, Vector3 pos)
        {
            Earned += points;
            Invoke_onEarn(points, pos);
        }

        public static void Earn(int points, GameObject target)
        {
            Earned += points;
            var pos = (target != null) ? target.transform.position : Vector3.zero;

            Invoke_onEarn(points, pos);
        }

        // The idea is that UI can have a way to 
        // present a breakdown of the tally of 
        // all earnings. For now, let's just 
        // trick the listeners we got zero points
        // so they can display feedback to user.
        public static void Tally()
        {
            Invoke_onEarn(0, Vector3.zero);
        }

        static void Invoke_onEarn(int points, Vector3 pos)
        {
            OnEarn?.Invoke(points, pos);
        }

        public static void LevelCleared(int level)
        {
            //Earn(level * 100, null);
        }
    }
}