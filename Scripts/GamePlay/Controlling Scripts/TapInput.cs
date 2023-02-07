using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VP.Nest.SceneManagement;

public class TapInput : MonoBehaviour
{ 
    [SerializeField] private UnityEvent tapAction;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tapAction.Invoke();
        }    
    }
}
