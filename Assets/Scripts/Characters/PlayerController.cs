using System;
using System.Collections.Generic;
using Core;
using Data;
using UI;
using UnityEngine;
using Weapon;

namespace Characters
{
    public class PlayerController : MainCharacter
    {
        HealthBar m_healthBar;
        [SerializeField] Transform m_weaponPoint;
        [SerializeField] Transform m_weaponRadiusSprite;
        [SerializeField] CharacterData data;
        [SerializeField] Gun equippedWeapon;
        [SerializeField] bl_Joystick joystick;
        List<Guid> m_availableWeapons = new List<Guid>() { new Guid("7374e364-caab-458b-aa6b-525108dcd02c") };


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
            m_healthBar = GetComponentInChildren<HealthBar>();
            m_healthBar.SetMaxHealth(data.Health);
            EquipWeapon(m_availableWeapons[0]);
        }


        void EquipWeapon(Guid weaponGuid)
        {
            equippedWeapon = GameManager.I.Spawner.SpawnWeapon<Gun>(weaponGuid, m_weaponPoint.position, m_weaponPoint);
            equippedWeapon.Init(GameManager.I.Data.GetWeaponData(weaponGuid));
            var weaponRadiusSprite = equippedWeapon.GetDamageRadius() - 1;
            m_weaponRadiusSprite.localScale = new Vector3(weaponRadiusSprite, weaponRadiusSprite, weaponRadiusSprite);
        }

        void Update()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            direction.x = joystick.Horizontal;
            direction.y = joystick.Vertical;
#else
            direction.x = Input.GetAxis("Horizontal");
            direction.y = Input.GetAxis("Vertical");
#endif
        }

        void FixedUpdate()
        {
            Rigidbody.MovePosition(Rigidbody.position + direction * (data.Speed * Time.fixedDeltaTime));
        }


        public float GetWeaponRadius()
        {
            return equippedWeapon.GetDamageRadius();
        }

        public override void Damage(int value)
        {
            data.Health -= value;
            m_healthBar.UpdateHealth(-value);
            if (data.Health <= 0)
            {
                data.Health = 0;
                Dead();
            }
        }

        public override void AddHealth(int health)
        {
            data.Health += health;
        }

        public override void Fire()
        {
            if (equippedWeapon)
            {
                var target = GameManager.I.GetNearestEnemy();
                equippedWeapon.Fire(target, 1.0f);
            }
        }
    }
}