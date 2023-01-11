using Announcers;
using System;
using System.Collections;
using System.Collections.Generic;
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

        [Header("Bonus")]
        [Range(0, 200)] public int timeBonus = 100;
        [Range(0, 200)] public int efficiencyBonus = 100;
        [Range(0, 200)] public int destructionBonus = 100;
        [Range(0, 200)] public int pickupBonus = 100;

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

        GameAnnouncer Announce
        {
            get
            {
                if (__announce == null)
                    __announce = GameAnnouncer.AnnounceTo(TextAnnouncerBase.TextComponent(GameManager.m_AnnouncerTextUI));

                return __announce;
            }
        }
        GameAnnouncer __announce;

        bool UIAudioPlaying => GameManager.UiManager.AudioPlaying;

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

        public bool CanActivate(LevelAction action)
        {
            return action switch
            {
                LevelAction.powerUp => _currentLevel.CanAddPowerup,
                LevelAction.greenUfo => _currentLevel.CanAddGreenUfo,
                LevelAction.redUfo => _currentLevel.CanAddRedUfo,
                _ => true
            };
        }

        public bool IsStageLoaded => sceneLoader.m_stageLoaded;

        #endregion

        public enum Statistic { powerupSpawn, powerupDestroyed, powerupPickup, shotFired, shotHit }

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
            AnnounceLevelStart(_currentLevel.Level);

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
            while (GameManager.m_playerShip.m_isAlive && _currentLevel.HasEnemy || GameManager.m_debug.NoAstroids)
            {
                _currentLevel.StagePlaytime += Time.deltaTime;
                yield return null;
            }
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
                    StartCoroutine(AnnounceStageCleared(_currentLevel.Stage));
                    yield return Wait(2f);

                    GameManager.m_GameManagerData.StageCompleteAnimation();
                    while (!GameManager.IsGamePlaying)
                        yield return null;
                }
                else
                {
                    StartCoroutine(AnnounceLevelCleared(_currentLevel.Level));
                    yield return Wait(2f);
                }

                _currentLevel.LevelAdvance();
            }
            yield return Wait(1);
        }


        void AnnounceLevelStart(int level)
        {
            if (level == 1)
            {
                Announce.GameStart();
                UIAudio(UISounds.Clip.gameStart);
            }
            else
                Announce.LevelStarts(level);

            StartCoroutine(AnnounceClear());
        }

        IEnumerator AnnounceLevelCleared(int level)
        {
            Announce.LevelCleared();
            StartCoroutine(AnnounceClear());

            Score.LevelCleared(level);

            UIAudio(UISounds.Clip.levelComplete);
            while (UIAudioPlaying)
                yield return null;

        }

        IEnumerator AnnounceStageCleared(int stage)
        {
            Announce.StageCleared();
            StartCoroutine(AnnounceClear());

            UIAudio(UISounds.Clip.stageComplete);
            while (UIAudioPlaying)
                yield return null;
        }

        IEnumerator AnnounceClear()
        {
            yield return Wait(1);
            Announce.ClearAnnouncements();
        }

        public IEnumerator AnnounceGameOver()
        {
            Announce.GameOver();
            yield return Wait(2);

            UIAudio(UISounds.Clip.gameOver);
            while (UIAudioPlaying)
                yield return null;

            Announce.ClearAnnouncements();
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
        public void RemoveUfo(UfoType type, bool destroyed) => _currentLevel.UfoRemove(type, destroyed);
        public void AddStatistic(Statistic stat) => _currentLevel.AddStat(stat);

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
        public StageStatistics GetStageResults() => _currentLevel.GetStageResults();

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

        void UIAudio(UISounds.Clip clip) => GameManager.UiManager.PlayAudio(clip);

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
        public string Name;
        public int SecondsToComplete;
        public SceneName SceneName;
        public Transform PathStartPoint;
        public Transform PathEndPoint;
        public LevelAction[] actions;
    }

    public class StageStatistics
    {
        public StageStatistics(int nr, Stage stage)
        {
            StageNr = nr;
            _stage = stage;
            _lvlManager = Instance.m_LevelManager;
        }

        public readonly int StageNr;
        public string Name => _stage.Name;

        public int TotalBonus { get; internal set; }

        public float ShotsFired;
        public float ShotsHit;
        public int UfosSpawned;
        public int UfosDestroyed;
        public int PowerupsSpawned;
        public int PowerupsPickedUp;
        public int PowerupsDestroyed;
        public int AstroidsDestroyed;

        public float Playtime;

        public int EfficiencyBonus;
        public int TimeBonus;
        public int DestrucionBonus;
        public int PickupBonus;

        readonly Stage _stage;
        readonly LevelManager _lvlManager;


        public void CalculateBonus()
        {
            if (ShotsFired > 0)
                EfficiencyBonus = (int)Mathf.Round(ShotsHit / ShotsFired * _lvlManager.efficiencyBonus);

            TimeBonus = CalcTimeBonus();

            if (UfosSpawned> 0)
                DestrucionBonus = (int)Mathf.Round(UfosDestroyed / UfosSpawned * _lvlManager.destructionBonus);
            
            if (PowerupsPickedUp> 0)
                PickupBonus = (int)Mathf.Round((PowerupsSpawned + PowerupsDestroyed) / PowerupsPickedUp * _lvlManager.pickupBonus);
            
            TotalBonus = EfficiencyBonus + TimeBonus + DestrucionBonus + PickupBonus;
        }

        int CalcTimeBonus()
        {
            if (Playtime <= _stage.SecondsToComplete)
                return _lvlManager.timeBonus;
            else
            {
                var t = Playtime - (2 * _stage.SecondsToComplete);
                if (t < 0)
                    return (int)(Mathf.Abs(t) * .01f * _lvlManager.timeBonus);

                return 0;
            }
        }
    }

    #region CurrentLevel

    public class CurrentLevel
    {
        public CurrentLevel(Level[] levels, Stage[] stages)
        {
            _levels = levels;
            _stages = stages;
            _stageStats = new List<StageStatistics>();
        }

        readonly Level[] _levels;
        readonly Stage[] _stages;
        readonly List<StageStatistics> _stageStats;

        int _level;
        int _stage;
        int _stageLevel;
        int _levelAsteriods;
        int _levelUfos;
        bool _levelUfoGreen;
        bool _levelUfoRed;
        bool _levelPowerup;

        int _asteroidsActive;
        int _ufosGreenActive;
        int _ufosRedActive;

        StageStatistics _stats;

        public int Level => _level;
        public int Stage => _stage;
        public int AsteroidsForLevel => _levelAsteriods;
        public int AsteroidsActive => _asteroidsActive;
        public int TotalUfosActive => _ufosGreenActive + _ufosRedActive;
        int UfosGreenActive => _ufosGreenActive;
        int UfosRedActive => _ufosRedActive;
        public bool HasEnemy => _asteroidsActive > 0 || TotalUfosActive > 0;
        public bool CanAddUfo => TotalUfosActive < _levelUfos && _asteroidsActive > 0;
        public bool CanAddGreenUfo => _levelUfoGreen;
        public bool CanAddRedUfo => _levelUfoRed;
        public bool CanAddPowerup => _levelPowerup;

        public float StagePlaytime
        {
            get { return _stats.Playtime; }
            set { _stats.Playtime = value; }
        }

        public void AstroidAdd() => _asteroidsActive++;
        public void AstroidRemove()
        {
            _stats.AstroidsDestroyed++;
            _asteroidsActive--;
        }

        public void UfoAdd(UfoType type)
        {
            if (type == UfoType.green)
                _ufosGreenActive++;
            else
                _ufosRedActive++;

            _stats.UfosSpawned++;
        }

        public void UfoRemove(UfoType type, bool destroyed)
        {
            if (type == UfoType.green)
                _ufosGreenActive--;
            else
                _ufosRedActive--;

            if (destroyed)
                _stats.UfosDestroyed++;
        }

        public void Level1() => SetLevel(1);

        public void LevelAdvance()
        {
            _stageLevel++;
            SetLevel(_level + 1);
        }

        public bool IsStageComplete() => _stageLevel == _levels.Length;

        public StageStatistics GetStageResults() => _stats;

        void SetLevel(int level)
        {
            bool loadStageActions = false;

            _level = level;

            if (_level == 1)
            {
                _stage = 1;
                _levelAsteriods = 0;
                _levelUfos = 0;
                _levelUfoGreen = false;
                _levelUfoRed = false;
                _levelPowerup = false;

                loadStageActions = true;
            }
            else if (_stageLevel > _levels.Length)
            {
                _stage++;
                loadStageActions = true;
            }

            if (loadStageActions)
            {
                _stageLevel = 1;
                SetActionsForLevel(true);
            }

            SetActionsForLevel();

            _stats = new StageStatistics(_stage, _stages[_stage - 1]);
            _stageStats.Add(_stats);

            _asteroidsActive = 0;
            _ufosGreenActive = 0;
            _ufosRedActive = 0;
        }

        void SetActionsForLevel(bool isStage = false)
        {
            _levelAsteriods += CountAction(LevelAction.asteroidAdd, isStage);
            _levelAsteriods -= CountAction(LevelAction.asteroidRemove, isStage);

            var greenUfo = CountAction(LevelAction.greenUfo, isStage);
            var redUfo = CountAction(LevelAction.redUfo, isStage);
            var powerup = CountAction(LevelAction.powerUp, isStage);

            if (greenUfo > 0) _levelUfoGreen = true;
            if (redUfo > 0) _levelUfoRed = true;
            if (powerup > 0) _levelPowerup = true;

            _levelUfos += greenUfo + redUfo;

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

        public void AddStat(LevelManager.Statistic stat)
        {
            switch (stat)
            {
                case LevelManager.Statistic.powerupSpawn:
                    _stats.PowerupsSpawned++;
                    break;
                case LevelManager.Statistic.powerupDestroyed:
                    _stats.PowerupsDestroyed++;
                    break;
                case LevelManager.Statistic.powerupPickup:
                    _stats.PowerupsPickedUp++;
                    break;
                case LevelManager.Statistic.shotFired:
                    _stats.ShotsFired++;
                    break;
                case LevelManager.Statistic.shotHit:
                    _stats.ShotsHit++;
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

}