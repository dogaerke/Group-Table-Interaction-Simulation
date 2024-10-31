using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Person : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    private Transform target;
    private bool _canMove;
    private float _speed;
    

    private void Start()
    {
        DetermineRandomSpeed();
    }
    
    
    public void SetDestination(Transform target)
    {
        this.target = target;
        agent.SetDestination(target.position);
        _canMove = true;
        agent.isStopped = false;
    }

    private void DetermineRandomSpeed()
    {
        var x = Random.Range(2, 11);
        agent.speed = x;
    }
}
