using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GroupManager : MonoBehaviour
{
    [SerializeField] private Person instancePrefab;
    [SerializeField] private float spawnRadius = 50f;
    [SerializeField] private float minDistance = 5f;
    
    public int instanceCount = 30;
    public Transform destinationPoint;
    
    public static GroupManager Instance { get; private set; }

    private void Start()
    {
        SpawnInstances();

    }
    
    private void SpawnInstances()
    {
        var createdInstancesCount = 0;
        var attempts = 0;

        while (createdInstancesCount < instanceCount && attempts < instanceCount * 10)
        {
            var randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = transform.position.y;
            
            var canPlace = true;
            
            foreach (Transform existingInstance in transform)
            {
                if (Vector3.Distance(randomPos, existingInstance.position) < minDistance)
                {
                    canPlace = false;
                    break;
                }
            }
            
            if (canPlace)
            {
                var instance = Instantiate(instancePrefab, randomPos, Quaternion.identity, transform);
                instance.SetDestination(destinationPoint);

                createdInstancesCount++;
            }
            attempts++;
            
        }
        
        if (createdInstancesCount < instanceCount)
            Debug.LogWarning("There is not enough space in the circle!");
    }
    
}
