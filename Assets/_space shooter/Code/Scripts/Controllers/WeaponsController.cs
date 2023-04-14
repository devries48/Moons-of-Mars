using MoonsOfMars.Shared;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static MoonsOfMars.Shared.Utils;

namespace Game.SpaceShooter
{
    public class WeaponsController : MonoBehaviour
    {
        public enum HardpointType { cannon, laser, missle }

        #region editor fields
        [SerializeField] ShipController _shipController;
        [SerializeField] TelemetryManager _telemetry;

        [Header("=== Hardpoint Settings ===")]
        [SerializeField] HardpointController[] _hardpoints;
        [SerializeField] LayerMask _shootableMask;

        [Header("=== Laser Settings ===")]
        [SerializeField] LineRenderer[] _lasers;
        [SerializeField] float _laserRange = 100f;
        [SerializeField] float _laserPower = 1f;
        [SerializeField] float _laserHeatThreshold = 2.25f;
        [SerializeField] float _laserHeatRate = .75f;
        [SerializeField] float _laserCoolRate = 1f;

        [Header("=== Missle Settings ===")]
        [SerializeField] GameObject _missilePrefab;
        [SerializeField] float _missileReloadTime = 5f;
        [SerializeField] float _missleLockRange = 100f;
        [SerializeField] float _missleLockSpeed = 30f;
        [SerializeField] float _missleLockAngle = 45f;

        [Header("=== Cannon Settings ===")]
        [SerializeField] BulletController _bulletPrefab;
        [SerializeField, Tooltip("Firing rate in Rounds Per Second")] float _cannonFireRate = 900f;
        [SerializeField] float _cannonSpread = 0.25f;
        [SerializeField] float _bulletForce = 1000f;
        [SerializeField] float _bulletLifetime = 2f;
        [SerializeField] float _bulletDamage = 10f;
        #endregion

        Camera Cam
        {
            get
            {
                if (__cam == null)
                    __cam = Camera.main;
                return __cam;
            }
        }
        Camera __cam;

        bool _cannonsFiring, _lasersFiring, _lasersOverheated;
        float _cannonFiringTimer;
        float _currentLaserHeat;

        #region Input Methods
        public void OnFireCannons(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                ActivateCannons(true);
            else if (context.phase == InputActionPhase.Canceled)
                ActivateCannons(false);
        }

        public void OnFireLasers(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started)
                ActivateLasers(true);
            else if (context.phase == InputActionPhase.Canceled)
                ActivateLasers(false);
        }

        #endregion


        public void ActivateCannons(bool activate)
        {
            //if (Dead) return;
            _cannonsFiring = activate;
        }

        public void ActivateLasers(bool activate)
        {
            //if (Dead) return;
            _lasersFiring = activate;
        }

        public void UpdateWeapons(float dt)
        {
            UpdateWeaponCooldown(dt);

            List<HardpointController> lasers = new();
            //UpdateMissileLock(dt);
            foreach (var hardpoint in _hardpoints)
                switch (hardpoint.m_Type)
                {
                    case HardpointType.cannon:
                        UpdateCannon(hardpoint);
                        break;
                    case HardpointType.laser:
                        lasers.Add(hardpoint);
                        break;
                    case HardpointType.missle:
                        break;
                    default:
                        break;
                }

            UpdateLasers(lasers);

            if (_cannonsFiring && _cannonFiringTimer == 0)
                _cannonFiringTimer = 1f / _cannonFireRate;

            _telemetry.LaserHeat = _currentLaserHeat / _laserHeatThreshold;
        }


        void UpdateCannon(HardpointController hardpoint)
        {
            if (_cannonsFiring && _cannonFiringTimer == 0)
            {
                var trans = hardpoint.transform;
                var spread = Random.insideUnitCircle * _cannonSpread;
                var bullet = Instantiate(_bulletPrefab, trans.position, trans.rotation * Quaternion.Euler(1f + spread.x, 1f + spread.y, 0));
                bullet.Fire(_bulletForce, _bulletLifetime, _bulletDamage, _shipController.m_RbShip);
                hardpoint.PlayAudioClip();
            }
        }


        void UpdateLasers(List<HardpointController> lasers)
        {
            if (_lasersFiring && !_lasersOverheated)
            {
                var pos1 = lasers.First().gameObject.transform.position;
                var pos2 = lasers.Last().gameObject.transform.position;
                var center = (pos1 + pos2) / 2;
                center.y += 2;

                if (TargetInfo.IsTargetInRange(
                    Cam.transform.position,
                    Cam.transform.forward,
                    out var hitInfo,
                    _laserRange,
                    _shootableMask))
                {
                    // Instantiate laser hit particles
                    foreach (var laser in _lasers)
                    {
                        var localHitPosition = laser.transform.InverseTransformPoint(hitInfo.point);
                        laser.gameObject.SetActive(true);
                        laser.SetPosition(1, localHitPosition);
                    }
                    GameManager.Instance.PlayEffect(EffectsManager.Effect.hitLaser, hitInfo.point, Quaternion.LookRotation(hitInfo.normal), 1.5f, OjectLayer.Default);
                }
                else
                {
                    foreach (var laser in _lasers)
                    {
                        laser.gameObject.SetActive(true);
                        laser.SetPosition(1, new Vector3(0, 0, _laserRange));
                    }
                }
            }
            else
            {
                foreach (var laser in _lasers)
                    laser.gameObject.SetActive(false);

            }
        }

        void UpdateWeaponCooldown(float dt)
        {
            _cannonFiringTimer = Mathf.Max(0, _cannonFiringTimer - dt);

            if (_lasersFiring)
            {
                if (_currentLaserHeat < _laserHeatThreshold)
                {
                    _currentLaserHeat += _laserHeatRate * dt;
                    if (_currentLaserHeat >= _laserHeatThreshold)
                    {
                        _lasersOverheated = true;
                        _lasersFiring = false;
                    }
                }
            }
            else
            {
                if (_lasersOverheated)
                {
                    if (_currentLaserHeat / _laserHeatThreshold <= .5f)
                        _lasersOverheated = false;
                }
                if (_currentLaserHeat > 0f)
                    _currentLaserHeat -= _laserCoolRate * dt;

                if (_currentLaserHeat < 0f)
                    _currentLaserHeat = 0f;
            }

            //for (int i = 0; i < missileReloadTimers.Count; i++)
            //{
            //    missileReloadTimers[i] = Mathf.Max(0, missileReloadTimers[i] - dt);

            //    if (missileReloadTimers[i] == 0)
            //    {
            //        animation.ShowMissileGraphic(i, true);
            //    }
            //}

        }
    }
}