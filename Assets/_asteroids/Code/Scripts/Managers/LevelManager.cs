using MoonsOfMars.Shared;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

using static MoonsOfMars.Shared.SceneLoader;

namespace MoonsOfMars.Game.Asteroids
{
    using static GameManager;
    using static Level;
    using static UfoManagerData;
    using static GameResultsController;

    [ExecuteInEditMode]
    public class LevelManager : MonoBehaviour
    {
        public enum Statistic { powerupSpawn, powerupDestroyed, powerupPickup, shotFired, shotHit }

        #region editor fields
        [SerializeField] SceneLoader sceneLoader;

        [Header("Elements")]
        [SerializeField] GameObject gameIntro;
        [SerializeField] GameResultsController gameResults;
        [SerializeField] Transform stageCompleteStart;
        [SerializeField] Transform stageCompleteEnd;

        [Header("Bonus")]
        [Range(0, 200)] public int m_TimeBonus = 100;
        [Range(0, 200)] public int m_EfficiencyBonus = 100;
        [Range(0, 200)] public int m_DestructionBonus = 100;
        [Range(0, 200)] public int m_PickupBonus = 100;

        [Header("Levels & stages")]
        [SerializeField] Level[] levels;
        [SerializeField] Stage[] stages;
        #endregion

        #region properties

        bool UIAudioPlaying => GmManager.UiManager.AudioPlaying;

        public int AsteroidsActive
        {
            get
            {
                if (_currentLevel == null)
                    return 0;

                return _currentLevel.AsteroidsActive;
            }
        }
        public bool HasActiveShuttle => GmManager.PowerupManager.ActiveShuttleCount > 0;
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
        public int CurrentLevel => _currentLevel.Level;
        #endregion

        #region fields
        CurrentLevel _currentLevel;
        int _currentStageIndex;
        readonly float _moveToCam = 8;
        bool _isFirstStageLoaded;
        bool _isInitialized;
        Vector3 _gizmoPosition;
        [HideInInspector] public int _gizmoStageIndex;

        int _gizmoCurrentIndex = -1;
        #endregion

        // Check if application is playing and not in editmode
        void OnEnable()
        {
            if (Application.isPlaying)
                StartCoroutine(Initialize());
        }

        IEnumerator Initialize()
        {
            _isInitialized = false;
            _currentStageIndex = 0;
            _isFirstStageLoaded = false;
            _currentLevel = new CurrentLevel(levels, stages);

            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                var scene = SceneManager.GetSceneAt(s);
                //print("scene " + s + " - " + scene.name);
                while (!scene.isLoaded)
                    yield return null;

                for (int i = 0; i < stages.Length; i++)
                {
                    if (scene.name.ToLower().Contains(stages[i].Name.ToLower()))
                    {
                        SceneManager.SetActiveScene(scene);

                        print("Set active scene: " + scene.name);

                        _isFirstStageLoaded = true;
                        _currentStageIndex = i;

                        break;
                    }
                    yield return null;
                }

                if (_isFirstStageLoaded)
                    break;
            }

            gameIntro.SetActive(true);
            _isInitialized = true;

            //gameResults.gameObject.SetActive(false);
            //gameContinue.SetActive(false);
        }

        public void ShowGameIntro() => StartCoroutine(ShowGameIntroCore());

        public IEnumerator LevelStartLoop()
        {
            AnnounceLevelStart(_currentLevel.Level);

            if (_currentLevel.Level == 1)
            {
                while (GmManager.UiManager.AudioPlaying)
                    yield return null;

                GmManager.m_playerShip.Spawn();
                yield return Wait(2);

                GmManager.m_HudManager.HudShow();
                GmManager.m_playerShip.EnableControls();
            }
            yield return Wait(1.5f);

            GmManager.m_playerShip.Refuel();
            GmManager.m_GameManagerData.SpawnAsteroids(_currentLevel.AsteroidsForLevel);
        }

        public IEnumerator LevelPlayLoop()
        {
            while (GmManager.m_playerShip.m_isAlive && _currentLevel.HasEnemy || GmManager.m_debug.NoAsteroids)
            {
                _currentLevel.AddPlayTime(Time.deltaTime);
                yield return null;
            }
        }

        public IEnumerator LevelEndLoop()
        {
            if (GmManager.m_gameAborted)
                yield break;

            bool gameover = !GmManager.m_playerShip.m_isAlive;

            if (gameover)
                GmManager.GameOver();
            else
            {
                if (_currentLevel.IsStageComplete())
                {
                    GmManager.m_HudManager.CancelPowerups();
                    StartCoroutine(AnnounceStageCompleted());
                    yield return Wait(2f);

                    GmManager.m_GameManagerData.StageCompleteAnimation();
                    while (!GmManager.IsGamePlaying)
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
                GmManager.Announce(GameAnnouncer.Ready);
                UIAudio(UISounds.Clip.gameStart);
            }
            else
                GmManager.Announce(GameAnnouncer.LevelStart, level);
        }

        IEnumerator AnnounceLevelCleared(int level)
        {
            GmManager.Announce(GameAnnouncer.LevelCleared);
            Score.LevelCleared(level);

            UIAudio(UISounds.Clip.levelComplete);
            while (UIAudioPlaying)
                yield return null;
        }

        IEnumerator AnnounceStageCompleted()
        {
            GmManager.Announce(GameAnnouncer.StageCompleted);

            UIAudio(UISounds.Clip.stageComplete);
            while (UIAudioPlaying)
                yield return null;
        }

        public IEnumerator AnnounceGameOver()
        {
            GmManager.Announce(GameAnnouncer.Gameover);
            yield return Wait(2);

            UIAudio(UISounds.Clip.gameOver);
            while (UIAudioPlaying)
                yield return null;
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
            gameResults.DisplayResults(GameResult.stageCleared);
        }

        public void ShowGameResults(bool gameover)
        {
            StartCoroutine(ShowGameResultsCore(gameover ? GameResult.gameOver : GameResult.gameComplete));
        }

        //public void HideStageResuls() => HideGroup(gameResults.gameObject);
        public SceneName GetCurrentStage() => GetStage(_currentStageIndex).SceneName;
        public Vector3[] GetStageCompletePath() => GetStagePath(_currentStageIndex);
        public void LoadNewStage() => StartCoroutine(WaitForStageToLoad());
        public StageStatistics GetStageStatistics() => _currentLevel.GetStageStatistics();
        public GameStatistics GetGameStatistics() => _currentLevel.GetGameStatistics();

        SceneName GetFirstStage() => GetStage(0).SceneName;
        SceneName GetNextStage() => GetStage(_currentStageIndex + 1).SceneName;

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
                stage.PathPoint0.position,
                stage.PathPoint1.position,
                stage.PathPoint2.position,
                stageCompleteEnd.position
            };
        }

        IEnumerator WaitForStageToLoad()
        {
            var r = _currentLevel.GetStageStatistics();
            Score.Earn(r.TotalBonus, new Vector3(7, -2, 0));

            sceneLoader.UnloadSceneAsync(GetCurrentStage());
            yield return new WaitForSeconds(1); // wait for music change (checks every .5 seconds). It interrupted the mixer group fade.

            GmManager.AudioManager.FadeOutBackgroundSfx();

            StartCoroutine(LoadStage(GetNextStage()));
            while (!IsStageLoaded)
                yield return null;

            gameResults.ShowContinueButton();

            //yield return Utils.WaitUntilTrue(IsAnyKeyPressed);
            yield return Utils.WaitUntilTrue(GmManager.InputManager.IsAnyKeyPressed);

            gameResults.HideResults(GameResult.stageCleared);
            yield return new WaitForSeconds(.5f);

            GmManager.StageStartNew();
            GmManager.AudioManager.FadeInBackgroundSfx();
        }

        // bool IsAnyKeyPressed() => Input.anyKey;

        IEnumerator ShowGameIntroCore()
        {
            // wait until levelmanager is initialized
            while (!_isInitialized)
                yield return null;

            // Load first stage
            var t = 0f;
            if (!_isFirstStageLoaded)
            {
                StartCoroutine(LoadStage(GetFirstStage()));
                while (!IsStageLoaded)
                    yield return null;
            }

            t += Time.deltaTime;
            yield return Wait(5 - t);
            Utils.MoveToCamAndHide(gameIntro, _moveToCam);
            yield return Wait(.5f);
            GmManager.SwitchStageCam(StageCamera.far);
            yield return Wait(.1f);
            GmManager.SwitchStageCam(StageCamera.background);
            yield return Wait(2f);

            GmManager.GameStart();
        }

        // Wait for the explosion to fade and show the game results
        IEnumerator ShowGameResultsCore(GameResult result)
        {
            yield return new WaitForSeconds(2.5f);
            gameResults.DisplayResults(result);

            yield return Utils.WaitUntilTrue(GmManager.InputManager.IsAnyKeyPressed);

            gameResults.HideResults(result);
            GmManager.GameQuit();
        }

        void UIAudio(UISounds.Clip clip) => GmManager.UiManager.PlayAudio(clip);

        void OnDrawGizmos()
        {
            if (_gizmoStageIndex >= stages.Length)
                return;

            var points = new Vector3[4];

            var trans = stages[_gizmoStageIndex].PathPoint0;
            if (trans != null)
                points[0] = trans.position;

            trans = stages[_gizmoStageIndex].PathPoint1;
            if (trans != null)
                points[1] = trans.position;

            trans = stages[_gizmoStageIndex].PathPoint2;
            if (trans != null)
                points[2] = trans.position;

            points[3] = stageCompleteEnd.position;

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

            if (_gizmoStageIndex != _gizmoCurrentIndex)
            {
                print($"Gizmo path: {stages[_gizmoStageIndex].Name}");
                _gizmoCurrentIndex = _gizmoStageIndex;
            }
        }

        public int GetGizmoStageIndex(string stage)
        {
            for (int i = 0; i < stages.Length; i++)
            {
                if (stages[i].Name.ToLower().Contains(stage.ToLower()))
                    return i;
            }
            return -1;
        }
    }

    [System.Serializable]
    public class Level
    {
        public enum LevelAction { asteroidAdd, asteroidRemove, powerUp, greenUfo, redUfo, bossLevel }
        public LevelAction[] actions;
    }

    [System.Serializable]
    public class Stage
    {
        public string Name;
        public int SecondsToComplete;
        public SceneName SceneName;
        public Transform PathPoint0;
        public Transform PathPoint1;
        public Transform PathPoint2;
        public float LightCamXposition;
        public LevelAction[] actions;
    }

    public class GameStatistics
    {
        public GameStatistics() => Playtime = 0;

        public int CurrentScore { get; set; }
        public int CurrentLevel { get; set; }
        public int CurrentLives { get; set; }

        public float Playtime { get; private set; }
        public float ShotsFired { get; private set; }
        public float ShotsHit { get; private set; }
        public int UfosSpawned { get; private set; }
        public int UfosDestroyed { get; private set; }
        public int PowerupsSpawned { get; private set; }
        public int PowerupsPickedUp { get; private set; }
        public int PowerupsDestroyed { get; private set; }
        public int AsteroidsDestroyed { get; private set; }

        public virtual void AddPlayTime(float t)
        {
            Playtime += t;
        }

        public virtual void ShotFired() => ShotsFired++;
        public virtual void ShotHit() => ShotsHit++;
        public virtual void UfoSpawned() => UfosSpawned++;
        public virtual void UfoDestroyed() => UfosDestroyed++;
        public virtual void PowerupSpawned() => PowerupsSpawned++;
        public virtual void PowerupPickedUp() => PowerupsPickedUp++;
        public virtual void PowerupDestroyed() => PowerupsDestroyed++;
        public virtual void AsteroidDestroyed() => AsteroidsDestroyed++;
    }

    public class StageStatistics : GameStatistics
    {
        public StageStatistics(int nr, Stage stage, GameStatistics gameStats) : base()
        {
            StageNr = nr;
            _stage = stage;
            _gameStats = gameStats;
            _lvlManager = GmManager != null ? GmManager.m_LevelManager : null;
            if (_lvlManager != null)
            {
                CurrentLevel = _lvlManager.CurrentLevel;
                CurrentScore = Score.Earned;
                CurrentLives = 1;
            }
        }

        public int StageNr { get; private set; }
        public string Name => _stage.Name;

        int BonusEfficiency => _lvlManager != null ? _lvlManager.m_EfficiencyBonus : 100;
        int BonusDestruction => _lvlManager != null ? _lvlManager.m_DestructionBonus : 100;
        int BonusPickup => _lvlManager != null ? _lvlManager.m_PickupBonus : 100;
        int BonusTime => _lvlManager != null ? _lvlManager.m_TimeBonus : 100;

        public int EfficiencyBonus;
        public int TimeBonus;
        public int DestrucionBonus;
        public int PickupBonus;

        readonly Stage _stage;
        readonly LevelManager _lvlManager;
        readonly GameStatistics _gameStats;

        public override void AddPlayTime(float t)
        {
            base.AddPlayTime(t);
            _gameStats.AddPlayTime(t);
        }

        public override void ShotFired()
        {
            base.ShotFired();
            _gameStats.ShotFired();
        }

        public override void ShotHit()
        {
            base.ShotHit();
            _gameStats.ShotHit();
        }

        public override void UfoSpawned()
        {
            base.UfoSpawned();
            _gameStats.UfoSpawned();
        }

        public override void UfoDestroyed()
        {
            base.UfoDestroyed();
            _gameStats.UfoDestroyed();
        }

        public override void PowerupSpawned()
        {
            base.PowerupSpawned();
            _gameStats.PowerupSpawned();
        }

        public override void PowerupPickedUp()
        {
            base.PowerupPickedUp();
            _gameStats.PowerupPickedUp();
        }

        public override void PowerupDestroyed()
        {
            base.PowerupDestroyed();
            _gameStats.PowerupDestroyed();
        }

        public override void AsteroidDestroyed()
        {
            base.AsteroidDestroyed();
            _gameStats.AsteroidDestroyed();
        }

        public int TotalBonus
        {
            get
            {
                if (__totalBonus == 0)
                    __totalBonus = CalculateBonus();

                return __totalBonus;
            }
        }
        int __totalBonus;

        int CalculateBonus()
        {
            if (ShotsFired > 0)
                EfficiencyBonus = (int)Mathf.Round(ShotsHit / ShotsFired * BonusEfficiency);

            TimeBonus = CalcTimeBonus();

            if (UfosSpawned > 0)
                DestrucionBonus = (int)Mathf.Round(UfosDestroyed / UfosSpawned * BonusDestruction);

            if (PowerupsPickedUp > 0)
                PickupBonus = (int)Mathf.Round(PowerupsPickedUp / PowerupsSpawned) * BonusPickup;

            return (EfficiencyBonus + TimeBonus + DestrucionBonus + PickupBonus) * StageNr;
        }

        int CalcTimeBonus()
        {
            if (Playtime <= _stage.SecondsToComplete)
                return BonusTime;
            else
            {
                var t = Playtime - (2 * _stage.SecondsToComplete);
                if (t < 0)
                    return (int)(Mathf.Abs(t) * .01f * BonusTime);

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
            // _stageStats = new List<StageStatistics>();
        }

        readonly Level[] _levels;
        readonly Stage[] _stages;
        //readonly List<StageStatistics> _stageStats;

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

        GameStatistics _gameStats;
        StageStatistics _stageStats;

        public int Level => _level;
        public int Stage => _stage;
        public int AsteroidsForLevel => _levelAsteriods;
        public int AsteroidsActive => _asteroidsActive;
        public int TotalUfosActive => _ufosGreenActive + _ufosRedActive;
        //int UfosGreenActive => _ufosGreenActive;
        //int UfosRedActive => _ufosRedActive;
        public bool HasEnemy => _asteroidsActive > 0 || TotalUfosActive > 0;
        public bool CanAddUfo => TotalUfosActive < _levelUfos && _asteroidsActive > 0;
        public bool CanAddGreenUfo => _levelUfoGreen;
        public bool CanAddRedUfo => _levelUfoRed;
        public bool CanAddPowerup => _levelPowerup;

        public void AddPlayTime(float t) => _stageStats.AddPlayTime(t);
        public void AstroidAdd() => _asteroidsActive++;
        public void AstroidRemove()
        {
            _stageStats.AsteroidDestroyed();
            _asteroidsActive--;
        }

        public void UfoAdd(UfoType type)
        {
            if (type == UfoType.green)
                _ufosGreenActive++;
            else
                _ufosRedActive++;

            _stageStats.UfoSpawned();
        }

        public void UfoRemove(UfoType type, bool destroyed)
        {
            if (type == UfoType.green)
                _ufosGreenActive--;
            else
                _ufosRedActive--;

            if (destroyed)
                _stageStats.UfoDestroyed();
        }

        public void Level1() => SetLevel(1);

        public void LevelAdvance()
        {
            _stageLevel++;
            SetLevel(_level + 1);
        }

        public bool IsStageComplete() => _stageLevel == _levels.Length;

        public StageStatistics GetStageStatistics() => _stageStats;
        public GameStatistics GetGameStatistics() => _gameStats;

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
                _gameStats = new GameStatistics();

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
                SetLightCheckCamOffset();
            }

            SetActionsForLevel();

            _stageStats = new StageStatistics(_stage, _stages[_stage - 1], _gameStats);

            //_stageStats.Add(_stats);

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

        void SetLightCheckCamOffset()
        {
            var offset = _stages?[_stage - 1].LightCamXposition;
            if (offset.HasValue)
                GmManager.m_LightsManager.SetLightCheckOffset(offset.Value);
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
                    _stageStats.PowerupSpawned();
                    break;
                case LevelManager.Statistic.powerupDestroyed:
                    _stageStats.PowerupDestroyed();
                    break;
                case LevelManager.Statistic.powerupPickup:
                    _stageStats.PowerupPickedUp();
                    break;
                case LevelManager.Statistic.shotFired:
                    _stageStats.ShotFired();
                    break;
                case LevelManager.Statistic.shotHit:
                    _stageStats.ShotHit();
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

}