using System;
using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using VP.Nest.UI.InGame;
using Random = UnityEngine.Random;

namespace _Main.Scripts.Managers
{
    public class EnemyManager : MonoBehaviour
    {
        [Header("General Components")] [SerializeField]
        private InGameUI inGameUI;

        [SerializeField] private TextMeshProUGUI stageEnemyCountText;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform maxAnchor;
        [SerializeField] private Transform minAnchor;

        [Header("Enemy Spawn Settings")] [SerializeField]
        private int waveSize;

        [SerializeField] private int waveSizeIncrement;
        [SerializeField] private float spawnRate;
        [SerializeField] private int stageMaxEnemySize;
        [SerializeField] private float enemyBaseSpeed;
        [SerializeField] private float enemySpeedIncrement;
        [SerializeField] private int enemyHealth;

        public static List<Enemy> Enemies = new List<Enemy>();
        public int KillCountPerTurn;

        Coroutine spawnEnemies;

        bool canSpawn = true;
        int stageCurrentEnemySize;
        int totalSpawnedEnemy;


        void Start()
        {
            Debug.Log(PlayerPrefKeys.CurrentLevel);
            if (PlayerPrefKeys.CurrentLevel <= RemoteConfigManager.TotalLevelCount)
            {
                waveSize = RemoteConfigManager.EnemyWaveSize[PlayerPrefKeys.CurrentLevel - 1];
                waveSizeIncrement = RemoteConfigManager.EnemyWaveIncrease[PlayerPrefKeys.CurrentLevel - 1];
                spawnRate = RemoteConfigManager.EnemySpawnSpeed[PlayerPrefKeys.CurrentLevel - 1];
            }

            //waveSizeIncrement = RemoteConfigManager.EnemyWaveIncrease;
            //spawnRate = RemoteConfigManager.EnemySpawnSpeed;
            enemyBaseSpeed = RemoteConfigManager.EnemySpeed;
            enemyHealth = RemoteConfigManager.EnemyHealth;

            if (StateManager.Instance.IsSaved)
            {
                stageCurrentEnemySize = PlayerPrefs.GetInt("KilledEnemies");
                totalSpawnedEnemy = (stageMaxEnemySize - stageCurrentEnemySize);
            }
            else
            {
                stageMaxEnemySize = RemoteConfigManager.EnemyPerLevel[PlayerPrefKeys.CurrentLevel - 1];
                stageCurrentEnemySize = stageMaxEnemySize;
            }

            stageEnemyCountText.text = $"{stageCurrentEnemySize}";
            inGameUI.SetupFillBar(stageCurrentEnemySize, false);
            CreateFirstEnemyWave();
        }

        public void Set()
        {
            if (PlayerPrefKeys.CurrentLevel <= RemoteConfigManager.TotalLevelCount)
            {
                waveSize = RemoteConfigManager.EnemyWaveSize[PlayerPrefKeys.CurrentLevel - 1];
                waveSizeIncrement = RemoteConfigManager.EnemyWaveIncrease[PlayerPrefKeys.CurrentLevel - 1];
                spawnRate = RemoteConfigManager.EnemySpawnSpeed[PlayerPrefKeys.CurrentLevel - 1];
            }

            StopCoroutine(spawnEnemies);
            canSpawn = true;
            enemyBaseSpeed = RemoteConfigManager.EnemySpeed;
            totalSpawnedEnemy = (stageMaxEnemySize - stageCurrentEnemySize);
            KillCountPerTurn = 0;
            CreateFirstEnemyWave();
        }

        public void DecreaseEnemyTableCount()
        {
            stageCurrentEnemySize--;
            PlayerPrefs.SetInt("KilledEnemies", stageCurrentEnemySize);

            if (stageCurrentEnemySize <= 0)
            {
                StateManager.Instance.ProcessLevelEnd();
                return;
            }

            stageEnemyCountText.text = $"{stageCurrentEnemySize}";
            inGameUI.UpdateFillBar(1, 0.1f);
            KillCountPerTurn++;
        }

        public void DiscardRemainingEnemies()
        {
            int count = Enemies.Count;

            foreach (Enemy enemy in Enemies)
            {
                enemy.Discard();
            }
        }

        public void StartSpawningEnemies()
        {
            spawnEnemies = StartCoroutine(SpawnEnemies());
        }

        IEnumerator SpawnEnemies()
        {
            while (canSpawn)
            {
                for (int i = 0; i < waveSize; i++)
                {
                    if (totalSpawnedEnemy < stageMaxEnemySize)
                    {
                        totalSpawnedEnemy++;
                    }
                    else
                    {
                        canSpawn = false;
                        break;
                    }

                    float randomX = Random.Range(minAnchor.position.x, maxAnchor.position.x);
                    float randomZ = Random.Range(minAnchor.position.z, maxAnchor.position.z);

                    Vector3 pos = new Vector3(randomX, minAnchor.position.y, randomZ);
                    GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity, transform);

                    Enemies.Add(enemy.GetComponent<Enemy>());
                    enemy.GetComponent<Enemy>().Health = enemyHealth;
                    enemy.GetComponent<NavMeshAgent>().speed = enemyBaseSpeed;
                    enemy.GetComponent<Enemy>().StartAttack();
                }

                enemyBaseSpeed += enemySpeedIncrement;
                waveSize += waveSizeIncrement;

                yield return new WaitForSeconds(spawnRate);

                if (SoldierManager.Soldiers.Count <= 0 || stageCurrentEnemySize <= 0)
                {
                    canSpawn = false;
                    yield return null;
                }
            }
        }

        void CreateFirstEnemyWave()
        {
            Enemies.Clear();

            for (int i = 0; i < waveSize; i++)
            {
                if (totalSpawnedEnemy < stageMaxEnemySize)
                {
                    totalSpawnedEnemy++;
                }
                else
                {
                    break;
                }

                float randomX = Random.Range(minAnchor.position.x, maxAnchor.position.x);
                float randomZ = Random.Range(minAnchor.position.z, maxAnchor.position.z);

                Vector3 pos = new Vector3(randomX, minAnchor.position.y, randomZ);

                GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity, transform);
                enemy.GetComponent<NavMeshAgent>().speed = enemyBaseSpeed;

                Enemies.Add(enemy.GetComponent<Enemy>());
            }
        }
    }
}