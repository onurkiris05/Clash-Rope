using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

public class MultiplyGate : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI headerText;
    [SerializeField] Transform maxAnchor;
    [SerializeField] Transform minAnchor;
    [SerializeField] int multiplyCount;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        headerText.text = $"Multiply \n x2";
    }

    public int GetCount()
    {
        return multiplyCount;
    }

    public Vector3 GetPosition()
    {
        float randomX = Random.Range(minAnchor.position.x, maxAnchor.position.x);
        float randomZ = Random.Range(minAnchor.position.z, maxAnchor.position.z);

        Vector3 pos = new Vector3(randomX, minAnchor.position.y, randomZ);

        return pos;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Rope"))
        {
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Rope"))
        {
            transform.localScale = originalScale;
        }
    }
}