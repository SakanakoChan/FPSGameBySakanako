using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfReturnToObjectPool : MonoBehaviour
{
    [SerializeField] private float selfReturnDelay = 5f;

    private Coroutine coroutine;

    private PooledObject pooledObject;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    private void OnEnable()
    {
        coroutine = StartCoroutine(SelfReturnToPool());
    }

    private void OnDisable()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

    }

    private IEnumerator SelfReturnToPool()
    {
        yield return new WaitForSeconds(selfReturnDelay);

        transform.SetParent(ObjectPoolManager.instance.transform);
        ObjectPoolManager.instance?.ReturnObjectToPool(gameObject, pooledObject.prefabReference);
    }
}
