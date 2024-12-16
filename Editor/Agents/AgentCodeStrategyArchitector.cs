// Copyright (c) Sanat. All rights reserved.

using System.Collections.Generic;
using System.Data;
using System.Text;
using Sanat.ApiGemini;
using UnityEngine;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentCodeStrategyArchitector : AbstractAgentHandler
    {
        public string Description { get; set; }
        public string[] Tools { get; set; }
        public float Temperature { get; set; }
        public string Instructions { get; set; }
        private string _prompt;

        protected override string PromptFilename() =>
            "PromptAgentStrategyArchitector.md";
        
        protected override string GetGeminiModel() => 
            ApiGemini.Model.Flash.Name;
        
        public AgentCodeStrategyArchitector(ApiKeys apiKeys, string task, string includedCode)
        {
            Name = "Agent Strategy Architector";
            Description = "Writes task solving strategy for agents";
            Temperature = 1f;
            Tools = new[] { "GetWholeProjectStructure" };
            StoreKeys(apiKeys);
            string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PromptFilename()}";
            Instructions = LoadPrompt(promptLocation);
            _prompt = $"{Instructions} # TASK: {task}. # CODE: " + includedCode;
            SelectedApiProvider = ApiProviders.Gemini;
        }
        
        public override void Handle(string input)
        {
            Debug.Log($"<color=orange>{Name}</color> asking: {_prompt}");
            var tools = ToolRequest();

            BotParameters botParameters = new BotParameters(_prompt, SelectedApiProvider, Temperature, delegate(string result)
            {
                Debug.Log($"<color=orange>{Name}</color> result: {result}");
                
                var responseData = JsonUtility.FromJson<ChatResponse>(result);
                FunctionCall functionCall = responseData?.candidates[0].content?.parts[0]?.functionCall;
                string methodName = functionCall.name;
                var args = functionCall.args;
                LaunchFunction(methodName, args);
                OnComplete?.Invoke(result);
                SaveResultToFile(result);
            });
            //botParameters.geminiToolRequest = tools;
            AskBot(botParameters);
        }
        
        private void GenerateSolution(string prompt)
        {
            Debug.Log($"<color=orange>{Name}</color> generating solution for: {prompt}");
            BotParameters botParameters = new BotParameters(prompt, SelectedApiProvider, Temperature, delegate(string result)
            {
                Debug.Log($"<color=orange>{Name}</color> result: {result}");
                OnComplete?.Invoke(result);
                SaveResultToFile(result);
            });
            AskBot(botParameters);
        }

        private ToolRequest ToolRequest()
        {
            Tool toolLibrary = new Tool();
            toolLibrary.function_declarations = new()
            {
                new FunctionDeclaration
                {
                    name = "GetWholeProjectStructure",
                    description = "Attaches all script locations of the project as an additional information for the task",
                },
                new FunctionDeclaration()
                {
                    name = "GenerateSolution",
                    description = "Generates a solution for the task",
                    parameters = new FunctionDeclarationSchema
                    {
                        type = FunctionDeclarationSchemaType.STRING,
                        properties = new Dictionary<string, FunctionDeclarationSchemaProperty>
                        {
                            {
                                "prompt",
                                new FunctionDeclarationSchemaProperty
                                {
                                    type = FunctionDeclarationSchemaType.STRING
                                }
                            }
                        },
                        required = new List<string>
                        {
                            "prompt"
                        }
                    }
                }
            };
            ToolRequest toolRequest = new ToolRequest();
            toolRequest.contents = new()
            {
                new Content
                {
                    role = "user",
                    parts = new()
                    {
                        new Part
                        {
                            text = _prompt
                        }
                    }
                }
            };
            toolRequest.tools = new()
            {
                toolLibrary
            };
            return toolRequest;
        }

        private string LaunchFunction(string methodName, object args)
        {
            switch (methodName)
            {
                case "GetWholeProjectStructure":
                    var filePathes = ToolGetWholeProjectStructure.GetWholeProjectStructure();
                    StringBuilder sb = new StringBuilder();
                    foreach (var filePath in filePathes)
                    {
                        sb.AppendLine(filePath);
                    }
                    return sb.ToString();
                    break;
                case "GenerateSolution":
                    Debug.Log($"<color=orange>{Name}</color> generating solution for: {args}");
                    GenerateSolution(JsonUtility.FromJson<GenerateSolutionRequest>(args.ToString()).prompt);
                    break;
            }
            return string.Empty;
        }
        
        public struct GenerateSolutionRequest
        {
            public string prompt;
        }
    }
}