using System;
using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.Managers;
using UnityEngine;
using VP.Nest.Haptic;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] RopeManager rope;

    public bool RollbackPressed { get; private set; }

    Rigidbody rb;
    SphereCollider _collider;

    float firstMass;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();

        firstMass = rb.mass;
    }

    public void StartRollback()
    {
        StartCoroutine(ProcessRollback());
    }

    IEnumerator ProcessRollback()
    {
        RollbackPressed = true;
        rb.isKinematic = false;
        rb.mass = 1f;
        rope.SetTenserKinematic(false);

        yield return new WaitForSeconds(0.5f);

        rope.SetObiUpdater(true);
        RollbackPressed = false;
    }

    public void ProcessRestartRope()
    {
        gameObject.layer = LayerMask.NameToLayer("PlayerRes");
        rope.SetObiUpdater(true);
        rb.isKinematic = false;
        rb.mass = 1f;
        rope.SetTenserKinematic(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishPin"))
        {
            StartCoroutine(HookedUp(other));
            HapticManager.Haptic(HapticType.MediumImpact);
        }
    }

    IEnumerator HookedUp(Collider other)
    {
        StateManager.Instance.GameActive = false;

        transform.position = other.transform.position;
        transform.position = new Vector3(other.transform.position.x,
            other.transform.position.y + 3f,
            other.transform.position.z);

        rb.isKinematic = true;
        rope.SetTenserKinematic(true);
        rope.SetObiUpdater(false,0.3f);

        yield return new WaitForSeconds(0.3f);

        StateManager.Instance. ProcessHookedUpRope();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("RopeSource"))
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
            rb.mass = firstMass;
            rope.SetTenserKinematic(true);
        }
    }
}