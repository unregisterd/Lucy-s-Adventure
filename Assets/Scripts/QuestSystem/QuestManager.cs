using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private QuestDefinition testQuest;   // 临时用，验收时拖一个进来
    private List<QuestState> activeQuests = new List<QuestState>();   // 做过的任务（声明处就建好，OnEnable 阶段已可用）

    private void Start()
    {
        AcceptQuest(testQuest);   // 临时用，验收时删掉
    }

    private void OnEnable()
    {
        // 订阅 GameEvents.ItemAdded
        EventManager<string>.StartListening(GameEvents.ItemAdded, OnItemAdded);   // 参数是 itemId
    }

    private void OnDisable()
    {
        EventManager<string>.StopListening(GameEvents.ItemAdded, OnItemAdded);   // 退订，和订阅成对出现
    }

    private void OnItemAdded(string itemId)
    {
        bool anyQuestMatched = false;   // 这一下有没有任何任务的目标匹配上，专门用来抓 targetId 拼错

        foreach (var quest in activeQuests)
        {
            if (quest.completed) continue;   // 已完成的任务不再吃进度
            bool advanced = false;           // 这个任务本轮有没有推进

            var definition = quest.definition;
            for (int i = 0; i < definition.objectives.Count; i++)
            {
                var objective = definition.objectives[i];
                if (objective.kind != QuestObjectiveKind.Collect) continue;
                if (objective.targetId != itemId) continue;

                anyQuestMatched = true;
                if (quest.progress[i].currentCount >= objective.requiredCount) continue;   // 已经满了，不再加

                quest.progress[i].currentCount++;
                advanced = true;
                Debug.Log($"Quest {definition.title}: Collected {quest.progress[i].currentCount}/{objective.requiredCount} {itemId}");

                if (quest.progress[i].currentCount >= objective.requiredCount)
                {
                    Debug.Log($"Objective {i} of quest {definition.title} completed!");
                }
            }

            if (advanced && AllObjectivesMet(quest))
            {
                quest.completed = true;
                Debug.Log($"Quest {definition.title} completed!");
            }
        }

        if (!anyQuestMatched)
        {
            Debug.Log($"[Quest] {itemId} 没有匹配任何 Collect 目标 —— 检查 targetId 是否拼错");
        }
    }

    private bool AllObjectivesMet(QuestState quest)
    {
        var definition = quest.definition;
        for (int j = 0; j < definition.objectives.Count; j++)
        {
            if (quest.progress[j].currentCount < definition.objectives[j].requiredCount)
            {
                return false;
            }
        }
        return true;
    }

    public void AcceptQuest(QuestDefinition definition)
    {
        if (definition == null)
        {
            Debug.LogError("QuestDefinition is null!");
            return;
        }
        if (definition.objectives == null || definition.objectives.Count == 0)
        {
            Debug.LogError($"Quest {definition.id}：objectives 是空的，这个任务接不了");
            return;
        }
        foreach (var objective in definition.objectives)
        {
            if (objective.requiredCount <= 0)
                Debug.LogWarning($"Quest {definition.id}：targetId={objective.targetId} 的 requiredCount={objective.requiredCount}，这个目标永远推进不了");
            if (string.IsNullOrEmpty(objective.targetId))
                Debug.LogWarning($"Quest {definition.id}：有目标没填 targetId");
        }

        QuestState newQuest = new QuestState();
        newQuest.questId = definition.id;
        newQuest.accepted = true;
        newQuest.completed = false;
        newQuest.progress = new List<ObjectiveProgress>();
        foreach (var objective in definition.objectives)
        {
            ObjectiveProgress progress = new ObjectiveProgress();
            progress.currentCount = 0;
            newQuest.progress.Add(progress);
        }
        newQuest.definition = definition;   // 运行时引用，不进存档

        activeQuests.Add(newQuest);
        Debug.Log("Quest accepted: " + definition.name);
    }
}
