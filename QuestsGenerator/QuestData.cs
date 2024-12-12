using Invector.vItemManager;
using UnityEngine;

namespace Sanat.CodeGenerator.QuestGenerator
{
    [CreateAssetMenu(fileName = "ADVGQuestData", menuName = "ScriptableObjects/QuestData", order = 0)]
    public class QuestData : ScriptableObject
    {
        public int Id;
        public string Name;
        public string Description;
        public Sprite questImage;
        public vItem itemTarget;
        
        [SerializeField] private GeneratedItemDefinition generatedItem;
        public GeneratedItemDefinition GeneratedItem => generatedItem;

        public enum QuestType { ItemCollection }

        public void SetGeneratedItem(GeneratedItemDefinition item)
        {
            generatedItem = item;
            if (item != null)
            {
                Name = $"Collect {item.AmountRequired} {item.ItemName}";
                Description = item.QuestDescription;
                questImage = item.Icon;
            }
        }

        public bool IsValid()
        {
            return Id > 0 
                && !string.IsNullOrEmpty(Name) 
                && !string.IsNullOrEmpty(Description) 
                && questImage != null 
                && generatedItem != null;
        }

        public int GetRequiredAmount()
        {
            return generatedItem != null ? generatedItem.AmountRequired : 0;
        }

        public string GetItemName()
        {
            return generatedItem != null ? generatedItem.ItemName : string.Empty;
        }

        public (string name, int identity, string faction) GetQuestGiverInfo()
        {
            if (generatedItem == null)
                return (string.Empty, 0, string.Empty);

            return (generatedItem.QuestGiverName, 
                   generatedItem.QuestGiverIdentity, 
                   generatedItem.QuestGiverFaction);
        }
    }
}