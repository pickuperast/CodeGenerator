// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sanat.CodeGenerator.Common;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Sanat.ApiOpenAI
{
    public enum TextModelName 
    { 
        GPT_4o, GPT_4_Turbo, GPT_4, GPT_4_32K, 
        GPT3_5_Turbo, GPT3_5_Turbo_16K, 
        Davinci, Curie, Babbage, Ada,
        FineTune
    }

    /// <summary>
    /// Handles web requests to OpenAI API.
    /// </summary>
    public static class OpenAI
    {

        /// <summary>
        /// If you want to use a different base URL that conforms to the same API,
        /// change BaseURL.
        /// </summary>
        public static string BaseURL { get; set; } = "https://api.openai.com/v1/";

        public static string CompletionsURL => $"{BaseURL}/completions";
        public static string ChatURL => $"{BaseURL}/chat/completions";
        public static string EditsURL => $"{BaseURL}/edits";
        public static string FineTunesURL => $"{BaseURL}/fine-tunes";
        public static string FineTuningURL => $"{BaseURL}/fine_tuning";
        public static string AudioTranscriptionsURL => $"{BaseURL}/audio/transcriptions";
        public static string AudioTranslationsURL => $"{BaseURL}/translations";
        public static string ImageGenerationsURL => $"{BaseURL}/images/generations";
        public static string ImageEditsURL => $"{BaseURL}/images/edits";
        public static string ImageVariationsURL => $"{BaseURL}/images/variations";

        public const string ResponseFormatURL = "url";
        public const string ResponseFormatB64JSON = "b64_json";

        public static bool IsApiKeyValid(string apiKey)
        {
            return !string.IsNullOrEmpty(apiKey) && apiKey.StartsWith("sk-");
        }

        #region Text Generation

        /// <summary>
        /// Given a prompt, the model will return one or more predicted completions, and can also return the probabilities of alternative tokens at each position.
        /// </summary>
        /// <param name="apiKey">OpenAI API key.</param>
        /// <param name="model">Model to use.</param>
        /// <param name="temperature">What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.</param>
        /// <param name="maxTokens">The maximum number of tokens to generate in the completion. The token count of your prompt plus max_tokens cannot exceed the model's context length.</param>
        /// <param name="prompt">The prompt(s) to generate completions for, encoded as a string, array of strings, array of tokens, or array of token arrays.</param>
        /// <param name="callback">This event handler will be passed the API result.</param>
        public static UnityWebRequestAsyncOperation SubmitCompletionAsync(string apiKey, Model model,
            float temperature, int maxTokens,
            string prompt, Action<string> callback)
        {
            return SubmitCompletionAsync(apiKey, model,
                temperature, top_p: 1, frequency_penalty: 0, presence_penalty: 0,
                maxTokens, prompt, callback);
        }

        /// <summary>
        /// Given a prompt, the model will return one or more predicted completions, and can also return the probabilities of alternative tokens at each position.
        /// </summary>
        /// <param name="apiKey">OpenAI API key.</param>
        /// <param name="model">Model to use.</param>
        /// <param name="temperature">What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic. We generally recommend altering this or top_p but not both.</param>
        /// <param name="top_p">An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered. We generally recommend altering this or temperature but not both.</param>
        /// <param name="frequency_penalty">Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.</param>
        /// <param name="presence_penalty">Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far, increasing the model's likelihood to talk about new topics.</param>
        /// <param name="maxTokens">The maximum number of tokens to generate in the completion. The token count of your prompt plus max_tokens cannot exceed the model's context length.</param>
        /// <param name="prompt">The prompt(s) to generate completions for, encoded as a string, array of strings, array of tokens, or array of token arrays.</param>
        /// <param name="callback">This event handler will be passed the API result.</param>
        public static UnityWebRequestAsyncOperation SubmitCompletionAsync(string apiKey, Model model,
            float temperature, float top_p,
            float frequency_penalty, float presence_penalty,
            int maxTokens,
            string prompt, Action<string> callback)
        {
            var completionRequest = new CompletionRequest(model.Name, prompt, temperature, top_p,
                frequency_penalty, presence_penalty, maxTokens);
            string jsonData = JsonUtility.ToJson(completionRequest);

            UnityWebRequest webRequest = CreateWebRequest(apiKey, CompletionsURL, jsonData);

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
                    var responseData = JsonUtility.FromJson<CompletionResponse>(text);
                    text = responseData.choices[0].message.content.Trim();
                }
                callback?.Invoke(text);
            };

            return asyncOp;
        }

        /// <summary>
        /// Given a list of messages comprising a conversation, the model will return a response.
        /// </summary>
        /// 
        /// <param name="apiKey">OpenAI API key.</param>
        /// <param name="model">Model to use.</param>
        /// <param name="temperature">What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.</param>
        /// <param name="maxTokens">The maximum number of tokens to generate in the completion. The token count of your prompt plus max_tokens cannot exceed the model's context length.</param>
        /// <param name="messages">A list of messages comprising the conversation so far.</param>
        /// <param name="callback">This event handler will be passed the API result.</param>
        public static UnityWebRequestAsyncOperation SubmitChatAsync(string apiKey, Model model, float temperature, int maxTokens,
            List<ChatMessage> messages, Action<string> callback, Tool[] tools = null)
        {
            return SubmitChatAsync(apiKey, model, temperature,
                top_p: 1, frequency_penalty: 0, presence_penalty: 0, maxTokens, messages, callback, tools);
        }

        /// <summary>
        /// Given a list of messages comprising a conversation, the model will return a response.
        /// </summary>
        /// 
        /// <param name="apiKey">OpenAI API key.</param>
        /// <param name="model">Model to use.</param>
        /// <param name="temperature">What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.</param>
        /// <param name="top_p">An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass. So 0.1 means only the tokens comprising the top 10% probability mass are considered. We generally recommend altering this or temperature but not both.</param>
        /// <param name="frequency_penalty">Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.</param>
        /// <param name="presence_penalty">Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far, increasing the model's likelihood to talk about new topics.</param>
        /// <param name="maxTokens">The maximum number of tokens to generate in the completion. The token count of your prompt plus max_tokens cannot exceed the model's context length.</param>
        /// <param name="messages">A list of messages comprising the conversation so far.</param>
        /// <param name="callback">This event handler will be passed the API result.</param>
        public static UnityWebRequestAsyncOperation SubmitChatAsync(string apiKey, Model model, 
            float temperature, float top_p,
            float frequency_penalty, float presence_penalty, 
            int maxTokens,
            List<ChatMessage> messages, Action<string> callback, Tool[] tools = null)
        {
            var chatRequest = new ChatRequest(model.Name, messages, 
                temperature, top_p, frequency_penalty, presence_penalty,
                maxTokens);
            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(chatRequest);
            if (tools != null)
            {
                jsonData = jsonData.Insert(jsonData.Length - 1, $",\"tool_choice\": \"auto\"");
                // Append  tools to the jsonData
                var toolsJson = Newtonsoft.Json.JsonConvert.SerializeObject(tools);
                jsonData = jsonData.Insert(jsonData.Length - 1, $",\"tools\": {toolsJson}");
            }
            UnityWebRequest webRequest = CreateWebRequest(apiKey, ChatURL, jsonData);

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
                    var prompt_price = GetPromptPrice(model.Name);
                    var response_price = GetResponsePrice(model.Name);
                    var costPrompt = tokensPrompt * prompt_price / 1000;
                    var costResponse = tokensCompletion * response_price / 1000;
                    var cost = costPrompt + costResponse;
                    Debug.Log($"{model.Name} Usage({cost.ToString("F3")}$): prompt_tokens: {tokensPrompt}; completion_tokens: {tokensCompletion}; total_tokens: {tokensTotal}");
                }
                callback?.Invoke(text);
            };

            return asyncOp;
        }
        
        public static UnityWebRequestAsyncOperation SubmitToolChatAsync(string apiKey, Model model, 
            float temperature, float top_p,
            float frequency_penalty, float presence_penalty, 
            int maxTokens,
            List<ChatMessage> messages, Action<CompletionResponse> callback, Tool[] tools)
        {
            var chatRequest = new ChatRequest(model.Name, messages, 
                temperature, top_p, frequency_penalty, presence_penalty,
                maxTokens);
            string jsonData = JsonUtility.ToJson(chatRequest);
            if (tools != null)
            {
                jsonData = jsonData.Insert(jsonData.Length - 1, $",\"tool_choice\": \"required\"");
                // Append  tools to the jsonData
                var toolsJson = Newtonsoft.Json.JsonConvert.SerializeObject(tools);
                jsonData = jsonData.Insert(jsonData.Length - 1, $",\"tools\": {toolsJson}");
            }

            UnityWebRequest webRequest = CreateWebRequest(apiKey, ChatURL, jsonData);

            var startTime = DateTime.Now;
            UnityWebRequestAsyncOperation asyncOp = webRequest.SendWebRequest();

            asyncOp.completed += (op) =>
            {
                var success = webRequest.result == UnityWebRequest.Result.Success;
                var text = success ? webRequest.downloadHandler.text : string.Empty;
                if (!success) Debug.Log($"{webRequest.error}\n{webRequest.downloadHandler.text}");
                webRequest.Dispose();
                webRequest = null;
                var elapsedTime = (DateTime.Now - startTime).TotalSeconds;
                if (!string.IsNullOrEmpty(text))
                {
                    var responseData = JsonUtility.FromJson<CompletionResponse>(text);
                    callback?.Invoke(responseData);
            
                    var tokensPrompt = responseData.usage.prompt_tokens / 1000f;
                    var tokensCompletion = responseData.usage.completion_tokens / 1000f;
                    var tokensTotal = responseData.usage.total_tokens / 1000f;
                    var prompt_price = GetPromptPrice(model.Name);
                    var response_price = GetResponsePrice(model.Name);
                    var costPrompt = tokensPrompt * prompt_price / 1000;
                    var costResponse = tokensCompletion * response_price / 1000;
                    var cost = costPrompt + costResponse;
                    Debug.Log($"{model.Name} [<color=orange>{elapsedTime:F0}</color> sec] Usage(<color=green>{cost.ToString("F3")}</color>$): {CommonForAnyApi.OUTPUT_TOKENS_SYMBOL} {tokensPrompt:F1}K; {CommonForAnyApi.INPUT_TOKENS_SYMBOL} {tokensCompletion:F1}K; total_tokens: {tokensTotal:F1}K");
                }
            };

            return asyncOp;
        }

        private static float GetPromptPrice(string modelName)
        {
            if (modelName == Model.GPT4o_16K.Name)
            {
                return 0.00025f;
            }else if (modelName == Model.GPT4o.Name)
            {
                return 0.005f;
            }else if (modelName == Model.GPT4omini.Name)
            {
                return 0.00015f;
            }else if (modelName == Model.GPT3_5_Turbo.Name)
            {
                return 0.0005f;
            }

            return .05f;
        }

        private static float GetResponsePrice(string modelName)
        {
            if (modelName == Model.GPT4o_16K.Name)
            {
                return 0.001f;
            }else if (modelName == Model.GPT4o.Name)
            {
                return 0.015f;
            }else if (modelName == Model.GPT4omini.Name)
            {
                return 0.00060f;
            }else if (modelName == Model.GPT3_5_Turbo.Name)
            {
                return 0.0015f;
            }

            return .05f;
        }

        #endregion

        #region Image Generation

        /// <summary>
        /// Given a prompt and/or an input image, the model will generate a new image.
        /// </summary>
        /// <param name="apiKey">OpenAI API key.</param>
        /// <param name="n">The number of images to generate. Must be between 1 and 10.</param>
        /// <param name="imageSize">The size of the generated images. Must be one of 256x256, 512x512, or 1024x1024.</param>
        /// <param name="response_format">The format in which the generated images are returned. Must be one of url or b64_json.</param>
        /// <param name="user">A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.</param>
        /// <param name="prompt">A text description of the desired image(s). The maximum length is 1000 characters.</param>
        /// <param name="callback">The data returned by the API call.</param>
        /// <returns></returns>
        public static UnityWebRequestAsyncOperation SubmitImageGenerationAsync(string apiKey, int n, string imageSize,
            string response_format, string user, string prompt, Action<List<string>> callback)
        {
            var imageRequest = new ImageGenerationRequest(prompt, n, imageSize, response_format, user);
            string jsonData = JsonUtility.ToJson(imageRequest);

            UnityWebRequest webRequest = CreateWebRequest(apiKey, ImageGenerationsURL, jsonData);

            UnityWebRequestAsyncOperation asyncOp = webRequest.SendWebRequest();

            asyncOp.completed += (op) =>
            {
                var success = webRequest.result == UnityWebRequest.Result.Success;
                var text = success ? webRequest.downloadHandler.text : string.Empty;
                if (!success) Debug.Log($"{webRequest.error}\n{webRequest.downloadHandler.text}");
                webRequest.Dispose();
                webRequest = null;

                var result = new List<string>();

                if (!string.IsNullOrEmpty(text))
                {
                    var responseData = JsonUtility.FromJson<ImagesResponse>(text);
                    foreach (var imageResponse in responseData.data)
                    {
                        result.Add((response_format == ResponseFormatB64JSON) ? imageResponse.b64_json : imageResponse.url);
                    }
                }
                callback?.Invoke(result);
            };

            return asyncOp;
        }

        /// <summary>
        /// Creates an edited or extended image given an original image and a prompt.
        /// </summary>
        /// <param name="apiKey">OpenAI API key.</param>
        /// <param name="image">The image to edit. Must be a valid PNG file, less than 4MB, and square. If mask is not provided, image must have transparency, which will be used as the mask.</param>
        /// <param name="mask">An additional image whose fully transparent areas (e.g. where alpha is zero) indicate where image should be edited. Must be a valid PNG file, less than 4MB, and have the same dimensions as image.</param>
        /// <param name="n">The number of images to generate. Must be between 1 and 10.</param>
        /// <param name="imageSize">The size of the generated images. Must be one of 256x256, 512x512, or 1024x1024.</param>
        /// <param name="response_format">The format in which the generated images are returned. Must be one of url or b64_json.</param>
        /// <param name="user">A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.</param>
        /// <param name="prompt">A text description of the desired image(s). The maximum length is 1000 characters.</param>
        /// <param name="callback">The data returned by the API call.</param>
        /// <returns></returns>
        public static UnityWebRequestAsyncOperation SubmitImageEditAsync(string apiKey, Texture2D image, Texture2D mask, 
            int n, string imageSize, string response_format, string user, string prompt, Action<List<string>> callback)
        {
            var imageBytes = image.EncodeToPNG();
            if (mask == null)
            {
                mask = new Texture2D(image.width, image.height);
                var resetColor = new Color32(255, 255, 255, 0);
                var colorArray = mask.GetPixels32();
                for (int i = 0; i < colorArray.Length; i++)
                {
                    colorArray[i] = resetColor;
                }
                mask.SetPixels32(colorArray);
                mask.Apply();
            }
            var maskBytes = mask.EncodeToPNG();

            List<IMultipartFormSection> formParts = new List<IMultipartFormSection>();
            formParts.Add(new MultipartFormDataSection("response_format", response_format));
            formParts.Add(new MultipartFormDataSection("size", imageSize));
            formParts.Add(new MultipartFormFileSection("image", imageBytes, "image.png", "image/png"));
            formParts.Add(new MultipartFormDataSection("prompt", prompt));
            formParts.Add(new MultipartFormFileSection("mask", maskBytes, "mask.png", "image/png"));
            if (n > 1) formParts.Add(new MultipartFormDataSection("n", n.ToString()));
            if (!string.IsNullOrEmpty(user)) formParts.Add(new MultipartFormDataSection("user", user));

            var webRequest = UnityWebRequest.Post(ImageEditsURL, formParts);

            webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);
            webRequest.disposeUploadHandlerOnDispose = true;
            webRequest.disposeDownloadHandlerOnDispose = true;

            UnityWebRequestAsyncOperation asyncOp = webRequest.SendWebRequest();

            asyncOp.completed += (op) =>
            {
                var success = webRequest.result == UnityWebRequest.Result.Success;
                var text = success ? webRequest.downloadHandler.text : string.Empty;
                if (!success) Debug.Log($"{webRequest.error}\n{webRequest.downloadHandler.text}");
                webRequest.Dispose();
                webRequest = null;

                var result = new List<string>();

                if (!string.IsNullOrEmpty(text))
                {
                    var responseData = JsonUtility.FromJson<ImagesResponse>(text);
                    foreach (var imageResponse in responseData.data)
                    {
                        result.Add((response_format == ResponseFormatB64JSON) ? imageResponse.b64_json : imageResponse.url);
                    }
                }
                callback?.Invoke(result);
            };

            return asyncOp;
        }

        #endregion
        
        public static UnityWebRequest CreateWebRequest(string apiKey, string url, string jsonData)
        {
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

#if UNITY_2022_2_OR_NEWER
            UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(url, jsonData);
#else
            UnityWebRequest webRequest = UnityWebRequest.Post(url, jsonData);
#endif
            webRequest.uploadHandler.Dispose();
            webRequest.uploadHandler = new UploadHandlerRaw(postData);
            webRequest.disposeUploadHandlerOnDispose = true;
            webRequest.disposeDownloadHandlerOnDispose = true;
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

            return webRequest;
        }
        
        public static UnityWebRequest CreateWebRequest(string apiKey, string url, ChatRequest chatRequest)
        {
            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(chatRequest);
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(jsonData);

#if UNITY_2022_2_OR_NEWER
            UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(url, jsonData);
#else
            UnityWebRequest webRequest = UnityWebRequest.Post(url, jsonData);
#endif
            webRequest.uploadHandler.Dispose();
            webRequest.uploadHandler = new UploadHandlerRaw(postData);
            webRequest.disposeUploadHandlerOnDispose = true;
            webRequest.disposeDownloadHandlerOnDispose = true;
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

            return webRequest;
        }
        
        
        public enum ChatFinishReason
        {
            Stop,
            Length,
            ContentFilter,
            ToolCalls,
            FunctionCall
        }
    }

}