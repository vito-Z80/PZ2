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
        readonly Guid m_baseCharacterGuid = new("75e2dc2b-5e8f-43cd-a638-9fa0baab741c");
        readonly Guid m_baseAmmoGuid = new("0e9c0b12-5a74-4dbf-a330-3bd252c81d1c");

        public int maxSpawnEnemies;
        public int killEnemiesForWin;

        [CanBeNull] SaveData m_saveData;
        public DataManager Data;
        public Spawner Spawner;
        Dictionary<string, Tilemap> m_levelLayers;

        Grid m_gridLevel;

        // public List<PlayerController> characters;
        public List<EnemyController> enemies;

        Button m_fireButton;
        GameObject m_retryGameMenu;
        GameObject m_continueGameMenu;
        PlayerController m_masterPlayerController;
        CameraTracker m_cameraTracker;

        public SaveData GetSaveData() => m_saveData;

        Coroutine m_enemySpawnerCoroutine;

        [HideInInspector] public bl_Joystick joystick;

        public static event Action<int> OnBulletsUpdate;

        void Awake()
        {
            PlatformPermission.CheckPermissions();
        }

        async void Start()
        {
            await Task.Yield();
            joystick ??= FindObjectOfType<bl_Joystick>();
            enemies ??= new List<EnemyController>(maxSpawnEnemies);
            enemies.Clear();
            // characters ??= new List<PlayerController>(1);
            await Init();
            await LevelInstance();
            await LoadGame();
            //  player spawn
            CharactersInstance();
            //  set target for camera
            m_cameraTracker ??= FindObjectOfType<CameraTracker>();
            m_cameraTracker.SetTarget(m_masterPlayerController.transform);
            InitButtons();
            await Task.Yield();
            m_gridLevel.gameObject.SetActive(true);
            //  enemy spawns
            m_enemySpawnerCoroutine = StartCoroutine(Spawner.EnemySpawner(m_masterPlayerController.transform));
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }

        void InitButtons()
        {
            var buttons = FindObjectsOfType<Button>();
            var gameMenu = FindObjectsOfType<PauseGame>(true);

            m_retryGameMenu ??= gameMenu.FirstOrDefault(b => b.name == "RetryGameMenu")?.gameObject ?? throw new NullReferenceException("Menu 'RetryGameMenu' not found");
            m_continueGameMenu ??= gameMenu.FirstOrDefault(b => b.name == "ContinueGameMenu")?.gameObject ?? throw new NullReferenceException("Menu 'ContinueGameMenu' not found");


            m_fireButton ??= buttons.FirstOrDefault(b => b.name == "FireButton") ?? throw new NullReferenceException("Button 'FireButton' not found");

            if (!m_fireButton.TryGetComponent<EventTrigger>(out var eventTrigger))
            {
                eventTrigger = m_fireButton.gameObject.AddComponent<EventTrigger>();
                var entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback.AddListener((_) => { m_masterPlayerController.Fire(); });
                eventTrigger.triggers.Add(entry);
            }
        }

        async Task LevelInstance()
        {
            var levelPrefab = await Data.LoadAsset<GameObject>("Level");
            m_gridLevel = Spawner.Spawn<Grid>(levelPrefab, Vector3.zero) ?? throw new NullReferenceException("Grid '" + levelPrefab.name + "' not found");
            m_gridLevel.gameObject.SetActive(false);
            m_levelLayers = m_gridLevel.GetComponentsInChildren<Tilemap>().ToDictionary(tMap => tMap.name, tMap => tMap);
        }

        void CharactersInstance()
        {
            if (m_saveData is null) throw new Exception("No saveData loaded");
            var spawnPosition = new Vector3(11, 11, 0);
            var spawnLayer = GetLayer("Objects").transform;
            foreach (var pair in m_saveData.Characters)
            {
                var prefab = Data.CharacterPrefabs[pair.Key];
                var characterData = pair.Value;
                m_masterPlayerController = Spawner.Spawn<PlayerController>(prefab, spawnPosition, spawnLayer);
                m_masterPlayerController.Init(characterData);
                m_masterPlayerController.EquipWeapon(characterData.EquippedWeaponGiud);
                if (pair.Value.IsMaster)
                {
                    var ammo = m_saveData.Items.First(p => p.Value.Type == "Ammo").Value?.Amount ?? 0;
                    OnBulletsUpdateDisplay(ammo);
                }
            }
        }


        public PlayerController GetMasterCharacter() => m_masterPlayerController;

        async Task Init()
        {
            Data ??= new DataManager();
            Spawner ??= new Spawner(this);
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
                m_saveData.AddItem(Data.GetItemData(baseCharacterData.EquippedWeaponGiud));
                m_saveData.AddItem(baseAmmoData);
                var saveResult = Data.SaveGame();
                if (!saveResult) throw new Exception("Failed to save game");
            }

            await Task.Yield();
        }


        public void Win()
        {
            m_continueGameMenu.SetActive(true);
            Data.SaveGame();
            Destroy();
            Start();
        }

        public void Lose()
        {
            m_retryGameMenu.SetActive(true);
            Destroy();
            Start();
        }

        void Destroy()
        {
            if (m_gridLevel)
                Destroy(m_gridLevel.gameObject);
            if (m_enemySpawnerCoroutine != null)
            {
                StopCoroutine(m_enemySpawnerCoroutine);
            }
        }

        [CanBeNull]
        public EnemyController GetNearestEnemy()
        {
            var distance = m_masterPlayerController.GetWeaponRadius();
            var targetId = -1;
            for (var id = 0; id < enemies.Count; id++)
            {
                var enemy = enemies[id];
                if (enemy is null || enemy.IsDestroyed()) continue;
                var distanceToTarget = Mathf.Abs(Vector2.Distance(enemy.transform.position, m_masterPlayerController.transform.position));
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

        public void OnBulletsUpdateDisplay(int count)
        {
            OnBulletsUpdate?.Invoke(count);
        }
    }
}