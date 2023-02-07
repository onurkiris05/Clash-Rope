using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using UnityEngine;
using VP.Nest.Analytics;
using VP.Nest.Haptic;

namespace _Main.Scripts.Managers
{
    public class RopeManager : MonoBehaviour
    {
        [Header("General Components")] [SerializeField]
        GameObject ropeTenser;
        [SerializeField] GameObject ropeSource;
        [SerializeField] Transform ziplineStart;
        [SerializeField] Transform ziplineEnd;

        [Header("Rope Settings")] [SerializeField]
        Color normalColor;
        [SerializeField] Color tensionColor;
        [SerializeField] float tensionTime;
        [SerializeField] float ropeIncrementValue;

        public bool DebugMode;

        ObiLateUpdater obiLateUpdater;
        ObiRopeCursor cursor;
        ObiRope rope;
        Material material;
        Rigidbody tenser;

        void Awake()
        {
            cursor = GetComponent<ObiRopeCursor>();
            rope = GetComponent<ObiRope>();
            material = GetComponent<MeshRenderer>().material;
            tenser = ropeTenser.GetComponent<Rigidbody>();
            obiLateUpdater = GetComponentInParent<ObiLateUpdater>();

            material.color = normalColor;
        }

        void Start()
        {
            if (PlayerPrefs.GetInt("IsSaved") > 0)
            {
                cursor.ChangeLength(PlayerPrefs.GetFloat("RopeLength"));
            }
            else
            {
                PlayerPrefs.SetFloat("RopeLength", rope.restLength);
            }
        }

        // void Update()
        // {
        //     Debug.Log(rope.restLength);
        // }

        public void SetObiUpdater(bool state)
        {
            obiLateUpdater.enabled = state;
        }

        public void SetObiUpdater(bool state, float delayTime)
        {
            StartCoroutine(ProcessObiUpdater(state, delayTime));
        }

        public void IncreaseRopeLength()
        {
            cursor.ChangeLength(rope.restLength + ropeIncrementValue);
            PlayerPrefs.SetFloat("RopeLength", rope.restLength);
            HapticManager.Haptic(HapticType.MediumImpact);
            AnalyticsManager.CustomEvent("rope_upgraded",0);
        }

        public void SetTenserKinematic(bool state)
        {
            tenser.isKinematic = state;
        }

        public void SetTensionColor()
        {
            StartCoroutine(ProcessColorChange());
        }

        public float GetTenserDistance()
        {
            return Vector3.Distance(ropeTenser.transform.position, ropeSource.transform.position);
        }

        public Vector3[] GetRopePath()
        {
            List<Vector3> pathList = new List<Vector3>();

            pathList.Add(ziplineStart.position);

            for (int i = 0; i < rope.elements.Count; i++)
            {
                if (rope.GetParticlePosition(i).z < 0f)
                {
                    continue;
                }

                Vector3 point = new Vector3(rope.GetParticlePosition(i).x,
                    rope.GetParticlePosition(i).y - 3.43f,
                    rope.GetParticlePosition(i).z);

                pathList.Add(point);
            }

            pathList.Add(ziplineEnd.position);
            Vector3[] path = pathList.ToArray();

            return path;
        }

        IEnumerator ProcessColorChange()
        {
            material.color = tensionColor;

            yield return new WaitForSeconds(tensionTime);

            material.color = normalColor;
        }

        IEnumerator ProcessObiUpdater(bool state, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            obiLateUpdater.enabled = state;
        }

        void OnDrawGizmos()
        {
            if (DebugMode)
            {
                Gizmos.color = Color.red;

                Vector3[] path = GetRopePath();

                for (int i = 0; i < path.Length; i++)
                {
                    Gizmos.DrawWireSphere(path[i], 2f);
                }
            }
        }
    }
}