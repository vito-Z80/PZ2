using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
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
        readonly string m_savePath = Application.persistentDataPath + "/save.json";
        GameDataLibrary m_gameDataLibrary;

        public readonly Dictionary<Guid, GameObject> EnemyPrefabs = new();
        public readonly Dictionary<Guid, GameObject> CharacterPrefabs = new();
        public readonly Dictionary<Guid, GameObject> WeaponPrefabs = new();
        public readonly Dictionary<Guid, Sprite> ItemSprites = new();

        public GameObject ItemPrefab;


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
            {
                if (!EnemyPrefabs.ContainsKey(data.Guid))
                    EnemyPrefabs[data.Guid] = await LoadAsset<GameObject>(data.AddressableName);
            }

            foreach (var data in m_gameDataLibrary.CharacterData)
            {
                if (!CharacterPrefabs.ContainsKey(data.Guid))
                    CharacterPrefabs[data.Guid] = await LoadAsset<GameObject>(data.AddressableName);
            }

            foreach (var data in m_gameDataLibrary.WeaponData)
            {
                if (!WeaponPrefabs.ContainsKey(data.Guid))
                    WeaponPrefabs[data.Guid] = await LoadAsset<GameObject>(data.AddressableName);
            }

            foreach (var data in m_gameDataLibrary.ItemData)
            {
                if (!ItemSprites.ContainsKey(data.Guid))
                    ItemSprites[data.Guid] = await LoadAsset<Sprite>(data.AddressableName);
            }

            //  одиночки
            ItemPrefab ??= await LoadAsset<GameObject>("Item");
        }


        public Sprite GetItemSprite(Guid guid) => ItemSprites[guid] ?? throw new KeyNotFoundException($"Item sprite with Guid: {guid} was not found.");

        public EnemyData[] GetEnemiesData() => m_gameDataLibrary.EnemyData;

        public EnemyData GetEnemyData(Guid guid) => m_gameDataLibrary.EnemyData.FirstOrDefault(data => data.Guid == guid) ??
                                                    throw new KeyNotFoundException($"Enemy data with Guid {guid} doesn't exist in library.");

        public CharacterData[] GetCharactersData() => m_gameDataLibrary.CharacterData;

        public CharacterData GetCharacterData(Guid guid) => m_gameDataLibrary.CharacterData.FirstOrDefault(data => data.Guid == guid) ??
                                                            throw new KeyNotFoundException($"Character data with Guid {guid} doesn't exist in library.");

        public ItemData[] GetItemsData() => m_gameDataLibrary.ItemData;

        public ItemData GetItemData(Guid guid) => m_gameDataLibrary.ItemData.FirstOrDefault(data => data.Guid == guid) ??
                                                  throw new KeyNotFoundException($"Item data with Guid {guid} doesn't exist in library.");

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
            if (m_gameDataLibrary != null) return m_gameDataLibrary;
            var json = Resources.Load<TextAsset>("game_data").text; //  from server
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
                File.WriteAllText(m_savePath, JsonConvert.SerializeObject(saveData, Formatting.Indented));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}