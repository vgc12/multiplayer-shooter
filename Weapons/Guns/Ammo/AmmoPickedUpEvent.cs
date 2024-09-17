namespace Weapons.Guns.Ammo
{
    public struct AmmoPickedUpEvent
    {
        public readonly AmmoPickUp AmmoPickUp;
        public AmmoPickedUpEvent(AmmoPickUp ammoPickUp)
        {
            AmmoPickUp = ammoPickUp;
        }
    }
}