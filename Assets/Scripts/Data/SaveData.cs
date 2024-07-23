using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

namespace Data
{
    public class SaveData
    {
        public Dictionary<Guid, CharacterData> Characters = new();
        public Dictionary<Guid, ItemData> Items = new();
        public Dictionary<Guid, WeaponData> Weapons = new();


        public CharacterData GetCharacterMaster() => Characters.FirstOrDefault(pair => pair.Value.IsMaster).Value
                                                              ?? throw new KeyNotFoundException("CharacterMaster not found");

        public void AddCharacter(CharacterData baseCharacterData)
        {
            if (!Items.ContainsKey(baseCharacterData.Guid))
            {
                Characters[baseCharacterData.Guid] = new CharacterData
                {
                    Guid = baseCharacterData.Guid,
                    Name = baseCharacterData.Name,
                    Type = baseCharacterData.Type,
                    Health = baseCharacterData.Health,
                    Armour = baseCharacterData.Armour,
                    Speed = baseCharacterData.Speed,
                    Level = baseCharacterData.Level,
                    EquippedWeaponGiud = baseCharacterData.EquippedWeaponGiud
                };
            }
        }


        public void AddWeapon(WeaponData baseWeaponData)
        {
            if (Weapons.ContainsKey(baseWeaponData.Guid))
            {
                Weapons[baseWeaponData.Guid].Amount += baseWeaponData.Amount;
            }
            else
            {
                Weapons[baseWeaponData.Guid] = new WeaponData
                {
                    Guid = baseWeaponData.Guid,
                    Name = baseWeaponData.Name,
                    Type = baseWeaponData.Type,
                    Attack = baseWeaponData.Attack,
                    Radius = baseWeaponData.Radius,
                    Range = baseWeaponData.Range,
                    Speed = baseWeaponData.Speed,
                    Durability = baseWeaponData.Durability,
                    Level = baseWeaponData.Level,
                    Amount = baseWeaponData.Amount
                };
            }
        }

        
        //  TODO есть 5 АК-74, удаляем 1, удаляем второй и ексепшн. (Видимо опять разные инстансы данных...)
        public void RemoveWeapon(WeaponData baseWeaponData, int amount = 1)
        {
            if (Items.ContainsKey(baseWeaponData.Guid))
            {
                Weapons[baseWeaponData.Guid].Amount -= amount;
                Debug.Log(Weapons[baseWeaponData.Guid].Amount);
                if (Weapons[baseWeaponData.Guid].Amount <= 0)
                {
                    Weapons.Remove(baseWeaponData.Guid);
                }
            }
        }

        public void AddItem(ItemData baseItemData)
        {
            if (baseItemData.Type == "Weapon")
            {
                var baseWeaponData = GameManager.I.Data.GetWeaponData(baseItemData.Guid);
                AddWeapon(baseWeaponData);
            }

            if (Items.ContainsKey(baseItemData.Guid))
            {
                Items[baseItemData.Guid].Amount += baseItemData.Amount;
            }
            else
            {
                Items[baseItemData.Guid] = new ItemData
                {
                    Guid = baseItemData.Guid,
                    Name = baseItemData.Name,
                    Type = baseItemData.Type,
                    ImageName = baseItemData.ImageName,
                    Amount = baseItemData.Amount,
                };
            }
        }

        public void RemoveItem(ItemData baseItemData, int amount = 1)
        {
            if (baseItemData.Type == "Weapon")
            {
                var baseWeaponData = GameManager.I.Data.GetWeaponData(baseItemData.Guid);
                RemoveWeapon(baseWeaponData,amount);
            }

            if (Items.ContainsKey(baseItemData.Guid))
            {
                Items[baseItemData.Guid].Amount -= amount;
                if (Items[baseItemData.Guid].Amount <= 0)
                {
                    Items.Remove(baseItemData.Guid);
                }
            }
        }
    }
}