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

        
        public bool isSelected = false;
        
        public async Task Init(ItemData itemData)
        {
            var manager = GameManager.I;
            var sprite = await manager.Data.LoadAsset<Sprite>(itemData.ImageName);
            itemIcon.sprite = sprite;
            var amount = manager.GetSaveData().Items.FirstOrDefault(saveData => saveData.Guid == itemData.Guid)?.Amount
                         ?? throw new NullReferenceException("Item not have amount");
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
            isSelected = false;
            selector.gameObject.SetActive(false);
        }
        
        public void SetSelection()
        {
            isSelected = true;
            selector.gameObject.SetActive(true);
            // GameManager.I.Inventory.SelectItem(data.Guid);
            var inventory = GetComponentInParent<InventoryUI>();
            inventory.SelectItem(data.Guid);
           
        }
    }
}