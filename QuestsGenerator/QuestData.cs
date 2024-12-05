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
        
        public enum QuestType { ItemCollection }
    }
}