using System;
using UnityEngine;

namespace Game.Asteroids
{
    [Serializable]
    public class UfoFields
    {
        public Material cockpit;
        public Material body;
        public Material shield;
        public Material bullet;
        public Color lights;
        [Range(0, 200)] public int score = 100;
        public AudioClip engineSound;
    }
}