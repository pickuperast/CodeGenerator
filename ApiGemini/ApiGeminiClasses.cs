using System;
using System.Collections.Generic;
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
        public string finishReason;
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

    public enum BlockReason
    {
        BLOCKED_REASON_UNSPECIFIED,
        OTHER,
        SAFETY
    }

    public enum ExecutableCodeLanguage
    {
        LANGUAGE_UNSPECIFIED,
        PYTHON
    }

    public enum FinishReason
    {
        FINISH_REASON_UNSPECIFIED,
        LANGUAGE,
        MAX_TOKENS,
        OTHER,
        RECITATION,
        SAFETY,
        STOP
    }

    public enum FunctionCallingMode
    {
        ANY,
        AUTO,
        MODE_UNSPECIFIED,
        NONE
    }

    public enum FunctionDeclarationSchemaType
    {
        ARRAY,
        BOOLEAN,
        INTEGER,
        NUMBER,
        OBJECT,
        STRING
    }

    public enum HarmBlockThreshold
    {
        BLOCK_LOW_AND_ABOVE,
        BLOCK_MEDIUM_AND_ABOVE,
        BLOCK_NONE,
        BLOCK_ONLY_HIGH,
        HARM_BLOCK_THRESHOLD_UNSPECIFIED
    }

    public enum HarmCategory
    {
        HARM_CATEGORY_DANGEROUS_CONTENT,
        HARM_CATEGORY_HARASSMENT,
        HARM_CATEGORY_HATE_SPEECH,
        HARM_CATEGORY_SEXUALLY_EXPLICIT,
        HARM_CATEGORY_UNSPECIFIED
    }

    public enum HarmProbability
    {
        HARM_PROBABILITY_UNSPECIFIED,
        HIGH,
        LOW,
        MEDIUM,
        NEGLIGIBLE
    }

    public enum Outcome
    {
        OUTCOME_DEADLINE_EXCEEDED,
        OUTCOME_FAILED,
        OUTCOME_OK,
        OUTCOME_UNSPECIFIED
    }

    public enum TaskType
    {
        CLASSIFICATION,
        CLUSTERING,
        RETRIEVAL_DOCUMENT,
        RETRIEVAL_QUERY,
        SEMANTIC_SIMILARITY,
        TASK_TYPE_UNSPECIFIED
    }

    public class BaseParams
    {
        public GenerationConfig GenerationConfig;
        public List<SafetySetting> SafetySettings;
    }

    public class BatchEmbedContentsRequest
    {
        public List<EmbedContentRequest> Requests;
    }

    public class BatchEmbedContentsResponse
    {
        public List<ContentEmbedding> Embeddings;
    }

    public class CachedContent : CachedContentBase
    {
        public string CreateTime;
        public string Name;
        public string Ttl;
        public string UpdateTime;
    }

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

    public class CitationMetadata
    {
        public List<CitationSource> CitationSources;
    }

    public class CitationSource
    {
        public int EndIndex;
        public string License;
        public int StartIndex;
        public string Uri;
    }

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

    public class ContentEmbedding
    {
        public List<float> Values;
    }

    public class CountTokensRequest
    {
        public List<Content> Contents;
        public GenerateContentRequest GenerateContentRequest;
    }

    public class CountTokensResponse
    {
        public int TotalTokens;
    }

    public class EmbedContentRequest
    {
        public Content Content;
        public TaskType TaskType;
        public string Title;
    }

    public class EmbedContentResponse
    {
        public ContentEmbedding Embedding;
    }

    public class ExecutableCode
    {
        public string Code;
        public ExecutableCodeLanguage Language;
    }

    public class FileData
    {
        public string FileUri;
        public string MimeType;
    }

    public class FunctionCall
    {
        public object args;
        public string name;
    }

    public class FunctionCallingConfig
    {
        public List<string> AllowedFunctionNames;
        public FunctionCallingMode Mode;
    }

    public class FunctionDeclaration
    {
        public string name;
        public string description;
        public FunctionDeclarationSchema parameters;
    }

    public class FunctionDeclarationSchema
    {
        public FunctionDeclarationSchemaType type;
        public Dictionary<string, FunctionDeclarationSchemaProperty> properties;
        public List<string> required;
    }

    public class FunctionDeclarationSchemaProperty : Schema
    {
    }

    public class FunctionResponse
    {
        public string Name;
        public object Response;
    }

    public class GenerateContentCandidate
    {
        public CitationMetadata CitationMetadata;
        public Content Content;
        public string FinishMessage;
        public FinishReason FinishReason;
        public int Index;
        public List<SafetyRating> SafetyRatings;
    }

    public class GenerateContentRequest : BaseParams
    {
        public string CachedContent;
        public List<Content> Contents;
        public string SystemInstruction;
        public ToolConfig ToolConfig;
        public List<Tool> tools;
    }

    public class GenerateContentResponse
    {
        public List<GenerateContentCandidate> Candidates;
        public PromptFeedback PromptFeedback;
        public UsageMetadata UsageMetadata;
    }

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
        public HarmCategory Category;
        public HarmProbability Probability;
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
        public FunctionDeclarationSchemaType type;
    }

    [Serializable]
    public class ToolConfig
    {
        public FunctionCallingConfig FunctionCallingConfig;
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
}