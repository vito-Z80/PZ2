﻿using System;

namespace Data
{
    [Serializable]
    public class WeaponData : MainGameData
    {
        public int Attack;
        public float Radius;
        public float Range;
        public float Speed;
        public int Durability;
        public int Level;
        public int Amount;
    }
}