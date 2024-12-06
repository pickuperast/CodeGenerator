// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        public int max_tokens;

        public CompletionRequest(string modelName, string prompt, 
            float temperature, float top_p, 
            float frequency_penalty, float presence_penalty,
            int maxTokens)
        {
            this.model = modelName;
            this.prompt = prompt;
            this.temperature = temperature;
            this.max_tokens = maxTokens;
            this.top_p = top_p;
            this.frequency_penalty = frequency_penalty;
            this.presence_penalty = presence_penalty;
        }

        public CompletionRequest(string modelName, string prompt,
            float temperature, 
            int maxTokens)
        {
            this.model = modelName;
            this.prompt = prompt;
            this.temperature = temperature;
            this.max_tokens = maxTokens;
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
        public Message message;
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
        public int max_tokens;

        public ChatRequest(string modelName, List<ChatMessage> messages, 
            float temperature, float top_p,
            float frequency_penalty, float presence_penalty, 
            int maxTokens)
        {
            this.model = modelName;
            this.messages = messages;
            this.temperature = temperature;
            this.max_tokens = maxTokens;
            this.top_p = top_p;
            this.frequency_penalty = frequency_penalty;
            this.presence_penalty = presence_penalty;
        }

        public ChatRequest(string modelName, List<ChatMessage> messages,
            float temperature, int maxTokens)
        {
            this.model = modelName;
            this.messages = messages;
            this.temperature = temperature;
            this.max_tokens = maxTokens;
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
    public class Function
    {
        public string arguments;
        public string name;
    }

    [Serializable]
    public class ChatResponse
    {
        public Choice[] choices;
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
    
    #region Tool
    
    [Serializable]
    public class FunctionCall
    {
        public string name;
        public string arguments;
    }
    
    [Serializable]
    public class Tool
    {
        public string type;
        public ToolFunction function;
        
        public Tool(string type, ToolFunction function)
        {
            this.type = type;
            this.function = function;
        }
    }

    [Serializable]
    public class ToolFunction
    {
        public string name;
        public string description;
        public Parameter parameters;
        
        public ToolFunction(string name, string description, Parameter parameters)
        {
            this.name = name;
            this.description = description;
            this.parameters = parameters;
        }
    }
    
    [System.Serializable]
    public class Parameter
    {
        [JsonProperty("type")]
        public string Type { get; set; } = DataTypes.OBJECT;

        [JsonProperty("properties")]
        public Dictionary<string, Property> Properties { get; set; } = new Dictionary<string, Property>();

        [JsonProperty("required")]
        public List<string> Required { get; set; } = new List<string>();
        
        [JsonProperty("additionalProperties")]
        public bool AdditionalProperties { get; set; } = false;

        /// <summary>
        /// Create parameter for the function call. 
        /// Use AddProperty() to add more properties. 
        /// Use DataTypes class to access data types available for propertyType
        /// </summary>
        public Parameter(string propertyName, string propertyType, string description)
        {
            Property property = new Property();
            property.Description = description;
            property.Type = propertyType;
            Properties.Add(propertyName, property);
        }

        public Parameter() { }

        /// <summary>
        /// Add a Property to the parameter. 
        /// Use DataTypes class to access data types available for propertyType
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyType"></param>
        /// <param name="description"></param>
        public void AddProperty(string propertyName, string propertyType, string description)
        {
            Property property = new Property();
            property.Description = description;
            property.Type = propertyType;
            Properties.Add(propertyName, property);
        }
    }

    [System.Serializable]
    public class Property
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
        public ToolCalls[] tool_calls;
    }
    
    [System.Serializable]
    public class ToolCalls
    {
        public string id;
        public string type;
        public FunctionCall function;
    }
    
    public static class DataTypes
    {
        public static readonly string STRING = "string";
        public static readonly string ARRAY = "array";
        public static readonly string NUMBER = "number";
        public static readonly string OBJECT = "object";
    }
        
    #endregion
}