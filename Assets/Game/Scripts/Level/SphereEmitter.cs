using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereEmitter : MonoBehaviour
{
    [SerializeField] private Vector3 emitRange;
    [SerializeField] private GameObject spherePrefab;
    [SerializeField] private int maxSphereAmountAtSameTime = 5;
    private int currentSphereAmount = 0;

    private void Start()
    {
        while (currentSphereAmount < maxSphereAmountAtSameTime)
        {
            SpawnSphere();
        }
    }

    private void SpawnSphere()
    {
        Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-emitRange.x, emitRange.x), Random.Range(2, emitRange.y), Random.Range(-emitRange.z, emitRange.z));
        var sphere = SpawnUtility.SpawnObject(spherePrefab);
        sphere.transform.position = spawnPosition;
        sphere.transform.SetParent(transform);

        var script = sphere.GetComponent<SphereForShot>();
        script?.SetupSphere(this);

        currentSphereAmount++;
    }

    public void RegisterSphereKill()
    {
        currentSphereAmount--;

        while (currentSphereAmount < maxSphereAmountAtSameTime)
        {
            SpawnSphere();
        }
    }
}
