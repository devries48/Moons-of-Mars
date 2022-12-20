using System;
using UnityEngine;

namespace Game.Astroids
{
    [ExecuteInEditMode]
    public class LevelManager : MonoBehaviour
    {
        public Transform[] controlPoints;
        public Vector3[] m_EarthPath;

        Vector3 gizmosPosition;

        public void EndStageAnimaions(Vector3[] path, float t)
        {

            var ltPath = new LTBezierPath(path);
            LeanTween.move(gameObject, ltPath, t)
                .setOrientToPath(true);
        }

        public void OnDrawGizmos()
        {
            if (controlPoints.Length != 4)
                return;

            for (float t = 0; t <= 1; t += 0.05f)
            {
                gizmosPosition = Mathf.Pow(1 - t, 3) * controlPoints[0].position + 3 * Mathf.Pow(1 - t, 2) * t * controlPoints[1].position + 3 * (1 - t) * Mathf.Pow(t, 2) * controlPoints[2].position + Mathf.Pow(t, 3) * controlPoints[3].position;

                Gizmos.DrawSphere(gizmosPosition, 0.05f);
            }

            Gizmos.DrawLine(new Vector3(controlPoints[0].position.x, controlPoints[0].position.y, controlPoints[0].position.z), new Vector3(controlPoints[1].position.x, controlPoints[1].position.y, controlPoints[1].position.z));
            Gizmos.DrawLine(new Vector3(controlPoints[2].position.x, controlPoints[2].position.y, controlPoints[2].position.z), new Vector3(controlPoints[3].position.x, controlPoints[3].position.y, controlPoints[3].position.z));

        }

    }
}