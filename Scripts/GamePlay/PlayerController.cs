using System;
using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.Managers;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;

public class PlayerController : MonoBehaviour
{
    [SerializeField] RopeManager rope;
    [SerializeField] LayerMask backLayer;
    [SerializeField] float endOffset;
    [SerializeField] float moveSpeed;

    PlayerManager playerManager;
    PlayerRecorder playerRecorder;
    Finger movementFinger;
    Rigidbody rb;
    RaycastHit hit;
    Ray ray;

    float firstMass;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerRecorder = GetComponent<PlayerRecorder>();
        playerManager = GetComponent<PlayerManager>();
    }

    void Start()
    {
        rb.isKinematic = true;
        firstMass = rb.mass;
    }

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();

        ETouch.Touch.onFingerDown += HandleFingerDown;
        ETouch.Touch.onFingerUp += HandleFingerUp;
        ETouch.Touch.onFingerMove += HandleFingerMove;
    }

    void OnDisable()
    {
        ETouch.Touch.onFingerDown -= HandleFingerDown;
        ETouch.Touch.onFingerUp -= HandleFingerUp;
        ETouch.Touch.onFingerMove -= HandleFingerMove;

        EnhancedTouchSupport.Disable();
    }

    void HandleFingerMove(Finger movedFinger)
    {
        if (!StateManager.Instance.GameActive)
        {
            return;
        }

        if (movedFinger == movementFinger && !playerRecorder.IsRewinding)
        {
            ETouch.Touch currentTouch = movedFinger.currentTouch;

            ray = Camera.main.ScreenPointToRay(currentTouch.screenPosition);

            if (Physics.Raycast(ray, out hit, 1000, backLayer))
            {
                if (hit.point.z > 1f)
                {
                    StartCoroutine(ProcessMove());
                }
            }
        }
    }

    void HandleFingerUp(Finger lostFinger)
    {
        if (!StateManager.Instance.GameActive)
        {
            return;
        }

        if (lostFinger == movementFinger)
        {
            movementFinger = null;

            if (playerManager.RollbackPressed || playerRecorder.IsRewinding)
            {
                return;
            }

            rb.isKinematic = true;

            //If you change delayTime, also change delay in ProcessRollback/PlayerManager
            rope.SetObiUpdater(false, 0.5f);
            rope.SetTenserKinematic(true);
        }
    }

    void HandleFingerDown(Finger touchedFinger)
    {
        if (!StateManager.Instance.GameActive)
        {
            return;
        }

        if (movementFinger == null)
        {
            movementFinger = touchedFinger;

            playerRecorder.IsRewinding = false;
            rope.SetObiUpdater(true);
            rope.SetTenserKinematic(false);
        }
    }

    IEnumerator ProcessMove()
    {
        if (rope.GetTenserDistance() > endOffset)
        {
            rb.isKinematic = false;
            rb.mass = firstMass;
            rope.SetTenserKinematic(false);
            rope.SetObiUpdater(true);

            transform.position = Vector3.Lerp(transform.position, hit.point, moveSpeed * Time.deltaTime);
            yield return new WaitForSeconds(moveSpeed * Time.deltaTime);
        }
        else
        {
            playerRecorder.StartRewind();
            rope.SetTensionColor();
        }

        rb.isKinematic = true;
    }
}