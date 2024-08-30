using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Sanat.ApiGroq
{
    public static class Groq
    {
        public static string BaseURL { get; set; } = "https://api.groq.com/openai/v1/";
        public static string ChatCompletionsURL => $"{BaseURL}chat/completions";

        public static bool IsApiKeyValid(string apiKey)
        {
            return !string.IsNullOrEmpty(apiKey);
        }

        public static UnityWebRequestAsyncOperation SubmitChatAsync(string apiKey, string model, float temp, int maxTokens,
            List<ChatMessage> messages, Action<string> callback)
        {
            var chatRequest = new ChatRequest(model, temp, messages, maxTokens);
            string jsonData = JsonUtility.ToJson(chatRequest);

            UnityWebRequest webRequest = CreateWebRequest(apiKey, ChatCompletionsURL, jsonData);

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
                    text = responseData.choices[0].message.content.Trim();
                    var tokensPrompt = responseData.usage.prompt_tokens;
                    var tokensCompletion = responseData.usage.completion_tokens;
                    var tokensTotal = responseData.usage.total_tokens;
                    
                    Debug.Log($"{model} Usage: prompt_tokens: {tokensPrompt}; " +
                              $"completion_tokens: {tokensCompletion}; " +
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
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");

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

        public ChatRequest(string model, float temperature, List<ChatMessage> messages, int maxTokens)
        {
            this.model = model;
            this.max_tokens = maxTokens;
            //If you set a temperature value of 0, it will be converted to 1e-8. If you run into any issues, please try setting the value to a float32 > 0 and <= 2.
            this.temperature = Mathf.Approximately(temperature, Mathf.Epsilon) ? 0.0001f : temperature;
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
        public string id;
        public string object_type;
        public long created;
        public string model;
        public List<Choice> choices;
        public Usage usage;
    }

    [Serializable]
    public class Choice
    {
        public int index;
        public ChatMessage message;
        public string finish_reason;
    }

    [Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }
}