using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AdventureSample/Quest Definition", fileName = "Quest_")]
public class QuestDefinition : ScriptableObject
{
    public string id;
    public string title;
    public List<QuestObjectiveDefinition> objectives;
}