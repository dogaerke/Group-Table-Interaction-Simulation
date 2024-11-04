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
    public event Action<Person> OnReach;
    public event Action<Person> OnExit;
    
    public State state;
    
    private const float MaxDistanceToReach = 0.4f;
    private const float MaxDistanceToSlowDown = 2.7f;
    private const float MinSpeed = 2f;
    private const float MaxSpeed = 11f;
    private float _speed;
    private Transform _target;
    private Collider _collider;

    private void Start()
    {
        _collider = GetComponent<Collider>();
        DetermineRandomSpeed();
        agent.isStopped = true;
    }

    private void Update()
    {
        IsPersonStopped();
        ControlAgentSpeeds();
    }

    private void ControlAgentSpeeds()       //When they get too close to each other, their speed decreases so that they do not collide,
                                            //and when they get farther away, they return to their previous speed.
    {
        var hitColliders = Physics.OverlapSphere(transform.position, MaxDistanceToSlowDown);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider != _collider)
            {
                var distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                agent.speed = distance < MaxDistanceToSlowDown ? 
                    Mathf.Lerp(MinSpeed, agent.speed, distance / MaxDistanceToSlowDown) : _speed;
            }
        }
    }
    
    private void IsPersonStopped()
    {
        if(!_target)
            return;
        
        var myPos = transform.position;
        myPos.y = 0f;                           
        var targetPos = _target.position;
        targetPos.y = 0f;
        if (Vector3.Distance(myPos, targetPos) < MaxDistanceToReach)
        {
            agent.isStopped = true;
            _target = null;
            OnReach?.Invoke(this);
        }
        
    }
    
    public void InteractWithTable()
    {
        state = State.Interactıng;
        
        StartCoroutine(WaitForSeconds(5f, () =>
        {
            OnExit?.Invoke(this);
        }));
        
    }
    
    private static IEnumerator WaitForSeconds(float t, Action onWaitEnd)
    {
        yield return new WaitForSeconds(t);
        onWaitEnd?.Invoke();
    }
    
    public void SetDestination(Transform target)
    {
        _target = target;
        agent.SetDestination(target.position);
        agent.isStopped = false;
        
    }

    private void DetermineRandomSpeed()
    {
        _speed = Random.Range(MinSpeed, MaxSpeed);
        agent.speed = _speed;
    }
}

public enum State
{
    Default,
    Walking,
    Waitingline,
    Interactıng,
    Exiting,
}
