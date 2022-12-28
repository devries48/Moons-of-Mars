using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    [ExecuteInEditMode]
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] GameObject gameIntro;
        [SerializeField] GameObject stageResults;
        [SerializeField] GameObject stageContinue;

        public Transform[] controlPoints;
        public Vector3[] m_EarthPath;

        internal bool m_StageLoaded;

        float _moveToCam = 8;

        AsteroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AsteroidsGameManager.Instance;

                if (__gameManager == null)
                    Debug.LogWarning("GameManager is null");

                return __gameManager;
            }
        }
        AsteroidsGameManager __gameManager;

        Vector3 _gizmosPosition;

        void Start()
        {
            gameIntro.SetActive(true);
            stageResults.SetActive(false);
            stageContinue.SetActive(false);
        }

        public void ShowStageResults()
        {
            stageResults.SetActive(true);
            stageContinue.SetActive(false);
            StartCoroutine(WaitForStageToLoad());
        }

        public void HideGameIntro() => HideGroup(gameIntro);

        public void HideStageResuls() => HideGroup(stageResults);

        public void HideGroup(GameObject group)
        {
            var p = group.transform.position;
            var to = new Vector3(p.x, p.y, p.z + _moveToCam);
            LeanTween.move(group, to, .5f)
                .setOnComplete(() =>
                {
                    group.SetActive(false);
                    group.transform.position = p;
                });
        }


        IEnumerator WaitForStageToLoad()
        {
            yield return new WaitForSeconds(1); // wait for music change (checks every .5 seconds). It interrupted the mixer group fade.
            GameManager.m_AudioManager.FadeOutBackgroundSfx();

            while (!m_StageLoaded)
                yield return null;

            yield return new WaitForSeconds(8);

            stageContinue.SetActive(true);
            yield return Utils.WaitUntilTrue(IsAnyKeyPressed);

            HideGroup(stageResults);
            yield return new WaitForSeconds(.5f);

            GameManager.StageStartNew();
            GameManager.m_AudioManager.FadeInBackgroundSfx();
        }

        bool IsAnyKeyPressed() => Input.anyKey;

        void OnDrawGizmos()
        {
            if (controlPoints.Length != 4)
                return;

            for (float t = 0; t <= 1; t += 0.05f)
            {
                _gizmosPosition = Mathf.Pow(1 - t, 3) * controlPoints[0].position + 3 * Mathf.Pow(1 - t, 2) * t * controlPoints[1].position + 3 * (1 - t) * Mathf.Pow(t, 2) * controlPoints[2].position + Mathf.Pow(t, 3) * controlPoints[3].position;

                Gizmos.DrawSphere(_gizmosPosition, 0.05f);
            }

            Gizmos.DrawLine(new Vector3(controlPoints[0].position.x, controlPoints[0].position.y, controlPoints[0].position.z), new Vector3(controlPoints[1].position.x, controlPoints[1].position.y, controlPoints[1].position.z));
            Gizmos.DrawLine(new Vector3(controlPoints[2].position.x, controlPoints[2].position.y, controlPoints[2].position.z), new Vector3(controlPoints[3].position.x, controlPoints[3].position.y, controlPoints[3].position.z));

        }

    }
}