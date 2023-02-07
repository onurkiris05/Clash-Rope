using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VP.Nest.Utilities;

public class Bullet : MonoBehaviour
{
    public int WeaponDamage;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        StartCoroutine(DieEventually());
    }

    IEnumerator DieEventually()
    {
        yield return new WaitForSeconds(3f);

        gameObject.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().GetHurt(WeaponDamage);

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            gameObject.SetActive(false);
        }
        else
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            gameObject.SetActive(false);
        }
    }
}