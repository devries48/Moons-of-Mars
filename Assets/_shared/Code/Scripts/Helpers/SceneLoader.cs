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
            Asteroids_Earth = 2,
            Asteroids_Mars = 3
        }

        internal const int SceneMain = 0;
        internal const int SceneAsteroids = 1;

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
                SceneManager.LoadScene(SceneAsteroids);
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
            m_stageUnloaded = false;
            StartCoroutine(UnloadAsync((int)scene));
        }

        IEnumerator LoadAsync(int scene)
        {
            var operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

            while (!operation.isDone)
            {
                Debug.Log("Scene load: " + operation.progress);
                yield return null;
            }
            m_stageLoaded = true;
        }

        IEnumerator UnloadAsync(int scene)
        {
            yield return null;
            SceneManager.UnloadSceneAsync(scene).completed += (Scene) =>
                {
                    Resources.UnloadUnusedAssets();
                    m_stageUnloaded = true;
                };
        }
    }
}