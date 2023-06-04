using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonsOfMars.Shared
{
    public class SceneLoader : MonoBehaviour
    {
        // The id's of the scenes can be obtained in File/Build Settings...
        public enum SceneName
        {
            Asteroids = 1,
            Asteroids_Earth = 2,
            Asteroids_Mars = 3
        }

        internal const int SceneMain = 0;

        [SerializeField] RectTransform fader;

        public static bool m_returnToMain;
        public bool m_stageLoaded;
        public bool m_stageUnloaded;

        void Start()
        {
            fader.gameObject.SetActive(true);

            LeanTween.alpha(fader, 1, 0);
            LeanTween.alpha(fader, 0, 0.5f).setOnComplete(() =>
            {
                fader.gameObject.SetActive(false);
            });
        }

        public void OpenMain()
        {
            m_returnToMain = true;

            fader.gameObject.SetActive(true);

            LeanTween.alpha(fader, 0, 0);
            LeanTween.alpha(fader, 1, 0.5f).setOnComplete(() =>
            {
                SceneManager.LoadScene(SceneMain);
            });
        }

        public void OpenAsteroids()
        {
            fader.gameObject.SetActive(true);

            LeanTween.alpha(fader, 0, 0);
            LeanTween.alpha(fader, 1, 0.5f).setOnComplete(() =>
            {
                SceneManager.LoadScene((int)SceneName.Asteroids);
            });
        }

        public void LoadSceneAsync(SceneName scene)
        {
            print("Scene: " + scene);

            m_stageLoaded = false;
            StartCoroutine(LoadAsync((int)scene));
        }

        public void UnloadSceneAsync(SceneName scene)
        {
            print("Unload scene: " + scene);
            m_stageUnloaded = false;
            StartCoroutine(UnloadAsync((int)scene));
        }

        /// <summary>
        /// Set the active scene: the Scene which will be used as the target for new GameObjects instantiated by scripts and from what Scene the lighting settings are used
        /// </summary>
        IEnumerator LoadAsync(int sceneIndex)
        {
            var operation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);

            while (!operation.isDone)
                yield return null;
            
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneIndex));

            m_stageLoaded = true;
        }

        IEnumerator UnloadAsync(int scene)
        {
            yield return null;
            SceneManager.UnloadSceneAsync(scene).completed += (Scene) =>
                {
                    //Resources.UnloadUnusedAssets();
                    m_stageUnloaded = true;
                };
        }
    }
}