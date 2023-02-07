using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Obi;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] private Transform movableRopePoint;
    [SerializeField] private Transform stableRopePoint;
    [SerializeField] private SphereCollider harpoonCollider;
    [SerializeField] private float animDuration = 0.3f;

    private Coroutine _coroutine;

    public void StartHarpoonRoutine(Transform target)
    {
        _coroutine = StartCoroutine(HarpoonRoutine(target));
    }

    public IEnumerator HarpoonRoutine(Transform target)
    {
        this.gameObject.transform.SetParent(null);
        yield return movableRopePoint.transform.DOMove(target.position, animDuration).WaitForCompletion();
        harpoonCollider.enabled = false;
        yield return movableRopePoint.transform.DOMove(stableRopePoint.transform.position, animDuration).WaitForCompletion();
        this.gameObject.SetActive(false);
        target.gameObject.SetActive(false);
    }
    public void StopHarpoonRoutine()
    {
        StopCoroutine(_coroutine);
    }
}
