// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Sanat.ApiAnthropic
{
public static class ApiAntrophicData
{
    [Serializable]
    public class ChatRequest
    {
        public string model;
        public int max_tokens;
        public ToolFunction[] tools;
        public ToolChoice tool_choice;
        public float temperature;
        public List<ChatMessage> messages;

        public ChatRequest(string model, float temperature, List<ChatMessage> messages, ToolFunction[] tools, int maxTokens)
        {
            this.model = model;
            this.max_tokens = maxTokens;
            this.temperature = temperature;
            this.messages = messages;
            this.tools = tools;
        }

        public ChatRequest() { }
    }

    [Serializable]
    public class ToolChoice
    {
        public string type;
        public string name;
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
        public Error error;
        public Usage usage;
    }

    [Serializable]
    public class Error
    {
        public string type;
        public string message;
    }

    [Serializable]
    public class ContentItem
    {
        public string text;
        public string type;
        public string id;
        public string name;
        public Dictionary<string, string> input;
    }

    [Serializable]
    public class Usage
    {
        public int input_tokens;
        public int output_tokens;
    }
    
    #region Tool
    
    [Serializable]
    public class ToolFunction
    {
        public string name;
        public string description;
        public InputSchema input_schema;
        
        public ToolFunction(string name, string description, InputSchema input_schema)
        {
            this.name = name;
            this.description = description;
            this.input_schema = input_schema;
        }
    }
    
    [System.Serializable]
    public class InputSchema
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
        public InputSchema(string propertyName, string propertyType, string description)
        {
            Property property = new Property();
            property.Description = description;
            property.Type = propertyType;
            Properties.Add(propertyName, property);
        }

        public InputSchema() { }

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
    
    public static class DataTypes
    {
        public static readonly string STRING = "string";
        public static readonly string ARRAY = "array";
        public static readonly string NUMBER = "number";
        public static readonly string OBJECT = "object";
    }
    
    #endregion
}
}