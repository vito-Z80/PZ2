using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Items;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


//  https://github.com/applejag/Newtonsoft.Json-for-Unity/wiki/Install-official-via-UPM

namespace Data
{
    public class DataManager
    {
        string m_jsonPath = "Assets/Resources/game_data.json";
        GameDataLibrary m_gameDataLibrary;

        public readonly Dictionary<string, GameObject> EnemyPrefabs = new();
        public readonly Dictionary<string, GameObject> CharacterPrefabs = new();
        public readonly Dictionary<string, GameObject> WeaponPrefabs = new();
        public readonly Dictionary<string, Sprite> ItemSprites = new();

        public GameObject ItemPrefab;


        // async void Start()
        // {
        //     // m_jsonPath = Application.persistentDataPath + "game_data.json";
        //     m_gameDataLibrary = LoadGameData() ?? throw new NullReferenceException("Failed to load game data.");
        //     await Init();
        // }


        public async Task Init()
        {
            m_gameDataLibrary = LoadGameData() ?? throw new NullReferenceException("Failed to load game data.");
            foreach (var data in m_gameDataLibrary.EnemyData)
                EnemyPrefabs[data.Name] = await LoadAsset<GameObject>(data.Name);
            foreach (var data in m_gameDataLibrary.CharacterData)
                CharacterPrefabs[data.Name] = await LoadAsset<GameObject>(data.Name);
            foreach (var data in m_gameDataLibrary.WeaponData)
                WeaponPrefabs[data.Name] = await LoadAsset<GameObject>(data.Name);
            foreach (var data in m_gameDataLibrary.ItemData)
                ItemSprites[data.Name] = await LoadAsset<Sprite>(data.ImageName);
            //  одиночки
            ItemPrefab = await LoadAsset<GameObject>("Item");
        }

        public EnemyData[] GetEnemiesData() => m_gameDataLibrary.EnemyData;
        public CharacterData[] GetCharactersData() => m_gameDataLibrary.CharacterData;
        public ItemData[] GetItemsData() => m_gameDataLibrary.ItemData;
        public WeaponData[] GetWeaponsData() => m_gameDataLibrary.WeaponData;

        public WeaponData GetWeaponData(Guid weaponGuid)
        {
            return m_gameDataLibrary.WeaponData.FirstOrDefault(weaponData => weaponData.Guid == weaponGuid)
                   ?? throw new Exception($"Not find weapon with GUID: {weaponGuid} on weapon library");
        }


        public async Task<GameObject[]> LoadAssets(string[] assetNames)
        {
            var tasks = new List<AsyncOperationHandle>();

            foreach (var assetName in assetNames)
            {
                var task = Addressables.LoadAssetAsync<GameObject>(assetName);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks.Select(aoh => aoh.Task));
            return (GameObject[])tasks.Select(task => task.Result);
        }


        public async Task<T> LoadAsset<T>(string assetName)
        {
            var handle = Addressables.LoadAssetAsync<T>(assetName);
            await handle.Task;
            return handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : default;
        }

        //  for start save json
        // public void SaveGameData(GameDataLibrary gameDataLibrary)
        // {
        //     var json = JsonConvert.SerializeObject(gameDataLibrary);
        //     System.IO.File.WriteAllText(m_jsonPath, json);
        // }

        GameDataLibrary LoadGameData()
        {
            var json = Resources.Load<TextAsset>("game_data").text;
            var gameData = JsonConvert.DeserializeObject<GameDataLibrary>(json);
            return gameData;
        }

        void OnDestroy()
        {
        }
    }
}