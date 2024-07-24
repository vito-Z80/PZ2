using System;

namespace Data
{
    [Serializable]
    public class CharacterData : MainGameData
    {
        public int Health;
        public int Armour;
        public float Speed;
        public int Level;
        public Guid EquippedWeaponGiud;
        public bool IsMaster;
        public float DetectionRadius;
    }
}