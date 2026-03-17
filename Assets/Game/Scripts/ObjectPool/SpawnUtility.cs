using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnUtility
{
    public static GameObject SpawnObject(GameObject _prefab)
    {
        GameObject result = null;

        var pooledObject = _prefab.GetComponent<PooledObject>();
        if (ObjectPoolManager.instance != null && pooledObject != null)
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
        if (_object == null)
            return;

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
