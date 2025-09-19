using UnityEngine;

public class EscalatorDetection : MonoBehaviour
{
    [SerializeField] EscalatorController escalatorController;
    void OnTriggerEnter(Collider other)
    {
        if (transform.tag == "EscalatorEntrance" && other.CompareTag("EscalatorStep"))
        {
            escalatorController.RowLaunched(other.transform);
        }

        if (transform.tag == "EscalatorExit" && other.CompareTag("NPC"))
        {
            NPCController npc = other.GetComponent<NPCController>();
            if (npc != null && npc.UsingEscalatorOrElevator)
            {
                Debug.Log("NPC exited escalator, updating state");
                npc.transform.SetParent(null);
                npc.UsingEscalatorOrElevator = false;
                npc.LeftEscalatorOrElevator(escalatorController.ExitFloorLevel, escalatorController.exitConnect);
            }
        }
    }
}
