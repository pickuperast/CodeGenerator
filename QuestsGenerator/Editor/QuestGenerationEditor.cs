// Assets\Sanat\CodeGenerator\Editor\QuestGenerationEditor.cs:
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sanat.CodeGenerator.Agents;
using Sanat.CodeGenerator.ApiComfiUI;

namespace Sanat.CodeGenerator
{
    public class QuestGenerationEditor : EditorWindow
    {
        private GeneratedItemsHolder generatedItemsHolder;
        private ComfyUIImageProcessor comfyUIImageProcessor;
        private string itemsDefinitionsSaveFolder = "Assets/-ZombieRoyale/Generated/GeneratedItems";
        private string itemsIconsDefinitionsSaveFolder = "Assets/-ZombieRoyale/Generated/GeneratedItems/Icons";
        private string promptDescription = "";
        private Vector2 scrollPosition;
        private Vector2Int fromTo = Vector2Int.up;
        private CodeGenerator codeGeneratorWindow;
        private bool canGenerateItems = true;
        private float generateCooldownTime = 5f;
        private float generateCooldownTimer = 0f;
        private AbstractAgentHandler.ApiKeys _apiKeys;
        private const string GeneratedItemsHolderKey = "GeneratedItemsHolder";

        private void OnEnable()
        {
            codeGeneratorWindow = EditorWindow.GetWindow<CodeGenerator>();
            _apiKeys = new AbstractAgentHandler.ApiKeys(
                codeGeneratorWindow.openaiApiKey,
                codeGeneratorWindow.antrophicApiKey, 
                codeGeneratorWindow.groqApiKey, 
                codeGeneratorWindow.geminiApiKey);
            LoadGeneratedItemsHolder();
            EditorApplication.update += UpdateCooldown;
        }

        private void OnDisable()
        {
            SaveGeneratedItemsHolder();
            EditorApplication.update -= UpdateCooldown;
        }
        
        [MenuItem("Tools/Sanat/Quest Generation")]
        public static void ShowWindow()
        {
            GetWindow<QuestGenerationEditor>("Quest Generation");
        }

        private void OnGUI()
        {
            if (codeGeneratorWindow == null)
            {
                EditorGUILayout.HelpBox("Code Generator window is not open.", MessageType.Warning);
                return;
            }
            GUILayout.Label("Quest Generation", EditorStyles.boldLabel);
            
            generatedItemsHolder = EditorGUILayout.ObjectField("Generated Items Holder", generatedItemsHolder, typeof(GeneratedItemsHolder), false) as GeneratedItemsHolder;
            itemsDefinitionsSaveFolder = EditorGUILayout.TextField("Items Definitions Save Folder", itemsDefinitionsSaveFolder);
            comfyUIImageProcessor = EditorGUILayout.ObjectField("Comfy UI Image Processor", comfyUIImageProcessor, typeof(ComfyUIImageProcessor), false) as ComfyUIImageProcessor;
            GUILayout.BeginHorizontal();
            fromTo = EditorGUILayout.Vector2IntField("From To (items index)", fromTo);

            // Add +50 button to the right of fromTo field
            if (GUILayout.Button("+50", GUILayout.Width(50)))
            {
                fromTo.x += 50;
                fromTo.y += 50;
            }
            GUILayout.EndHorizontal();
            
            promptDescription = EditorGUILayout.TextField("Prompt Description", promptDescription, GUILayout.Height(100));

            EditorGUI.BeginDisabledGroup(!canGenerateItems);
            if (GUILayout.Button("Generate Items"))
            {
                GenerateItems();
                StartCooldown();
            }
            if (GUILayout.Button("Generate Icon Prompts for Items"))
            {
                GenerateIconsPrompts();
                StartCooldown();
            }
            if (GUILayout.Button("Generate Icon for Items"))
            {
                GenerateIcons();
                StartCooldown();
            }
            if (GUILayout.Button("Generate Quest Giver And quest Description"))
            {
                GenerateQuestGiverAndQuestDescription();
                StartCooldown();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void GenerateQuestGiverAndQuestDescription()
        {
            Vector2Int fromToStored = fromTo;
            int stepSize = 3;
            if (fromToStored.y - fromToStored.x > stepSize) {
                Debug.Log($"Generating in batches of {stepSize}. Total requests: {(fromToStored.y - fromToStored.x) / stepSize + 1}");
                for (int i = fromToStored.x; i <= fromToStored.y; i += stepSize) {
                    Vector2Int fromToTemp = new Vector2Int(i, i + stepSize - 1);
                    DoQuestGeneration(fromToTemp, () => Debug.Log($"Generated from {fromToTemp.x} to {fromToTemp.y}"));
                }
            }
            else {
                DoQuestGeneration(fromToStored);
            }
        }

        private void DoQuestGeneration(Vector2Int fromToTemp, Action action = null)
        {
            string task = GenerateItemsPromptNameDescriptionAmount(fromToTemp);
            string existingQuests = GetAlreadyExistingQuests(fromToTemp);
            AgentQuestWriter agentItemsGenerator = new AgentQuestWriter(_apiKeys, task, existingQuests);
            agentItemsGenerator.OnComplete = (result) =>
            {
                Debug.Log($"Generated answer: {result}");
                bool isFirstAndLastIsQuote = result.Substring(0, 1) == "\"" && result.Substring(result.Length - 1, 1) == "\"";
                if (isFirstAndLastIsQuote) {
                    result = result.Substring(1, result.Length - 2);
                }
                InsertQuestsFromResult(result, fromToTemp);
            };
            agentItemsGenerator.Handle("");
        }

        private void GenerateIcons()
        {
            ComfyUIImageProcessor.GenerateImageWithComfyUI("llama on grass", "icons_generator_api", itemsIconsDefinitionsSaveFolder);
        }

        private void GenerateIconsPrompts() {
            Vector2Int fromToStored = fromTo;
            if (fromToStored.y - fromToStored.x > 50) {
                Debug.Log($"Generating in batches of 50. Total requests: {(fromToStored.y - fromToStored.x) / 50 + 1}");
                for (int i = fromToStored.x; i <= fromToStored.y; i += 50) {
                    Vector2Int fromToTemp = new Vector2Int(i, i + 49);
                    DoGeneration(fromToTemp, () => Debug.Log($"Generated from {fromToTemp.x} to {fromToTemp.y}"));
                }
            }
            else {
                DoGeneration(fromToStored);
            }
        }

        private void DoGeneration(Vector2Int fromToStored, Action onComplete = null) {
            string task = GenerateIconPrompsTask(fromToStored);
            AgentDescriptionToPrompt agentItemsGenerator = new AgentDescriptionToPrompt(_apiKeys, task);
            agentItemsGenerator.OnComplete = (result) =>
            {
                Debug.Log($"Generated answer: {result}");
                bool isFirstAndLastIsQuote = result.Substring(0, 1) == "\"" && result.Substring(result.Length - 1, 1) == "\"";
                if (isFirstAndLastIsQuote) {
                    result = result.Substring(1, result.Length - 2);
                }
                GenerateItemsPromptsFromResult(result, fromToStored);
            };
            agentItemsGenerator.Handle("");
        }

        private string GenerateIconPrompsTask(Vector2Int fromToStored) {
            string task = $"Generate icon prompts for items: ";
            
            for (int i = fromToStored.x; i <= fromToStored.y; i++) {
                var item = generatedItemsHolder.GeneratedItems[i];
                if (item.IsItemInvalid()) continue;
                var nameWithDescription = $"{item.ItemName}. {item.ItemDescription}";
                task += nameWithDescription;
                if (i != fromToStored.y) {
                    task += ";";
                }
            }
            return task;
        }

        private string GetAlreadyExistingQuests(Vector2Int fromToStored)
        {
            if (fromToStored.x <= 0)
                return String.Empty;
            
            string task = $"";
            
            for (int i = 0; i < fromToStored.x; i++) {
                var item = generatedItemsHolder.GeneratedItems[i];
                if (!item.IsValidForQuestTask()) continue;
                var nameWithDescription = $"{item.GetQuestDescription()}";
                task += nameWithDescription;
                if (i != fromToStored.y) {
                    task += ";";
                }
            }
            return task;
        }

        private string GenerateItemsPromptNameDescriptionAmount(Vector2Int fromToStored) {
            string task = $"Generate quests for items:";
            
            for (int i = fromToStored.x; i <= fromToStored.y; i++) {
                var item = generatedItemsHolder.GeneratedItems[i];
                if (item.IsItemInvalid()) continue;
                var nameWithDescription = $"{item.GetNameDescriptionAmount()}";
                task += nameWithDescription;
                if (i != fromToStored.y) {
                    task += ";";
                }
            }
            return task;
        }

        private void GenerateItems()
        {
            AgentItemsGenerator agentItemsGenerator = new AgentItemsGenerator(_apiKeys, promptDescription, GetAlreadyExistingItems());
            
            agentItemsGenerator.OnComplete = (result) =>
            {
                Debug.Log($"Items generated: {result}");
                GenerateItemsFromResult(result);
            };
            agentItemsGenerator.Handle("");
        }
        
        

        private void InsertQuestsFromResult(string result, Vector2Int fromToStored)
        {
            string[] items = result.Split(';');
            List<GeneratedItemDefinition> itemsToChange = new List<GeneratedItemDefinition>();
            for (int i = fromToStored.x; i <= fromToStored.y; i++) {
                itemsToChange.Add(generatedItemsHolder.GeneratedItems[i]);
            }
            bool isLastInvalid = items[items.Length - 1] == "" || items[items.Length - 1] == " ";
            int promptsCount = isLastInvalid ? items.Length - 1 : items.Length;
            if (promptsCount != itemsToChange.Count * 4) {
                Debug.LogError($"Prompts count({promptsCount}) doesn't match items count({itemsToChange.Count * 4}) fromToStored: {fromToStored}");
                return;
            }
            try
            {
                AssetDatabase.StartAssetEditing();
                
                int currentIndex = fromToStored.x;
                for (int i = 0; i < items.Length; i += 4)
                {
                    generatedItemsHolder.GeneratedItems[currentIndex].QuestGiverName = ClearNameFromUselessChars(items[i]);
                    generatedItemsHolder.GeneratedItems[currentIndex].QuestGiverIdentity = int.TryParse(items[i + 1], out int amount) ? amount : 1;
                    generatedItemsHolder.GeneratedItems[currentIndex].QusetGiverFaction = items[i + 2].Replace("\"", "");
                    generatedItemsHolder.GeneratedItems[currentIndex].QuestDescription = items[i + 3].Replace("\"", "");
                    EditorUtility.SetDirty(generatedItemsHolder.GeneratedItems[currentIndex]);
                }
                AssetDatabase.SaveAssets();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            Debug.Log($"{itemsToChange.Count} prompts inserted from {fromToStored.x} - {fromToStored.y}");
        }

        private void GenerateItemsPromptsFromResult(string result, Vector2Int fromToStored) {
            string[] prompts = result.Split(';');
            List<GeneratedItemDefinition> itemsToChange = new List<GeneratedItemDefinition>();
            for (int i = fromToStored.x; i <= fromToStored.y; i++) {
                itemsToChange.Add(generatedItemsHolder.GeneratedItems[i]);
            }
            
            bool isLastInvalid = prompts[prompts.Length - 1] == "" || prompts[prompts.Length - 1] == " ";
            int promptsCount = isLastInvalid ? prompts.Length - 1 : prompts.Length;
            if (promptsCount != itemsToChange.Count) {
                Debug.LogError($"Prompts count({prompts.Length}) doesn't match items count({itemsToChange.Count}) fromToStored: {fromToStored}");
                return;
            }
            
            try
            {
                AssetDatabase.StartAssetEditing();
                for (int i = 0; i < promptsCount; i++) {
                    itemsToChange[i].IconPrompt = ClearNameFromUselessChars(prompts[i]);
                    EditorUtility.SetDirty(itemsToChange[i]);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            Debug.Log($"{itemsToChange.Count} prompts inserted from {fromToStored.x} - {fromToStored.y}");
        }

        private void GenerateItemsFromResult(string result)
        {
            string[] items = result.Split(';');
            
            try
            {
                AssetDatabase.StartAssetEditing();

                for (int i = 0; i < items.Length; i += 3)
                {
                    GeneratedItemDefinition newItem = CreateInstance<GeneratedItemDefinition>();
                    newItem.ItemName = ClearNameFromUselessChars(items[i]);
                    newItem.AmountRequired = int.TryParse(items[i + 1], out int amount) ? amount : 1;
                    newItem.ItemDescription = items[i + 2].Replace("\"", "");
                
                    string fileName = newItem.ItemName.Replace(" ", "_");
                    string assetPath = $"{itemsDefinitionsSaveFolder}/GeneratedItem_{fileName}_{newItem.AmountRequired}.asset";
                    AssetDatabase.CreateAsset(newItem, assetPath);
                    AssetDatabase.SaveAssets();

                    if (generatedItemsHolder != null)
                    {
                        generatedItemsHolder.GeneratedItems.Add(newItem);
                        EditorUtility.SetDirty(generatedItemsHolder);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private string GetAlreadyExistingItems()
        {
            string existingItems = String.Empty;
            foreach (GeneratedItemDefinition item in generatedItemsHolder.GeneratedItems)
            {
                existingItems += item.ItemName + ";";
            }

            return existingItems;
        }

        private string ClearNameFromUselessChars(string item)
        {
            string result = item.Replace("\"", "");
            if (result.Substring(0, 1) == " ")
            {
                result = result.Substring(1);
            }
            if (result.Substring(result.Length - 1, 1) == " ")
            {
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }
        
        private void UpdateCooldown()
        {
            if (!canGenerateItems)
            {
                generateCooldownTimer -= Time.deltaTime;
                if (generateCooldownTimer <= 0f)
                {
                    canGenerateItems = true;
                }
            }
        }

        private void StartCooldown()
        {
            canGenerateItems = false;
            generateCooldownTimer = generateCooldownTime;
        }
        
        private void SaveGeneratedItemsHolder()
        {
            if (generatedItemsHolder != null)
            {
                // string json = JsonUtility.ToJson(generatedItemsHolder);
                // PlayerPrefs.SetString(GeneratedItemsHolderKey, json);
                // PlayerPrefs.Save();
            }
        }

        private void LoadGeneratedItemsHolder()
        {
            // if (PlayerPrefs.HasKey(GeneratedItemsHolderKey))
            // {
            //     string json = PlayerPrefs.GetString(GeneratedItemsHolderKey);
            //     generatedItemsHolder = JsonUtility.FromJson<GeneratedItemsHolder>(json);
            // }
        }
    }
}