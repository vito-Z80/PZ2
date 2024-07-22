using System;
using System.Linq;
using Core;
using Data;
using UnityEngine;

namespace Items
{
    public class Inventory
    {
        public void AddItem(Guid guid, int amount = 1)
        {
            var saveItemsList = GameManager.I.GetSaveData().Items;
            var itemSaveData = saveItemsList.FirstOrDefault(data => data.Guid == guid);
            if (itemSaveData == null)
            {
                var newItemData = new ItemSaveData
                {
                    Guid = guid,
                    Amount = 1,
                    IsEquipped = false,
                    IsSelected = false,
                };
                saveItemsList.Add(newItemData);
            }
            else
            {
                itemSaveData.Amount++;
            }
        }

        public void RemoveItem(Guid guid, int amount = 1)
        {
            var saveItemsList = GameManager.I.GetSaveData().Items;
            var itemSaveData = saveItemsList.FirstOrDefault(data => data.Guid == guid);
            if (itemSaveData != null)
            {
                if (itemSaveData.Amount - amount < 1)
                {
                    saveItemsList.Remove(itemSaveData);
                }
                else
                {
                    itemSaveData.Amount--;
                }
            }
        }

        public void SelectItem(Guid guid)
        {
            var saveItemsList = GameManager.I.GetSaveData().Items;
            foreach (var itemSaveData in saveItemsList)
            {
                itemSaveData.IsSelected = itemSaveData.Guid == guid;
            }

            Debug.Log(string.Join(",", saveItemsList.Where(itemSaveData => itemSaveData.IsSelected)));
        }

        public void EquipItem(Guid guid)
        {
            var saveItemsList = GameManager.I.GetSaveData().Items;
            foreach (var itemSaveData in saveItemsList)
            {
                itemSaveData.IsEquipped = itemSaveData.Guid == guid;
            }
        }
    }
}