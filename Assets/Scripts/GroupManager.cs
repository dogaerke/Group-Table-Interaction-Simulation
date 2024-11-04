using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public class GroupManager : MonoBehaviour
{
    private int _randomNum;

    [SerializeField] private float spawnRadius = 6f;
    [SerializeField] private float minDistance = 1.2f;
    [SerializeField] private Transform destinationPoint;
    [SerializeField] private Person instancePrefab;
    [SerializeField] private GameObject randomExitPoint;

    public Transform exitPoint;
    public List<Transform> inLinePointList;
    public List<Person> waitingPersonList = new List<Person>();
    public List<Person> walkingPersonList = new List<Person>();
    public List<Person> inLinePersonList = new List<Person>();
    public List<Person> exitingPersonList = new List<Person>();

    public int instanceCount = 30;

    private void Start()
    {
        SpawnInstances();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && waitingPersonList.Count > 0 &&
            inLinePersonList.Count + walkingPersonList.Count < inLinePointList.Count)
        {
            _randomNum = Random.Range(0, waitingPersonList.Count); //Choose random instance

            MoveInstanceToLine(waitingPersonList[_randomNum]);
        }
    }

    private void MoveInstanceToLine(Person instance)
    {
        instance.state = State.Walking;

        if (waitingPersonList.Count > 0)
            waitingPersonList.Remove(instance);
        
        if(!walkingPersonList.Contains(instance))
            walkingPersonList.Add(instance);

        instance.SetDestination(destinationPoint);
    }


    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    private void SpawnInstances()
    {
        var createdInstancesCount = 0;
        var attempts = 0;

        while (createdInstancesCount < instanceCount && attempts < instanceCount * 15)
        {
            var randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = transform.position.y;

            var canPlace = transform.Cast<Transform>().All(existingInstance => 
                !(Vector3.Distance(randomPos, existingInstance.position) < minDistance));

            if (canPlace)
            {
                var instance = Instantiate(instancePrefab, randomPos, Quaternion.identity, transform);
                waitingPersonList.Add(instance);

                instance.OnReach += AddInLine;
                instance.OnExit += HandleExit;

                createdInstancesCount++;
            }

            attempts++;
        }

        if (createdInstancesCount < instanceCount)
            Debug.LogWarning("There is not enough space in the spawn circle!");
    }

    private void AddInLine(Person instance)
    {
        instance.OnReach -= AddInLine;
        instance.state = State.Waitingline;

        if (walkingPersonList.Count > 0)
            walkingPersonList.Remove(instance);
        
        if(!inLinePersonList.Contains(instance))
            inLinePersonList.Add(instance);
        
        if(inLinePersonList[0] == instance)
            instance.InteractWithTable();
            
        ShiftCustomersInLine();
    }

    private void ShiftCustomersInLine()  
    {
        if (inLinePersonList.Count == 0) return;
        
        var i = 0;
        foreach (var c in inLinePersonList)
        {
            c.SetDestination(inLinePointList[i++].transform);
        }

        if(inLinePersonList.Count > 0)
            inLinePersonList[0].InteractWithTable();
        
    }

    private void MoveToExitRandomPlace(Person instance)
    {
        var attempts = 0;
        var tryingCount = 0;
        
        spawnRadius = 11f;
        minDistance = 2.2f;
        
        while (attempts < 15)
        {
            
            var randomPos = exitPoint.transform.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = exitPoint.transform.position.y;
            
            var canPlace = exitingPersonList.All(existingInstance => 
                !(Vector3.Distance(randomPos, existingInstance.transform.position) < minDistance));

            if (canPlace)
            {
                randomExitPoint.transform.position = randomPos;
                instance.SetDestination(randomExitPoint.transform);

            }
            else
                tryingCount++;
            
            attempts++;
            
        }

        if (tryingCount == attempts)
            Debug.LogWarning("There is not enough space in the exit circle!");
        
    }
    
    private void HandleExit(Person instance)
    {
        instance.OnExit -= HandleExit;
        instance.state = State.Exiting;

        MoveToExitRandomPlace(instance);

        if (inLinePersonList.Count > 0)
            inLinePersonList.Remove(instance);

        if(!exitingPersonList.Contains(instance))
            exitingPersonList.Add(instance);
        
        ShiftCustomersInLine();
    }
}