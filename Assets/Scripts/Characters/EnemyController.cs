using System;
using Core;
using Data;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    public class EnemyController : MainCharacter
    {
        Vector2 m_direction;
        Rigidbody2D m_rigidbody2D;
        HealthBar m_healthBar;
        [SerializeField] Transform flippedTransform;
        [SerializeField] EnemyData data;
        Transform m_target;

        const float DamageTimer = 1.0f; //  TODO move to json for any enemy.
        float m_damageTime = 0.5f;

        public void Init(EnemyData enemyData, Transform playerControllerTransform)
        {
            m_target = playerControllerTransform;
            data = new EnemyData
            {
                Guid = enemyData.Guid,
                Name = enemyData.Name,
                Type = enemyData.Type,
                Health = enemyData.Health,
                Armour = enemyData.Armour,
                Speed = enemyData.Speed,
                Level = enemyData.Level
            };
            m_rigidbody2D = GetComponent<Rigidbody2D>();
            var speedCoefficient = Random.Range(1.0f, 1.2f);
            data.Speed *= speedCoefficient;
            m_healthBar = GetComponentInChildren<HealthBar>();
            var baseEnemyData = GameManager.I.Data.GetEnemyData(enemyData.Guid);
            m_healthBar.Init(baseEnemyData.Health, data.Health);
        }

        void Update()
        {
            if (data == null) return;
            if (m_target != null)
            {
                m_direction = (m_target.position - transform.position).normalized;
            }
            else
            {
                m_direction = Vector2.zero;
            }

            Flip();
        }

        void FixedUpdate()
        {
            if (data == null) return;
            m_rigidbody2D.MovePosition(m_rigidbody2D.position + m_direction * (data.Speed * Time.fixedDeltaTime));
        }

        void Flip()
        {
            var localScale = flippedTransform.localScale;
            if (m_direction.x > 0.0f)
            {
                localScale.x = 1.0f;
            }
            else if (m_direction.x < 0.0f)
            {
                localScale.x = -1.0f;
            }

            flippedTransform.localScale = localScale;
        }


        public override void Damage(int value)
        {
            data.Health -= value;
            m_healthBar.UpdateHealth(data.Health);
            if (data.Health <= 0)
            {
                data.Health = 0;
                GameManager.I.Spawner.SpawnRandomItem(transform.position);
                Dead();
            }
        }

        public override void AddHealth(int health)
        {
        }

        public override void Fire()
        {
        }


        void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                m_damageTime += Time.deltaTime;
                if (m_damageTime >= DamageTimer)
                {
                    m_damageTime = 0.0f;
                    //  TODO create BaseDamage parameter in json for enemies and characters/
                    other.GetComponentInParent<PlayerController>().Damage(37);
                }
            }
        }
    }
}