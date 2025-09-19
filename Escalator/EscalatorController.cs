using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class EscalatorController : MonoBehaviour
{
    [SerializeField] bool isMovingUp = false;
    [SerializeField] bool standOnRight = false;
    [SerializeField] int entranceFloorLevel = 0;
    public int EntranceFloorLevel => entranceFloorLevel;
    [SerializeField] int exitFloorLevel = 1;
    public int ExitFloorLevel => exitFloorLevel;

    [SerializeField] Transform bottomEntrance;
    [SerializeField] Transform bottomEntranceConnect;
    [SerializeField] Collider bottomDetection;
    [SerializeField] Transform topEntrance;
    [SerializeField] Transform topEntranceConnect;
    [SerializeField] Collider topDetection;
    public Transform entrance => isMovingUp ? bottomEntrance : topEntrance;

    // ***For path validation, not implemented yet
    public Transform entranceConnect => isMovingUp ? bottomEntranceConnect : topEntranceConnect;
    public Transform exitConnect => isMovingUp ? topEntranceConnect : bottomEntranceConnect;

    [SerializeField] private List<WaitRow> topWaitRows = new List<WaitRow>();
    [SerializeField] private List<WaitRow> bottomWaitRows = new List<WaitRow>();

    List<NPCController> awaitingNPCs = new List<NPCController>();

    void Start()
    {
        Animation anim = GetComponent<Animation>();
        anim[anim.clip.name].speed = isMovingUp ? -1 : 1;
        topDetection.tag = isMovingUp ? "EscalatorExit" : "EscalatorEntrance";
        bottomDetection.tag = isMovingUp ? "EscalatorEntrance" : "EscalatorExit";
    }

    public void RequestUse(NPCController npc)
    {
        List<WaitRow> checkRows = isMovingUp ? bottomWaitRows : topWaitRows;

        for (int rowIndex = 0; rowIndex < checkRows.Count; rowIndex++)
        {
            var row = checkRows[rowIndex];

            // Determine slot order based on standOnRight
            WaitSlot firstSlot = standOnRight ? row.right : row.left;
            WaitSlot secondSlot = standOnRight ? row.left : row.right;

            if (TryAssignSlot(firstSlot, npc, rowIndex)) return;
            if (TryAssignSlot(secondSlot, npc, rowIndex)) return;
        }

        // No slot available, add to awaiting queue
        awaitingNPCs.Add(npc);
        npc.SetNPCState(NPCState.WaitingEscalator);
    }

    private bool TryAssignSlot(WaitSlot slot, NPCController npc, int rowIndex)
    {
        if (slot.occupant == null)
        {
            slot.occupant = npc;

            // Set NPC destination to slot
            switch (rowIndex)
            {
                case 0: npc.SetDestination(slot.point.position, NPCState.WaitingEscalatorRow1); break;
                case 1: npc.SetDestination(slot.point.position, NPCState.WaitingEscalatorRow2); break;
                default: Debug.LogWarning("Invalid row index"); break;
            }
            return true;
        }

        return false;
    }

    public void RowLaunched(Transform EscalatorStep)
    {
        List<WaitRow> checkRows = isMovingUp ? bottomWaitRows : topWaitRows;

        // Clear first row if occupants have already used escalator
        bool firstRowCleared = false;

        foreach (var slot in new WaitSlot[] { checkRows[0].left, checkRows[0].right })
        {
            if (slot.occupant != null && slot.occupant.ReadyToUseEscalatorOrElevator)
            {
                slot.occupant.transform.SetParent(EscalatorStep);
                slot.occupant.ReadyToUseEscalatorOrElevator = false;
                slot.occupant.UsingEscalatorOrElevator = true;
                slot.occupant.SetNPCState(NPCState.UsingEscalator);
                slot.occupant = null;
                firstRowCleared = true;
            }
        }

        if (!firstRowCleared) return; // no npc moved, nothing else to do

        // Move NPCs from second row to first row
        for (int i = 0; i < 2; i++)
        {
            WaitSlot firstRowSlot = (i == 0) ? checkRows[0].left : checkRows[0].right;
            WaitSlot secondRowSlot = (i == 0) ? checkRows[1].left : checkRows[1].right;

            if (firstRowSlot.occupant == null && secondRowSlot.occupant != null)
            {
                firstRowSlot.occupant = secondRowSlot.occupant;
                firstRowSlot.occupant.SetDestination(firstRowSlot.point.position, NPCState.WaitingEscalatorRow1);
                secondRowSlot.occupant = null;
            }
        }

        FillWaitingNPCs(); // handle remaining NPCs or assign new slots
    }

    private void FillWaitingNPCs()
    {
        if (awaitingNPCs.Count == 0) return;

        List<WaitRow> checkRows = isMovingUp ? bottomWaitRows : topWaitRows;

        for (int rowIndex = 0; rowIndex < checkRows.Count && awaitingNPCs.Count > 0; rowIndex++)
        {
            var row = checkRows[rowIndex];

            // Pick order depending on standOnRight
            WaitSlot first = standOnRight ? row.right : row.left;
            WaitSlot second = standOnRight ? row.left : row.right;

            // Try first slot
            if (first.occupant == null && awaitingNPCs.Count > 0)
            {
                NPCController npc = awaitingNPCs[0];
                first.occupant = npc;
                npc.SetDestination(first.point.position, GetRowState(rowIndex));
                awaitingNPCs.RemoveAt(0);
            }

            // Try second slot
            if (second.occupant == null && awaitingNPCs.Count > 0)
            {
                NPCController npc = awaitingNPCs[0];
                second.occupant = npc;
                npc.SetDestination(second.point.position, GetRowState(rowIndex));
                awaitingNPCs.RemoveAt(0);
            }
        }
    }

    private NPCState GetRowState(int rowIndex)
    {
        return rowIndex == 0 ? NPCState.WaitingEscalatorRow1 : NPCState.WaitingEscalatorRow2;
    }
}

[System.Serializable]
public class WaitRow
{
    public WaitSlot left;
    public WaitSlot right;
}

[System.Serializable]
public class WaitSlot
{
    public Transform point;            // where the NPC stands
    [HideInInspector] public NPCController occupant; // who’s standing here (runtime only)
}