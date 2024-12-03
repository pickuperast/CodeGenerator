using Sanat.ApiOpenAI;

namespace Sanat.ApiGroq
{
    public static class ApiGroqModels
    {
        public static Model Llama3_70b_8192_tool { get; } = new Model("llama3-groq-70b-8192-tool-use-preview", ModelType.Chat, 8192, 0.00f, 0.00f, 8192);
        public static Model Llama3_70b_8192 { get; } = new Model("llama3-70b-8192", ModelType.Chat, 8192, 0.00f, 0.00f, 8192);
        public static Model Mixtral_8x7b_32768 { get; } = new Model("mixtral-8x7b-32768", ModelType.Chat, 8192, 0.00f, 0.00f, 8192);
        public static Model Gemma2_9b_it { get; } = new Model("gemma2-9b-it", ModelType.Chat, 8192, 0.00f, 0.00f, 8192);
        public static Model Whisper_large_v3 { get; } = new Model("whisper-large-v3", ModelType.Chat, 8192, 0.00f, 0.00f, 8192);
        
        public static Model GetModelByName(string modelName)
        {
            switch (modelName)
            {
                case "llama3-groq-70b-8192-tool-use-preview":
                    return Llama3_70b_8192_tool;
                case "llama3-70b-8192":
                    return Llama3_70b_8192;
                case "mixtral-8x7b-32768":
                    return Mixtral_8x7b_32768;
                case "gemma2-9b-it":
                    return Gemma2_9b_it;
                case "whisper-large-v3":
                    return Whisper_large_v3;
                default:
                    return null;
            }
        }
    }
}