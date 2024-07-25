using System;
using System.Collections.Generic;
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
        Vector2 m_pointTarget;
        Transform m_targetTransform;

        GameManager m_gm;
        BoundsInt m_levelBounds;
        const float DamageTimer = 1.0f; //  TODO move to json for any enemy.
        float m_damageTime = 0.5f;

        List<PlayerController> m_targetList;

        public void Init(EnemyData enemyData, List<PlayerController> characters)
        {
            m_gm = GameManager.I;
            m_levelBounds = m_gm.GetLayer("Floor").cellBounds;
            m_pointTarget = m_gm.Spawner.GetRandomPointInBounds(m_levelBounds);
            m_targetList = characters;
            data = new EnemyData
            {
                Guid = enemyData.Guid,
                Name = enemyData.Name,
                Type = enemyData.Type,
                Health = enemyData.Health,
                Armour = enemyData.Armour,
                Speed = enemyData.Speed,
                Level = enemyData.Level,
                DetectionRadius = enemyData.DetectionRadius,
                AddressableName = enemyData.AddressableName
            };
            m_rigidbody2D = GetComponent<Rigidbody2D>();
            var speedCoefficient = Random.Range(1.0f, 1.2f);
            data.Speed *= speedCoefficient;
            m_healthBar = GetComponentInChildren<HealthBar>();
            var baseEnemyData = m_gm.Data.GetEnemyData(enemyData.Guid);
            m_healthBar.Init(baseEnemyData.Health, data.Health);
        }

        void Update()
        {
            if (data == null) return;

            if (m_gm.enemiesAlwaysPursueGoal)
            {
                m_targetTransform = m_targetList[0].transform;
            }
            else
            {
                MoveToTarget();
                if (m_targetTransform == null) MoveToPoint();
            }
            


            // var target = m_gm.GetNearestObject(m_targetList, m_rigidbody2D.position, data.DetectionRadius)?.transform;
            if (m_targetTransform != null)
            {
                m_direction = ((Vector2)m_targetTransform.position - m_rigidbody2D.position).normalized;
            }
            else
            {
                m_direction = (m_pointTarget - m_rigidbody2D.position).normalized;
            }

            Flip();
        }


        float m_targetSearchTime = 0.0f;

        void MoveToTarget()
        {
            if (m_targetTransform != null)
            {
                if (Vector2.Distance(m_rigidbody2D.position, m_targetTransform.position) >= data.DetectionRadius)
                {
                    m_targetTransform = null;
                    m_targetSearchTime = 0.0f;
                }
            }

            if (m_targetTransform == null && (m_targetSearchTime += Time.deltaTime) < 0.5f) return;
            m_targetSearchTime = 0.0f;
            m_targetTransform = m_gm.GetNearestObject(m_targetList, m_rigidbody2D.position, data.DetectionRadius)?.transform;
        }

        Vector2 m_lastTargetPosition;

        void MoveToPoint()
        {
            if (Mathf.Abs(Vector2.Distance(m_pointTarget, m_rigidbody2D.position)) < 0.5f)
            {
                m_pointTarget = m_gm.Spawner.GetRandomPointInBounds(m_levelBounds);
            }

            if (Mathf.Abs(Vector2.Distance(m_lastTargetPosition, m_rigidbody2D.position)) <= 0.01f)
            {
                m_pointTarget = m_gm.Spawner.GetRandomPointInBounds(m_levelBounds);
            }
        }

        void FixedUpdate()
        {
            if (data == null) return;
            m_lastTargetPosition = m_rigidbody2D.position;
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
                m_gm.Spawner.SpawnRandomItem(transform.position);
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