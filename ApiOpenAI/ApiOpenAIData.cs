// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Sanat.ApiOpenAI
{

    #region Text Generation

    [Serializable]
    public class CompletionRequest
    {
        public string model;
        public string prompt;
        public float temperature;
        public float top_p;
        public float frequency_penalty;
        public float presence_penalty;
        public int max_completion_token;

        public CompletionRequest(string modelName, string prompt, 
            float temperature, float top_p, 
            float frequency_penalty, float presence_penalty,
            int maxCompletionToken)
        {
            this.model = modelName;
            this.prompt = prompt;
            this.temperature = temperature;
            this.max_completion_token = maxCompletionToken;
            this.top_p = top_p;
            this.frequency_penalty = frequency_penalty;
            this.presence_penalty = presence_penalty;
        }

        public CompletionRequest(string modelName, string prompt,
            float temperature, 
            int maxCompletionToken)
        {
            this.model = modelName;
            this.prompt = prompt;
            this.temperature = temperature;
            this.max_completion_token = maxCompletionToken;
            this.top_p = 1;
            this.frequency_penalty = 0;
            this.presence_penalty = 0;
        }
    }

    [Serializable]
    public class CompletionResponse
    {
        public string id;
        public string @object;
        public int created;
        public string model;
        public Choice[] choices;
        public Usage usage;
    }

    [Serializable]
    public class Choice
    {
        public string text;
        public int index;
        public object logprobs;
        public string finish_reason;
    }

    [Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }


    [Serializable]
    public class EditRequest
    {
        public string model;
        public string input;
        public string instruction;
        public float temperature;
        public float top_p;
        public float frequency_penalty;
        public float presence_penalty;
        public int max_tokens;

        public EditRequest(string modelName, string input, string instruction, 
            float temperature, float top_p,
            float frequency_penalty, float presence_penalty, 
            int maxTokens)
        {
            this.model = modelName;
            this.input = input;
            this.instruction = instruction;
            this.temperature = temperature;
            this.max_tokens = maxTokens;
            this.top_p = top_p;
            this.frequency_penalty = frequency_penalty;
            this.presence_penalty = presence_penalty;
        }

        public EditRequest(string modelName, string input, string instruction,
            float temperature, 
            int maxTokens)
        {
            this.model = modelName;
            this.input = input;
            this.instruction = instruction;
            this.temperature = temperature;
            this.max_tokens = maxTokens;
            this.top_p = 1;
            this.frequency_penalty = 0;
            this.presence_penalty = 0;
        }
    }

    [Serializable]
    public class EditResponse
    {
        public string @object;
        public int created;
        public Choice[] choices;
        public Usage usage;
    }

    [Serializable]
    public class ChatRequest
    {
        public string model;
        public List<ChatMessage> messages;
        public float temperature;
        public float top_p;
        public float frequency_penalty;
        public float presence_penalty;
        public int max_completion_tokens;

        public ChatRequest(string modelName, List<ChatMessage> messages, 
            float temperature, float top_p,
            float frequency_penalty, float presence_penalty, 
            int maxCompletionTokens)
        {
            this.model = modelName;
            this.messages = messages;
            this.temperature = temperature;
            this.max_completion_tokens = maxCompletionTokens;
            this.top_p = top_p;
            this.frequency_penalty = frequency_penalty;
            this.presence_penalty = presence_penalty;
        }

        public ChatRequest(string modelName, List<ChatMessage> messages,
            float temperature, int maxCompletionTokens)
        {
            this.model = modelName;
            this.messages = messages;
            this.temperature = temperature;
            this.max_completion_tokens = maxCompletionTokens;
            this.top_p = 1;
            this.frequency_penalty = 0;
            this.presence_penalty = 0;
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
        public string @object;
        public int created;
        public string model;
        public ChatChoice[] choices;
        public Usage usage;
    }

    [Serializable]
    public class ChatChoice
    {
        public ChatMessage message;
        public string finish_reason;
        public int index;
    }

    #endregion

    #region Image Generation

    [Serializable]
    public class ImageGenerationRequest
    {
        public string prompt;
        public int n; // Number of images to generate.
        public string size; // Must be 256x256, 512x512, or 1024x1024.
        public string response_format;
        public string user;

        public ImageGenerationRequest(string prompt, int n, string size, string response_format, string user)
        {
            this.prompt = prompt;
            this.n = n;
            this.size = size;
            this.response_format = response_format;
            this.user = user;
        }
    }

    [Serializable]
    public class ImageEditRequest
    {
        public string image;
        public string mask;
        public string prompt;
        public int n; // Number of images to generate.
        public string size; // Must be 256x256, 512x512, or 1024x1024.
        public string response_format;
        public string user;

        public ImageEditRequest(string image, string mask, string prompt, int n, string size, string response_format, string user)
        {
            this.image = image;
            this.mask = mask;
            this.prompt = prompt;
            this.n = n;
            this.size = size;
            this.response_format = response_format;
            this.user = user;
        }
    }

    [Serializable]
    public class ImagesResponse
    {
        public int created;
        public List<ImageResult> data;
    }

    [Serializable]
    public class ImageResult
    {
        public string url;
        public string b64_json;
    }

    #endregion

    #region Audio

    public enum AudioResponseFormat { Json, Text, SRT, Verbose_Json, VTT }

    [Serializable]
    public class AudioTranscriptionResponse
    {
        public string text;
    }

    #endregion

    #region Embeddings

    [System.Serializable]
    public class EmbeddingRequest
    {
        public string input;
        public string model;

        public EmbeddingRequest(string input, string model)
        {
            this.input = input;
            this.model = model;
        }
    }

    [System.Serializable]
    public class EmbeddingResponse
    {
        public string Object;
        public List<EmbeddingData> data;
        public string model;
        public Usage usage;
    }

    [System.Serializable]
    public class EmbeddingData
    {
        public string Object;
        public int index;
        public List<float> embedding;
    }

    #endregion
}