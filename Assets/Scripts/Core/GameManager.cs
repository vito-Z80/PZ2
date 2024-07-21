using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Characters;
using Data;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Core
{
    public class GameManager : GlobalSingleton<GameManager>
    {
        public DataManager Data;
        public Spawner Spawner;
        [SerializeField] Grid gridLevel;
        Dictionary<string, Tilemap> m_levelLayers;
        public List<PlayerController> characters = new(1);
        public List<EnemyController> enemies = new(3);

        
        CameraTracker m_cameraTracker;
        
        async void Start()
        {
            await Init();

            //  player spawn
            var playerId = 0;
            var playerSpawnPosition = new Vector3(10, 10, 0);
            var playerData = Data.GetCharacters()[playerId];
            var playerPrefab = Data.CharacterPrefabs[playerData.Name];
            var player = Spawner.Spawn<PlayerController>(playerPrefab, playerSpawnPosition, GetLayer("Objects").transform);
            player.Init(playerData);
            //  set target for camera
            m_cameraTracker = Camera.main.GetComponent<CameraTracker>();
            m_cameraTracker.SetTarget(player.transform);
            //  enemy spawns
            StartCoroutine(Spawner.EnemySpawner(player));
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
        }


        async Task Init()
        {
            PlatformPermission.CheckPermissions();
            m_levelLayers = GameObject.Find("Level").GetComponentsInChildren<Tilemap>().ToDictionary(tMap => tMap.name, tMap => tMap);
            Data = new DataManager();
            Spawner = new Spawner(this);
            await Data.Init();
        }

        //  LEVEL
        public Tilemap GetLayer(string layerName) => m_levelLayers[layerName];
    }
}