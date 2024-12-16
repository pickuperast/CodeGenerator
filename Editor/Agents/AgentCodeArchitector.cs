// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sanat.ApiGemini;
using Sanat.ApiOpenAI;
using UnityEngine;
using Antrophic = Sanat.ApiAnthropic.ApiAntrophicData;
using GeminiChatRequest = Sanat.ApiGemini.ChatRequest;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentCodeArchitector : AbstractAgentHandler
    {
        private string _prompt;
        private string _promptLocation;
        private string _task;
        public Action<List<FileContent>> OnFileContentProvided;
        public Action<string> OnJobFailed;
        private readonly string _systemInstructions;
        private AgentFunctionDefinitions _functionDefinitions; // Add this field

        protected override string PromptFilename() => "PromptAgentArchitectorTool.md";
        protected override Sanat.ApiOpenAI.Model GetModel()
        {
            if (_isModelChanged && _isChangedModelOpenai)
            {
                return _newOpenaiModel;
            }
            return Sanat.ApiOpenAI.Model.GPT4o1mini;
        }

        protected override string GetGeminiModel()
        {
            if (_isModelChanged && _isChangedModelGemini)
            {
                return _newGeminiModel;
            }
            return ApiGemini.Model.Pro.Name;
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
            _systemInstructions = $"{Instructions} # PROJECT CODE: " + includedCode;
            SelectedApiProvider = ApiProviders.Gemini;
            _functionDefinitions = new AgentFunctionDefinitions(); // Initialize the new class
        }

        public void ChangeTask(string task) => _task = task;
        public override void Handle(string input)
        {
            ToolHandle();
        }
        private async Task ToolHandle(List<Antrophic.ChatMessage> additionalMessages = null, string task = "")
        {
            string agentName = $"<color=green>{Name}</color>";
            await Task.Delay(100);
            Debug.Log($"{agentName} asking [{SelectedApiProvider}][{_modelName}]: {_task}");
            List<FileContent> fileContents = new List<FileContent>();
            BotParameters botParameters = new BotParameters(_task, SelectedApiProvider, Temperature, null, _modelName, true);
            string systemPrompt = PrepareSystemPrompt();
            switch (botParameters.apiProvider)
            {
                case ApiProviders.OpenAI:
                    ToolHandlingOpenAI(botParameters, systemPrompt, agentName);
                    break;
                case ApiProviders.Gemini:
                    ToolHandlingGemini(botParameters, systemPrompt, agentName);
                    break;
                case ApiProviders.Anthropic:
                    ToolHandlingAntrophic(additionalMessages, systemPrompt, agentName, fileContents);
                    break;
            }

            AskBot(botParameters);
        }
        #region Tools OpenAI
        private void ToolHandlingOpenAI(BotParameters botParameters, string systemPrompt, string agentName)
        {
            _modelName = ApiOpenAI.Model.GPT4omini.Name;
            var openaiTools = new ApiOpenAI.Tool[] { new("function", _functionDefinitions.GetFunctionData_OpenaiSReplaceScriptFile()) }; // Use the new class
            botParameters.isToolUse = true;
            botParameters.openaiTools = openaiTools;
            botParameters.systemMessage = systemPrompt;
            botParameters.onOpenaiChatResponseComplete += (response) =>
            {
                Debug.Log($"{agentName} ToolHandle Result: {response}");
                if (response.choices[0].finish_reason == "tool_calls")
                {
                    ToolCalls[] toolCalls = response.choices[0].message.tool_calls;
                    int filesAmount = toolCalls.Length;
                    List<FileContent> fileContents = new List<FileContent>();
                    string logFileNames = $"<color=green>{Name}</color>: ";

                    for (int i = 0; i < filesAmount; i++)
                    {
                        SaveResultToFile(toolCalls[i].function.arguments);
                        fileContents.Add(JsonConvert.DeserializeObject<FileContent>(toolCalls[i].function.arguments));
                        logFileNames += $"{fileContents[i].FilePath}, ";
                        if (i == filesAmount - 1)
                        {
                            logFileNames = logFileNames.Remove(logFileNames.Length - 2);
                        }
                    }
                    Debug.Log(logFileNames);
                    ReportFunctionResult_ReplaceScriptFiles(fileContents, agentName);
                }
            };
        }
        #endregion

        #region Tools Handling Antrophic
        private BotParameters ToolHandlingAntrophic(List<Antrophic.ChatMessage> additionalMessages, string systemPrompt, string agentName,
            List<FileContent> fileContents)
        {
            BotParameters botParameters;
            Antrophic.ToolFunction[] tools = new[] { _functionDefinitions.GetFunctionData_AntrophicReplaceScriptFile() }; // Use the new class
            Sanat.ApiAnthropic.Model model = ApiAnthropic.Model.GetModelByName(_modelName);
            List<Antrophic.ChatMessage> messages = new List<Antrophic.ChatMessage>
                    {
                        new ("assistant", systemPrompt),
                        new ("user", _task)
                    };

            if (additionalMessages != null) messages.AddRange(additionalMessages);

            botParameters = new BotParameters(_prompt, SelectedApiProvider, Temperature, null);
            botParameters.antrophicRequest = new Antrophic.ChatRequest(_modelName, .5f, messages, tools, model.MaxOutputTokens);
            botParameters.antrophicRequest.tool_choice = new Antrophic.ToolChoice { type = "any" };
            botParameters.onAntrophicChatResponseComplete += (response) =>
            {
                Debug.Log($"{agentName} Working on ToolHandle Result...");
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
                            if (responseContent.name == "ReplaceScriptFile")
                            {
                                FileContent fileContent = new FileContent();
                                foreach (var keyValuePair in responseContent.input)
                                {
                                    switch (keyValuePair.Key)
                                    {
                                        case "FilePath":
                                            fileContent.FilePath = keyValuePair.Value;
                                            break;
                                        case "Content":
                                            fileContent.Content = keyValuePair.Value;
                                            break;
                                    }
                                }
                                SaveResultToFile($"//{fileContent.FilePath}{fileContent.Content}");
                                Debug.Log($"{agentName} tool_use: {responseContent.name}({fileContent.FilePath}, {fileContent.Content})");
                                fileContents.Add(fileContent);
                            }
                        }
                    }
                    ReportFunctionResult_ReplaceScriptFiles(fileContents, agentName);
                }
            };
            return botParameters;
        }
        #endregion
        #region Tools Handling Gemini
        private void ToolHandlingGemini(BotParameters botParameters, string systemPrompt, string agentName)
        {
            _modelName = ApiGemini.Model.Flash2.Name;
            var geminiTools = new List<ApiGemini.Tool> { new() { function_declarations = new List<ApiGemini.FunctionDeclaration>() { _functionDefinitions.GetFunctionData_GeminiReplaceScriptFile() } } }; // Use the new class
            botParameters.geminiRequest = new GeminiChatRequest(_task, Temperature)
            {
                tools = geminiTools,
                tool_config = new ToolConfig { function_calling_config = new FunctionCallingConfig { mode = "any", AllowedFunctionNames = new List<string> { "ReplaceScriptFile" } } }
            };
            botParameters.geminiRequest.system_instruction = new Content { parts = new List<Part> { new Part { text = _systemInstructions } } };
            botParameters.onComplete += (result) =>
            {
                if (string.IsNullOrEmpty(result)) return;
                var responseData = JsonConvert.DeserializeObject<ApiGemini.GenerateContentResponse>(result);
                List<FileContent> files = new List<FileContent>();
                if (responseData.candidates != null && responseData.candidates.Count > 0)
                {
                    var candidate = responseData.candidates[0];
                    foreach (var part in candidate.content.parts)
                    {
                        if (part.functionCall != null)
                        {
                            SaveResultToFile(JsonConvert.SerializeObject(part.functionCall.args));
                            var fileContent = JsonConvert.DeserializeObject<FileContent>(JsonConvert.SerializeObject(part.functionCall.args));
                            files.Add(fileContent);
                        }
                    }
                    if (files.Count > 0) ReportFunctionResult_ReplaceScriptFiles(files, agentName);
                }
            };
        }
        #endregion
        private void ReportFunctionResult_ReplaceScriptFiles(List<FileContent> fileContents, string agentName)
        {
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
        private string PrepareSystemPrompt()
        {
            string rawPrompt = LoadPrompt(_promptLocation);
            rawPrompt = rawPrompt.Replace("\r", "");
            rawPrompt = rawPrompt.Replace("\n", "");
            return rawPrompt;
        }
        public void WorkWithFeedback(string invalidationComment, string possibleSolution, Action<string> callback)
        {
            string newPrompt = $"{_prompt} # SOLUTION: {possibleSolution} # COMMENT: {invalidationComment}";
            SaveResultToFile(newPrompt);
            Debug.Log($"<color=green>{Name}</color> asking: {newPrompt}");
            var additionalMessages = new List<Antrophic.ChatMessage>
            {
                new ("assistant", possibleSolution),
                new ("user", invalidationComment)
            };
            ToolHandle(additionalMessages);
        }
    }
}