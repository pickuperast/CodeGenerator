// Copyright (c) Sanat. All rights reserved.
using System;
using UnityEngine;
using Sanat.ApiAnthropic;
using Sanat.ApiGemini;
using Sanat.ApiGroq;
using Sanat.ApiOpenAI;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentExtractCodeByFilepath: AbstractAgentHandler
    {
        private string _prompt;

        protected override string PromptFilename() => "PromptAgentExtractCodeByFilePath.md";

        protected override Sanat.ApiOpenAI.Model GetModel() => Sanat.ApiOpenAI.Model.GPT4omini;
        
        protected override string GetGeminiModel() => ApiGeminiModels.Pro;
        
        public AgentExtractCodeByFilepath(ApiKeys apiKeys, string filePath)
        {
            Name = "Agent Extract Code By FilePath";
            Description = "Extracts code snippets";
            Temperature = .0f;
            string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PromptFilename()}";
            Instructions = LoadPrompt(promptLocation);
            Instructions = Instructions.Replace("@filePathes@", filePath);
            StoreKeys(apiKeys);
            SelectedApiProvider = ApiProviders.OpenAI;
        }

        public override void Handle(string inputSolution) { }

        public void SplitToFilePathContent(string inputSolution, Action<string> callback)
        {
            Debug.Log($"<color=green>{Name}</color> SplitToFilePathContent {SelectedApiProvider} asking: {inputSolution}");
            BotParameters botParameters = new BotParameters(inputSolution, SelectedApiProvider, Temperature, delegate(string result)
            {
                Debug.Log($"<color=green>{Name}</color> SplitToFilePathContent RESULT({result.Length} chars): {result}");
                callback(result);
            });
            AskBot(botParameters);
        }
    }
}