using Core;
using Data;
using JetBrains.Annotations;
using UnityEngine;

namespace Items
{
    public class Item : MonoBehaviour
    {
        [SerializeField] float flySpeed;
        [SerializeField] ItemData data;
        SpriteRenderer m_spriteRenderer;

        [CanBeNull] Transform m_target = null;


        public void Init(ItemData itemData)
        {
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            data = itemData;
            m_spriteRenderer.sprite = GameManager.I.Data.ItemSprites[itemData.ImageName];
        }


        void Update()
        {
            if (m_target == null) return;
            transform.position = Vector3.Lerp(transform.position, m_target.position, Time.deltaTime * flySpeed);
            flySpeed += Time.deltaTime * flySpeed;
            var distance = Mathf.Abs(Vector3.Distance(transform.position, m_target.position));
            if (distance < 0.5f)
            {
                m_target = null;
                AddItemToInventory();
                Destroy(gameObject);
            }
        }


        public void SetTargetToFly(Transform target)
        {
            m_target = target;
        }

        public ItemData GetData()
        {
            return data;
        }

        void AddItemToInventory()
        {
            //  add to inventory and destroy item
        }
    }
}