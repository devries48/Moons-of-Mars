using MoonsOfMars.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MoonsOfMars.Game.Asteroids
{
    using static SpaceShipMonoBehaviour;

    [CreateAssetMenu(fileName = "Ufo Manager data", menuName = "Asteroids/Ufo Manager")]
    public class UfoManagerData : ScriptableObject
    {
        #region editor fields
        [SerializeField, Range(10, 60)] int minSpawnWait = 15;
        [SerializeField, Range(10, 60)] int maxSpawnWait = 30;

        [Header("Prefab")]
        [SerializeField, Tooltip("Select an UFO prefab")]
        GameObject ufoPrefab;

        [Header("UFO's")]
        public UfoFields m_GreenUfo = new();
        public UfoFields m_RedUfo = new();
        #endregion

        #region properties
        protected AsteroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AsteroidsGameManager.GmManager;

                return __gameManager;
            }
        }
        AsteroidsGameManager __gameManager;

        LevelManager LevelManager
        {
            get
            {
                if (__levelManager == null)
                    __levelManager = GameManager.m_LevelManager;

                return __levelManager;
            }
        }
        LevelManager __levelManager;

        #endregion


        GameObjectPool _ufoPool;

        public enum UfoType { green, red }

        public IEnumerator UfoSpawnLoop()
        {
            if (_ufoPool == null)
                GameManager.CreateObjectPool(BuildPoolsAction);

            while (!GameManager.IsGameExit)
            {
                while (!GameManager.IsGamePlaying || !LevelManager.CanAddUfo || GameManager.m_debug.NoUfos)
                    yield return null;

                yield return new WaitForSeconds(Random.Range(minSpawnWait, maxSpawnWait));

                if ( GameManager.IsGamePlaying &&  LevelManager.AsteroidsActive > 1 )
                    UfoLaunch();
            }
        }

        public void UfoLaunch() => _ufoPool.GetFromPool();

        public void SetUfoMaterials(UfoController ufo)
        {
            foreach (var rend in ufo.m_Model.GetComponentsInChildren<Renderer>())
            {
                var n = rend.material.name;
                var mats = new List<Material>();

                if (ufo.m_ufoType == UfoType.green && !n.Contains("green"))
                {
                    if (n.StartsWith(m_RedUfo.cockpit.name))
                        mats.Add(m_GreenUfo.body);
                    else if (n.StartsWith(m_RedUfo.body.name))
                    {
                        mats.Add(m_GreenUfo.body);
                        mats.Add(m_GreenUfo.cockpit);
                    }
                }
                else if (ufo.m_ufoType == UfoType.red && !n.Contains("red"))
                {
                    if (n.StartsWith(m_GreenUfo.cockpit.name))
                        mats.Add(m_RedUfo.body);
                    else if (n.StartsWith(m_GreenUfo.body.name))
                    {
                        mats.Add(m_RedUfo.body);
                        mats.Add(m_RedUfo.cockpit);
                    }
                }

                if (mats.Count > 0)
                    rend.materials = mats.ToArray();
            }

            SetLightsColor(ufo);
            SetShieldMaterial(ufo);
        }

        void SetLightsColor(UfoController ufo)
        {
            var color = ufo.m_ufoType == UfoType.green ? m_GreenUfo.lights : m_RedUfo.lights;

            foreach (var light in ufo.m_LightsModel.GetComponentsInChildren<Light>(false))
                light.color = color;
        }

        void SetShieldMaterial(UfoController ufo)
        {
            var rend = ufo.m_Shield.Renderer;

            if (rend)
            {
                var n = rend.material.name;
                var mat = ufo.m_ufoType == UfoType.green ? m_GreenUfo.shield : m_RedUfo.shield;

                if (ufo.m_ufoType == UfoType.green && !n.Contains("green")
                    || ufo.m_ufoType == UfoType.red && !n.Contains("red"))
                {
                    rend.material = mat;
                }
            }
        }

        public void SetBulletMaterial(BulletController bullet, ShipType type)
        {
            if (type == ShipType.ufoGreen || type == ShipType.ufoRed)
            {
                var rend = bullet.Renderer;

                if (rend)
                {
                    var n = rend.material.name;
                    var mat = type == ShipType.ufoGreen ? m_GreenUfo.bullet : m_RedUfo.bullet;

                    if (type == ShipType.ufoGreen && !n.Contains("green")
                        || type == ShipType.ufoRed && !n.Contains("red"))
                    {
                        rend.material = mat;
                    }
                }

            }
        }

        public int GetDestructionScore(UfoType type)
        {
            return type == UfoType.green ? m_GreenUfo.score : m_RedUfo.score;
        }

        void BuildPoolsAction()
        {
            if (ufoPrefab == null)
            {
                Debug.LogError("UfoPrefab Prefab not set!");
                return;
            }

            _ufoPool = GameManager.CreateObjectPool(ufoPrefab, 1);
        }

    }
}