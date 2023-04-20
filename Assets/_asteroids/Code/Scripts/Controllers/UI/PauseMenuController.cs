using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    public class PauseMenuController : MonoBehaviour
    {
        public void ResumeGame()
        {
            AsteroidsGameManager.GmManager.GameResume();
        }
     }
}