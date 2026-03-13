using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager instance { get; private set; }

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetObjectFromPool(GameObject _prefab)
    {
        GameObject result;

        if (poolDictionary.ContainsKey(_prefab) == false)
        {
            poolDictionary[_prefab] = new Queue<GameObject>();
        }

        if (poolDictionary[_prefab].Count > 0)
        {
            result = poolDictionary[_prefab].Dequeue();
            result.SetActive(true);

            var script = result.GetComponent<PooledObject>();
            if (script == null)
            {
                Debug.LogError($"{result.name}: Pooled object doesnt have PooledObject script!");
            }

            if (script.prefabReference == null)
            {
                script.prefabReference = _prefab;
            }

            result.transform.SetParent(transform);
            return result;
        }


        result = Instantiate(_prefab);
        result.transform.SetParent(transform);
        var pooledObjectScript = result.GetComponent<PooledObject>();
        if (pooledObjectScript == null)
        {
            Debug.LogError($"{result.name}: Pooled object doesnt have PooledObject script!");
        }
        pooledObjectScript.prefabReference = _prefab;
        

        return result;
    }

    public void ReturnObjectToPool(GameObject _obj, GameObject _prefab)
    {
        _obj.SetActive(false);
        _obj.transform.SetParent(transform);
        poolDictionary[_prefab].Enqueue(_obj);
    }
}
