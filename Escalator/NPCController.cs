using System.Collections;
using ProjectDawn.Navigation;
using ProjectDawn.Navigation.Hybrid;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    NPCState currentState = NPCState.Idle;
    int currentFloorlevel = 0;
    [SerializeField] Transform currentPlatform;

    bool usingEscalatorOrElevator = false;
    public bool UsingEscalatorOrElevator
    {
        get { return usingEscalatorOrElevator; }
        set { usingEscalatorOrElevator = value; }
    }
    bool readyToUseEscalatorOrElevator = false;
    public bool ReadyToUseEscalatorOrElevator
    {
        get { return readyToUseEscalatorOrElevator; }
        set { readyToUseEscalatorOrElevator = value; }
    }

    [SerializeField] AgentAuthoring agentAuthoring;
    [SerializeField] AgentNavMeshAuthoring agentNavMeshAuthoring;
    [SerializeField] Animator animator;

    InteractTarget currentTarget;
    EscalatorController targetEscalator;

    void Start()
    {
        FindRandomTarget();
    }

    void FindRandomTarget()
    {
        InteractTarget target = NPCPathManager.Instance.GetRandomInteractTarget();
        if (target != null)
        {
            currentTarget = target;
            TowardTarget(target);
        }
    }

    private void TowardTarget(InteractTarget target)
    {
        if (target.platform != currentPlatform)
        {
            NPCPathManager.Instance.GetTargetEscalator(currentFloorlevel, target.floorLevel, out targetEscalator);
            if (targetEscalator != null)
            {
                SetNPCState(NPCState.TowardEscalator);
                SetDestination(targetEscalator.entrance.position, NPCState.TowardEscalator);
            }
            else
            {
                Debug.LogWarning("No escalator found for path from floor " + currentFloorlevel + " to " + target.floorLevel);
            }
        }
        else
        {
            SetDestination(target.transform.position, NPCState.TowardAction);
        }
    }

    public void SetNPCState(NPCState state)
    {
        currentState = state;
    }

    public void AnimationTrigger(string trigger)
    {
        animator.SetTrigger(trigger);
    }

    public void SetDestination(Vector3 destination, NPCState state = NPCState.Walking)
    {
        currentState = state;

        animator.SetTrigger("walk");
        agentAuthoring.SetDestination(destination);

        StartCoroutine(WaitForDestination());
    }

    private IEnumerator WaitForDestination()
    {
        while (agentAuthoring.EntityBody.IsStopped == false)
        {
            yield return null;
        }

        OnReachedDestination();
    }

    private void OnReachedDestination()
    {
        switch (currentState)
        {
            case NPCState.TowardAction:
                currentState = NPCState.Action;
                if (currentTarget != null)
                {
                    Vector3 targetPos = currentTarget.transform.position;

                    // Keep same Y as this object
                    targetPos.y = transform.position.y;

                    transform.LookAt(targetPos);
                }
                animator.SetTrigger("action");
                break;
            case NPCState.TowardEscalator:
                animator.SetTrigger("idle");
                targetEscalator.RequestUse(this);
                break;
            case NPCState.TowardElevator:
                animator.SetTrigger("idle");
                break;
            case NPCState.WaitingEscalator:
                break;
            case NPCState.WaitingEscalatorRow1:
                animator.SetTrigger("idle");
                agentAuthoring.enabled = false;
                agentNavMeshAuthoring.enabled = false;
                readyToUseEscalatorOrElevator = true;
                break;
            case NPCState.WaitingEscalatorRow2:
                animator.SetTrigger("idle");
                break;
            case NPCState.Walking:
                break;
            default:
                break;
        }
    }

    public void LeftEscalatorOrElevator(int newFloorLevel, Transform newPlatform)
    {
        currentPlatform = newPlatform;
        currentFloorlevel = newFloorLevel;

        agentAuthoring.enabled = true;
        agentNavMeshAuthoring.enabled = true;

        targetEscalator = null;
        targetEscalator = null;

        usingEscalatorOrElevator = false;

        if (currentTarget != null)
        {
            if (currentTarget.platform != currentPlatform)
            {
                TowardTarget(currentTarget);
            }
            else
            {
                SetDestination(currentTarget.transform.position, NPCState.TowardAction);
            }
        }
        else
        {
            Debug.LogWarning("No target assigned after leaving escalator/elevator");
        }
    }

    // Animation Event
    public void InteractTargetFinished()
    {
        FindRandomTarget();
    }
}

public enum NPCState
{
    Idle,
    Walking,
    TowardEscalator,
    TowardElevator,
    WaitingEscalator,
    WaitingEscalatorRow1,
    WaitingEscalatorRow2,
    UsingEscalator,
    UsingElevator,
    ExitEscalator,
    ExitElevator,
    TowardAction,
    Action
}
