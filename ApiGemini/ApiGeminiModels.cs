// Copyright (c) Sanat. All rights reserved.
namespace Sanat.ApiGemini
{
    public enum ModelType { Chat }

    public class Model
    {
        public string Name { get; set; }
        public ModelType ModelType { get; private set; }
        public int MaxInputTokens { get; private set; }
        public int MaxOutputTokens { get; private set; }
        public float InputPricePerMil { get; private set; }
        public float OutputPricePerMil { get; private set; }
        const string FlashModelName = "gemini-1.5-flash-latest";
        const string ProModelName = "gemini-1.5-pro";
        const string Flash2ModelName = "gemini-2.0-flash-exp";

        public Model(string name, ModelType modelType, int maxInputTokens, float inputPricePerMil, float outputPricePerMil, int maxOutputTokens = 4095)
        {
            Name = name;
            ModelType = modelType;
            MaxInputTokens = maxInputTokens;
            MaxOutputTokens = maxOutputTokens;
            InputPricePerMil = inputPricePerMil;
            OutputPricePerMil = outputPricePerMil;
        }
        
        public static Model GetModelByName(string modelName)
        {
            switch (modelName)
            {
                case Flash2ModelName:
                    return Pro;
                case ProModelName:
                    return Flash;
                case FlashModelName:
                    return Flash;
                default:
                    return Flash;
            }
        }
        
        public static string DowngradeModel(string modelName)
        {
            switch (modelName)
            {
                case "gemini-2.0-flash-exp":
                    return "gemini-1.5-pro";
                case "gemini-1.5-pro":
                    return "gemini-1.5-flash-latest";
                default:
                    return "";
            }
        }
       
        public static Model Flash { get; } = new Model(FlashModelName, ModelType.Chat, 1048576, 3f, 15f, 8192);
        
        public static Model Pro { get; } = new Model(ProModelName, ModelType.Chat, 1048576, 3f, 15f, 8192);
        
        public static Model Flash2 { get; } = new Model(Flash2ModelName, ModelType.Chat, 1048576, 0f, 0f, 8192);
    }
}