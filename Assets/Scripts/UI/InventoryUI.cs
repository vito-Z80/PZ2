using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Data;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InventoryUI : MonoBehaviour
    {
        SaveData m_saveData;
        GameObject m_itemPrefab;
        GameManager m_gm;
        [SerializeField] Transform container;
        List<ItemIcon> m_items = new();
        [CanBeNull] ItemIcon m_selectedIcon;

        [SerializeField] Button removeButton;
        [SerializeField] Button equipButton;

        void OnEnable()
        {
            ShowInventory();
        }

        void OnDisable()
        {
            Clear();
        }

        async void ShowInventory()
        {
            removeButton.gameObject.SetActive(false);
            equipButton.gameObject.SetActive(false);
            m_gm ??= GameManager.I;
            m_itemPrefab ??= await m_gm.Data.LoadAsset<GameObject>("ItemIcon");
            m_saveData = m_gm.GetSaveData();
            await CreateIcons();
        }

        public void RemoveItem()
        {
            if (m_selectedIcon == null) return;
            m_saveData.RemoveItem(m_selectedIcon.GetData());
            var amount = m_selectedIcon.GetData().Amount;
            m_selectedIcon.SetAmount(amount);
            if (m_selectedIcon.GetData().Amount <= 0)
            {
                m_items.Remove(m_selectedIcon);
                Destroy(m_selectedIcon.gameObject);
            }

            // GameManager.I.Data.SaveGame();
        }

        public void EquipWeapon()
        {
            if (m_selectedIcon == null) return;
            GameManager.I.GetMasterCharacter().EquipWeapon(m_selectedIcon.GetData().Guid);
        }


        void Clear()
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            m_items.Clear();
        }
        
        Task CreateIcons()
        {
            //  TODO dont remove child`s, reuse.
            Clear();
            foreach (var data in m_saveData.Items.Values)
            {
                var iconInstance = Instantiate(m_itemPrefab, container).GetComponent<ItemIcon>();
                iconInstance.Init(data);
                m_items.Add(iconInstance);
            }

            return Task.CompletedTask;
        }


        public void SelectItem(ItemIcon icon)
        {
            m_selectedIcon = icon;
            foreach (var itemIcon in m_items)
            {
                if (itemIcon == icon)
                {
                    equipButton.gameObject.SetActive(itemIcon.GetData().Type == "Weapon");
                    removeButton.gameObject.SetActive(true);
                }
                else
                {
                    itemIcon.ResetSelection();
                }
            }
        }
    }
}