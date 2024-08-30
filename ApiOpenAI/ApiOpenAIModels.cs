// Copyright (c) Sanat. All rights reserved.
namespace Sanat.ApiOpenAI
{
    public enum ModelType { Completion, Chat, Edit, Transcription }

    public class Model
    {
        public string Name { get; set; }
        public ModelType ModelType { get; private set; }
        public int MaxTokens { get; private set; }
        
        public int MaxOutput { get; private set; }

        public Model(string name, ModelType modelType, int maxTokens, int maxOutput = 4095)
        {
            Name = name;
            ModelType = modelType;
            MaxTokens = maxTokens;
            MaxOutput = maxOutput;
        }

        public static Model GPT4o { get; } = new Model("gpt-4o", ModelType.Chat, 128000);
        public static Model GPT4o_16K { get; } = new Model("gpt-4o-2024-08-06", ModelType.Chat, 16384);
        public static Model GPT4omini { get; } = new Model("gpt-4o-mini", ModelType.Chat, 128000, 16384);

        public static Model GPT4_Turbo { get; } = new Model("gpt-4-turbo", ModelType.Chat, 128000);

        public static Model GPT4 { get; } = new Model("gpt-4", ModelType.Chat, 8192);

        public static Model GPT4_32K { get; } = new Model("gpt-4-32k", ModelType.Chat, 32768);

        public static Model GPT3_5_Turbo { get; } = new Model("gpt-3.5-turbo", ModelType.Chat, 4096);

        public static Model GPT3_5_Turbo_16K { get; } = new Model("gpt-3.5-turbo-16k", ModelType.Chat, 16384);
  
        public static Model Whisper_1 { get; } = new Model("whisper-1", ModelType.Transcription, 65536);
    }

}