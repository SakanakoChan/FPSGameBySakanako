using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfReturnToObjectPool : MonoBehaviour
{
    [SerializeField] private float selfReturnDelay = 5f;

    private void OnEnable()
    {
        StartCoroutine(SelfReturnToPool());
    }


    private IEnumerator SelfReturnToPool()
    {
        yield return new WaitForSeconds(selfReturnDelay);

        SpawnUtility.DestroyObject(gameObject);
    }
}
