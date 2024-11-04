using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Person : MonoBehaviour
{
    public event Action<Person> OnReach;
    public event Action<Person> OnDestination;
    public event Action<Person> OnInteract;

    [SerializeField] private NavMeshAgent agent;
    public int speed;
    
    private Transform _target;
    private bool _isDestDetermined;

    private void Start()
    {
        DetermineRandomSpeed();
        agent.isStopped = true;
    }

    private void Update()
    {
        IsPersonStopped();
        DetermineDestinationPoint();
    }
    
    private void IsPersonStopped()
    {
        if(!_target)
            return;
        
        var myPos = transform.position;
        myPos.y = 0f;                           
        var targetPos = _target.position;
        targetPos.y = 0f;
        if (Vector3.Distance(myPos, targetPos) < .2f)
        {
            agent.isStopped = true;
            _target = null;
            OnReach?.Invoke(this);
            
            if(GroupManager.Instance.inLinePersonList.Count > 0 && this == GroupManager.Instance.inLinePersonList[0])
                OnInteract?.Invoke(this);
        }
        
    }
    

    private void DetermineDestinationPoint() //X=2 Point
    {
        if (GroupManager.Instance.walkingExitingPersonList.Contains(this)) return;
        
        if (Mathf.Abs(transform.position.x - GroupManager.Instance.inLinePointList
                [GroupManager.Instance.inLinePersonList.Count + GroupManager.Instance.determinedWalkingPersonList.Count].position.x) < .2f && !_isDestDetermined)
        {
            _isDestDetermined = true;
            OnDestination?.Invoke(this);
            Debug.Log("OnDestination invoked " + " transform.position.x is " + transform.position.x 
                      + " inLinePersonList.Count " + GroupManager.Instance.inLinePersonList.Count);
            
        }
    }
    public void MoveInstanceToLine()
    {
        if (GroupManager.Instance.inLinePersonList.Count + GroupManager.Instance.walkingPersonList.Count + 
         GroupManager.Instance.determinedWalkingPersonList.Count >= GroupManager.Instance.inLinePointList.Count) return;
        
        if(GroupManager.Instance.waitingPersonList.Count > 0)
            GroupManager.Instance.waitingPersonList.Remove(this);
        
        SetDestination(GroupManager.Instance.inLinePointList[GroupManager.Instance.inLinePersonList.Count + GroupManager.Instance.determinedWalkingPersonList.Count]);
                                                                                                                      
        GroupManager.Instance.walkingPersonList.Add(this);
        
        OnDestination += SetDestinationPoint;

    }
    
    private void SetDestinationPoint(Person instance)
    {
        OnDestination -= SetDestinationPoint;

        GroupManager.Instance.determinedWalkingPersonList.Add(this);
        
        if(GroupManager.Instance.walkingPersonList.Count > 0)
            GroupManager.Instance.walkingPersonList.Remove(this);
        
        SetDestination(GroupManager.Instance.inLinePointList
            [GroupManager.Instance.inLinePersonList.Count + GroupManager.Instance.determinedWalkingPersonList.IndexOf(this)]);
    
        OnReach += GroupManager.Instance.AddInLine;

    }
    
    public void MoveInstanceToExit()
    {
        GroupManager.Instance.MoveToExitRandomPlace(this);
        
        if(GroupManager.Instance.inLinePersonList.Count > 0)
            GroupManager.Instance.inLinePersonList.Remove(this);
        
        GroupManager.Instance.walkingExitingPersonList.Add(this);

        GroupManager.Instance.ShiftCustomersInLine();

    }
    
    public void SetDestination(Transform target)
    {
        _target = target;
        agent.SetDestination(target.position);
        agent.isStopped = false;
        
    }

    private void DetermineRandomSpeed()
    {
        speed = Random.Range(2, 11);
        agent.speed = speed;
    }
}
