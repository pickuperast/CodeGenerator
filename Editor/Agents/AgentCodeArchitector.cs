// Copyright (c) Sanat. All rights reserved.
using System;
using Sanat.ApiGemini;
using Sanat.ApiOpenAI;
using UnityEngine;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentCodeArchitector : AbstractAgentHandler
    {
        private string _prompt;

        protected override string PromptFilename() => "Unity code architector.md";
        protected override Model GetModel() => Model.GPT4o;
        protected override string GetGeminiModel() => ApiGeminiModels.Pro;
        
        public AgentCodeArchitector(ApiKeys apiKeys, string task, string includedCode)
        {
            Name = "Agent Unity3D Architect";
            Description = "Writes code for agents";
            Temperature = .2f;
            StoreKeys(apiKeys);
            string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PromptFilename()}";
            Instructions = LoadPrompt(promptLocation);
            _prompt = $"{Instructions} # TASK: {task}. # CODE: " + includedCode;
            SelectedApiProvider = ApiProviders.Anthropic;
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

        public void WorkWithFeedback(string validationResult, string possibleSolution, Action<string> callback)
        {
            string newPrompt = $"{_prompt} # SOLUTION: {possibleSolution} # VALIDATION: {validationResult}";
            SaveResultToFile(newPrompt);
            Debug.Log($"<color=purple>{Name}</color> asking: {newPrompt}");
            BotParameters botParameters = new BotParameters(newPrompt, SelectedApiProvider, Temperature, delegate(string result)
            {
                Debug.Log($"<color=purple>{Name}</color> result: {result}");
                SaveResultToFile(result);
                callback?.Invoke(result);
                SaveResultToFile(result);
            });
            AskBot(botParameters);
        }
    }
}