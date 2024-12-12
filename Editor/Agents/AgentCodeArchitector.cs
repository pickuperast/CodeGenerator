// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        
        #region ReplaceScriptFile
        private const string TOOL_NAME_REPLACE_SCRIPT_FILE = "ReplaceScriptFile";
        private const string PROPERTY_ReplaceScriptFile_FILEPATH = "FilePath";
        private const string PROPERTY_ReplaceScriptFile_CONTENT = "Content";
        private const string FUNCTION_REPLACE_SCRIPT_FILE_DESCRIPTION = "Fully replaces script file code content with new code content. It should be used when you want to replace the content of a script file with new content.";
        private const string FUNCTION_PROPERTY_ReplaceScriptFile_FILEPATH_DESCRIPTION = "Filepath of the code snippet, e.g. Assets\\Path\\To\\File.cs";
        private const string FUNCTION_PROPERTY_ReplaceScriptFile_CONTENT_DESCRIPTION = "FULL code for selected filepath, partial code snippets are NOT ALLOWED.";
        #endregion
        
        private string _task;
        public Action<List<FileContent>> OnFileContentProvided;
        public Action<string> OnJobFailed;

        protected override string PromptFilename() => "PromptAgentArchitectorTool.md";//"Unity code architector.md";
        protected string PromptGenerateHighLevelSolution => "AgentCodeHighLevelArchitector.md";//"Unity code architector.md";

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
            //GetHighLevelSolutionMessages();
            ToolHandle();
            //OldHandle(input);
        }

        private void GetHighLevelSolutionMessages()
        {
            string agentName = $"<color=green>{Name}</color>";
            List<Antrophic.ChatMessage> messages = new List<Antrophic.ChatMessage>
            {
                new ("assistant", LoadPrompt(Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PromptGenerateHighLevelSolution}")),
                new ("user", _task)
            };
            BotParameters botParameters = new BotParameters(_prompt, SelectedApiProvider, 1, null, _modelName);
            Sanat.ApiAnthropic.Model model = ApiAnthropic.Model.GetModelByName(_modelName);
            botParameters.antrophicRequest = new Antrophic.ChatRequest(_modelName, .5f, messages, null, model.MaxOutputTokens);
            botParameters.onAntrophicChatResponseComplete += (response) =>
            {
                string logText = $"{agentName} result: ";
                if (response.type == "error")
                {
                    logText += $"error[{response.error.type}]; message: {response.error.message}";
                    Debug.LogError(logText);
                } 
                else if (response.stop_reason == "tool_use")
                {
                    
                }
                else
                {
                    SaveResultToFile(response.content[0].text);
                    logText += response.content[0].text;
                    _task += response.content[0].text;
                    //SelectedApiProvider = ApiProviders.OpenAI;
                    _modelName = ApiAnthropic.Model.Haiku35Latest.Name;
                    ToolHandle();
                }
                Debug.Log(logText);
            };
            AskBot(botParameters);
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

        private async Task ToolHandle(List<Antrophic.ChatMessage> additionalMessages = null, string task = "")
        {
            string agentName = $"<color=green>{Name}</color>";
            await Task.Delay(100);
            Debug.Log($"{agentName} asking [{SelectedApiProvider}][{_modelName}]: {_task}");
            List<FileContent> fileContents = new List<FileContent>();
            BotParameters botParameters = new BotParameters(_task, SelectedApiProvider, Temperature, null, _modelName, true);
            switch (botParameters.apiProvider)
            {
                case ApiProviders.OpenAI:
                    //GetFunctionData_OpenaiSReplaceScriptFile
                    _modelName = Model.GPT4omini.Name;//ApiGroqModels.Llama3_70b_8192_tool.Name;
                    var openaiTools = new ApiOpenAI.Tool[] { new ("function", GetFunctionData_OpenaiSReplaceScriptFile()) };
                    botParameters.isToolUse = true;
                    botParameters.openaiTools = openaiTools;
                    botParameters.systemMessage = PrepareSystemPrompt();
                    botParameters.onOpenaiChatResponseComplete += (response) =>
                    {
                        Debug.Log($"{agentName} ToolHandle Result: {response}");
                        if (response.choices[0].finish_reason == "tool_calls")
                        {
                            ToolCalls[] toolCalls = response.choices[0].message.tool_calls;
                            int filesAmount = toolCalls.Length;
                            List<FileContent> fileContents = new List<FileContent>();
                            string logFileNames = $"<color=cyan>{Name}</color>: ";
                            
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
                    break;
                case ApiProviders.Gemini:
                    break;
                case ApiProviders.Anthropic:
                    Antrophic.ToolFunction[] tools = new [] {GetFunctionData_AntrophicReplaceScriptFile()};
                    Sanat.ApiAnthropic.Model model = ApiAnthropic.Model.GetModelByName(_modelName);
                    List<Antrophic.ChatMessage> messages = new List<Antrophic.ChatMessage>
                    {
                        new ("assistant", PrepareSystemPrompt()),
                        new ("user", _task)
                    };
                    
                    if (additionalMessages != null) messages.AddRange(additionalMessages);
                    
                    botParameters = new BotParameters(_prompt, SelectedApiProvider, Temperature, null);
                    botParameters.antrophicRequest = new Antrophic.ChatRequest(_modelName, .5f, messages, tools, model.MaxOutputTokens);
                    botParameters.antrophicRequest.tool_choice = new Antrophic.ToolChoice {type = "any"};
                    botParameters.onAntrophicChatResponseComplete += (response) =>
                    {
                        Debug.Log($"{agentName} Working on ToolHandle Result...");
                        if (response.type == "error")
                        {
                            Debug.LogError(
                                $"{agentName} error[{response.error.type}]; message: {response.error.message}");
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
                                        
                                        SaveResultToFile($"//{fileContent.FilePath}\n {fileContent.Content}");
                                        Debug.Log($"{agentName} tool_use: {responseContent.name}({fileContent.FilePath}, {fileContent.Content})");
                                        fileContents.Add(fileContent);
                                    }
                                }
                            }

                            ReportFunctionResult_ReplaceScriptFiles(fileContents, agentName);
                        }
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            AskBot(botParameters);
        }

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
        
        #region Tool_ReplaceScriptFile
        
        protected ToolFunction GetFunctionData_OpenaiSReplaceScriptFile()
        {
            Parameter parameters = new ();
            parameters.AddProperty(PROPERTY_ReplaceScriptFile_FILEPATH, DataTypes.STRING, FUNCTION_PROPERTY_ReplaceScriptFile_FILEPATH_DESCRIPTION);
            parameters.AddProperty(PROPERTY_ReplaceScriptFile_CONTENT, DataTypes.STRING, FUNCTION_PROPERTY_ReplaceScriptFile_CONTENT_DESCRIPTION);
            parameters.Required.Add(PROPERTY_ReplaceScriptFile_FILEPATH);
            parameters.Required.Add(PROPERTY_ReplaceScriptFile_CONTENT);
            ToolFunction function = new ToolFunction(TOOL_NAME_REPLACE_SCRIPT_FILE, FUNCTION_REPLACE_SCRIPT_FILE_DESCRIPTION, parameters);
            return function;
        }
        
        
        protected Antrophic.ToolFunction GetFunctionData_AntrophicReplaceScriptFile()
        {
            Antrophic.InputSchema parameters = new ();
            parameters.AddProperty(PROPERTY_ReplaceScriptFile_FILEPATH, Antrophic.DataTypes.STRING, FUNCTION_PROPERTY_ReplaceScriptFile_FILEPATH_DESCRIPTION);
            parameters.AddProperty(PROPERTY_ReplaceScriptFile_CONTENT, Antrophic.DataTypes.STRING, FUNCTION_PROPERTY_ReplaceScriptFile_CONTENT_DESCRIPTION);
            parameters.Required.Add(PROPERTY_ReplaceScriptFile_FILEPATH);
            parameters.Required.Add(PROPERTY_ReplaceScriptFile_CONTENT);
            Antrophic.ToolFunction function = new Antrophic.ToolFunction(TOOL_NAME_REPLACE_SCRIPT_FILE, FUNCTION_REPLACE_SCRIPT_FILE_DESCRIPTION, parameters);
            return function;
        }
        #endregion

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