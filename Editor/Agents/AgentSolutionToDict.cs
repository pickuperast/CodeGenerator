// Copyright (c) Sanat. All rights reserved.

using System;
using UnityEngine;
using Sanat.ApiAnthropic;
using Sanat.ApiGemini;
using Sanat.ApiGroq;
using Sanat.ApiOpenAI;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentSolutionToDict: AbstractAgentHandler
    {
        private string _prompt;

        protected override string PromptFilename() => 
            "Unity code solution to several codes splitter.md";

        protected override Sanat.ApiOpenAI.Model GetModel() => 
            Sanat.ApiOpenAI.Model.GPT4omini;
        
        protected override string GetGeminiModel() => 
            ApiGemini.Model.Pro.Name;
        
        
        public AgentSolutionToDict(ApiKeys apiKeys, string[] filePathes)
        {
            Name = "Agent Solution to List of FileContent";
            Description = "Merges code snippets";
            Temperature = .1f;
            string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PromptFilename()}";
            Instructions = LoadPrompt(promptLocation);
            string filePathesMerged = string.Join("\n", filePathes);
            Instructions = Instructions.Replace("@filePathes@", filePathesMerged);
            StoreKeys(apiKeys);
            SelectedApiProvider = ApiProviders.OpenAI;
        }

        public override void Handle(string inputSolution)
        {
            string prompt = Instructions + inputSolution;
            SplitToFilePathContent(prompt, OnComplete);
        }

        public void SplitToFilePathContent(string prompt, Action<string> callback)
        {
            Debug.Log($"<color=green>{Name}</color> {SelectedApiProvider} asking: {prompt}");
            BotParameters botParameters = new BotParameters(prompt, SelectedApiProvider, Temperature, delegate(string result)
            {
                Debug.Log($"<color=green>{Name}</color> RESULT({result.Length} chars): {result}");
                SaveResultToFile(result);
            });
            AskBot(botParameters);
        }
    }
}