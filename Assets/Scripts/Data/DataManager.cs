using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Items;
using JetBrains.Annotations;
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
        string m_savePath;
        GameDataLibrary m_gameDataLibrary;

        public readonly Dictionary<Guid, GameObject> EnemyPrefabs = new();
        public readonly Dictionary<Guid, GameObject> CharacterPrefabs = new();
        public readonly Dictionary<Guid, GameObject> WeaponPrefabs = new();
        public readonly Dictionary<Guid, Sprite> ItemSprites = new();

        public GameObject ItemPrefab;

        public DataManager()
        {
            m_savePath = Application.persistentDataPath + "/save.json";
            Debug.Log(m_savePath);
        }


        // async void Start()
        // {
        //     // m_jsonPath = Application.persistentDataPath + "game_data.json";
        //     m_gameDataLibrary = LoadGameData() ?? throw new NullReferenceException("Failed to load game data.");
        //     await Init();
        // }


        public async Task Init()
        {
            m_gameDataLibrary = LoadLibraryData() ?? throw new NullReferenceException("Failed to load game data.");
            foreach (var data in m_gameDataLibrary.EnemyData)
                EnemyPrefabs[data.Guid] = await LoadAsset<GameObject>(data.Name);
            foreach (var data in m_gameDataLibrary.CharacterData)
                CharacterPrefabs[data.Guid] = await LoadAsset<GameObject>(data.Name);
            foreach (var data in m_gameDataLibrary.WeaponData)
                WeaponPrefabs[data.Guid] = await LoadAsset<GameObject>(data.Name);
            foreach (var data in m_gameDataLibrary.ItemData)
                ItemSprites[data.Guid] = await LoadAsset<Sprite>(data.ImageName);
            //  одиночки
            ItemPrefab = await LoadAsset<GameObject>("Item");
        }

        public EnemyData[] GetEnemiesData() => m_gameDataLibrary.EnemyData;
        public CharacterData[] GetCharactersData() => m_gameDataLibrary.CharacterData;

        public CharacterData GetCharacterData(Guid guid) => m_gameDataLibrary.CharacterData.FirstOrDefault(data => data.Guid == guid) ??
                                                             throw new KeyNotFoundException($"$Character data with Guid {guid} doesn't exist in library.");

        public ItemData[] GetItemsData() => m_gameDataLibrary.ItemData;
        public ItemData GetItemData(Guid guid) => m_gameDataLibrary.ItemData.FirstOrDefault(data => data.Guid == guid) ??
                                                   throw new KeyNotFoundException($"$Item data with Guid {guid} doesn't exist in library.");
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

        GameDataLibrary LoadLibraryData()
        {
            var json = Resources.Load<TextAsset>("game_data").text;
            var gameData = JsonConvert.DeserializeObject<GameDataLibrary>(json);
            return gameData;
        }

        [CanBeNull]
        public SaveData LoadGame()
        {
            if (File.Exists(m_savePath))
            {
                var json = File.ReadAllText(m_savePath);
                return JsonConvert.DeserializeObject<SaveData>(json);
            }

            // remote first time load data
            return null;
        }

        public bool SaveGame()
        {
            var saveData = GameManager.I.GetSaveData();
            try
            {
                File.WriteAllText(m_savePath, JsonConvert.SerializeObject(saveData));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        void OnDestroy()
        {
        }
    }
}