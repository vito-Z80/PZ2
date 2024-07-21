using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Characters;
using Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Button = UnityEngine.UI.Button;
using Utils;

namespace Core
{
    public class GameManager : GlobalSingleton<GameManager>
    {
        public DataManager Data;
        public Spawner Spawner;
        Dictionary<string, Tilemap> m_levelLayers;
        [SerializeField] Grid gridLevel;
        public List<PlayerController> characters = new(1);
        public List<EnemyController> enemies = new(3);

        Button m_fireButton;
        PlayerController m_playerController;

        CameraTracker m_cameraTracker;

        async void Start()
        {
            await Init();

            //  player spawn
            var playerId = 0;
            var playerSpawnPosition = new Vector3(10, 10, 0);
            var playerData = Data.GetCharactersData()[playerId];
            var playerPrefab = Data.CharacterPrefabs[playerData.Name];
            m_playerController = Spawner.Spawn<PlayerController>(playerPrefab, playerSpawnPosition, GetLayer("Objects").transform);
            m_playerController.Init(playerData);
            //  set target for camera
            m_cameraTracker = Camera.main.GetComponent<CameraTracker>();
            m_cameraTracker.SetTarget(m_playerController.transform);
            //  enemy spawns
            StartCoroutine(Spawner.EnemySpawner(m_playerController));

            m_fireButton = GameObject.Find("FireButton").GetComponent<Button>();
            m_fireButton.onClick.AddListener(m_playerController.Fire);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
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