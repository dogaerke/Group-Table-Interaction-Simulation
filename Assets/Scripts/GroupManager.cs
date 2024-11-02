using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GroupManager : MonoBehaviour
{
    private int _randomNum;
    
    [SerializeField] private Person instancePrefab;
    [SerializeField] private float spawnRadius = 6f;
    [SerializeField] private float minDistance = 1.2f;

    public List<Transform> inLinePointList;
    public Transform exitPoint;
    public Transform tablePoint;
    
    public List<Person> waitingPersonList = new List<Person>();
    public List<Person> walkingPersonList = new List<Person>();
    public List<Person> inLinePersonList = new List<Person>();
    public List<Person> exitingPersonList = new List<Person>();
    public List<Person> determinedWalkingPersonList = new List<Person>();
    
    public int instanceCount = 30;
    public bool isInCoroutine;

    public static GroupManager Instance { get; private set; }

    
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);

        else
            Instance = this;
    }
    
    private void Start()
    {
        SpawnInstances();

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && waitingPersonList.Count > 0 && 
            inLinePersonList.Count + walkingPersonList.Count + determinedWalkingPersonList.Count < inLinePointList.Count)
        {
            _randomNum = Random.Range(0, waitingPersonList.Count);  //Choose random person
            
            waitingPersonList[_randomNum].MoveInstanceToLine();
            
        }

    }


    private void OnDestroy()
    {
        StopAllCoroutines();
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
                waitingPersonList.Add(instance);
                
                createdInstancesCount++;
            }
            attempts++;

        }
        
        if (createdInstancesCount < instanceCount)
            Debug.LogWarning("There is not enough space in the circle!");
        
    }
    
    public void AddInLine(Person instance)
    {
        instance.OnReach -= AddInLine;
        
        inLinePersonList.Add(instance);
        
        if(determinedWalkingPersonList.Count > 0)
            determinedWalkingPersonList.Remove(instance);
        
        if (!isInCoroutine)
            inLinePersonList[0].OnInteract += InteractWithTable;

        
    }
    
    public void ShiftCustomersInLine()
    {
        if(inLinePersonList.Count == 0) return;
        
        var i = 0;
        foreach (var c in inLinePersonList)
        {
            c.SetDestination(inLinePointList[i++].transform);
        }
        
        if (!isInCoroutine)
            inLinePersonList[0].OnInteract += InteractWithTable;

        
    }
    
    private void InteractWithTable(Person instance)
    {
        instance.OnReach -= InteractWithTable;
        
        StartCoroutine(WaitForSeconds(5f, () =>
        {
            inLinePersonList[0].MoveInstanceToExit();

        }));
        
    }
    
    private IEnumerator WaitForSeconds(float t, Action onWaitEnd)
    {
        isInCoroutine = true;
        yield return new WaitForSeconds(t);
        isInCoroutine = false;
        onWaitEnd?.Invoke();
    }
    
}
