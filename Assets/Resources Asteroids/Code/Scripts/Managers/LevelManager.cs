using System.Collections;
using System.Linq;
using UnityEngine;
using static SceneLoader;

namespace Game.Astroids
{
    [ExecuteInEditMode]
    public class LevelManager : MonoBehaviour
    {
        [Header("Elements")]
        [SerializeField] GameObject gameIntro;
        [SerializeField] GameObject stageResults;
        [SerializeField] GameObject stageContinue;
        [SerializeField] Transform stageCompleteStart;
        [SerializeField] Transform stageCompleteEnd;

        [SerializeField] Stage[] stages;

        Vector3 _gizmoPosition;
        [HideInInspector] public int _gizmoStageIndex;

        internal bool m_stageLoaded;
        int _currentStageIndex;
        readonly float _moveToCam = 8;

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

        void Start()
        {
            _currentStageIndex = 0;

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
        public SceneName GetFirstStage() => GetStage(0).SceneName;
        public SceneName GetNextStage() => GetStage(_currentStageIndex + 1).SceneName;

        public Vector3[] GetStageCompletePath() => GetStagePath(_currentStageIndex);

        Stage GetStage(int index)
        {
            _currentStageIndex = index;
            return stages[index];
        }

        Vector3[] GetStagePath(int index)
        {
            var stage = GetStage(index);

            return new Vector3[]
            {
                stageCompleteStart.position,
                stage.pathStartPoint.position,
                stage.pathEndPoint.position,
                stageCompleteEnd.position
            };
        }

        void HideGroup(GameObject group)
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

            while (!m_stageLoaded)
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
            if (_gizmoStageIndex >= stages.Length)
                return;

            var points = new Vector3[4];
            points[0] = stageCompleteStart.position;
            points[3] = stageCompleteEnd.position;

            var trans = stages[_gizmoStageIndex].pathStartPoint;
            if (trans != null)
                points[1] = trans.position;

            trans = stages[_gizmoStageIndex].pathEndPoint;
            if (trans != null)
                points[2] = trans.position;

            foreach (var p in points)
                if (p == null)
                    return;

            for (float t = 0; t <= 1; t += 0.05f)
            {
                _gizmoPosition = Mathf.Pow(1 - t, 3) * points[0] + 3 * Mathf.Pow(1 - t, 2) * t * points[1] + 3 * (1 - t) * Mathf.Pow(t, 2) * points[2] + Mathf.Pow(t, 3) * points[3];

                Gizmos.DrawSphere(_gizmoPosition, 0.05f);
            }

            Gizmos.DrawLine(new Vector3(points[0].x, points[0].y, points[0].z), new Vector3(points[1].x, points[1].y, points[1].z));
            Gizmos.DrawLine(new Vector3(points[2].x, points[2].y, points[2].z), new Vector3(points[3].x, points[3].y, points[3].z));
        }
    }

    [System.Serializable]
    public class Stage
    {
        public SceneName SceneName;
        public Transform pathStartPoint;
        public Transform pathEndPoint;
    }
}