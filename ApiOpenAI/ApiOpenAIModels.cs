// Copyright (c) Sanat. All rights reserved.
namespace Sanat.ApiOpenAI
{
    public enum ModelType { Completion, Chat, Edit, Transcription }

    public class Model
    {
        public string Name { get; set; }
        public ModelType ModelType { get; private set; }
        public int MaxInputTokens { get; private set; }
        public int MaxOutputTokens { get; private set; }
        public float InputPrice { get; private set; }
        public float OutputPrice { get; private set; }

        public Model(string name, ModelType modelType, int maxInputTokens, float inputPrice, float outputPrice, int maxOutputTokens = 4095)
        {
            Name = name;
            ModelType = modelType;
            MaxInputTokens = maxInputTokens;
            MaxOutputTokens = maxOutputTokens;
            InputPrice = inputPrice;
            OutputPrice = outputPrice;
        }

        public static Model GPT4o1mini { get; } = new Model("o1-mini", ModelType.Chat, 128000, 3.00f, 12.00f, 65536);
        public static Model GPT4o1preview { get; } = new Model("o1-preview", ModelType.Chat, 128000, 15.00f, 60.00f, 32768);
        public static Model GPT4o { get; } = new Model("gpt-4o", ModelType.Chat, 128000, 2.50f, 10.00f, 16384);
        public static Model GPT4o_16K { get; } = new Model("gpt-4o-2024-08-06", ModelType.Chat, 128000, 2.50f, 10.00f, 16384);
        public static Model GPT4omini { get; } = new Model("gpt-4o-mini", ModelType.Chat, 128000, 0.15f, 0.60f, 16384);
        public static Model GPT4_Turbo { get; } = new Model("gpt-4-turbo", ModelType.Chat, 128000, 10.00f, 20.00f, 4096);
        public static Model GPT4 { get; } = new Model("gpt-4", ModelType.Chat, 8192, 0.03f, 0.06f, 8192);
        public static Model GPT4_32K { get; } = new Model("gpt-4-32k", ModelType.Chat, 32768, 0.06f, 0.12f, 32768);
        public static Model GPT3_5_Turbo { get; } = new Model("gpt-3.5-turbo", ModelType.Chat, 4096, 0.002f, 0.004f, 4096);
        public static Model GPT3_5_Turbo_16K { get; } = new Model("gpt-3.5-turbo-16k", ModelType.Chat, 16384, 0.003f, 0.006f, 16384);
        public static Model Whisper_1 { get; } = new Model("whisper-1", ModelType.Transcription, 65536, 0.006f, 0.006f, 65536);
    }
}