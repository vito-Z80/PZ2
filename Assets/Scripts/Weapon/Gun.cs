using Characters;
using Data;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace Weapon
{
    public class Gun : MonoBehaviour
    {
        SpriteRenderer m_gunSprite;
        [SerializeField] WeaponData data;
        [CanBeNull] EnemyController m_target;
        readonly Color m_fireColor = new(1f, 1f, 1f, 0.5f);
        
        void Start()
        {
            m_gunSprite = GetComponentInChildren<SpriteRenderer>();
        }

        public void Init(WeaponData weaponData)
        {
            data = new WeaponData
            {
                Guid = weaponData.Guid,
                Name = weaponData.Name,
                Type = weaponData.Type,
                Attack = weaponData.Attack,
                Range = weaponData.Range,
                Speed = weaponData.Speed,
                Durability = weaponData.Durability,
                Level = weaponData.Level,
                Radius = weaponData.Radius,
                AddressableName = weaponData.AddressableName
            };
        }

        void Update()
        {
            Tracking();
        }

        public float GetDamageRadius()
        {
            return data.Radius;
        }

        void Tracking()
        {
            if (m_target != null && !m_target.IsDestroyed())
            {
                var direction = m_target.transform.position - transform.position;
                var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * data.Speed);
                m_gunSprite.flipY = transform.position.x > m_target.transform.position.x;
            }
            else
            {
                var targetAngle = m_gunSprite.flipY ? 180 : 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), Time.deltaTime * data.Speed);
                m_gunSprite.color = Color.white;
            }
        }


        // int GetNearestEnemyId()
        // {
        //     var distance = m_collider.radius;
        //     var targetId = -1;
        //     for (var id = 0; id < m_enemies.Count; id++)
        //     {
        //         var enemy = m_enemies[id];
        //         if (enemy is null || enemy.IsDestroyed()) continue;
        //         var distanceToTarget = Mathf.Abs(Vector2.Distance(enemy.transform.position, transform.position));
        //         if (distanceToTarget < distance)
        //         {
        //             distance = distanceToTarget;
        //             targetId = id;
        //         }
        //     }
        //
        //     return targetId;
        // }


        public void Fire([CanBeNull] EnemyController target, float coefficient = 1.0f)
        {
            m_target = target;
            target?.Damage((int)(data.Attack * coefficient));
            //  visibility
            m_gunSprite.color = m_gunSprite.color.a < 1.0f ? Color.white : m_fireColor;
        }


        public WeaponData GetData() => data;

        // void OnEnable()
        // {
        //     m_fireButton = GameObject.Find("FireButton").GetComponent<Button>();
        //     m_fireButton.onClick.AddListener(() => Fire());
        // }
        //
        // void OnDisable()
        // {
        //     m_fireButton.onClick.RemoveAllListeners();
        // }
    }
}