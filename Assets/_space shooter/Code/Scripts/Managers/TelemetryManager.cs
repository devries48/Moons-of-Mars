using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SpaceShooter
{
    public class TelemetryManager : MonoBehaviour
    {

        /// <summary>
        /// Monitor laser overheating, range 0 to 1
        /// </summary>
        public float LaserHeat { get; set; }

        /// <summary>
        /// Monitor boost duration, range 0 to 1
        /// </summary>
        public float Boosting { get; set; }
    }
}