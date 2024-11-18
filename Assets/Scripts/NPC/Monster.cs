using System;
using Unity.Burst;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections.Generic;


[RequireComponent(typeof(Monster))]
public class Monster : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    [Header("General")]
    public MonsterBehaviourMode behaviourMode = MonsterBehaviourMode.Patrol;
    public Transform eyesPoint;
    [Header("Movement")]
    public float patrolMoveSpeed = 2f;
    public float patrolRotateSpeed = 90f;
    public float chaseMoveSpeed = 3f;
    public float chaseRotateSpeed = 120f;
    public float distanceThreshold = 0.5f;
    [Header("Spotting")]
    public float maxSeekDistance = 30f;
    public float spottingBuildup = 0f;
    public float maxSpottingBuildup = 1f;
    public AnimationCurve spottingBuildupFactorCurve;
    public Vector3 lastPlayerPosition = Vector3.zero;
    [Header("Patroling")]
    public int currentPointId = 0;
    public int nextPointId = 1;
    public int patrolDirection = 1;
    public NpcPatrolPoint[] patrolPoints;
    public float lastPointId => patrolPoints.Length - 1;

    private void OnValidate()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    #region Looks

    private void LateUpdate()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
    #endregion

    #region Logic
    private void FixedUpdate()
    {
        if (behaviourMode == MonsterBehaviourMode.Patrol)
        {
            // Goes to the next patrol point
            if (PatrolPointReachCheck())
                OnPatrolReachNext();
            Patrol();

            // Ray from monster traces player
            if (TrySeekPlayer())
            {
                // If spot build up factor is present, than do spotting logic
                if (GetBuildupFactor() > 0)
                {
                    // On Player Spotted
                    if (TryBuildupSpotted())
                    {
                        RememberPlayerPosition();
                        FollowLastPlayerPosition();
                        BeginChase();
                    }
                }
                // Else, cool down the monster
                else
                {
                    TryBuilddownSpotted();
                }
            }
            else
            {
                TryBuilddownSpotted();
            }
        }
        if (behaviourMode == MonsterBehaviourMode.Chase)
        {
            if (TrySeekPlayer() && GetBuildupFactor() > 0)
            {
                TryBuildupSpotted();
                RememberPlayerPosition();
                FollowLastPlayerPosition();
            }
            else
            {
                if (LastPlayerPosReachCheck())
                {
                    if (TryBuilddownSpotted())
                    {
                        EndChase();
                        OnPatrolReturn();
                    }
                }
                else
                {
                    FollowLastPlayerPosition();
                }
            }
        }
    }

    private void OnPatrolReachNext()
    {
        currentPointId = nextPointId;
        if (IsAtPatrolsDeadEnd())
        {
            FlipPatrolDirection();
        }
        nextPointId = currentPointId + patrolDirection;
    }
    private void OnPatrolReturn()
    {
        int[] closestPoints = GetTwoClosestPatrolPoints();

        int minPoint = Mathf.Min(closestPoints[0], closestPoints[1]);
        int maxPoint = Mathf.Max(closestPoints[0], closestPoints[1]);

        if (patrolDirection == 1)
        {
            currentPointId = minPoint;
            nextPointId = maxPoint;
        }
        else
        {
            currentPointId = maxPoint;
            nextPointId = minPoint;
        }
    }
    /// <summary>
    /// Does patrolling logic
    /// </summary>
    private void Patrol()
    {
        Vector3 position = GetPatrolPointPos(nextPointId);
        agent.SetDestination(position);
    }
    /// <summary>
    /// Tweaks monster's movement on chase begin
    /// </summary>
    private void BeginChase()
    {
        behaviourMode = MonsterBehaviourMode.Chase;
        agent.speed = chaseMoveSpeed;
        agent.angularSpeed = chaseRotateSpeed;
    }
    /// <summary>
    /// Tweaks monster's movement on chase end
    /// </summary>
    private void EndChase()
    {
        behaviourMode = MonsterBehaviourMode.Patrol;
        agent.speed = patrolMoveSpeed;
        agent.angularSpeed = patrolRotateSpeed;
    }
    /// <summary>
    /// Monster trying to see player
    /// </summary>
    /// <returns></returns>
    private bool TrySeekPlayer()
    {
        RaycastHit hitInfo;

        if (!Physics.Raycast(eyesPoint.position, GetDirectionToPlayer(eyesPoint), out hitInfo, maxSeekDistance)) return false;
        return hitInfo.collider.tag == "Player";
    }
    /// <summary>
    /// Builds up spotted
    /// </summary>
    /// <returns>True if built up, false if not</returns>
    private bool TryBuildupSpotted()
    {
        spottingBuildup += GetBuildupFactor() * Time.fixedDeltaTime;

        if (spottingBuildup > maxSpottingBuildup)
        {
            spottingBuildup = maxSpottingBuildup;
            return true;
        }
        return false;
    }
    /// <summary>
    /// Gets buildup factor varied by monster's orientation to player
    /// </summary>
    private float GetBuildupFactor()
    {
        Vector3 playerDir = GetDirectionToPlayer(transform);
        playerDir.y = 0;
        playerDir.Normalize();

        float monsterRotation = transform.eulerAngles.y;
        float toPlayerRotation = Quaternion.LookRotation(playerDir).eulerAngles.y;

        float difference = Mathf.Abs(monsterRotation - toPlayerRotation);
        difference = Mathf.Clamp(difference, 0f, 180f);
        return spottingBuildupFactorCurve.Evaluate(difference);
    }
    /// <summary>
    /// Builds down spotted
    /// </summary>
    /// <returns>True if succeed, false if not</returns>
    private bool TryBuilddownSpotted()
    {
        spottingBuildup -= 0.25f * Time.fixedDeltaTime;
        if (spottingBuildup < 0)
        {
            spottingBuildup = 0;
            return true;
        }
        return false;
    }
    /// <summary>
    /// Sets last known position as destination
    /// </summary>
    private void FollowLastPlayerPosition()
    {
        agent.SetDestination(lastPlayerPosition);
    }
    /// <summary>
    /// Remembers last position of the player that monster saw
    /// </summary>
    private void RememberPlayerPosition()
    {
        lastPlayerPosition = GetPlayerPosition();
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
    private bool LastPlayerPosReachCheck()
    {
        float distance = Vector3.Distance(transform.position, lastPlayerPosition);

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
    private bool IsAtPatrolsDeadEnd()
    {
        return IsPointDeadEnd(currentPointId);
    }
    private bool IsPointDeadEnd(int pointId)
    {
        return pointId == 0 || pointId == lastPointId;
    }
    private int[] GetTwoClosestPatrolPoints()
    {
        var patrolPointDistances = new Dictionary<int, float>();

        for (int i = 0; i <= lastPointId; ++i)
            patrolPointDistances[i] = GetPatrolPointDistance(i);

        var closestPoints = patrolPointDistances
                            .OrderBy(x => x.Value)
                            .Take(2)
                            .Select(x => x.Key)
                            .ToArray();

        return closestPoints;
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
    private Vector3 GetPlayerPosition()
    {
        return Player.local.transform.position;
    }
    private Vector3 GetDirectionToPlayer(Transform fromTransform)
    {
        return ((GetPlayerPosition() + Vector3.up) - fromTransform.position).normalized;
    }
    #endregion

}

public enum MonsterBehaviourMode
{
    Patrol,
    Chase
}