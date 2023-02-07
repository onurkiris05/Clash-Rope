using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.Managers;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    [SerializeField] RopeManager rope;
    [SerializeField] float recordTime;
    [SerializeField] float rewindStepDuration;

    public bool IsRewinding { get; set; }

    List<Vector3> positions = new List<Vector3>();
    Rigidbody rb;
    PlayerManager playerManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerManager=GetComponent<PlayerManager>();
    }

    void Update()
    {
        if (!IsRewinding && !rb.isKinematic)
        {
            Record();
        }
    }

    public void StartRewind()
    {
        StartCoroutine(ProcessRewind());
    }

    IEnumerator ProcessRewind()
    {
        IsRewinding = true;

        rope.SetTenserKinematic(false);

        while (positions.Count > 0)
        {
            transform.position = positions[0];
            positions.RemoveAt(0);

            yield return new WaitForSeconds(rewindStepDuration);
        }
        yield return new WaitForSeconds(0.5f);

        if (!playerManager.RollbackPressed)
        {
            rope.SetTenserKinematic(true);
        }
        rope.SetObiUpdater(false,0.5f);
    }

    void Record()
    {
        if (positions.Count > Mathf.Round(recordTime / Time.fixedDeltaTime))
        {
            positions.RemoveAt(positions.Count - 1);
        }
        positions.Insert(0, transform.position);
    }
}
