using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnUtility
{
    public static GameObject SpawnObject(GameObject _prefab)
    {
        GameObject result = null;

        if (ObjectPoolManager.instance != null)
        {
            result = ObjectPoolManager.instance.GetObjectFromPool(_prefab);
        }
        else
        {
            result = GameObject.Instantiate(_prefab);
        }

        return result;
    }

    public static void DestroyObject(GameObject _object)
    {
        var pooledObject = _object.GetComponent<PooledObject>();
        if (ObjectPoolManager.instance != null && pooledObject != null)
        {
            ObjectPoolManager.instance.ReturnObjectToPool(_object, pooledObject.prefabReference);
        }
        else
        {
            GameObject.Destroy(_object);
        }
    }
}
