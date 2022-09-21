using UnityEngine;

namespace Game.Astroids
{
    public class FirePowerup : PowerupController
    {
        public override void GrantPower()
        {
            SpaceShipMonoBehaviour ship = m_ship.GetComponent<SpaceShipMonoBehaviour>();
            //int weaponChoice = Random.Range(1, (int)ShipShooter.Weapons.Count);
            //shooter.activeWeapon = weaponChoice;
            base.GrantPower();
        }

        public override void DenyPower()
        {
            //ShipShooter shooter = ship.GetComponent<ShipShooter>();
            //shooter.activeWeapon = (int)ShipShooter.Weapons.Default;
            base.DenyPower();
        }
    }
}