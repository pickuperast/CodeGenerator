using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Invector.vItemManager;
using Sanat.CodeGenerator.QuestGenerator;

[CreateAssetMenu(fileName = "QuestDataHolder", menuName = "ScriptableObjects/QuestDataHolder")]
public class QuestDataHolder : ScriptableObject
{
    [SerializeField] public GeneratedItemsHolder generatedItemsHolder;
    [SerializeField] public vItemListData vItemListData;
    public List<QuestData> quests = new List<QuestData>();
    private Dictionary<int, QuestData> questLookup = new Dictionary<int, QuestData>();

    private void OnEnable()
    {
        questLookup = quests.ToDictionary(q => q.Id, q => q);
    }

    public QuestData CreateQuestFromGeneratedItem(GeneratedItemDefinition generatedItem, string savePath)
    {
        var quest = CreateInstance<QuestData>();
        quest.Id = GetNextQuestId();
        
        quest.SetGeneratedItem(generatedItem);
        
        if (!string.IsNullOrEmpty(savePath))
        {
#if UNITY_EDITOR
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(savePath, $"Quest_{generatedItem.ItemName}_{generatedItem.AmountRequired}.asset"));
            AssetDatabase.CreateAsset(quest, uniquePath);
            AssetDatabase.SaveAssets();
#endif
        }
        
        AddQuest(quest);
        return quest;
    }

    private int GetNextQuestId()
    {
        return quests.Count > 0 ? quests.Max(q => q.Id) + 1 : 1;
    }

    public QuestData GetQuestById(int questId)
    {
        return questLookup.TryGetValue(questId, out var quest) ? quest : null;
    }

    public QuestData[] GetRandomQuests(int count)
    {
        return quests.OrderBy(x => Random.value).Take(count).ToArray();
    }

    public int[] GetRandomQuestIds(int count)
    {
        return GetRandomQuests(count).Select(q => q.Id).ToArray();
    }

    public bool HasQuest(int questId)
    {
        return questLookup.ContainsKey(questId);
    }

    public int GetQuestCount()
    {
        return quests.Count;
    }

    public void AddQuest(QuestData quest)
    {
        if (!HasQuest(quest.Id))
        {
            quests.Add(quest);
            questLookup[quest.Id] = quest;
        }
    }

    public bool RemoveQuest(int questId)
    {
        var quest = GetQuestById(questId);
        if (quest != null)
        {
            quests.Remove(quest);
            return questLookup.Remove(questId);
        }
        return false;
    }

    public QuestData[] GetQuestsByIds(int[] questIds)
    {
        return questIds.Select(id => GetQuestById(id)).Where(q => q != null).ToArray();
    }

    public void Clear()
    {
        quests.Clear();
        questLookup.Clear();
    }
}