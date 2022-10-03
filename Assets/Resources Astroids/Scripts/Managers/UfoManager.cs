using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Game.Astroids.SpaceShipMonoBehaviour;

namespace Game.Astroids
{
    [CreateAssetMenu(fileName = "UfoManager", menuName = "UfoManager")]
    public class UfoManager : ScriptableObject
    {
        #region editor fields
        [SerializeField, Range(10, 60)] int minSpawnWait = 15;
        [SerializeField, Range(10, 60)] int maxSpawnWait = 30;

        [Header("Prefab")]
        [SerializeField, Tooltip("Select an UFO prefab")]
        GameObject ufoPrefab;

        [Header("Green UFO")]
        [SerializeField] Material greenCockpit;
        [SerializeField] Material greenBody;
        [SerializeField] Material greenShield;
        [SerializeField] Material greenBullet;
        [SerializeField] Color greenLights;
        [SerializeField, Range(0, 200)] int greenScore = 100;


        [Header("Red UFO")]
        [SerializeField] Material redCockpit;
        [SerializeField] Material redBody;
        [SerializeField] Material redShield;
        [SerializeField] Material redBullet;

        [SerializeField] Color redLights;
        [SerializeField, Range(0, 200)] int redScore = 100;
        #endregion

        #region properties
        protected AstroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AstroidsGameManager.Instance;

                return __gameManager;
            }
        }
        AstroidsGameManager __gameManager;
        #endregion

        GameObjectPool _ufoPool;

        public enum UfoType { green, red }

        public IEnumerator UfoSpawnLoop()
        {
            if (_ufoPool == null)
                BuildPools();

            while (GameManager.m_GamePlaying)
            {
                while (GameManager.m_GamePaused || !GameManager.m_level.CanAddUfo)
                    yield return null;

                yield return new WaitForSeconds(Random.Range(minSpawnWait, maxSpawnWait));

                if (!GameManager.m_GamePaused)
                {
                    _ufoPool.GetFromPool();
                    GameManager.m_level.UfoAdd();
                }
            }
        }

        public void SetUfoMaterials(UfoController ufo)
        {
            foreach (var rend in ufo.m_Model.GetComponentsInChildren<Renderer>())
            {
                var n = rend.material.name;
                var mats = new List<Material>();

                if (ufo.m_ufoType == UfoType.green && !n.Contains("green"))
                {
                    if (n.StartsWith(redCockpit.name))
                        mats.Add(greenBody);
                    else if (n.StartsWith(redBody.name))
                    {
                        mats.Add(greenBody);
                        mats.Add(greenCockpit);
                    }
                }
                else if (ufo.m_ufoType == UfoType.red && !n.Contains("red"))
                {
                    if (n.StartsWith(greenCockpit.name))
                        mats.Add(redBody);
                    else if (n.StartsWith(greenBody.name))
                    {
                        mats.Add(redBody);
                        mats.Add(redCockpit);
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
            var color = ufo.m_ufoType == UfoType.green ? greenLights : redLights;

            foreach (var light in ufo.m_LightsModel.GetComponentsInChildren<Light>(false))
                light.color = color;
        }

        void SetShieldMaterial(UfoController ufo)
        {
            var rend = ufo.m_Shield.Renderer;

            if (rend)
            {
                var n = rend.material.name;
                var mat = ufo.m_ufoType == UfoType.green ? greenShield : redShield;

                if (ufo.m_ufoType == UfoType.green && !n.Contains("green")
                    || ufo.m_ufoType == UfoType.red && !n.Contains("red"))
                {
                    rend.material = mat;
                }
            }
        }

        public void SetBulletMaterial(BulletController bullet, ShipType type)
        {
            if (type ==ShipType.ufoGreen || type == ShipType.ufoRed)
            {
                var rend = bullet.Renderer;

                if (rend)
                {
                    var n = rend.material.name;
                    var mat = type == ShipType.ufoGreen ? greenBullet : redBullet;

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
            return type == UfoType.green ? greenScore : redScore;
        }

        void BuildPools()
        {
            if (ufoPrefab == null)
            {
                Debug.LogError("UfoPrefab Prefab not set!");
                return;
            }

            _ufoPool = GameObjectPool.Build(ufoPrefab, 1);
        }

    }
}