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
        [SerializeField] Transform weaponPoint;
        [SerializeField] Transform m_weaponRadiusSprite;
        [SerializeField] CharacterData data;
        [SerializeField] Gun equippedWeapon;

        GameManager m_gm;
        ItemData m_itemDataAmmo;

        public void Init(CharacterData characterData)
        {
            m_gm = GameManager.I;
            data = characterData;
            var baseCharacterData = m_gm.Data.GetCharacterData(characterData.Guid);
            m_healthBar = GetComponentInChildren<HealthBar>();
            m_healthBar.Init(baseCharacterData.Health, characterData.Health);
        }


        public void EquipWeapon(Guid weaponGuid)
        {
            if (equippedWeapon != null)
            {
                if (equippedWeapon.GetData().Guid == weaponGuid) return;
                foreach (Transform wp in weaponPoint)
                {
                    Destroy(wp.gameObject);
                }
            }

            data.EquippedWeaponGiud = weaponGuid;
            equippedWeapon = m_gm.Spawner.SpawnWeapon<Gun>(weaponGuid, weaponPoint.position, weaponPoint);
            equippedWeapon.Init(GameManager.I.Data.GetWeaponData(weaponGuid));
            var weaponRadiusSprite = equippedWeapon.GetDamageRadius() - 1;
            m_weaponRadiusSprite.localScale = new Vector3(weaponRadiusSprite, weaponRadiusSprite, weaponRadiusSprite);
        }

        void Update()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            direction.x = m_gm.joystick.Horizontal;
            direction.y = m_gm.joystick.Vertical;
#else
            direction.x = Input.GetAxis("Horizontal");
            direction.y = Input.GetAxis("Vertical");
#endif

            if (Time.timeScale == 0.0f) direction = Vector2.zero;
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
            m_healthBar.UpdateHealth(data.Health);
            if (data.Health <= 0)
            {
                data.Health = 0;
                Dead();
                m_gm.Lose();
            }
        }

        public override void AddHealth(int health)
        {
            data.Health += health;
        }

        public override void Fire()
        {
            m_itemDataAmmo ??= GameManager.I.GetSaveData().Items.FirstOrDefault(pair => pair.Value.Type == "Ammo").Value; // shit
            if (m_itemDataAmmo == null)
            {
                Debug.Log("No ammo.");
            }
            else
            {
                if (equippedWeapon)
                {
                    if (m_itemDataAmmo.Amount > 1)
                    {
                        var target = m_gm.GetNearestEnemy();
                        if (target != null)
                        {
                            m_itemDataAmmo.Amount--;
                            m_gm.OnBulletsUpdateDisplay(m_itemDataAmmo.Amount);
                            equippedWeapon.Fire(target, 1.0f);
                        }
                    }
                    else
                    {
                        m_gm.GetSaveData().RemoveItem(m_itemDataAmmo);
                        m_itemDataAmmo = null;
                    }
                }
            }
        }
    }
}