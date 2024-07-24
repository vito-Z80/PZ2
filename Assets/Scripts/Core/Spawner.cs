using System;
using System.Collections;
using Characters;
using Items;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Core
{
    public class Spawner
    {
        public static event Action<int> OnEnemyDead;
        readonly GameManager m_gm;

        public Spawner(GameManager gameManager)
        {
            m_gm = gameManager;
        }

        public T SpawnWeapon<T>(Guid weaponGuid, Vector3 position, Transform parent = null)
        {
            var data = m_gm.Data.GetWeaponData(weaponGuid);
            var prefab = m_gm.Data.WeaponPrefabs[data.Guid];
            return Object.Instantiate(prefab, position, Quaternion.identity, parent).GetComponent<T>();
        }

        public T Spawn<T>(GameObject prefab, Vector3 position, Transform parent = null)
        {
            return Object.Instantiate(prefab, position, Quaternion.identity, parent).GetComponent<T>();
        }

        public T SpawnInBoundsRandomly<T>(GameObject prefab, BoundsInt bounds, Transform parent = null) where T : Component
        {
            var x = Random.Range(bounds.xMin, bounds.size.x + bounds.xMin);
            var y = Random.Range(bounds.yMin, bounds.size.y + bounds.yMin);
            var position = new Vector3(x, y, 0);
            var instance = Spawn<T>(prefab, position, parent);
            return instance;
        }


        public void SpawnRandomItem(Vector3 position)
        {
            var rndId = Random.Range(0, m_gm.Data.GetItemsData().Length);
            var itemData = m_gm.Data.GetItemsData()[rndId];
            var item = Spawn<Item>(m_gm.Data.ItemPrefab, position, m_gm.GetLayer("Objects").transform);
            item.Init(itemData);
        }

        public IEnumerator EnemySpawner(Transform target)
        {
            m_gm.enemies.Clear();
            var floor = m_gm.GetLayer("Floor");
            var objectsLayer = m_gm.GetLayer("Objects");
            var bounds = floor.cellBounds;
            var offset = Mathf.CeilToInt(floor.GetComponent<CompositeCollider2D>().edgeRadius);
            bounds.xMin += offset;
            bounds.yMin += offset;
            bounds.size = new Vector3Int(bounds.size.x - offset / 2, bounds.size.y - offset / 2, bounds.size.z);
            var killEnemiesForWin = m_gm.killEnemiesForWin;
            while (killEnemiesForWin > 0)
            {
                if (CanSpawn(m_gm.maxSpawnEnemies, ref killEnemiesForWin))
                {
                    var randomEnemyDataId = Random.Range(0, m_gm.Data.GetEnemiesData().Length);
                    var data = m_gm.Data.GetEnemiesData()[randomEnemyDataId];
                    var prefab = m_gm.Data.EnemyPrefabs[data.Guid];
                    var enemy = SpawnInBoundsRandomly<EnemyController>(prefab, bounds, objectsLayer.transform);
                    m_gm.enemies.Add(enemy);
                    enemy.Init(data, target.transform);
                }

                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(2.0f);
            m_gm.Win();
        }

        bool CanSpawn(int maxSpawnEnemies, ref int killEnemiesForWin)
        {
            killEnemiesForWin -= m_gm.enemies.RemoveAll(o => o.IsDestroyed());
            UpdateEnemiesLeft(killEnemiesForWin);
            return m_gm.enemies.Count < maxSpawnEnemies;
        }

        static void UpdateEnemiesLeft(int count)
        {
            OnEnemyDead?.Invoke(count);
        }
    }
}