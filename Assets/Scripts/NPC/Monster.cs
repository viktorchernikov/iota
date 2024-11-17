using System;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Monster))]
public class Monster : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] NavMeshAgent agent;
    [Header("General")]
    public MonsterBehaviourMode behaviourMode = MonsterBehaviourMode.Patrol;
    [Header("Movement")]
    public float patrolMoveSpeed = 2f;
    public float fightMoveSpeed = 4f;
    [Header("Patroling")]
    public int currentPointId = 0;
    public int nextPointId = 1;
    public int patrolDirection = 1;
    public float distanceThreshold = 0.5f;
    public NpcPatrolPoint[] patrolPoints;
    public float lastPointId => patrolPoints.Length - 1;

    private void OnValidate()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    #region Logic
    private void FixedUpdate()
    {
        if (behaviourMode == MonsterBehaviourMode.Patrol)
        {
            if (PatrolPointReachCheck())
                OnPatrolReachNext();
            Patrol();
        }
        else if (behaviourMode == MonsterBehaviourMode.Fight)
        {
            throw new NotImplementedException();
        }
    }

    private void OnPatrolReachNext()
    {
        currentPointId = nextPointId;
        if (IsAtPatrolsEndPoint())
        {
            FlipPatrolDirection();
        }
        nextPointId = currentPointId + patrolDirection;
    }
    /// <summary>
    /// Does patrolling logic
    /// </summary>
    private void Patrol()
    {
        Vector3 position = GetPatrolPointPos(nextPointId);
        agent.SetDestination(position);
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Checks if monster reached patrol point
    /// </summary>
    private bool PatrolPointReachCheck()
    {
        float distance = GetPatrolPointDistance(nextPointId);

        return distance < distanceThreshold;
    }
    /// <summary>
    /// Flips patrol's direction
    /// </summary>
    private void FlipPatrolDirection()
    {
        patrolDirection = -patrolDirection;
    }
    /// <summary>
    /// Says if it is at any of patrols end points
    /// </summary>
    /// <returns></returns>
    private bool IsAtPatrolsEndPoint()
    {
        return currentPointId == 0 || currentPointId == lastPointId;
    }
    private int GetClosestPatrolPointId()
    {
        Vector3 monsterPos = transform.position;
        int closest = 0;
        float closestDistance = GetPatrolPointDistance(closest);
        for (int pointId = 1; pointId <= lastPointId; ++pointId)
        {
            float pointDistance = GetPatrolPointDistance(pointId);
            if (pointDistance < closestDistance)
            {
                closest = pointId;
            }
        }

        return closest;
    }
    private NpcPatrolPoint GetPatrolPoint(int pointId)
    {
        return patrolPoints[pointId];
    }
    private Vector3 GetPatrolPointPos(int pointId)
    {
        return GetPatrolPoint(pointId).GetPosition();
    }
    private float GetPatrolPointDistance(int pointId)
    {
        return Vector3.Distance(transform.position, GetPatrolPointPos(pointId));
    }
    #endregion
}

public enum MonsterBehaviourMode
{
    Patrol,
    Fight
}