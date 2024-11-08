using System.Collections.Generic;
using System.Linq;
using Sanat.CodeGenerator.Agents;
using Sanat.CodeGenerator.Bookmarks;
using Sanat.CodeGenerator.CodebaseRag;
using UnityEngine;

namespace Sanat.CodeGenerator.Editor
{
    public class CodeGeneratorSettingsManager
    {
        public void SaveSettings(CodeGenerator codeGenerator)
        {
            PlayerPrefs.SetString("SelectedClassNames", string.Join(",", codeGenerator.selectedClassNames));
            PlayerPrefs.SetString("GeminiApiKey", codeGenerator.geminiApiKey);
            PlayerPrefs.SetString("OpenaiApiKey", codeGenerator.openaiApiKey);
            PlayerPrefs.SetString("GroqApiKey", codeGenerator.groqApiKey);
            PlayerPrefs.SetString("AntrophicApiKey", codeGenerator.antrophicApiKey);
            PlayerPrefs.SetString("GeminiProjectName", codeGenerator.GeminiProjectName);
            PlayerPrefs.SetString("SupabaseUrl", codeGenerator.supabaseUrl);
            PlayerPrefs.SetString("SupabaseKey", codeGenerator.supabaseKey);
            PlayerPrefs.SetString("TableName", codeGenerator.tableName);
            PlayerPrefs.SetInt("IsRag", codeGenerator.isRag ? 1 : 0);
            PlayerPrefs.SetString("IgnoredFolders", string.Join(",", codeGenerator._ignoredFolders));
            PlayerPrefs.SetString(CodeGenerator.INCLUDED_FOLDERS_PREFS_KEY, JsonUtility.ToJson(codeGenerator.includedFolders));
            codeGenerator.bookmarkManager.SaveBookmarksToPrefs();
            PlayerPrefs.Save();
            codeGenerator.bookmarkManager.OnBookmarkLoaded -= codeGenerator.LoadBookmarkData;
            SaveAgentModelSettings(codeGenerator);
        }
        
        public void LoadSettings(CodeGenerator codeGenerator)
        {
            codeGenerator.selectedClassNames = new List<string>(
                PlayerPrefs.GetString("SelectedClassNames", "")
                    .Split(',')
                    .Where(s => !string.IsNullOrEmpty(s))
            );
            codeGenerator.geminiApiKey = PlayerPrefs.GetString("GeminiApiKey", "");
            codeGenerator.openaiApiKey = PlayerPrefs.GetString("OpenaiApiKey", "");
            codeGenerator.groqApiKey = PlayerPrefs.GetString("GroqApiKey", "");
            codeGenerator.antrophicApiKey = PlayerPrefs.GetString("AntrophicApiKey", "");
            codeGenerator.supabaseUrl = PlayerPrefs.GetString("SupabaseUrl", "");
            codeGenerator.supabaseKey = PlayerPrefs.GetString("SupabaseKey", "");
            codeGenerator.tableName = PlayerPrefs.GetString("TableName", "");
            codeGenerator.isRag = PlayerPrefs.GetInt("IsRag", 0) == 1;
            codeGenerator.GeminiProjectName = PlayerPrefs.GetString(CodeGenerator.PREFS_GEMINI_PROJECT_NAME, "");
            codeGenerator._ignoredFolders = PlayerPrefs.GetString("IgnoredFolders", "").Split(',').Where(s => !string.IsNullOrEmpty(s)).ToList();
            codeGenerator.includedFolders = JsonUtility.FromJson<List<CodeGenerator.IncludedFolder>>(PlayerPrefs.GetString(CodeGenerator.INCLUDED_FOLDERS_PREFS_KEY, "[]"));
            codeGenerator.taskInput = PlayerPrefs.GetString(CodeGenerator.PREFS_KEY_TASK, "");
            codeGenerator.isGeneratingCode = false;
            
            CodeGenerator.IncludedFoldersManager = new IncludedFoldersManager();
            codeGenerator.RefreshClassList();
            codeGenerator.CheckAndHandleFirstLaunch();
            codeGenerator.bookmarkManager = new CodeGeneratorBookmarks();
            codeGenerator.bookmarkManager.OnBookmarkLoaded += codeGenerator.LoadBookmarkData;
            codeGenerator.bookmarkManager.LoadBookmarksFromPrefs();
            var bookmarks = codeGenerator.bookmarkManager.GetBookmarks();
            codeGenerator.bookmarkManager.InitializeReorderableList();
            codeGenerator.ragProcessor = new RagProcessor();
            InitializeAgentModelSettings(codeGenerator);
            codeGenerator.IsSettingsLoaded = true;
        }
        
        public void InitializeAgentModelSettings(CodeGenerator codeGenerator)
        {
            codeGenerator.agentModelSettings = new Dictionary<string, AgentModelSettings>
            {
                { "AgentCodeArchitector", new AgentModelSettings 
                {
                    AgentName = "Code Architector",
                    ApiProvider = AbstractAgentHandler.ApiProviders.Anthropic,
                    ModelName = ApiAnthropic.Model.Claude35Latest.Name
                }},
                { "AgentCodeMerger", new AgentModelSettings 
                {
                    AgentName = "Code Merger",
                    ApiProvider = AbstractAgentHandler.ApiProviders.Gemini,
                    ModelName = Sanat.ApiGemini.ApiGeminiModels.Pro
                }},
                { "AgentCodeValidator", new AgentModelSettings 
                {
                    AgentName = "Code Validator",
                    ApiProvider = AbstractAgentHandler.ApiProviders.Anthropic,
                    ModelName = ApiAnthropic.Model.Haiku35Latest.Name
                }}
            };
        }
        
        public void SaveAgentModelSettings(CodeGenerator codeGenerator)
        {
            // Convert dictionary to JSON and save to PlayerPrefs
            string settingsJson = JsonUtility.ToJson(new SerializableAgentModelSettingsList(codeGenerator.agentModelSettings));
            PlayerPrefs.SetString("AgentModelSettings", settingsJson);
            PlayerPrefs.Save();
        }
        
        [System.Serializable]
        private class SerializableAgentModelSettingsList
        {
            public List<AgentModelSettings> settings;

            public SerializableAgentModelSettingsList(Dictionary<string, AgentModelSettings> dict)
            {
                settings = dict.Values.ToList();
            }

            public Dictionary<string, AgentModelSettings> ToDictionary()
            {
                return settings.ToDictionary(s => GetAgentKey(s.AgentName));
            }
        }

        private static string GetAgentKey(string agentName)
        {
            switch (agentName)
            {
                case "Code Architector": return "AgentCodeArchitector";
                case "Code Merger": return "AgentCodeMerger";
                case "Code Validator": return "AgentCodeValidator";
                default: return agentName;
            }
        }
    }
}