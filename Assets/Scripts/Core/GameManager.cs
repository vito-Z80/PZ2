using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Characters;
using Data;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Utils;
using Button = UnityEngine.UI.Button;

namespace Core
{
    public class GameManager : GlobalSingleton<GameManager>
    {
        readonly Guid m_baseCharacterGuid = new Guid("75e2dc2b-5e8f-43cd-a638-9fa0baab741c");
        readonly Guid m_baseAmmoGuid = new Guid("0e9c0b12-5a74-4dbf-a330-3bd252c81d1c");

        [CanBeNull] SaveData m_saveData;
        public DataManager Data;
        public Spawner Spawner;
        Dictionary<string, Tilemap> m_levelLayers;
        [SerializeField] Grid gridLevel;
        public List<PlayerController> characters = new(1);
        public List<EnemyController> enemies = new(3);

        Button m_fireButton;
        PlayerController m_playerController;

        CameraTracker m_cameraTracker;

        public SaveData GetSaveData() => m_saveData;

        async void Start()
        {
            await Init();
            await LoadGame();
            //  player spawn
            CharactersInstance();
            //  set target for camera
            m_cameraTracker = FindObjectOfType<CameraTracker>();
            m_cameraTracker.SetTarget(m_playerController.transform);
            //  enemy spawns
            StartCoroutine(Spawner.EnemySpawner(m_playerController));
            FireButton();
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        void FireButton()
        {
            m_fireButton = GameObject.Find("FireButton").GetComponent<Button>();
            var eventTrigger = m_fireButton.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((_) => { m_playerController.Fire(); });
            eventTrigger.triggers.Add(entry);
        }


        void CharactersInstance()
        {
            if (m_saveData is null) throw new Exception("No saveData loaded");
            var spawnPosition = new Vector3(10, 10, 0);
            var spawnLayer = GetLayer("Objects").transform;
            foreach (var guid in m_saveData.Characters)
            {
                var prefab = Data.CharacterPrefabs[guid.Key];
                var characterData = Data.GetCharacterData(guid.Key);
                m_playerController = Spawner.Spawn<PlayerController>(prefab, spawnPosition, spawnLayer);
                m_playerController.Init(characterData);
                Debug.Log(characterData.EquippedWeaponGiud);
                m_playerController.EquipWeapon(characterData.EquippedWeaponGiud); //  TODO get equipped weapon from available
            }
        }


        public PlayerController GetMasterCharacter() => m_playerController;
        
        async Task Init()
        {
            PlatformPermission.CheckPermissions();
            m_levelLayers = FindObjectOfType<Grid>().GetComponentsInChildren<Tilemap>().ToDictionary(tMap => tMap.name, tMap => tMap);
            Data = new DataManager();
            Spawner = new Spawner(this);
            await Data.Init();
        }


        async Task LoadGame()
        {
            m_saveData = Data.LoadGame();
            if (m_saveData == null)
            {
                m_saveData = new SaveData();
                var baseCharacterData = Data.GetCharacterData(m_baseCharacterGuid);
                var baseAmmoData = Data.GetItemData(m_baseAmmoGuid);
                m_saveData.AddCharacter(baseCharacterData);
                //  TODO при запуске игры у перса автомат, даже если его нет и был выбран пестик в предыдущей сессии.
                //  TODO данные игрока не сохраняются. Напутано с данными... 
                // m_saveData.AddWeapon(baseWeaponData);
                m_saveData.AddItem(Data.GetItemData(baseCharacterData.EquippedWeaponGiud));
                m_saveData.AddItem(baseAmmoData);

                // var characterData = new CharacterData
                // {
                //     Guid = baseCharacterData.Guid,
                //     Name = baseCharacterData.Name,
                //     Type = baseCharacterData.Type,
                //     Health = baseCharacterData.Health,
                //     Armour = baseCharacterData.Armour,
                //     Speed = baseCharacterData.Speed,
                //     Level = baseCharacterData.Level,
                //     EquippedWeaponGiud = baseCharacterData.EquippedWeaponGiud
                // };
                // m_saveData.Characters[m_baseCharacterGuid] = characterData;
                // m_saveData.Weapons[characterData.EquippedWeaponGiud] = Data.GetWeaponData(characterData.EquippedWeaponGiud);
                // m_saveData.Items[characterData.EquippedWeaponGiud] = Data.GetItemData(characterData.EquippedWeaponGiud);
                // m_saveData.Items[m_baseAmmoGuid] = Data.GetItemData(m_baseAmmoGuid);
                var saveResult = Data.SaveGame();
                if (!saveResult) throw new Exception("Failed to save game");
            }

            await Task.Yield();
        }

        [CanBeNull]
        public EnemyController GetNearestEnemy()
        {
            var distance = m_playerController.GetWeaponRadius();
            var targetId = -1;
            for (var id = 0; id < enemies.Count; id++)
            {
                var enemy = enemies[id];
                if (enemy is null || enemy.IsDestroyed()) continue;
                var distanceToTarget = Mathf.Abs(Vector2.Distance(enemy.transform.position, m_playerController.transform.position));
                if (distanceToTarget < distance)
                {
                    distance = distanceToTarget;
                    targetId = id;
                }
            }

            return targetId >= 0 ? enemies[targetId] : null;
        }


        //  LEVEL
        public Tilemap GetLayer(string layerName) => m_levelLayers[layerName];


        void OnDisable()
        {
            m_fireButton.onClick.RemoveAllListeners();
        }
    }
}