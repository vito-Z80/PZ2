using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Characters;
using Data;
using Items;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Button = UnityEngine.UI.Button;
using Utils;

namespace Core
{
    public class GameManager : GlobalSingleton<GameManager>
    {
        [CanBeNull] SaveData m_saveData;
        public DataManager Data;
        public Spawner Spawner;
        Dictionary<string, Tilemap> m_levelLayers;
        [SerializeField] Grid gridLevel;
        public List<PlayerController> characters = new(1);
        public List<EnemyController> enemies = new(3);

        public Inventory Inventory = new();
        
        Button m_fireButton;
        PlayerController m_playerController;

        CameraTracker m_cameraTracker;

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

            m_fireButton = GameObject.Find("FireButton").GetComponent<Button>();
            m_fireButton.onClick.AddListener(m_playerController.Fire);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        public SaveData GetSaveData() => m_saveData;

        void CharactersInstance()
        {
            if (m_saveData is null) throw new Exception("No saveData loaded");
            var spawnPosition = new Vector3(10, 10, 0);
            var spawnLayer = GetLayer("Objects").transform;
            foreach (var guid in m_saveData.Characters)
            {
                var prefab = Data.CharacterPrefabs[guid];
                var data = Data.GetCharacterData(guid);
                m_playerController = Spawner.Spawn<PlayerController>(prefab, spawnPosition, spawnLayer);
                m_playerController.Init(data);
                m_playerController.EquipWeapon(m_saveData.Items[0].Guid); //  TODO get equipped weapon from available
            }
        }

        async Task Init()
        {
            PlatformPermission.CheckPermissions();
            m_levelLayers = FindObjectOfType<Grid>().GetComponentsInChildren<Tilemap>().ToDictionary(tMap => tMap.name, tMap => tMap);
            Debug.Log(m_levelLayers.Count);
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
                m_saveData.Characters.Add(new Guid("75e2dc2b-5e8f-43cd-a638-9fa0baab741c"));
                var itemSaveData = new ItemSaveData
                {
                    Guid = new Guid("7374e364-caab-458b-aa6b-525108dcd02c"),
                    Amount = 1,
                    IsEquipped = true,
                    IsSelected = false,
                };
                m_saveData.Items.Add(itemSaveData);
                var saveResult = Data.SaveGame();
                if (!saveResult) throw new Exception("Failed to save game");
            }

            await Task.Yield();
        }

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