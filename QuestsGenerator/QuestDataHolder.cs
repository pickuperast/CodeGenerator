using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sanat.CodeGenerator.QuestGenerator
{
    //[CreateAssetMenu(fileName = "ADVGQuestDataHolderSO", menuName = "ScriptableObjects/QuestDataHolder", order = 0)]
    public class QuestDataHolder : ScriptableObject
    {
        public List<QuestData> quests;
        private Dictionary<int, QuestData> questLookup;

        private void OnEnable()
        {
            questLookup = quests.ToDictionary(q => q.Id, q => q);
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
}