// Copyright (c) Sanat. All rights reserved.
using System;
using Sanat.ApiGemini;
using Sanat.ApiOpenAI;
using UnityEngine;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentItemsGenerator : AbstractAgentHandler
    {
        private string _prompt;

        protected override string PromptFilename() => "PromptGenerateRpgItems.md";
        protected override Model GetModel() => Model.GPT4o_16K;
        protected override string GetGeminiModel() => ApiGeminiModels.Pro;
        private const string LOCAL_PROMPTS_FOLDER_PATH = "/Sanat/CodeGenerator/QuestsGenerator/Editor/Prompts/";
        
        public AgentItemsGenerator(ApiKeys apiKeys, string task, string alreadyExistingItems)
        {
            Name = "AgentItemNamesGenerator";
            Description = "Generates items";
            Temperature = .7f;
            StoreKeys(apiKeys);
            string promptLocation = Application.dataPath + $"{LOCAL_PROMPTS_FOLDER_PATH}{PromptFilename()}";
            Instructions = LoadPrompt(promptLocation);
            if (alreadyExistingItems != String.Empty)
                alreadyExistingItems = "# Already existing items: " + alreadyExistingItems;
            _prompt = $"{Instructions} # TASK: {task}. " + alreadyExistingItems;
            SelectedApiProvider = ApiProviders.OpenAI;
        }

        public override void Handle(string input)
        {
            Debug.Log($"<color=purple>{Name}</color> asking: {_prompt}");
            BotParameters botParameters = new BotParameters(_prompt, SelectedApiProvider, Temperature, delegate(string result)
            {
                Debug.Log($"<color=purple>{Name}</color> result: {result}");
                OnComplete?.Invoke(result);
                SaveResultToFile(result);
            });
            AskBot(botParameters);
        }
    }
}