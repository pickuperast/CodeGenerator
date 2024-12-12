// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Sanat.ApiAnthropic
{
    public static class Anthropic
    {
        public static string BaseURL { get; set; } = "https://api.anthropic.com/v1/";
        public static string MessagesURL => $"{BaseURL}messages";
        public static bool IsApiKeyValid(string apiKey) => !string.IsNullOrEmpty(apiKey);

        public static UnityWebRequestAsyncOperation SubmitChatAsync(string apiKey, ApiAntrophicData.ChatRequest chatRequest, Action<ApiAntrophicData.ChatResponse> callback)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            string jsonData = JsonConvert.SerializeObject(chatRequest, settings);

            UnityWebRequest webRequest = CreateWebRequest(apiKey, MessagesURL, jsonData);

            var startTime = DateTime.Now;
            UnityWebRequestAsyncOperation asyncOp = webRequest.SendWebRequest();

            asyncOp.completed += (op) =>
            {
                var success = webRequest.result == UnityWebRequest.Result.Success;
                var text = success ? webRequest.downloadHandler.text : string.Empty;
                ApiAntrophicData.ChatResponse responseData = null;
                if (!success)
                {
                    Debug.Log($"{webRequest.error}\n{webRequest.downloadHandler.text}");
                    if (webRequest.downloadHandler.text.Contains("Overloaded"))
                    {
                        string oldModel = chatRequest.model;
                        chatRequest.model = Model.DowngradeModel(chatRequest.model);
                        Debug.Log($"Changing model {oldModel} -> {chatRequest.model}");
                        SubmitChatAsync(apiKey, chatRequest, callback);
                        return;
                    }
                }
                webRequest.Dispose();
                webRequest = null;
                var elapsedTime = (DateTime.Now - startTime).Seconds;
                if (!string.IsNullOrEmpty(text))
                {
                    responseData = JsonConvert.DeserializeObject<ApiAntrophicData.ChatResponse>(text);
                    Debug.Log($"ChatResponse: {responseData}");
                    var tokensPrompt = responseData.usage.input_tokens;
                    var tokensCompletion = responseData.usage.output_tokens;
                    var tokensTotal = tokensPrompt + tokensCompletion;

                    Model model = Model.GetModelByName(responseData.model);
                    float inputCost = (tokensPrompt / 1000000f) * model.InputPricePerMil;
                    float outputCost = (tokensCompletion / 1000000f) * model.OutputPricePerMil;
                    float totalCost = inputCost + outputCost;

                    Debug.Log($"{model.Name} [<color=orange>{elapsedTime}</color> sec] Usage(<color=green>{totalCost:F3}</color>$): input_tokens: {tokensPrompt} (${inputCost:F6}); " +
                              $"output_tokens: {tokensCompletion} (${outputCost:F6}); " +
                              $"total_tokens: {tokensTotal}");
                }
                callback?.Invoke(responseData);
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
}