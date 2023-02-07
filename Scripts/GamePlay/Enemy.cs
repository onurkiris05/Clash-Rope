using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Main.Scripts.Managers;
using DG.Tweening;
using ElephantSDK;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;
using VP.Nest.Haptic;

public class Enemy : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private Gradient color;
    [SerializeField] private int incomeValue;

    EconomyManager economyManager;
    EnemyManager enemyManager;
    Animator animator;
    NavMeshAgent agent;
    Collider collider;

    public ParticleSystem dieSplash;
    public int Health;
    public bool IsDead;

    int maxHealth;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        collider = GetComponent<Collider>();
        economyManager = FindObjectOfType<EconomyManager>();
        enemyManager = FindObjectOfType<EnemyManager>();
    }

    void Start()
    {
        maxHealth = Health;

        //Sets gradient[0].color to current material color
        List<GradientColorKey> keys = color.colorKeys.ToList();
        keys[0] = new GradientColorKey(skinnedMeshRenderer.material.color, color.colorKeys[0].time);
        color.SetKeys(keys.ToArray(), color.alphaKeys);
    }

    public void Discard()
    {
        StopAllCoroutines();
        animator.SetBool("Running", false);
        IsDead = true;
        collider.enabled = false;
        agent.isStopped = true;
        skinnedMeshRenderer.enabled = false;
        dieSplash.Play();
        Destroy(gameObject, dieSplash.main.duration);
    }

    public void StartAttack()
    {
        StartCoroutine(ProcessAttack());
    }

    IEnumerator ProcessAttack()
    {
        while (!IsDead)
        {
            Transform closestSoldier = GetClosestSoldier();

            if (closestSoldier == null)
            {
                agent.isStopped = true;
                animator.SetBool("Running", false);
                StateManager.Instance.ProcessTurnEnd();

                yield return null;
            }
            else
            {
                agent.SetDestination(closestSoldier.position);
                animator.SetBool("Running", true);
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    public void GetHurt(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Die();
            return;
        }

        transform.DOComplete();
        transform.DOPunchScale(new Vector3(0.7f, 0.7f, 0.7f), 0.1f);

        //Lerp material color as it gets hit
        skinnedMeshRenderer.material.color = color.Evaluate((float)(maxHealth - Health) / maxHealth);
    }

    void Die()
    {
        if (!IsDead)
        {
            IsDead = true;
            collider.enabled = false;
            EnemyManager.Enemies.Remove(this);
            agent.isStopped = true;
            skinnedMeshRenderer.enabled = false;
            animator.SetBool("Running", false);
            economyManager.IncreaseWallet(incomeValue);
            enemyManager.DecreaseEnemyTableCount();
            dieSplash.Play();
            HapticManager.Haptic(HapticType.MediumImpact);

            Destroy(gameObject, dieSplash.main.duration);
        }
    }

    Transform GetClosestSoldier()
    {
        Transform tMin = null;

        float minDist = Mathf.Infinity;

        Vector3 currentPos = transform.position;

        if (SoldierManager.Soldiers.Count > 0)
        {
            foreach (Soldier t in SoldierManager.Soldiers)
            {
                float dist = Vector3.Distance(t.transform.position, currentPos);
                if (dist < minDist)
                {
                    tMin = t.transform;
                    minDist = dist;
                }
            }
            return tMin;
        }
        return null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Soldier"))
        {
            other.GetComponent<Soldier>().Die();
            StartAttack();
        }
    }
}