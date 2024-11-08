// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Sanat.ApiAnthropic
{
    public static class Anthropic
    {
        public static string BaseURL { get; set; } = "https://api.anthropic.com/v1/";
        public static string MessagesURL => $"{BaseURL}messages";
        public static bool IsApiKeyValid(string apiKey) => !string.IsNullOrEmpty(apiKey);

        public static UnityWebRequestAsyncOperation SubmitChatAsync(string apiKey, Model model, float temp, int maxTokens,
            List<ChatMessage> messages, Action<string> callback)
        {
            var chatRequest = new ChatRequest(model, temp, messages, maxTokens);
            string jsonData = JsonUtility.ToJson(chatRequest);

            UnityWebRequest webRequest = CreateWebRequest(apiKey, MessagesURL, jsonData);

            UnityWebRequestAsyncOperation asyncOp = webRequest.SendWebRequest();

            asyncOp.completed += (op) =>
            {
                var success = webRequest.result == UnityWebRequest.Result.Success;
                var text = success ? webRequest.downloadHandler.text : string.Empty;
                if (!success) Debug.Log($"{webRequest.error}\n{webRequest.downloadHandler.text}");
                webRequest.Dispose();
                webRequest = null;

                if (!string.IsNullOrEmpty(text))
                {
                    var responseData = JsonUtility.FromJson<ChatResponse>(text);
                    text = responseData.content[0].text.Trim();
                    var tokensPrompt = responseData.usage.input_tokens;
                    var tokensCompletion = responseData.usage.output_tokens;
                    var tokensTotal = tokensPrompt + tokensCompletion;
                    
                    float inputCost = (tokensPrompt / 1000000f) * model.InputPricePerMil;
                    float outputCost = (tokensCompletion / 1000000f) * model.OutputPricePerMil;
                    float totalCost = inputCost + outputCost;
                    
                    Debug.Log($"{model.Name} Usage({totalCost:F3}$): input_tokens: {tokensPrompt} (${inputCost:F6}); " +
                              $"output_tokens: {tokensCompletion} (${outputCost:F6}); " +
                              $"total_tokens: {tokensTotal}");
                }
                callback?.Invoke(text);
            };

            return asyncOp;
        }

        public static UnityWebRequest CreateWebRequest(string apiKey, string url, string jsonData)
        {
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

            UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(postData);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.disposeUploadHandlerOnDispose = true;
            webRequest.disposeDownloadHandlerOnDispose = true;
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("x-api-key", apiKey);
            webRequest.SetRequestHeader("anthropic-version", "2023-06-01");

            return webRequest;
        }
    }

    [Serializable]
    public class ChatRequest
    {
        public string model;
        public int max_tokens;
        public float temperature;
        public List<ChatMessage> messages;

        public ChatRequest(Model model, float temperature, List<ChatMessage> messages, int maxTokens)
        {
            this.model = model.Name;
            this.max_tokens = maxTokens;
            this.temperature = temperature;
            this.messages = messages;
        }
    }

    [Serializable]
    public class ChatMessage
    {
        public string role;
        public string content;

        public ChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [Serializable]
    public class ChatResponse
    {
        public List<ContentItem> content;
        public string id;
        public string model;
        public string role;
        public string stop_reason;
        public string stop_sequence;
        public string type;
        public Usage usage;
    }

    [Serializable]
    public class ContentItem
    {
        public string text;
        public string type;
    }

    [Serializable]
    public class Usage
    {
        public int input_tokens;
        public int output_tokens;
    }
}