using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GateManager : MonoBehaviour
{
    [SerializeField] private GameObject[] gateTypes;
    [SerializeField] private Transform[] gatePositions;
    [SerializeField] private int maxActiveGates;

    private List<GameObject> activeGates = new List<GameObject>();
    private List<Transform> availableGatePositions = new List<Transform>();

    private void Start()
    {
        ChangeGates();
    }

    public void ChangeGates()
    {
        availableGatePositions.Clear();

        foreach (Transform gate in gatePositions)
        {
            availableGatePositions.Add(gate);
        }

        if (activeGates.Count > 0)
        {
            foreach (GameObject gate in activeGates)
            {
                Destroy(gate.gameObject);
            }
        }

        for (int i = 0; i < maxActiveGates; i++)
        {
            int randomPos = Random.Range(0, gatePositions.Length);
            int randomGate = Random.Range(0, gateTypes.Length);

            if (availableGatePositions.Contains(gatePositions[randomPos]))
            {
                GameObject gate = Instantiate(gateTypes[randomGate], gatePositions[randomPos].position,
                    Quaternion.identity, transform);

                gate.transform.localRotation = Quaternion.Euler(-90,0,0);

                activeGates.Add(gate);
                availableGatePositions.Remove(gatePositions[randomPos]);
            }
            else
            {
                i--;
            }
        }
    }
}