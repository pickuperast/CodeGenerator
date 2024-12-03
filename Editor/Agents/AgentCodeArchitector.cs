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

        protected override Sanat.ApiOpenAI.Model GetModel()
        {
            if (_isModelChanged)
            {
                if (_isChangedModelOpenai)
                {
                    return _newOpenaiModel;
                }
            }
            return Sanat.ApiOpenAI.Model.GPT4o1mini;
        }
        
        protected override string GetGeminiModel()
        {
            if (_isModelChanged)
            {
                if (_isChangedModelGemini)
                {
                    return _newGeminiModel;
                }
            }
            return ApiGeminiModels.Pro;
        }
        
        public AgentCodeArchitector(ApiKeys apiKeys, string task, string includedCode)
        {
            Name = "Agent Unity3D Architect";
            Description = "Writes code for agents";
            Temperature = .5f;
            StoreKeys(apiKeys);
            string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PromptFilename()}";
            Instructions = LoadPrompt(promptLocation);
            _prompt = $"{Instructions} # TASK: {task}. # CODE: " + includedCode;
            SelectedApiProvider = ApiProviders.Anthropic;
        }

        public override void Handle(string input)
        {
            Debug.Log($"<color=green>{Name}</color> asking: {_prompt}");
            BotParameters botParameters = new BotParameters(_prompt, SelectedApiProvider, Temperature, delegate(string result)
            {
                Debug.Log($"<color=purple>{Name}</color> result: {result}");
                if (result.Contains("HTTP/1.1 404"))
                {
                    Debug.LogError($"<color=purple>{Name}</color>: Stop");
                    return;
                }
                OnComplete?.Invoke(result);
                SaveResultToFile(result);
            }, _modelName);
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

        public void ChangeLLM(ApiProviders provider, string modelName)
        {
            SelectedApiProvider = provider;
            _modelName = modelName;
            _isModelChanged = true;
            switch (provider)
            {
                case ApiProviders.OpenAI:
                    _isChangedModelAnthropic = false;
                    _isChangedModelOpenai = true;
                    _isChangedModelGemini = false;
                    break;
                case ApiProviders.Gemini:
                    _isChangedModelAnthropic = false;
                    _isChangedModelOpenai = false;
                    _isChangedModelGemini = true;
                    break;
                case ApiProviders.Anthropic:
                    _isChangedModelAnthropic = true;
                    _isChangedModelOpenai = false;
                    _isChangedModelGemini = false;
                    break;
            }
        }
    }
}