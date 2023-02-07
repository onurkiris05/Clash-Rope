using System;
using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.Managers;
using UnityEngine;
using UnityEngine.AI;
using VP.Nest.Utilities;
using DG.Tweening;
using ElephantSDK;
using VP.Nest.Haptic;
using Random = UnityEngine.Random;

public class Soldier : MonoBehaviour
{
    [Header("General Components")]
    public GameObject[] Weapons;
    public GameObject[] Armors;
    [SerializeField] Transform gunPos;

    [Header("General Settings")]
    public int WeaponDamage;
    public float FireTime;
    public GameObject SpawnedGate;
    public int Index;
    public bool IsSpawnedFromGate;
    [SerializeField] float bulletSpeed;

    SoldierManager soldierManager;
    BattleManager battleManager;
    Animator animator;
    NavMeshAgent agent;
    Vector3[] path;

    bool isDead;
    float transferSpeed;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        soldierManager = FindObjectOfType<SoldierManager>();
        battleManager = FindObjectOfType<BattleManager>();
    }

    void Start()
    {
        WeaponDamage = RemoteConfigManager.BaseWeaponDamage;
        FireTime = RemoteConfigManager.BaseFireDelay;
    }

    public void SetNavmeshAgent(bool state)
    {
        agent.enabled = state;
    }

    public void SetAnimator(string parameter, bool state)
    {
        switch (parameter)
        {
            case "Running":
                animator.SetBool(parameter, state);
                break;
            case "Ziplining":
                animator.SetBool(parameter, state);
                break;
            case "Shooting":
                animator.SetBool(parameter, state);
                break;
        }
    }

    public void SetAnimator(string parameter)
    {
        switch (parameter)
        {
            case "Die":
                animator.SetTrigger(parameter);
                break;
        }
    }

    public void StartShooting()
    {
        StartCoroutine(ShootEnemy());
    }

    public void Die()
    {
        SoldierManager.Soldiers.Remove(this);
        isDead = true;
        animator.SetTrigger("Die");
        Destroy(gameObject, 3f);
    }

    public void IncrementIndexAndLookAt(int value)
    {
        if (Index > 1 && (Index + 1) < path.Length)
        {
            transform.DOLookAt(path[Index + 1], 0.2f);
        }

        if (IsSpawnedFromGate)
        {
            Index++;
        }
        else
        {
            Index = value;
            Index++;
        }
    }

    public void SetMyPath(Vector3[] originalPath, float speed)
    {
        path = originalPath;
        transferSpeed = speed;
    }

    public void ApplyMyFireRate(float originalFireTime, GameObject[] originalWeapons)
    {
        FireTime = originalFireTime;

        for (int i = 0; i < originalWeapons.Length; i++)
        {
            if (originalWeapons[i].activeSelf)
            {
                Weapons[i].SetActive(true);
                break;
            }
            else
            {
                Weapons[i].SetActive(false);
            }
        }
    }

    public void ApplyMyWeaponDamage(int originalDamage, GameObject[] originalArmors)
    {
        WeaponDamage = originalDamage;

        for (int i = 0; i < originalArmors.Length; i++)
        {
            if (originalArmors[i].activeSelf)
            {
                Armors[i].SetActive(true);
            }
        }
    }

    Transform GetClosestEnemy()
    {
        Transform tMin = null;

        float minDist = Mathf.Infinity;

        Vector3 currentPos = transform.position;

        if (EnemyManager.Enemies.Count > 0)
        {
            foreach (Enemy t in EnemyManager.Enemies)
            {
                if (t != null)
                {
                    float dist = Vector3.Distance(t.transform.position, currentPos);
                    if (dist < minDist)
                    {
                        tMin = t.transform;
                        minDist = dist;
                    }
                }
            }
            return tMin;
        }
        return null;
    }

    IEnumerator ShootEnemy()
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.3f));

        animator.SetBool("Shooting", true);

        while (!isDead)
        {
            Transform closestEnemy = GetClosestEnemy();

            if (closestEnemy == null || closestEnemy.GetComponent<Enemy>().IsDead)
            {
                animator.SetBool("Shooting", false);
                yield return null;
            }
            else
            {
                transform.LookAt(closestEnemy);

                GameObject bullet = ObjectPooler.Instance.Spawn("Bullet", gunPos.position, transform.rotation);

                bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletSpeed);
                bullet.GetComponent<Bullet>().WeaponDamage = WeaponDamage;
            }

            float randomRate = Random.Range(FireTime - 0.2f, FireTime + 0.2f);
            yield return new WaitForSeconds(randomRate);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == SpawnedGate)
        {
            return;
        }

        switch (other.tag)
        {
            case "MultiplyGate":
                MultiplyGate multiplyGate = other.GetComponent<MultiplyGate>();
                StartCoroutine(SpawnCloneSoldier(multiplyGate));
                HapticManager.Haptic(HapticType.MediumImpact);
                break;

            case "FireRateGate":
                FireRateGate fireRateGate = other.GetComponent<FireRateGate>();
                fireRateGate.transform.DOComplete();
                fireRateGate.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.2f);
                AdjustFireRate(fireRateGate);
                HapticManager.Haptic(HapticType.MediumImpact);
                break;

            case "PowerUpGate":
                PowerUpGate powerUpGate = other.GetComponent<PowerUpGate>();
                powerUpGate.transform.DOComplete();
                powerUpGate.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.2f);
                AdjustWeaponDamage(powerUpGate);
                HapticManager.Haptic(HapticType.MediumImpact);
                break;
        }
    }

    IEnumerator SpawnCloneSoldier(MultiplyGate multiplyGate)
    {
        int count = multiplyGate.GetCount();

        for (int i = 0; i < count; i++)
        {
            multiplyGate.transform.DOComplete();
            multiplyGate.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.2f);

            Soldier soldier = soldierManager.SpawnSoldier(multiplyGate.GetPosition());
            soldier.SpawnedGate = multiplyGate.gameObject;
            soldier.GetComponent<Collider>().enabled = true;
            soldier.IsSpawnedFromGate = true;
            soldier.Index = Index;
            soldier.ApplyMyFireRate(FireTime, Weapons);
            soldier.ApplyMyWeaponDamage(WeaponDamage, Armors);
            soldier.SetNavmeshAgent(false);
            soldier.SetAnimator("Ziplining", true);

            Vector3[] newPath = new Vector3[path.Length - Index];

            int newIndex = 0;

            for (int j = Index; j < path.Length; j++)
            {
                newPath[newIndex] = path[j];
                newIndex++;
            }

            soldier.transform.DOPath(newPath, transferSpeed, PathType.Linear)
                .SetSpeedBased(true)
                .OnWaypointChange(soldier.IncrementIndexAndLookAt)
                .SetEase(Ease.Linear).OnComplete(() =>
                {
                    soldier.SetAnimator("Ziplining", false);
                    battleManager.DeploySoldier(soldier);
                });

            yield return new WaitForSeconds(0.15f);
        }
    }

    void AdjustFireRate(FireRateGate fireRateGate)
    {
        //Adjust fire rate
        FireTime -= fireRateGate.FireRateIncrement;

        if (FireTime < 0.1f)
        {
            FireTime = 0.1f;
        }

        //Upgrade weapon
        for (int i = 0; i < Weapons.Length; i++)
        {
            if (Weapons[i].activeSelf)
            {
                Weapons[i].SetActive(false);

                if (i + 1 <= Weapons.Length)
                {
                    Weapons[i + 1].SetActive(true);
                    break;
                }
            }
        }
    }


    void AdjustWeaponDamage(PowerUpGate powerUpGate)
    {
        //Adjust weapon damage
        WeaponDamage += powerUpGate.WeaponDamageIncrement;

        //Upgrade soldier
        for (int i = 0; i < Armors.Length; i++)
        {
            if (!Armors[i].activeSelf)
            {
                Armors[i].SetActive(true);
                break;
            }
        }
    }
}