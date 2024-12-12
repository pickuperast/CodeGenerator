using UnityEngine;
using UnityEditor;
using System.IO;
using Sanat.CodeGenerator.QuestGenerator;

[CustomEditor(typeof(QuestDataHolder))]
public class QuestDataHolderEditor : Editor
{
    private bool showGenerateQuestsButton = true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        QuestDataHolder questDataHolder = (QuestDataHolder)target;

        if (questDataHolder.generatedItemsHolder == null)
        {
            EditorGUILayout.HelpBox("Please assign a GeneratedItemsHolder to create quests.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space(10);
        showGenerateQuestsButton = EditorGUILayout.Foldout(showGenerateQuestsButton, "Quest Generation Tools");

        if (showGenerateQuestsButton)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button("Generate Quests from Generated Items"))
            {
                GenerateQuestsFromItems(questDataHolder);
            }

            if (GUILayout.Button("Clear All Quests"))
            {
                if (EditorUtility.DisplayDialog("Clear All Quests",
                    "Are you sure you want to delete all quests? This action cannot be undone.",
                    "Yes, Clear All", "Cancel"))
                {
                    ClearAllQuests(questDataHolder);
                }
            }
            
            if (GUILayout.Button("Create Quest Items"))
            {
                if (questDataHolder.vItemListData != null)
                {
                    questDataHolder.vItemListData.CreateQuestItems(questDataHolder);
                }
                else
                {
                    Debug.LogError("vItemListData reference is missing in QuestDataHolder!");
                }
            }

            EditorGUILayout.EndVertical();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(questDataHolder);
        }
    }

    private void GenerateQuestsFromItems(QuestDataHolder questDataHolder)
    {
        if (questDataHolder.generatedItemsHolder.GeneratedItems == null || 
            questDataHolder.generatedItemsHolder.GeneratedItems.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No generated items found!", "OK");
            return;
        }

        string holderPath = AssetDatabase.GetAssetPath(questDataHolder);
        string questsDirectory = Path.Combine(Path.GetDirectoryName(holderPath), "Quests");

        if (!Directory.Exists(questsDirectory))
        {
            Directory.CreateDirectory(questsDirectory);
        }

        AssetDatabase.StartAssetEditing();

        try
        {
            foreach (var generatedItem in questDataHolder.generatedItemsHolder.GeneratedItems)
            {
                if (generatedItem != null && generatedItem.IsValidForQuestTask())
                {
                    QuestData quest = questDataHolder.CreateQuestFromGeneratedItem(
                        generatedItem, 
                        questsDirectory
                    );

                    if (quest != null)
                    {
                        Debug.Log($"Created quest: {quest.Name} with ID: {quest.Id}");
                    }
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("Success", 
            $"Generated {questDataHolder.GetQuestCount()} quests successfully!", "OK");
    }

    private void ClearAllQuests(QuestDataHolder questDataHolder)
    {
        string holderPath = AssetDatabase.GetAssetPath(questDataHolder);
        string questsDirectory = Path.Combine(Path.GetDirectoryName(holderPath), "Quests");

        if (Directory.Exists(questsDirectory))
        {
            AssetDatabase.StartAssetEditing();

            try
            {
                string[] questFiles = Directory.GetFiles(questsDirectory, "Quest_*.asset");
                foreach (string questFile in questFiles)
                {
                    AssetDatabase.DeleteAsset(questFile.Replace('\'', '/'));
                }

                questDataHolder.Clear();
                EditorUtility.SetDirty(questDataHolder);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        Debug.Log("All quests have been cleared.");
    }
}