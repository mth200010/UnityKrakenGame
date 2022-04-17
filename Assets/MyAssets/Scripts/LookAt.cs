using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    [SerializeField] GameObject objectToLookAt;

    Transform target;

    private void Start()
    {
        target = objectToLookAt.GetComponent<Transform>();

        transform.LookAt(target, Vector3.up);
                
    }       
  
}
