using System;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Data;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ItemIcon : MonoBehaviour
    {
        int m_amount = 0;
        [SerializeField] TextMeshProUGUI amountText;
        [SerializeField] Image selector;
        [SerializeField] Image itemIcon;
        [SerializeField] ItemData data;

        public async Task Init(ItemData itemData)
        {
            data = itemData;
            var manager = GameManager.I;
            var sprite = await manager.Data.LoadAsset<Sprite>(itemData.ImageName);
            itemIcon.sprite = sprite;
            var amount = manager.GetSaveData().Items[itemData.Guid].Amount;
            SetAmount(amount);
        }


        public ItemData GetData() => data;

        public void SetAmount(int amount)
        {
            m_amount = amount;
            amountText.text = amount.ToString();
            amountText.gameObject.SetActive(m_amount > 1);
        }


        public void ResetSelection()
        {
            selector.gameObject.SetActive(false);
        }

        public void SetSelection()
        {
            selector.gameObject.SetActive(true);
            var inventory = GetComponentInParent<InventoryUI>();
            inventory.SelectItem(this);
        }
    }
}