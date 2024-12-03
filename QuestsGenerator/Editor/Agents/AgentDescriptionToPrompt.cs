// Copyright (c) Sanat. All rights reserved.
using System;
using Sanat.ApiGemini;
using Sanat.ApiOpenAI;
using UnityEngine;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentDescriptionToPrompt : AbstractAgentHandler
    {
        private string _prompt;

        protected override string PromptFilename() => "PromptGeneratePromptsFromDescription.md";
        protected override Model GetModel() => Model.GPT4o_16K;
        protected override string GetGeminiModel() => ApiGeminiModels.Pro;
        private const string LOCAL_PROMPTS_FOLDER_PATH = "/Sanat/CodeGenerator/QuestsGenerator/Editor/Prompts/";
        
        public AgentDescriptionToPrompt(ApiKeys apiKeys, string task)
        {
            Name = "AgentDescriptionToPrompt";
            Description = "Generates prompts";
            Temperature = .7f;
            StoreKeys(apiKeys);
            string promptLocation = Application.dataPath + $"{LOCAL_PROMPTS_FOLDER_PATH}{PromptFilename()}";
            Instructions = LoadPrompt(promptLocation);
            _prompt = $"{Instructions} # TASK: {task}. ";
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