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
        // replace with Dijkstra Algorithm later
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

    // ***Find shortest path with Dijkstra Algorithm, weight affected by current queue count
    public bool GetNpcPath(int currentLevel, Transform currentPlatform, int targetLevel, Transform targetPlatform)
    {
        // cache path result to reduce calls
        // npc check caches first and find optimal options to reach each platform
        return true;
    }
}
