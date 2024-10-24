// Copyright (c) Sanat. All rights reserved.

using Sanat.ApiGemini;
using UnityEngine;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentCodeValidator : AbstractAgentHandler
    {
        public string[] Tools { get; set; }
        private string _prompt;

        protected override string PromptFilename() => "UnityCodeValidatorInstructions.md";
        
        public AgentCodeValidator(ApiKeys apiKeys, string task, string includedCode, string possibleAnswer)
        {
            Name = "Agent Code Validator";
            Description = "Validates code for agents";
            Temperature = .0f;
            StoreKeys(apiKeys);
            string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PromptFilename()}";
            Instructions = LoadPrompt(promptLocation);
            _prompt = $"{Instructions} " +
                      $"# TASK: {task}. " +
                      $"# CODE: {includedCode} " +
                      $"# POSSIBLE ANSWER: {possibleAnswer}";
            SelectedApiProvider = ApiProviders.Anthropic;
        }
        
        protected override string GetGeminiModel() => ApiGeminiModels.Pro;

        public override void Handle(string input)
        {
            Debug.Log($"<color=purple>{Name}</color> asking: {_prompt}");
            BotParameters botParameters = new BotParameters(_prompt, SelectedApiProvider, Temperature, delegate(string result)
            {
                //Debug.Log($"<color=purple>{Name}</color> result: {result}");
                OnComplete?.Invoke(result);
                SaveResultToFile(result);
            });
            AskBot(botParameters);
        }
    }
}