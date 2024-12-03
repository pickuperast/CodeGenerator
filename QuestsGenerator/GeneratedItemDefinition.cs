using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GeneratedItemDefinition", menuName = "ScriptableObjects/GeneratedItemDefinition", order = 1)]
public class GeneratedItemDefinition : ScriptableObject
{
    public Sprite Icon;
    public string ItemName;
    public int AmountRequired;
    public string ItemDescription;
    public string QuestGiverName;
    public int QuestGiverIdentity;
    public string QusetGiverFaction;
    public string QuestDescription;
    public string IconPrompt;
    
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
        return $"{GetNameDescriptionAmount()};{QuestGiverName};{QuestGiverIdentity};{QusetGiverFaction};{QuestDescription}";
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
        if (string.IsNullOrEmpty(QusetGiverFaction))
            return false;
        if (string.IsNullOrEmpty(QuestDescription))
            return false;
        return true;
    }
}
