// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sanat.ApiGemini;
using Sanat.ApiOpenAI;
using UnityEngine;
using Antrophic = Sanat.ApiAnthropic.ApiAntrophicData;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentCodeArchitector : AbstractAgentHandler
    {
        private string _prompt;
        private string _promptLocation;
        private string TOOL_NAME_REPLACE_SCRIPT_FILE = "ReplaceScriptFile";
        private const string PROPERTY_ReplaceScriptFile_FILEPATH = "FilePath";
        private const string PROPERTY_ReplaceScriptFile_CONTENT = "Content";
        private string _task;
        public Action<List<FileContent>> OnFileContentProvided;
        public Action<string> OnJobFailed;

        protected override string PromptFilename() => "PromptAgentArchitectorTool.md";//"Unity code architector.md";

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
            _promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PromptFilename()}";
            _task = $"# TASK: {task}. # CODE: {includedCode}";
            Instructions = LoadPrompt(_promptLocation);
            _prompt = $"{Instructions} # TASK: {task}. # CODE: " + includedCode;
            SelectedApiProvider = ApiProviders.Anthropic;
        }

        public override void Handle(string input)
        {
            ToolHandle();
            //OldHandle(input);
        }
        
        private void OldHandle(string input)
        {
            Debug.Log($"<color=green>{Name}</color> asking: {_prompt}");
            BotParameters botParameters = new BotParameters(_prompt, SelectedApiProvider, Temperature, delegate(string result)
            {
                Debug.Log($"<color=purple>{Name}</color> result: {result}");
                if (result.Contains("HTTP/1.1 404") || result.Contains("HTTP/1.1 529"))
                {
                    Debug.LogError($"<color=purple>{Name}</color>: Stop");
                    return;
                }
                OnComplete?.Invoke(result);
                SaveResultToFile(result);
            }, _modelName);
            AskBot(botParameters);
        }

        private void ToolHandle(List<Antrophic.ChatMessage> additionalMessages = null)
        {
            string agentName = $"<color=green>{Name}</color>";
            Debug.Log($"{agentName} asking: {_task}");
            BotParameters botParameters = new BotParameters(_prompt, SelectedApiProvider, Temperature, null);
            List<FileContent> fileContents = new List<FileContent>();
            if (botParameters.apiProvider == ApiProviders.Anthropic)
            {
                Antrophic.ToolFunction[] tools = new [] {GetFunctionData_AntrophicReplaceScriptFile()};
                Sanat.ApiAnthropic.Model model = ApiAnthropic.Model.GetModelByName(_modelName);
                List<Antrophic.ChatMessage> messages = new List<Antrophic.ChatMessage>
                {
                    new ("assistant", PrepareSystemPrompt()),
                    new ("user", _task)
                };
                
                if (additionalMessages != null) messages.AddRange(additionalMessages);
                
                botParameters.antrophicRequest = new Antrophic.ChatRequest(_modelName, .5f, messages, tools, model.MaxOutputTokens);
                botParameters.antrophicRequest.tool_choice = new Antrophic.ToolChoice {type = "any"};
                botParameters.onAntrophicChatResponseComplete += (response) =>
                {
                    Debug.Log($"{agentName} result: {response}");
                    if (response.type == "error")
                    {
                        Debug.LogError($"{agentName} error[{response.error.type}]; message: {response.error.message}");
                    } 
                    else if (response.stop_reason == "tool_use")
                    {
                        foreach (var responseContent in response.content)
                        {
                            if (responseContent.type == "tool_use")
                            {
                                if (responseContent.name == TOOL_NAME_REPLACE_SCRIPT_FILE)
                                {
                                    FileContent fileContent = new FileContent();
                                    foreach (var keyValuePair in responseContent.input)
                                    {
                                        switch (keyValuePair.Key)
                                        {
                                            case PROPERTY_ReplaceScriptFile_FILEPATH:
                                                fileContent.FilePath = keyValuePair.Value;
                                                break;
                                            case PROPERTY_ReplaceScriptFile_CONTENT:
                                                fileContent.Content = keyValuePair.Value;
                                                break;
                                        }
                                    }
                                    Debug.Log($"{agentName} tool_use: {responseContent.name}({fileContent.FilePath}, {fileContent.Content})");
                                    fileContents.Add(fileContent);
                                }
                            }
                        }
                        if (fileContents.Count > 0)
                        {
                            Debug.Log($"{agentName} fileContents: {fileContents.Count}");
                            OnFileContentProvided?.Invoke(fileContents);
                        }
                        else
                        {
                            Debug.LogError($"{agentName} No file content received");
                            OnJobFailed?.Invoke("No file content received");
                        }
                    }
                };
            }
            
            AskBot(botParameters);
        }

        private string PrepareSystemPrompt()
        {
            string rawPrompt = LoadPrompt(_promptLocation);
            rawPrompt = rawPrompt.Replace("\\r", "");
            rawPrompt = rawPrompt.Replace("\\n", "");
            return rawPrompt;
        }

        public void WorkWithFeedback(string invalidationComment, string possibleSolution, Action<string> callback)
        {
            string newPrompt = $"{_prompt} # SOLUTION: {possibleSolution} # COMMENT: {invalidationComment}";
            SaveResultToFile(newPrompt);
            Debug.Log($"<color=purple>{Name}</color> asking: {newPrompt}");
            var additionalMessages = new List<Antrophic.ChatMessage>
            {
                new ("assistant", possibleSolution),
                new ("user", invalidationComment)
            };
            ToolHandle(additionalMessages);
            
            
            
            
            
            
            
            
            BotParameters botParameters = new BotParameters(newPrompt, SelectedApiProvider, Temperature, delegate(string result)
            {
                Debug.Log($"<color=purple>{Name}</color> result: {result}");
                SaveResultToFile(result);
                callback?.Invoke(result);
            });
            AskBot(botParameters);
        }
        
        protected Antrophic.ToolFunction GetFunctionData_AntrophicReplaceScriptFile()
        {
            string description = "Fully replaces script file code content with new code content. It should be used when you want to replace the content of a script file with new content.";
            Antrophic.InputSchema parameters = new ();
            parameters.AddProperty(PROPERTY_ReplaceScriptFile_FILEPATH, Antrophic.DataTypes.STRING, $"Filepath of the code snippet, e.g. Assets\\Path\\To\\File.cs");
            parameters.AddProperty(PROPERTY_ReplaceScriptFile_CONTENT, Antrophic.DataTypes.STRING, $"FULL code for selected filepath, partial code snippets are NOT ALLOWED.");
            parameters.Required.Add(PROPERTY_ReplaceScriptFile_FILEPATH);
            parameters.Required.Add(PROPERTY_ReplaceScriptFile_CONTENT);
            Antrophic.ToolFunction function = new Antrophic.ToolFunction(TOOL_NAME_REPLACE_SCRIPT_FILE, description, parameters);
            return function;
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