using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Data;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    SaveData m_saveData;
    GameObject m_itemPrefab;
    GameManager m_manager;
    [SerializeField] Transform container;
    List<ItemIcon> m_items = new();


    [SerializeField] Button removeButton;
    [SerializeField] Button equipButton;

    void OnEnable()
    {
        ShowInventory();
    }

    async void ShowInventory()
    {
        m_manager ??= GameManager.I;
        m_saveData ??= m_manager.GetSaveData();
        m_itemPrefab ??= await m_manager.Data.LoadAsset<GameObject>("ItemIcon");
        await CreateIcons();
    }

    async Task CreateIcons()
    {
        //  TODO dont remove child`s, reuse.
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        m_items.Clear();

        foreach (var data in m_saveData.Items)
        {
            var icon = m_items.Find(icon => icon.GetData().Guid == data.Guid);
            if (icon)
            {
                icon.SetAmount(data.Amount);
            }
            else
            {
                var iconInstance = Instantiate(m_itemPrefab, container).GetComponent<ItemIcon>();
                await iconInstance.Init(m_manager.Data.GetItemData(data.Guid));
                m_items.Add(iconInstance);
            }
        }
    }


    public void SelectItem(Guid guid)
    {
        foreach (var itemIcon in m_items)
        {
            if (itemIcon.GetData().Guid == guid)
            {
                equipButton.gameObject.SetActive(itemIcon.GetData().Type == "Weapon");
                removeButton.gameObject.SetActive(itemIcon.GetData().Type != "Weapon");
            }
            else
            {
                itemIcon.ResetSelection();
            }
        }
    }

    
}