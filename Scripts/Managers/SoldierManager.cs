using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ElephantSDK;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;
using VP.Nest.Analytics;
using VP.Nest.Haptic;
using Random = UnityEngine.Random;

namespace _Main.Scripts.Managers
{
    public class SoldierManager : MonoBehaviour
    {
        [Header("Genereal Components")] [SerializeField]
        private BattleManager battleManager;

        [Header("Soldier Settings")]
        [SerializeField] private GameObject soldierPrefab;
        [SerializeField] private int baseSoldierCount;
        [SerializeField] private float transferSpeed;
        [SerializeField] private float timeBetweenTransfers;
        [SerializeField] private Transform maxAnchor;
        [SerializeField] private Transform minAnchor;
        [SerializeField] private Transform zipStartPos;

        public static List<Soldier> Soldiers = new List<Soldier>();

        Vector3[] currentPath;

        void Start()
        {
            baseSoldierCount = RemoteConfigManager.BaseSoldierCount;

            if (PlayerPrefs.GetInt("IsSaved") > 0)
            {
                baseSoldierCount = PlayerPrefs.GetInt("SoldierCount");
            }
            else
            {
                PlayerPrefs.SetInt("SoldierCount", baseSoldierCount);
            }
            CreateSoldiers();
        }

        public void TransferSoldiers(Vector3[] path)
        {
            StartCoroutine(ProcessTransferSoldiers(path));
        }

        public Soldier SpawnSoldier(Vector3 pos)
        {
            GameObject soldier = Instantiate(soldierPrefab, pos, Quaternion.identity, transform);
            Soldier spawnedSoldier = soldier.GetComponentInChildren<Soldier>();

            spawnedSoldier.SetMyPath(currentPath, transferSpeed);
            Soldiers.Add(spawnedSoldier);

            return spawnedSoldier;
        }

        public void AddSoldier()
        {
            baseSoldierCount++;
            PlayerPrefs.SetInt("SoldierCount", baseSoldierCount);
            HapticManager.Haptic(HapticType.MediumImpact);
            AnalyticsManager.CustomEvent("count_upgraded",0);
        }

        public void CreateSoldiers()
        {
            Soldiers.Clear();

            for (int i = 0; i < baseSoldierCount; i++)
            {
                float randomX = Random.Range(minAnchor.position.x, maxAnchor.position.x);
                float randomZ = Random.Range(minAnchor.position.z, maxAnchor.position.z);

                Vector3 pos = new Vector3(randomX, minAnchor.position.y, randomZ);

                GameObject soldier = Instantiate(soldierPrefab, pos, Quaternion.identity, transform);
                soldier.name = $"Soldier{i}";

                Soldiers.Add(soldier.GetComponentInChildren<Soldier>());
            }
        }

        IEnumerator ProcessTransferSoldiers(Vector3[] path)
        {
            currentPath = path;

            List<Soldier> currentSoldiers = new List<Soldier>(Soldiers);

            foreach (Soldier soldier in currentSoldiers)
            {
                soldier.SetNavmeshAgent(false);
                soldier.SetAnimator("Running", true);
                soldier.GetComponent<Collider>().enabled = true;
                soldier.SetMyPath(currentPath, transferSpeed);

                soldier.transform.DOMove(zipStartPos.position, 10f).SetSpeedBased(true).OnComplete(() =>
                {
                    soldier.GetComponent<NavMeshAgent>().radius = 0.01f;
                    soldier.SetAnimator("Running", false);
                    soldier.SetAnimator("Ziplining", true);
                    soldier.transform.DOPath(path, transferSpeed, PathType.Linear)
                        .SetSpeedBased(true)
                        .OnWaypointChange(soldier.IncrementIndexAndLookAt)
                        .SetEase(Ease.Linear).OnComplete(() =>
                        {
                            soldier.SetAnimator("Ziplining", false);
                            battleManager.DeploySoldier(soldier);
                        });
                });
                yield return new WaitForSeconds(timeBetweenTransfers);
            }
        }
    }
}