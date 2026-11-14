using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestObjectiveKind { Collect, Talk, Interact }

[System.Serializable]
public class QuestObjectiveDefinition
{
    public QuestObjectiveKind kind;
    public string targetId;        // "wood_stick" / "npc_blacksmith"
    public int requiredCount;      // Collect 用；Talk/Interact 填 1
}