using UnityEngine;
using ProjectDawn.Navigation.Hybrid;

public class AgentSetDestination : MonoBehaviour
{
    public Transform Target;
    void Start()
    {
        GetComponent<AgentAuthoring>().SetDestination(Target.position);

        GetComponent<Animator>().SetTrigger("walk");
        Debug.Log("Destination Set: " + Target.position);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered by: " + other.gameObject.name);
    }
}