using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace CarterVerseM.interact
{
    public class interact_RoombaAI : MonoBehaviour
    {
        [SerializeField] interact_NPCInteractor interactor;
        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] GameObject playerObject;
        Coroutine currentAI;
        float pathFindingTimer = 5f;
        float roamingDistance = 5f; 
        float roamInterval = 4f;
        bool isInteracting;
        bool isMoving;
        bool targetPositionCreated;
        Vector3 targetPosition;
        Vector3 direction;
        Quaternion targetRotation;
        public float rotationSpeed = 0.1f;
        public float rotationThreshold = 0.1f;

        private void Start()
        {
            currentAI = StartCoroutine(Roaming());
        }

        private void OnMouseDown()
        {
            if (currentAI != null)
            {
                StopCoroutine(currentAI);
            }
            isInteracting = true;
            targetPositionCreated = false;
            isMoving = false;
            if (navMeshAgent.hasPath) { navMeshAgent.ResetPath(); }
            playerObject.GetComponent<interact_PlayerInteractor>().InteractWithPartyMember(interactor, true);
        }

        IEnumerator Roaming()
        {
            while (!isInteracting)
            {
                if (!isMoving)
                {
                    if (!targetPositionCreated)
                    {
                        pathFindingTimer = 5f;
                        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized * roamingDistance;
                        targetPosition = transform.position + new Vector3(randomDirection.x, 0, randomDirection.y);

                        direction = targetPosition - transform.position;

                        targetRotation = Quaternion.LookRotation(direction);

                        targetPositionCreated = true;
                    }
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                    if (Quaternion.Angle(transform.rotation, targetRotation) < rotationThreshold)
                    {
                        navMeshAgent.enabled = true;
                        navMeshAgent.SetDestination(targetPosition);
                        isMoving = true;
                    }
                }
                else
                {
                    pathFindingTimer -= Time.deltaTime;
                    if (pathFindingTimer < 0)
                    {
                        isMoving = false;
                        navMeshAgent.ResetPath();
                    }

                    if (CheckIfReachedDestination())
                    {
                        targetPositionCreated = false;
                        isMoving = false;
                        navMeshAgent.ResetPath();
                        navMeshAgent.enabled = false;
                    }
                }
                yield return null;
            }
        }

        private bool CheckIfReachedDestination()
        {
            // Check if we've reached the destination
            if (!navMeshAgent.pathPending)
            {
                // stopping distance should be > 0, otherwise it is very likely to stuck and cause infinite loop
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f)
                    {
                        // Done
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else { return false; }
            }
            else { return false; }
        }

        public void InteractReset()
        {
            Debug.Log("Reset");
            targetPositionCreated = false;
            isInteracting = false;
            isMoving = false;
            navMeshAgent.ResetPath();
            currentAI = StartCoroutine(Roaming());
        }
    }
}