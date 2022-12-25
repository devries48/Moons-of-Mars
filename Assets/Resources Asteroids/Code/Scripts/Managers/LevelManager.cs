using System;
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
        public bool m_StageLoaded;

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
            StartCoroutine(WaitForStageToLoad());
        }

        public void HideGameIntro() => HideGroup(gameIntro);

        public void HideStageResuls() => HideGroup(stageResults);

        void HideGroup(GameObject group)
        {
            LeanTween.value(group, 1f, 0f, .5f)
            .setOnUpdate((value) =>
            {
                group.transform.localScale = new Vector3(value, value, 1);
            })
            .setOnComplete(() => group.SetActive(false));
        }

        IEnumerator WaitForStageToLoad()
        {
            while (!m_StageLoaded)
                yield return null;

            yield return new WaitForSeconds(10);
            stageContinue.SetActive(true);
            yield return Utils.WaitUntilTrue(IsAnyKeyPressed);
            HideGroup(stageResults);
            GameManager.NewStageStart(2f);
        }

        bool IsAnyKeyPressed()
        {
            return Input.anyKey;
        }

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