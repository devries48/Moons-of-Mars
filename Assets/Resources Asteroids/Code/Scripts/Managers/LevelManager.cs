using System.Collections;
using UnityEngine;

using static Game.Astroids.AsteroidsGameManager;
using static Game.Astroids.Level;
using static Game.Astroids.UfoManagerData;
using static SceneLoader;

namespace Game.Astroids
{
    [ExecuteInEditMode]
    public class LevelManager : MonoBehaviour
    {
        #region editor fields
        [SerializeField] SceneLoader sceneLoader;

        [Header("Elements")]
        [SerializeField] GameObject gameIntro;
        [SerializeField] GameObject stageResults;
        [SerializeField] GameObject stageContinue;
        [SerializeField] Transform stageCompleteStart;
        [SerializeField] Transform stageCompleteEnd;

        [SerializeField] Level[] levels;
        [SerializeField] Stage[] stages;

        Vector3 _gizmoPosition;
        [HideInInspector] public int _gizmoStageIndex;
        #endregion

        #region fields
        CurrentLevel _currentLevel;
        int _currentStageIndex;
        readonly float _moveToCam = 8;
        #endregion

        #region properties
        AsteroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = Instance;

                if (__gameManager == null)
                    Debug.LogWarning("GameManager is null");

                return __gameManager;
            }
        }
        AsteroidsGameManager __gameManager;

        public int AstroidsActive
        {
            get
            {
                if (_currentLevel == null)
                    return 0;

                return _currentLevel.AsteroidsActive;
            }
        }

        public int UfosActive
        {
            get
            {
                if (_currentLevel == null)
                    return 0;

                return _currentLevel.TotalUfosActive;
            }
        }

        public bool CanAddUfo => _currentLevel.CanAddUfo;

        public bool IsStageLoaded => sceneLoader.m_stageLoaded;

        #endregion

        void Start()
        {
            _currentStageIndex = 0;
            _currentLevel = new CurrentLevel(levels, stages);

            gameIntro.SetActive(true);
            stageResults.SetActive(false);
            stageContinue.SetActive(false);
        }

        public void ShowGameIntro() => StartCoroutine(ShowGameIntroCore());

        public IEnumerator LevelStartLoop()
        {
            GameManager.UiManager.LevelStarts(_currentLevel.Level);

            if (_currentLevel.Level == 1)
            {
                while (GameManager.UiManager.AudioPlaying)
                    yield return null;

                GameManager.m_playerShip.Spawn();
                yield return Wait(2);

                GameManager.m_HudManager.HudShow();
                GameManager.m_playerShip.EnableControls();
            }
            yield return Wait(1.5f);

            GameManager.m_playerShip.Refuel();
            GameManager.m_GameManagerData.SpawnAsteroids(_currentLevel.AsteroidsForLevel);
        }

        public IEnumerator LevelPlayLoop()
        {
            GameManager.UiManager.LevelPlay();

            while (GameManager.m_playerShip.m_isAlive && _currentLevel.HasEnemy || GameManager.m_debug.NoAstroids)
                yield return null;
        }

        public IEnumerator LevelEndLoop()
        {
            bool gameover = !GameManager.m_playerShip.m_isAlive;

            if (gameover)
                GameManager.GameOver();
            else
            {
                if (_currentLevel.IsStageComplete())
                {
                    StartCoroutine(GameManager.UiManager.StageCleared(_currentLevel.Stage));
                    yield return Wait(2f);
                    GameManager.UiManager.ClearAnnouncements();
                    GameManager.m_GameManagerData.StageCompleteAnimation();

                    while (!GameManager.IsGamePlaying)
                        yield return null;
                }
                else
                {
                    StartCoroutine(GameManager.UiManager.LevelCleared(_currentLevel.Level));
                    yield return Wait(2f);
                }

                _currentLevel.LevelAdvance();
            }
            yield return Wait(1);
        }

        public void StartLevel1() => _currentLevel.Level1();

        public IEnumerator LoadStage(SceneName scene)
        {
            sceneLoader.LoadSceneAsync(scene);
            while (!IsStageLoaded)
                yield return null;
        }

        public void AddAstroid() => _currentLevel.AstroidAdd();
        public void RemoveAstroid() => _currentLevel.AstroidRemove();
        public void AddUfo(UfoType m_ufoType) => _currentLevel.UfoAdd(m_ufoType);
        public void RemoveUfo(UfoType type) => _currentLevel.UfoRemove(type);

        public void ShowStageResults()
        {
            stageResults.SetActive(true);
            stageContinue.SetActive(false);
        }

        public void HideStageResuls() => HideGroup(stageResults);
        public SceneName GetFirstStage() => GetStage(0).SceneName;
        public SceneName GetNextStage() => GetStage(_currentStageIndex + 1).SceneName;
        public SceneName GetCurrentStage() => GetStage(_currentStageIndex).SceneName;
        public Vector3[] GetStageCompletePath() => GetStagePath(_currentStageIndex);
        public void LoadNewStage() => StartCoroutine(WaitForStageToLoad());

        void HideGameIntro() => HideGroup(gameIntro);

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
                stage.PathStartPoint.position,
                stage.PathEndPoint.position,
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
            sceneLoader.UnloadSceneAsync(GetCurrentStage());
            yield return new WaitForSeconds(1); // wait for music change (checks every .5 seconds). It interrupted the mixer group fade.

            GameManager.m_AudioManager.FadeOutBackgroundSfx();

            StartCoroutine(LoadStage(GetNextStage()));
            while (!IsStageLoaded)
                yield return null;

            stageContinue.SetActive(true);
            yield return Utils.WaitUntilTrue(IsAnyKeyPressed);

            HideGroup(stageResults);
            yield return new WaitForSeconds(.5f);

            GameManager.StageStartNew();
            GameManager.m_AudioManager.FadeInBackgroundSfx();
        }

        bool IsAnyKeyPressed() => Input.anyKey;

        IEnumerator ShowGameIntroCore()
        {
            // Load first stage
            var t = 0f;
            StartCoroutine(LoadStage(GetFirstStage()));
            while (!IsStageLoaded)
                yield return null;

            t += Time.deltaTime;

            yield return Wait(5 - t);
            HideGameIntro();
            yield return Wait(.5f);
            GameManager.SwitchStageCam(StageCamera.far);
            yield return Wait(.1f);
            GameManager.SwitchStageCam(StageCamera.background);
            yield return Wait(2f);

            GameManager.GameStart();

        }

        void OnDrawGizmos()
        {
            if (_gizmoStageIndex >= stages.Length)
                return;

            var points = new Vector3[4];
            points[0] = stageCompleteStart.position;
            points[3] = stageCompleteEnd.position;

            var trans = stages[_gizmoStageIndex].PathStartPoint;
            if (trans != null)
                points[1] = trans.position;

            trans = stages[_gizmoStageIndex].PathEndPoint;
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
    public class Level
    {
        public enum LevelAction { asteroidAdd, asteroidRemove, powerUp, greenUfo, redUfo }
        public LevelAction[] actions;
    }

    [System.Serializable]
    public class Stage
    {
        public SceneName SceneName;
        public Transform PathStartPoint;
        public Transform PathEndPoint;
        public LevelAction[] actions;
    }

    #region CurrentLevel

    public class CurrentLevel
    {
        public CurrentLevel(Level[] levels, Stage[] stages)
        {
            _levels = levels;
            _stages = stages;
        }

        readonly Level[] _levels;
        readonly Stage[] _stages;

        int _level;
        int _stage;
        int _stageLevel;
        int _asteroidsForLevel;
        int _ufosForLevel;

        int _asteroidsActive;
        int _ufosGreenActive;
        int _ufosRedActive;

        public int Level => _level;
        public int Stage => _stage;
        public int AsteroidsForLevel => _asteroidsForLevel;
        public int AsteroidsActive => _asteroidsActive;
        public int TotalUfosActive => _ufosGreenActive + _ufosRedActive;
        int UfosGreenActive => _ufosGreenActive;
        int UfosRedActive => _ufosRedActive;
        public bool HasEnemy => _asteroidsActive > 0 || TotalUfosActive > 0;
        public bool CanAddUfo => TotalUfosActive < _ufosForLevel && _asteroidsActive > 0;

        public void AstroidAdd() => _asteroidsActive++;
        public void AstroidRemove() => _asteroidsActive--;
        public void UfoAdd(UfoType type)
        {
            if (type == UfoType.green)
                _ufosGreenActive++;
            else
                _ufosRedActive++;
        }

        public void UfoRemove(UfoType type)
        {
            if (type == UfoType.green)
                _ufosGreenActive--;
            else
                _ufosRedActive--;
        }

        public void Level1()
        {
            SetLevel(1);
        }

        public void LevelAdvance()
        {
            _stageLevel++;
            SetLevel(_level + 1);
        }

        public bool IsStageComplete() => _stageLevel == _levels.Length;

        void SetLevel(int level)
        {
            _level = level;

            if (_level == 1)
            {
                _stage = 1;
                _stageLevel = 1;
            }

            if (_stageLevel > _levels.Length)
            {
                _stageLevel = 1;
                _stage++;
                SetAstroidsForLevel(true);
                SetUfosForLevel(true);
            }

            SetAstroidsForLevel();
            SetUfosForLevel();

            _asteroidsActive = 0;
            _ufosGreenActive = 0;
            _ufosRedActive = 0;
        }

        void SetAstroidsForLevel(bool isStage = false)
        {
            Debug.Log("Stage: " + _stage);
            Debug.Log("Level: " + _level);

            _asteroidsForLevel += CountAction(LevelAction.asteroidAdd, isStage);
            _asteroidsForLevel -= CountAction(LevelAction.asteroidRemove, isStage);
            Debug.Log("Asteroids: " + _asteroidsForLevel);
        }

        void SetUfosForLevel(bool isStage = false)
        {
            _ufosForLevel += CountAction(LevelAction.greenUfo, isStage);
            _ufosForLevel += CountAction(LevelAction.redUfo, isStage);
        }

        int CountAction(LevelAction action, bool isStage = false)
        {
            if (Level == 0)
                return 0;

            var actions = isStage ? _stages?[_stage - 1].actions : _levels?[_stageLevel - 1].actions;
            int c = 0;

            for (int i = 0; i < actions.Length; i++)
                if (actions[i] == action)
                    c++;

            return c;
        }
    }
    #endregion

}