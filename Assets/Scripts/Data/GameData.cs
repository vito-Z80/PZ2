using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Data
{
    public class GameData
    {
        readonly string m_savePath = Application.persistentDataPath + "/save.json";
        const string DefaultDataFileName = "game_data";
        UserData m_userData;
        DefaultData m_defaultData;
        GamePrefabs m_gamePrefabs;

        void Init()
        {
            m_defaultData ??= LoadDefaultData();
            if (m_gamePrefabs == null)
            {
                m_gamePrefabs = new GamePrefabs();
                m_gamePrefabs.Init(m_defaultData);
            }

            m_userData = LoadGame() ?? FirstRunData();
        }

        public CharacterData GetDefaultCharacterData(Guid guid) => m_defaultData.CharacterData.FirstOrDefault(data => data.Guid == guid);
        public EnemyData GetDefaultEnemyData(Guid guid) => m_defaultData.EnemyData.FirstOrDefault(data => data.Guid == guid);
        public WeaponData GetDefaultWeaponData(Guid guid) => m_defaultData.WeaponData.FirstOrDefault(data => data.Guid == guid);
        public ItemData GetDefaultItemData(Guid guid) => m_defaultData.ItemData.FirstOrDefault(data => data.Guid == guid);

        public CharacterData GetUserCharacterData(Guid guid) => m_userData.Characters[guid];
        public ItemData GetUserItemData(Guid guid) => m_userData.Items[guid];
        public WeaponData GetUserWeaponData(Guid guid) => m_userData.Weapons[guid];

        public GameObject GetCharacterPrefab(Guid guid) => m_gamePrefabs.CharacterPrefabs[guid];
        public GameObject GetEnemyPrefab(Guid guid) => m_gamePrefabs.EnemyPrefabs[guid];
        public GameObject GetWeaponPrefab(Guid guid) => m_gamePrefabs.WeaponPrefabs[guid];
        public Sprite GetItemSprite(Guid guid) => m_gamePrefabs.ItemSprites[guid];


        UserData FirstRunData()
        {
            m_userData = new UserData();
            var characterData = m_userData.Characters[m_defaultData.CharacterData[0].Guid] = m_defaultData.CharacterData[0];
            var weaponData = m_userData.Weapons[characterData.EquippedWeaponGiud] = m_defaultData.WeaponData.FirstOrDefault(d => d.Guid == characterData.EquippedWeaponGiud)!;
            m_userData.Items[weaponData.Guid] = m_defaultData.ItemData.FirstOrDefault(d => d.Guid == weaponData.Guid)!;
            var ammoData = m_defaultData.ItemData.FirstOrDefault(d => d.Type == "Ammo")!;
            m_userData.Items[ammoData.Guid] = ammoData;
            var sg = SaveGame();
            if (!sg) throw new Exception("Failed to save game");
            return m_userData;
        }


        [CanBeNull]
        UserData LoadGame()
        {
            if (File.Exists(m_savePath))
            {
                var json = File.ReadAllText(m_savePath);
                return JsonConvert.DeserializeObject<UserData>(json);
            }

            return null;
        }

        bool SaveGame()
        {
            try
            {
                File.WriteAllText(m_savePath, JsonConvert.SerializeObject(m_userData, Formatting.Indented));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        DefaultData LoadDefaultData()
        {
            var json = Resources.Load<TextAsset>(DefaultDataFileName).text; //  from server
            var gameData = JsonConvert.DeserializeObject<DefaultData>(json);
            return gameData;
        }
    }


    class GamePrefabs
    {
        public readonly Dictionary<Guid, GameObject> EnemyPrefabs = new();
        public readonly Dictionary<Guid, GameObject> CharacterPrefabs = new();
        public readonly Dictionary<Guid, GameObject> WeaponPrefabs = new();
        public readonly Dictionary<Guid, Sprite> ItemSprites = new();

        public GameObject ItemPrefab;

        public void Init(DefaultData data)
        {
            GetPrefabs(CharacterPrefabs, data.CharacterData);
            GetPrefabs(EnemyPrefabs, data.EnemyData);
            GetPrefabs(WeaponPrefabs, data.WeaponData);
            GetSprites(ItemSprites, data.ItemData);
        }

        async void GetPrefabs<T>(Dictionary<Guid, T> dict, MainGameData[] data)
        {
            foreach (var d in data)
            {
                dict[d.Guid] = await LoadAsset<T>(d.Name);
            }
        }

        async void GetSprites<T>(Dictionary<Guid, T> dict, ItemData[] data)
        {
            foreach (var d in data)
            {
                dict[d.Guid] = await LoadAsset<T>(d.AddressableName);
            }
        }

        async Task<T> LoadAsset<T>(string assetName)
        {
            var handle = Addressables.LoadAssetAsync<T>(assetName);
            await handle.Task;
            return handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : default;
        }
    }

    class UserData
    {
        public Dictionary<Guid, CharacterData> Characters = new();
        public Dictionary<Guid, ItemData> Items = new();
        public Dictionary<Guid, WeaponData> Weapons = new();

        //  add and remove methods
    }

    class DefaultData
    {
        public WeaponData[] WeaponData;
        public CharacterData[] CharacterData;
        public EnemyData[] EnemyData;
        public ItemData[] ItemData;
        
        //  split items to subtypes.
        //  add dictionaries for all arrays.
    }
}