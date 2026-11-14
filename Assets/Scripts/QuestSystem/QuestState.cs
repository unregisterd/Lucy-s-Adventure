using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectiveProgress
{
    public int currentCount;
}

[System.Serializable]
public class QuestState
{
    public string questId;
    public bool accepted;
    public bool completed;
    public List<ObjectiveProgress> progress;   // 与 definition.objectives 一一对应，同下标
    [System.NonSerialized] public QuestDefinition definition;
}