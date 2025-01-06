using System;
using Unity.Burst;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System;
using System.Collections;
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
    [Header("Interaction")]
    public float armsReach = 3.0f;
    [Header("Spotting")]
    public float maxSeekDistance = 30f;
    public float nearbySpotDistance = 4f;
    public float nearbySpottingFactor = 0.5f;
    public float spottingBuildup = 0f;
    public float maxSpottingBuildup = 1f;
    public float lastKnownToHidingMaxDistance = 5f;
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
                    TryBeginChase();
                }
                // Else, cool down the monster
                else if (AtNearbySpotDistance())
                {
                    TryBeginChase(overwrite: true, overwriteValue: nearbySpottingFactor);
                }
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

            if ((TrySeekPlayer() && GetBuildupFactor() > 0) || IsHidingNearbyLastKnownPos())
            {
                if (PlayerWithinReach()) { // + and not in a hiding spot
                    InitPlayerDeath();
                    return;
                } 

                TryBuildupSpotted();
                RememberPlayerPosition();

                TryFollowLastPlayerPosition();

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
                    if (PlayerWithinReach())
                    {
                        InitPlayerDeath();
                        return;
                    }
                    TryFollowLastPlayerPosition();
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
    public void TryBeginChase(bool overwrite = false, float overwriteValue = 0f)
    {
        if (TryBuildupSpotted(overwrite, overwriteValue))
        {
            RememberPlayerPosition();
            BeginChase();
            TryFollowLastPlayerPosition();
        }
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
        if (hitInfo.collider.tag != "Player") return false;
        if (!Player.local.isAlive) return false;

        return true;
    }
    /// <summary>
    /// Builds up spotted
    /// </summary>
    /// <returns>True if built up, false if not</returns>
    private bool TryBuildupSpotted(bool overwrite = false, float overwriteValue = 0f)
    {
        spottingBuildup += !overwrite ? GetBuildupFactor() : overwriteValue * Time.fixedDeltaTime;

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
    private bool TryFollowLastPlayerPosition()
    {
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(lastPlayerPosition, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(lastPlayerPosition);
            return true;
        }
        EndChase();
        OnPatrolReturn();
        return true;
    }
    /// <summary>
    /// Remembers last position of the player that monster saw
    /// </summary>
    private void RememberPlayerPosition()
    {
        lastPlayerPosition = GetPlayerPosition();
    }

    private bool IsHidingNearbyLastKnownPos()
    {
        return Vector3.Distance(lastPlayerPosition, Player.local.transform.position) < lastKnownToHidingMaxDistance && Player.local.isHiding;
    }

    /// <summary>
    ///  checks if the player is close enough
    ///  </summary>
    private bool PlayerWithinReach()
    {
        return (Vector3.Distance(GetPlayerPosition(), transform.position) < armsReach);
    }

    /// <summary>
    /// tells the player object to go to hell
    /// </summary>
    private void InitPlayerDeath()
    {
        if (Player.local.isAlive)
        {
            behaviourMode = MonsterBehaviourMode.Attacking;
            StartCoroutine(AttackCo());
        }
    }

    IEnumerator AttackCo()
    {
        Player.local.PrepareToDie(eyesPoint.position);
        animator.SetTrigger("IsAttacking");
        Quaternion beginning = transform.rotation;
        Quaternion destination = Quaternion.LookRotation(GetDirectionToPlayer(transform));
        agent.ResetPath();
        agent.SetDestination(transform.position);
        agent.updateRotation = false;
        float time = 0.0f;
        while (time < 1.5f)
        {
            transform.rotation = Quaternion.Slerp(beginning, destination, time / 1.35f);
            time += Time.deltaTime;
            yield return null;
        }
        Player.local.Die();
        yield return new WaitForSeconds(2);
        RestartPatrolling();
        yield return new WaitForSeconds(1);
        agent.updateRotation = true;
        // tyle trwa animacja :-))
        agent.isStopped = false;
        EndChase(); //no ale itak od razu go znajdzie
        OnPatrolReturn();
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
    public bool AtNearbySpotDistance()
    {
        return Vector3.Distance(transform.position, GetPlayerPosition()) < nearbySpotDistance;
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

    private void RestartPatrolling()
    {
        currentPointId = 0;
        nextPointId = 1;
        patrolDirection = 1;
        transform.position = GetPatrolPointPos(0);
        behaviourMode = MonsterBehaviourMode.Patrol;
        lastPlayerPosition = Vector3.zero;
    }
    #endregion

}

public enum MonsterBehaviourMode
{
    Patrol,
    Chase,
    Attacking
}