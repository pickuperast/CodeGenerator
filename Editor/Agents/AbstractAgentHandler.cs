// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Sanat.ApiOpenAI;
using Sanat.ApiAnthropic;
using Sanat.ApiGemini;
using Sanat.ApiGroq;
using UnityEngine;
using UnityEngine.Networking;

namespace Sanat.CodeGenerator.Agents
{
    public abstract class AbstractAgentHandler
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Tools { get; set; }
        public float Temperature { get; set; }
        public string Instructions { get; set; }
        public ApiProviders SelectedApiProvider = ApiProviders.Anthropic;
        public ApiKeys Apikeys;
        public Action<string> OnComplete;
        public Action OnUnsuccessfull;
        public const string RESULTS_SAVE_FOLDER = "Sanat/CodeGenerator/Results";
        public const string PROMPTS_FOLDER_PATH = "/Sanat/CodeGenerator/Editor/Agents/Prompts/";
        public const string KEY_FIGURE_OPEN = "[figureOpen]";
        public const string KEY_FIGURE_CLOSE = "[figureClose]";
        public enum ApiProviders { OpenAI, Anthropic, Groq, Gemini }

        public enum Brackets { round, square, curly, angle }

        public void SaveResultToFile(string result)
        {
            string directoryPath = Path.Combine(Application.dataPath, RESULTS_SAVE_FOLDER);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileName = $"result_{Name}_{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.txt";
            string filePath = Path.Combine(directoryPath, fileName);
            File.WriteAllText(filePath, result);
            Debug.Log($"Result saved to: {filePath}");
        }
        
        public AbstractAgentHandler SetNext(AbstractAgentHandler handler)
        {
            _nextHandler = handler;
            return handler;
        }

        protected AbstractAgentHandler _nextHandler;

        protected abstract string PromptFilename();

        protected void StoreOpenAIKey(string key)
        {
            Apikeys.openAI = key;
        }
        
        public void StoreKeys(ApiKeys keys) => Apikeys = keys;

        protected virtual Model GetModel() => Model.GPT4omini;
        
        protected virtual string GetGeminiModel() => ApiGeminiModels.Flash;
        
        public static string LoadPrompt(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    //Debug.Log($"Successfully loaded instructions from: {path}");
                    return File.ReadAllText(path);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error reading .md file: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"The .md file does not exist in the specified path.[{path}]");
            }
            return String.Empty;
        }

        public virtual void Handle(string input)
        {
            if (input.Contains("HTTP/1.1 400 Bad Request"))
            {
                OnUnsuccessfull?.Invoke();
                return;
            }
            
            if (input.Contains("HTTP/1.1 401 Unauthorized"))
            {
                OnUnsuccessfull?.Invoke();
                return;
            }
            OnComplete?.Invoke(input);
            // if (_nextHandler != null)
            //     _nextHandler.Handle(input);
        }

        public class BotParameters
        {
            public string prompt;
            public ApiProviders apiProvider;
            public float temp;
            public Action<string> onComplete;
            public ToolRequest geminiToolRequest;
            
            public BotParameters(string prompt, ApiProviders apiProvider, float temp, Action<string> onComplete)
            {
                this.prompt = prompt;
                this.apiProvider = apiProvider;
                this.temp = temp;
                this.onComplete = onComplete;
            }
        }
        
        public void AskBot(BotParameters botParameters) {
            switch (botParameters.apiProvider)
            {
                case ApiProviders.OpenAI:
                    AskChatGpt(botParameters.prompt, botParameters.temp, botParameters.onComplete);
                    break;
                case ApiProviders.Anthropic:
                    AskAntrophic(botParameters.prompt, botParameters.temp, botParameters.onComplete);
                    break;
                case ApiProviders.Groq:
                    AskGroq(botParameters.prompt, botParameters.temp, botParameters.onComplete);
                    break;
                case ApiProviders.Gemini:
                    AskGemini(botParameters.prompt, botParameters.temp, botParameters.onComplete);
                    break;
            }
        }
        
        public void AskChatGpt(string prompt, float temp, Action<string> onComplete) {
            List<ApiOpenAI.ChatMessage> messages = new List<ApiOpenAI.ChatMessage>();
            messages.Add(new ApiOpenAI.ChatMessage("user", prompt));
            var model = GetModel();
            
            UnityWebRequestAsyncOperation request = OpenAI.SubmitChatAsync(
                Apikeys.openAI,
                model,
                temp,
                model.MaxOutput,
                messages,
                onComplete
            );
        }
        
        public void AskAntrophic(string prompt, float temp, Action<string> onComplete) {
            List<ApiAnthropic.ChatMessage> messages = new List<ApiAnthropic.ChatMessage>();
            messages.Add(new ApiAnthropic.ChatMessage("user", prompt));

            UnityWebRequestAsyncOperation request = Anthropic.SubmitChatAsync(
                Apikeys.antrophic,
                ApiAnthropicModels.Claude35,
                temp,
                4000,
                messages,
                onComplete
            );
        }
        
        public void AskGroq(string prompt, float temp, Action<string> onComplete) {
            List<ApiGroq.ChatMessage> messages = new List<ApiGroq.ChatMessage>();
            messages.Add(new ApiGroq.ChatMessage("user", prompt));

            UnityWebRequestAsyncOperation request = Groq.SubmitChatAsync(
                Apikeys.groq,
                ApiGroqModels.Llama3_70b_8192,
                temp,
                4095,
                messages,
                onComplete
            );
        }
        
        public void AskGemini(string prompt, float temp, Action<string> onComplete) {
            List<ApiGemini.ChatMessage> messages = new List<ApiGemini.ChatMessage>();
            messages.Add(new ApiGemini.ChatMessage("user", prompt));

            UnityWebRequestAsyncOperation request = Gemini.SubmitChatAsync(
                Apikeys.gemini,
                GetGeminiModel(), // or whatever model you're using
                temp,
                4095,
                messages,
                onComplete
            );
        }
        
        public static string ClearResult(string input, Brackets bracket = Brackets.square)
        {
            string pattern = @"(\[.*\])";
            switch (bracket)
            {
                case Brackets.round:
                    pattern = @"(\(.*\))";
                    break;
                case Brackets.curly:
                    pattern = @"(\{.*\})";
                    break;
                case Brackets.angle:
                    pattern = @"(\<.*\>)";
                    break;
            }
            Match match = Regex.Match(input, pattern, RegexOptions.Singleline);
    
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            Debug.LogError("No match found");
            return input;
        }

        public struct ApiKeys
        {
            public string openAI;
            public string antrophic;
            public string groq;
            public string gemini;
            
            public ApiKeys(string openAI, string antrophic, string groq, string gemini)
            {
                this.openAI = openAI;
                this.antrophic = antrophic;
                this.groq = groq;
                this.gemini = gemini;
            }
        }
    }
}