using MoonsOfMars.Shared;
using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    [CreateAssetMenu(fileName = "Game Manager data", menuName = "Asteroids/Game Manager Data")]
    public class GameManagerData : ScriptableObject
    {
        #region editor fields
        [Header("Managers")]
        public UfoManagerData m_UfoManager;
        public PowerupManagerData m_PowerupManager;
        public UIManagerData m_UiManager;

        [Header("Prefabs")]
        [SerializeField, Tooltip("Select rocket prefab")]
        GameObject RocketPrefab;

        [SerializeField, Tooltip("Select astroid prefab")]
        GameObject asteroidPrefab;

        [SerializeField, Tooltip("Select rocket-animations prefab")]
        GameObject rocketAnimations;
        #endregion

        #region properties
        protected GameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = GameManager.GmManager;

                return __gameManager;
            }
        }
        GameManager __gameManager;
        #endregion

        #region fields
        GameObjectPool _asteroidPool;
        GameObjectPool _rocketAnimationPool;
        #endregion

        public void Initialize()
        {
            GameManager.CreateObjectPool(BuildPoolsAction);
        }

        public PlayerShipController CreatePlayer()
        {
            var ship = Instantiate(RocketPrefab);
            GameManager.AddObjectToPoolScene(ship);

            ship.TryGetComponent(out PlayerShipController shipCtrl);
            if (ship)
                GameManager.m_HudManager.ConnectToShip(shipCtrl);

            return shipCtrl;
        }

        public void SpawnAsteroids(float asteroidsNum, int generation = 1, Vector3 position = default)
        {
            if (GameManager.m_debug.NoAsteroids)
                return;

            var isRandom = position == default;

            for (int i = 1; i <= asteroidsNum; i++)
            {
                if (isRandom)
                    position = new Vector3(Random.Range(-20, 20), 10f);

                var scale = generation switch
                {
                    1 => 1f,
                    2 => .5f,
                    _ => .25f
                };

                var astroid = _asteroidPool.GetFromPool(position, size: new Vector3(2f, 2f, 2f) * scale);
                astroid.GetComponent<AsteroidController>().SetGeneration(generation);

                GameManager.m_LevelManager.AddAstroid();
            }
        }

        public AlliedShipController HyperJumpAnimation(float duration)
        {
            var ship = _rocketAnimationPool.GetFromPool();
            Utils.SetGameObjectLayer(ship, Utils.ObjectLayer.Default);
            ship.TryGetComponent(out AlliedShipController ctrl);
            ctrl.PlayerShipJumpOut(duration);

            return ctrl;
        }

        public void StageCompleteAnimation()
        {
            GameManager.SetGameStatus(GameManager.GameStatus.stageResults);
            GameManager.m_HudManager.HudHide();
            GameManager.m_LevelManager.ShowStageResults();

            if (GameManager.m_playerShip)
                GameManager.m_playerShip.Teleport();

            var ship = _rocketAnimationPool.GetFromPool();
            Utils.SetGameObjectLayer(ship, Utils.ObjectLayer.Background);
            GameManager.m_StageEndCamera.Follow = ship.transform;

            ship.TryGetComponent(out AlliedShipController ctrl);
            ctrl.PlayerShipStageCompleteAnimation();
        }

        void BuildPoolsAction()
        {
            _rocketAnimationPool = GameManager.CreateObjectPool(rocketAnimations, 1);
            _asteroidPool = GameManager.CreateObjectPool(asteroidPrefab, 20, 100);
            Debug.Log("Asteroid Pool Created");
        }

    }
}
