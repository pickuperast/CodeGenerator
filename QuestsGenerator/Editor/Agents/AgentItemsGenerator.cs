// Copyright (c) Sanat. All rights reserved.
using System;
using Newtonsoft.Json;
using Sanat.ApiGemini;
using Sanat.ApiOpenAI;
using UnityEditor;
using UnityEngine;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentItemsGenerator : AbstractAgentHandler
    {
        private string _prompt;

        protected override string PromptFilename() => "PromptGenerateRpgItems.md";
        protected override ApiOpenAI.Model GetModel() => ApiOpenAI.Model.GPT4omini;
        protected override string GetGeminiModel() => ApiGemini.Model.Pro.Name;
        private const string LOCAL_PROMPTS_FOLDER_PATH = "/Sanat/CodeGenerator/QuestsGenerator/Editor/Prompts/";
        
        public AgentItemsGenerator(ApiKeys apiKeys, string task, string alreadyExistingItems)
        {
            Name = "AgentItemNamesGenerator";
            Description = "Generates items";
            Temperature = 1.0f;
            StoreKeys(apiKeys);
            string promptLocation = Application.dataPath + $"{LOCAL_PROMPTS_FOLDER_PATH}{PromptFilename()}";
            PromptFromMdFile = LoadPrompt(promptLocation);
            if (alreadyExistingItems != String.Empty)
                alreadyExistingItems = "# Already existing items: " + alreadyExistingItems;
            _prompt = $"{PromptFromMdFile} # TASK: {task}. " + alreadyExistingItems;
            SelectedApiProvider = ApiProviders.OpenAI;
            _modelName = GetModel().Name;
        }
        
        protected ToolFunction GetFunctionData_OpenaiSplitCodeToFilePathes()
        {
            string description = "Inserts new items into the file.";
            string name = "InsertNewItem";
            string propertyItemName = "ItemName";
            string propertyItemDescription = "ItemDescription";
            string propertyQuestGiverName = "QuestGiverName";
            string propertyQuestGiverIdentity = "QuestGiverIdentity";
            string propertyQuestGiverFaction = "QuestGiverFaction";
            string propertyQuestDescription = "QuestDescription";
            string propertyIconPrompt = "IconPrompt";
            string propertyAmountRequired = "AmountRequired";
            var strType = DataTypes.STRING;

            Parameter parameters = new ();
            parameters.AddProperty(propertyItemName, strType, $"AI must generate item name");
            parameters.AddProperty(propertyItemDescription, strType, $"AI must provide FULL item description");
            parameters.AddProperty(propertyQuestGiverName, strType, $"AI must generate quest giver name that can give such item");
            parameters.AddProperty(propertyQuestGiverIdentity, DataTypes.NUMBER, $"AI must generate quest giver identity 0=Male, 1=Female");
            parameters.AddProperty(propertyQuestGiverFaction, strType, $"AI must generate quest giver faction");
            parameters.AddProperty(propertyAmountRequired, DataTypes.NUMBER, $"AI must generate how many items player should bring to fulfill the quest");
            parameters.AddProperty(propertyQuestDescription, strType, $"AI must generate quest description");
            parameters.AddProperty(propertyIconPrompt, strType, $"AI must generate icon prompt");
            parameters.Required.Add(propertyItemName);
            parameters.Required.Add(propertyItemDescription);
            parameters.Required.Add(propertyQuestGiverName);
            parameters.Required.Add(propertyQuestGiverIdentity);
            parameters.Required.Add(propertyQuestGiverFaction);
            parameters.Required.Add(propertyAmountRequired);
            parameters.Required.Add(propertyQuestDescription);
            parameters.Required.Add(propertyIconPrompt);
            ToolFunction function = new ToolFunction(name, description, parameters);
            return function;
        }
        
        private void GenerateItemsFromResult(ToolCalls[] toolCallsArray)
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                int filesAmount = toolCallsArray.Length;
                GeneratedItemDefinition[] items = new GeneratedItemDefinition[filesAmount];
                for (int i = 0; i < filesAmount; i++)
                {
                    items[i] = JsonConvert.DeserializeObject<GeneratedItemDefinition>(toolCallsArray[i].function.arguments);
                    GeneratedItemDefinition newItem = ScriptableObject.CreateInstance<GeneratedItemDefinition>();
                    newItem.FillValues(items[i]);
                    
                
                    string fileName = newItem.ItemName.Replace(" ", "_");
                    string assetPath = $"{QuestGenerationEditor.itemsDefinitionsSaveFolder}/GeneratedItem_{fileName}_{newItem.AmountRequired}.asset";
                    AssetDatabase.CreateAsset(newItem, assetPath);
                    AssetDatabase.SaveAssets();

                    if (QuestGenerationEditor.generatedItemsHolder != null)
                    {
                        QuestGenerationEditor.generatedItemsHolder.GeneratedItems.Add(newItem);
                        EditorUtility.SetDirty(QuestGenerationEditor.generatedItemsHolder);
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        public override void Handle(string input)
        {
            Debug.Log($"<color=purple>{Name}</color> asking: {_prompt}");
            BotParameters botParameters = new BotParameters(_prompt, SelectedApiProvider, Temperature, null, _modelName, true);
            var openaiTools = new ApiOpenAI.Tool[] { new ("function", GetFunctionData_OpenaiSplitCodeToFilePathes()) };
            botParameters.openaiTools = openaiTools;
            botParameters.systemMessage = PromptFromMdFile;
            botParameters.isToolUse = true;
            botParameters.onOpenaiChatResponseComplete += (response) =>
            {
                if (response.choices[0].finish_reason == "tool_calls")
                {
                    GenerateItemsFromResult(response.choices[0].message.tool_calls);
                }
            };
            
            AskBot(botParameters);
        }
    }
}