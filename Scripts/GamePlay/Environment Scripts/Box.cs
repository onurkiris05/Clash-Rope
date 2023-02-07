using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Rope>() != null)
        {
            this.transform.SetParent(other.transform);
            this.gameObject.GetComponent<BoxCollider>().enabled = false;
            this.gameObject.layer = 0;
            Debug.Log(other.gameObject.name);
        }
    }
}
