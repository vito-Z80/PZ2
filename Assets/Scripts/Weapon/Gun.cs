using System.Collections.Generic;
using Characters;
using Core;
using Data;
using Unity.VisualScripting;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace Weapon
{
    public class Gun : MonoBehaviour
    {
        [SerializeField] int attack;
        [SerializeField] float gunRotateSpeed;
        [SerializeField] SpriteRenderer gunSprite;

        [SerializeField] WeaponData data;

        // [CanBeNull] EnemyController m_target;
        readonly Color m_fireColor = new Color(1f, 1f, 1f, 0.5f);
        CircleCollider2D m_collider;
        Button m_fireButton;
        List<EnemyController> m_enemies;

        float m_shotTime;
        int m_targetId = -1;
        


        void Start()
        {
            m_enemies = GameManager.I.enemies;
            m_collider = GetComponent<CircleCollider2D>();
        }

        public void Init(WeaponData weaponData)
        {
            data = new WeaponData
            {
            };
        }

        void Update()
        {
            Tracking();
        }

        // bool m_isFlipping;

        void Tracking()
        {
            if (m_targetId >= 0 && m_enemies[m_targetId] != null && !m_enemies[m_targetId].IsDestroyed())
            {
                var target = m_enemies[m_targetId];
                var direction = target.transform.position - transform.position;
                var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * gunRotateSpeed);
                gunSprite.flipY = transform.position.x > target.transform.position.x;
            }
            else
            {
                var targetAngle = gunSprite.flipY ? 180 : 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle), Time.deltaTime * gunRotateSpeed);
                gunSprite.color = Color.white;
            }
        }


        void SetTargetId()
        {
            var distance = m_collider.radius;
            m_targetId = -1;
            for (var id = 0; id < m_enemies.Count; id++)
            {
                var enemy = m_enemies[id];
                if (enemy is null || enemy.IsDestroyed()) continue;
                var distanceToTarget = Mathf.Abs(Vector2.Distance(enemy.transform.position, transform.position));
                if (distanceToTarget < distance)
                {
                    distance = distanceToTarget;
                    m_targetId = id;
                }
            }
        }


        void Fire()
        {
            SetTargetId();
            if (m_targetId < 0) return;
            // if (m_shotTime < 0.03f)
            // {
            //     m_shotTime += Time.deltaTime;
            //     return;
            // }
            // m_shotTime = 0.0f;
            m_enemies[m_targetId].Damage(attack);
            //  visibility
            gunSprite.color = gunSprite.color.a < 1.0f ? Color.white : m_fireColor;
        }


        void OnEnable()
        {
            m_fireButton = GameObject.Find("FireButton").GetComponent<Button>();
            m_fireButton.onClick.AddListener(Fire);
            // EnemyDetection.OnDetect += ChangeGunAngle;
            // EnemyDetection.OnExit += TargetLost;
        }

        void OnDisable()
        {
            // EnemyDetection.OnDetect -= ChangeGunAngle;
            // EnemyDetection.OnExit -= TargetLost;
            m_fireButton.onClick.RemoveAllListeners();
        }

        // void ChangeGunAngle(Collider2D other)
        // {
        //     m_target = other.GetComponentInParent<EnemyController>();
        //     // m_isFlipping = gunSprite.flipY;
        // }
        //
        //
        // void TargetLost(Collider2D other)
        // {
        //     if (other.GetComponentInParent<EnemyController>() == m_target)
        //     {
        //         m_target = null;
        //     }
        // }
    }
}