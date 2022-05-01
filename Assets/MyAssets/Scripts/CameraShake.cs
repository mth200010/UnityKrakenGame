using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraShake : MonoBehaviour
{    
    public IEnumerator Shake (float duration, float magnitude)
    {
        Debug.Log("Camera");
        Vector3 originalPos = transform.localPosition;
        
        float elapsed = 0.0f;
        GetComponentInChildren<CameraFollow>().enabled = false;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            
            transform.position = new Vector3(x, originalPos.y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
        GetComponentInChildren<CameraFollow>().enabled = true;

    }
   
}
