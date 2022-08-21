using UnityEngine;

namespace Game.Astroids
{
    public class AlliedShipController : SpaceShipMonoBehaviour
    {
        enum SpawnSide
        {
            left,
            right,
            top
        }

        Vector3 oldPos;
        AstroidsGameManager _gameManager;

        protected override void Awake()
        {
            _gameManager = AstroidsGameManager.Instance;
            m_isAllied = true;

            base.Awake();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            oldPos = new();
            MoveShip();
        }

        void Update()
        {
            Vector3 direction = (transform.position - oldPos).normalized;

            if (direction != Vector3.zero || oldPos == Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }

        void LateUpdate()
        {
            oldPos = transform.position;
        }

        void MoveShip()
        {
            LeanTween.move(gameObject, CreatePath(), 4f)
                .setEaseOutQuad()
                .setOrientToPath(true);
        }

        LTBezierPath CreatePath(int increments = 4)
        {
            var path = new Vector3[increments];

            float x;
            float y;
            var side = RandomEnumUtil<SpawnSide>.Get();

            // first position
            if (side == SpawnSide.top)
            {
                x = Random.Range(-50f, 50f);
                y = 20f;
            }
            else
            {
                x = side == SpawnSide.left ? -50f : 50f;
                y = Random.Range(-20f, 20f);
            }
            oldPos = new Vector3(x, y, -100);
            transform.position = oldPos;

            path[0] = oldPos;

            // second position
            x = Random.Range(-15f, 15f);
            path[1] = new Vector3(x, Random.Range(-7f, 4f), 0);

            // third position
            if (x < 0)
                x += 20;
            else
                x -= 20;

            path[2] = new Vector3(x, Random.Range(-7f, 4f), 0);

            // last position
            y = Random.Range(-2f, 5f);

            // leave on the bottom of the screen sometimes.
            if (Random.Range(1f, 5f) == 1f)
                y = -5.2f;

            path[3] = new Vector3(Random.Range(-10f, 10f), y, 25);
            print("----");
            print(path[1]);
            print(path[2]);
            return new LTBezierPath(path);
        }

    }
}
