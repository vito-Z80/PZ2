using System;
using System.Linq;
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

        ItemData m_itemDataAmmo;

        public void Init(CharacterData characterData)
        {
            joystick = FindObjectOfType<bl_Joystick>();
            data = characterData;
            m_healthBar = GetComponentInChildren<HealthBar>();
            m_healthBar.SetMaxHealth(data.Health);
        }


        public void EquipWeapon(Guid weaponGuid)
        {
            if (equippedWeapon != null)
            {
                if (equippedWeapon.GetData().Guid == weaponGuid) return;
                DestroyImmediate(equippedWeapon.gameObject);
            }

            if (equippedWeapon == null)
            {
                data.EquippedWeaponGiud = weaponGuid;
                equippedWeapon = GameManager.I.Spawner.SpawnWeapon<Gun>(weaponGuid, m_weaponPoint.position, m_weaponPoint);
                equippedWeapon.Init(GameManager.I.Data.GetWeaponData(weaponGuid));
                var weaponRadiusSprite = equippedWeapon.GetDamageRadius() - 1;
                m_weaponRadiusSprite.localScale = new Vector3(weaponRadiusSprite, weaponRadiusSprite, weaponRadiusSprite);
            }
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
            if (m_itemDataAmmo == null)
            {
                m_itemDataAmmo = GameManager.I.GetSaveData().Items.FirstOrDefault(pair => pair.Value.Type == "Ammo").Value; // shit
                if (m_itemDataAmmo == null)
                {
                    Debug.Log("No ammo.");
                    return;
                }
            }


            if (equippedWeapon)
            {
                if (m_itemDataAmmo.Amount < 0)
                {
                    Debug.Log("No ammo");
                    GameManager.I.GetSaveData().RemoveItem(m_itemDataAmmo);
                    return;
                }

                var target = GameManager.I.GetNearestEnemy();
                if (target == null) return;
                m_itemDataAmmo.Amount--;
                equippedWeapon.Fire(target, 1.0f);
            }
        }
    }
}