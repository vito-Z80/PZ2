using System;
using Data;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using Weapon;

//  нарезка коллайдеров в спрайтэдиторе
//  https://forum.unity.com/threads/the-problem-of-colliders-on-tilemap.1288397/

namespace Characters
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 5f;
        Rigidbody2D m_rb;
        [SerializeField] Vector2 m_direction;
        HealthBar m_healthBar;
        [SerializeField] CharacterData data;
        [SerializeField] Gun weapon;


        [SerializeField] bl_Joystick joystick;

        public void Init(CharacterData characterData)
        {
            joystick = FindObjectOfType<bl_Joystick>();
            data = new CharacterData
            {
                Guid = characterData.Guid,
                Name = characterData.Name,
                Type = characterData.Type,
                Health = characterData.Health,
                Armour = characterData.Armour,
                Speed = characterData.Speed,
                Level = characterData.Level
            };
            m_rb = GetComponent<Rigidbody2D>();
            m_healthBar = GetComponentInChildren<HealthBar>();
            m_healthBar.SetMaxHealth(data.Health);
        }


        void Update()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            m_direction.x = joystick.Horizontal;
            m_direction.y = joystick.Vertical;
#else
            m_direction.x = Input.GetAxis("Horizontal");
            m_direction.y = Input.GetAxis("Vertical");
#endif
        }


        void FixedUpdate()
        {
            m_rb.MovePosition(m_rb.position + m_direction * (moveSpeed * Time.fixedDeltaTime));
        }


        public void Damage(int value)
        {
            data.Health -= value;
            m_healthBar.UpdateHealth(-value);
            if (data.Health <= 0)
            {
                data.Health = 0;
                Dead();
            }
        }

        void Dead()
        {
            if (gameObject)
                Destroy(gameObject);
        }
    }
}