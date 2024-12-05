using UnityEngine;

[CreateAssetMenu(fileName = "GeneratedItemDefinition", menuName = "ScriptableObjects/GeneratedItemDefinition", order = 1)]
public class GeneratedItemDefinition : ScriptableObject
{
    public Sprite Icon;
    public string ItemName;
    public int AmountRequired;
    public string ItemDescription;
    public string QuestGiverName;
    public int QuestGiverIdentity;
    public string QuestGiverFaction;
    public string QuestDescription;
    public string IconPrompt;

    public struct RelevantInfoForNextGeneration
    {
        public string ItemName;
        public string QuestGiverName;
        public int QuestGiverIdentity;
        public string QuestGiverFaction;
        public string QuestDescription;
    }

    public void FillValues(GeneratedItemDefinition data)
    {
        ItemName = data.ItemName;
        AmountRequired = data.AmountRequired;
        ItemDescription = data.ItemDescription;
        QuestGiverName = data.QuestGiverName;
        QuestGiverIdentity = data.QuestGiverIdentity;
        QuestGiverFaction = data.QuestGiverFaction;
        QuestDescription = data.QuestDescription;
        IconPrompt = data.IconPrompt;
    }
    
    public struct AddNewQuestData
    {
        public string QuestGiverName;
        public int QuestGiverIdentity;
        public string QusetGiverFaction;
        public string QuestDescription;
    }
    
    public string GetNameDescriptionAmount()
    {
        return $"{ItemName};{ItemDescription};{AmountRequired}";
    }
    
    public string GetQuestDescription()
    {
        return $"{GetNameDescriptionAmount()};{QuestGiverName};{QuestGiverIdentity};{QuestGiverFaction};{QuestDescription}";
    }
    public bool IsItemInvalid()
    {
        if (ItemName == null)
        {
            Debug.LogError($"Item name at index is null");
            return true;
        }
        if (ItemDescription == null)
        {
            Debug.LogError($"Item description at {ItemName} is null");
            return true;
        }

        return false;
    }
    
    public bool IsValidForQuestTask()
    {
        if (AmountRequired < 0)
            return false;
        if (string.IsNullOrEmpty(ItemName))
            return false;
        if (string.IsNullOrEmpty(ItemDescription))
            return false;
        if (string.IsNullOrEmpty(QuestGiverName))
            return false;
        if (string.IsNullOrEmpty(QuestGiverFaction))
            return false;
        if (string.IsNullOrEmpty(QuestDescription))
            return false;
        return true;
    }
}
