using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.Serialization;

namespace Sanat.ApiGemini
{
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
        public List<Candidate> candidates;
        public UsageMetadata usageMetadata;
    }

    [Serializable]
    public class Candidate
    {
        public Content content;
        public FinishReason finishReason;
        public List<SafetyRating> safetyRatings;
    }

    [Serializable]
    public class Contents
    {
        public string role;
        public Parts parts;
    }

    [Serializable]
    public class Parts
    {
        public string text;
    }

    [Serializable]
    public class SafetySettings
    {
        public string category;
        public string threshold;
    }
    
    [Serializable]
    public enum BlockReason
    {
        BLOCKED_REASON_UNSPECIFIED,
        OTHER,
        SAFETY
    }
    
    [Serializable]
    public enum ExecutableCodeLanguage
    {
        LANGUAGE_UNSPECIFIED,
        PYTHON
    }

    [Serializable]
    public enum FinishReason
    {
        FINISH_REASON_UNSPECIFIED,
        LANGUAGE,
        MAX_TOKENS,
        OTHER,
        RECITATION,
        SAFETY,
        STOP,
        MALFORMED_FUNCTION_CALL
    }

    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FunctionCallingMode
    {
        [JsonProperty("any")] ANY,
        [JsonProperty("auto")] AUTO,
        [JsonProperty("mode_unspecified")] MODE_UNSPECIFIED,
        [JsonProperty("none")] NONE
    }

    [Serializable]
    [JsonConverter(typeof(LowercaseStringEnumConverter))]
    public enum FunctionDeclarationSchemaType
    {
        [JsonProperty("array")] ARRAY,
        [JsonProperty("boolean")] BOOLEAN,
        [JsonProperty("integer")] INTEGER,
        [JsonProperty("number")] NUMBER,
        [JsonProperty("object")] OBJECT,
        [JsonProperty("string")] STRING
    }
    
    [Serializable]
    public enum HarmBlockThreshold
    {
        BLOCK_LOW_AND_ABOVE,
        BLOCK_MEDIUM_AND_ABOVE,
        BLOCK_NONE,
        BLOCK_ONLY_HIGH,
        HARM_BLOCK_THRESHOLD_UNSPECIFIED
    }
    
    [Serializable]
    public enum HarmCategory
    {
        HARM_CATEGORY_DANGEROUS_CONTENT,
        HARM_CATEGORY_HARASSMENT,
        HARM_CATEGORY_HATE_SPEECH,
        HARM_CATEGORY_SEXUALLY_EXPLICIT,
        HARM_CATEGORY_UNSPECIFIED
    }

    [Serializable]
    public enum HarmProbability
    {
        HARM_PROBABILITY_UNSPECIFIED,
        HIGH,
        LOW,
        MEDIUM,
        NEGLIGIBLE
    }

    [Serializable]
    public enum Outcome
    {
        OUTCOME_DEADLINE_EXCEEDED,
        OUTCOME_FAILED,
        OUTCOME_OK,
        OUTCOME_UNSPECIFIED
    }

    [Serializable]
    public enum TaskType
    {
        CLASSIFICATION,
        CLUSTERING,
        RETRIEVAL_DOCUMENT,
        RETRIEVAL_QUERY,
        SEMANTIC_SIMILARITY,
        TASK_TYPE_UNSPECIFIED
    }

    [Serializable]
    public class BaseParams
    {
        public GenerationConfig GenerationConfig;
        public List<SafetySetting> SafetySettings;
    }
    [Serializable]
    public class BatchEmbedContentsRequest
    {
        public List<EmbedContentRequest> Requests;
    }
    [Serializable]
    public class BatchEmbedContentsResponse
    {
        public List<ContentEmbedding> Embeddings;
    }
    [Serializable]
    public class CachedContent : CachedContentBase
    {
        public string CreateTime;
        public string Name;
        public string Ttl;
        public string UpdateTime;
    }
    [Serializable]
    public class CachedContentBase
    {
        public List<Content> contents;
        public string DisplayName;
        public string ExpireTime;
        public string Model;
        public string SystemInstruction;
        public ToolConfig ToolConfig;
        public List<Tool> tools;
    }
    [Serializable]
    public class CitationMetadata
    {
        public List<CitationSource> CitationSources;
    }
    [Serializable]
    public class CitationSource
    {
        public int EndIndex;
        public string License;
        public int StartIndex;
        public string Uri;
    }
    [Serializable]
    public class CodeExecutionResult
    {
        public Outcome Outcome;
        public string Output;
    }

    [Serializable]
    public class Content
    {
        public List<Part> parts;
        public string role;
    }
    [Serializable]
    public class ContentEmbedding
    {
        public List<float> Values;
    }
    [Serializable]
    public class CountTokensRequest
    {
        public List<Content> Contents;
        public GenerateContentRequest GenerateContentRequest;
    }
    [Serializable]
    public class CountTokensResponse
    {
        public int TotalTokens;
    }
    [Serializable]
    public class EmbedContentRequest
    {
        public Content Content;
        public TaskType TaskType;
        public string Title;
    }
    [Serializable]
    public class EmbedContentResponse
    {
        public ContentEmbedding Embedding;
    }
    [Serializable]
    public class ExecutableCode
    {
        public string Code;
        public ExecutableCodeLanguage Language;
    }
    [Serializable]
    public class FileData
    {
        public string FileUri;
        public string MimeType;
    }
    [Serializable]
    public class FunctionCall
    {
        public string name;
        public object args;
    }
    [Serializable]
    public class FunctionCallingConfig
    {
        [JsonProperty("allowed_function_names")]
        public List<string> AllowedFunctionNames;

        public string mode;
    }

    [Serializable]
    public class FunctionDeclaration
    {
        public string name;
        public string description;
        public FunctionDeclarationSchema parameters;
    }

    [Serializable]
    public class FunctionDeclarationSchema
    {
        [JsonConverter(typeof(LowercaseStringEnumConverter))]
        public FunctionDeclarationSchemaType type;
        public Dictionary<string, FunctionDeclarationSchemaProperty> properties;
        public List<string> required;
    }

    public class FunctionDeclarationSchemaProperty : Schema
    {
    }
    [Serializable]
    public class FunctionResponse
    {
        public string Name;
        public object Response;
    }
    [Serializable]
    public class GenerateContentCandidate
    {
        public CitationMetadata citationMetadata;
        public Content content;
        public string finishMessage;
        public FinishReason finishReason;
        public int index;
        public List<SafetyRating> safetyRatings;
        public float avgLogprobs;
    }
    [Serializable]
    public class GenerateContentRequest : BaseParams
    {
        public string CachedContent;
        public List<Content> Contents;
        public string SystemInstruction;
        public ToolConfig ToolConfig;
        public List<Tool> tools;
    }
    [Serializable]
    public class GenerateContentResponse
    {
        public List<GenerateContentCandidate> candidates;
        public PromptFeedback PromptFeedback;
        public UsageMetadata usageMetadata;
    }
    [Serializable]
    public class GenerationConfig
    {
        public int candidateCount = 1;
        public int maxOutputTokens = 8192;
        public string responseMimeType;
        public ResponseSchema responseSchema;
        public List<string> stopSequences;
        public float temperature;
        public int topK;
        public float topP;
    }

    [Serializable]
    public class GenerativeContentBlob
    {
        public string Data;
        public string MimeType;
    }

    [Serializable]
    public class PromptFeedback
    {
        public BlockReason BlockReason;
        public string BlockReasonMessage;
        public List<SafetyRating> SafetyRatings;
    }

    public class ResponseSchema : Schema
    {
    }

    [Serializable]
    public class SafetyRating
    {
        public HarmCategory category;
        public HarmProbability probability;
    }

    [Serializable]
    public class SafetySetting
    {
        public HarmCategory Category;
        public HarmBlockThreshold Threshold;
    }

    [Serializable]
    public class Schema
    {
        public string description;
        [JsonConverter(typeof(LowercaseStringEnumConverter))]
        public FunctionDeclarationSchemaType type;
    }

    [Serializable]
    public class ToolConfig
    {
        public FunctionCallingConfig function_calling_config;
    }

    [Serializable]
    public class UsageMetadata
    {
        public int cachedContentTokenCount;
        public int candidatesTokenCount;
        public int promptTokenCount;
        public int totalTokenCount;
    }

    [Serializable]
    public class Part
    {
        public string text;
        public GenerativeContentBlob inlineData;
        public FunctionCall functionCall;
        public FunctionResponse functionResponse;
        public FileData fileData;
        public ExecutableCode executableCode;
        public CodeExecutionResult codeExecutionResult;
    }

    [Serializable]
    public class ToolRequest
    {
        public List<Content> contents;
        public List<Tool> tools;
    }

    [Serializable]
    public class Tool
    {
        public List<FunctionDeclaration> function_declarations;
    }
    
    

    [Serializable]
    public class TextToSpeechRequest
    {
        public Input input;
        public Voice voice;
        public AudioConfig audioConfig;

        public TextToSpeechRequest(string text, string languageCode, string name, string ssmlGender)
        {
            input = new Input { text = text };
            voice = new Voice { languageCode = languageCode, name = name, ssmlGender = ssmlGender };
            audioConfig = new AudioConfig { audioEncoding = "MP3" };
        }
    }

    [Serializable]
    public class Input
    {
        public string text;
    }

    [Serializable]
    public class Voice
    {
        public string languageCode;
        public string name;
        public string ssmlGender;
    }

    [Serializable]
    public class AudioConfig
    {
        public string audioEncoding;
    }

    [Serializable]
    public class ChatRequest
    {
        public Contents contents;
        public SafetySettings safety_settings;
        public GenerationConfig generation_config;
        public List<Tool> tools;
        public ToolConfig tool_config;
        public Content system_instruction;

        public ChatRequest(string message, float temperature)
        {
            contents = new Contents
            {
                role = "user",
                parts = new Parts { text = message }
            };
            safety_settings = new SafetySettings
            {
                category = "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                threshold = "BLOCK_LOW_AND_ABOVE"
            };
            generation_config = new GenerationConfig
            {
                temperature = temperature,
                topP = 0.8f,
                topK = 40
            };
        }
    }
    
    public class LowercaseStringEnumConverter : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Type enumType = value.GetType();
            string enumName = Enum.GetName(enumType, value);


            if (enumType.GetCustomAttribute<JsonConverterAttribute>() != null) {

                var prop = enumType.GetMember(enumName)[0].GetCustomAttribute<JsonPropertyAttribute>();

                if (prop != null) {
                    writer.WriteValue(prop.PropertyName);
                    return;
                }

            }
            
            writer.WriteValue(enumName.ToLowerInvariant());
        }
    }
}