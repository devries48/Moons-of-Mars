using UnityEngine;

namespace Game.Astroids
{
    public class UfoController : SpaceShipMonoBehaviour
    {
        enum SpawnSide
        {
            left,
            right
        }

        [Header("UFO")]

        [SerializeField]
        float speed = 10f;

        [SerializeField]
        float rotationSpeed = 50f;

        AstroidsGameManager _gameManager;
        Vector3 _target;

        void Awake() => _gameManager = AstroidsGameManager.Instance;

        void OnEnable() => SetRandomUfoBehaviour();

        void FixedUpdate()
        {
            MoveUfo();
            SpinUfo();
        }

        void SetRandomUfoBehaviour()
        {
            var side = RandomEnumUtil<SpawnSide>.Get();

            Rb.transform.position = SpawnPoint(side == SpawnSide.left);
            _target = SpawnPoint(side != SpawnSide.left);
        }

        void MoveUfo()
        {
            //linear movement
            var step = speed * Time.fixedDeltaTime;

            Rb.transform.position = Vector3.MoveTowards(Rb.transform.position, _target, step);
           
            if (Vector3.Distance(Rb.transform.position, _target) < 0.1f)
            {
                _gameManager.UfoDestroyed();
                RemoveFromGame();
            }
        }

        void SpinUfo()
        {
            Rb.transform.Rotate(new Vector3(0, rotationSpeed  * speed * Time.fixedDeltaTime, 0));
        }

        Vector3 SpawnPoint(bool left)
        {
            var xPos = (left)
                 ? _gameManager.m_camBounds.LeftEdge - 1
                 : _gameManager.m_camBounds.RightEdge + 1;

            var yPos = Random.Range(
                _gameManager.m_camBounds.TopEdge - 1,
                _gameManager.m_camBounds.BottomEdge + 1);

            return new Vector3(xPos, yPos);
        }
    }
}