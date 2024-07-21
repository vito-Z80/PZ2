using System.Collections;
using System.Collections.Generic;
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
        int m_maxEnemiesToWin = 20;
        const int MaxSpawnCount = 3;
        readonly List<EnemyController> m_spawnedEnemies;
        GameManager m_gm;

        public Spawner(GameManager gameManager)
        {
            m_gm = gameManager;
            m_spawnedEnemies = m_gm.enemies;
        }

        public bool CanSpawn()
        {
            m_spawnedEnemies.RemoveAll(o => o.IsDestroyed());
            return m_spawnedEnemies.Count < MaxSpawnCount;
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
            var rndId = Random.Range(0, m_gm.Data.GetItems().Length);
            var itemData = m_gm.Data.GetItems()[rndId];
            var sprite = m_gm.Data.ItemSprites[itemData.ImageName];
            var item = Spawn<Item>(m_gm.Data.ItemPrefab, position, m_gm.GetLayer("Objects").transform);
            item.Init(itemData);
        }


        public IEnumerator EnemySpawner(PlayerController playerController)
        {
            var floor = m_gm.GetLayer("Floor");
            var objectsLayer = m_gm.GetLayer("Objects");
            m_gm.characters.Add(playerController);
            var bounds = floor.cellBounds;
            var offset = Mathf.CeilToInt(floor.GetComponent<CompositeCollider2D>().edgeRadius);
            bounds.xMin += offset;
            bounds.yMin += offset;
            bounds.size = new Vector3Int(bounds.size.x - offset / 2, bounds.size.y - offset / 2, bounds.size.z);
            while (m_maxEnemiesToWin > 0)
            {
                if (CanSpawn())
                {
                    var randomEnemyDataId = Random.Range(0, m_gm.Data.GetEnemies().Length);
                    var data = m_gm.Data.GetEnemies()[randomEnemyDataId];
                    var prefab = m_gm.Data.EnemyPrefabs[data.Name];
                    var enemy = SpawnInBoundsRandomly<EnemyController>(prefab, bounds, objectsLayer.transform);
                    m_gm.enemies.Add(enemy);
                    enemy.Init(data);
                    m_maxEnemiesToWin--;
                }

                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}