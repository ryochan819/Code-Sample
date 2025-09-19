using System.Linq;
using UnityEngine;

public class NPCPathManager : MonoBehaviour
{
    public static NPCPathManager Instance;
    [SerializeField] EscalatorController[] escalators;
    [SerializeField] InteractTarget[] interactTargets;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public InteractTarget GetRandomInteractTarget()
    {
        if (interactTargets.Length == 0) return null;

        int randomIndex = Random.Range(0, interactTargets.Length);
        return interactTargets[randomIndex];
    }

    public void GetTargetEscalator(int entranceFloor, int exitFloor, out EscalatorController escalator)
    {
        escalator = null;

        foreach (var esc in escalators)
        {
            if (esc.EntranceFloorLevel == entranceFloor && esc.ExitFloorLevel == exitFloor)
            {
                escalator = esc;
                return;
            }
        }

        // Special case: from -1 to 1 (or 1 to -1) → use 0 as middle
        // replace with validatepath later
        if (entranceFloor == -1 && exitFloor == 1)
        {
            escalator = escalators.FirstOrDefault(e => e.EntranceFloorLevel == -1 && e.ExitFloorLevel == 0);
            return;
        }
        if (entranceFloor == 1 && exitFloor == -1)
        {
            escalator = escalators.FirstOrDefault(e => e.EntranceFloorLevel == 1 && e.ExitFloorLevel == 0);
            return;
        }
    }

    // ***Add path validation later, npc validate path when not standing on the target platform
    public bool ValidatePath(int currentLevel, Transform currentPlatform, int targetLevel, Transform targetPlatform)
    {
        // First check if any escalator exit is connected to the target platform to list
        // Then check next level until return to current level
        // Return closest escalator path from list
        return true;
    }
}
