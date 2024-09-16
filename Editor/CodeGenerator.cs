// Copyright (c) Sanat. All rights reserved.
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using Sanat.CodeGenerator.Agents;
using Sanat.CodeGenerator.Bookmarks;
using Sanat.CodeGenerator.Extensions;

namespace Sanat.CodeGenerator
{
    public class CodeGenerator : EditorWindow
    {
        public List<string> SelectedClassNames => selectedClassNames;
        private List<string> selectedClassNames = new();
        private Vector2 scrollPosition;
        private Dictionary<string, string> classToPath = new();
        private string classNameInput = "";
        private string generatedPrompt = "";
        private string taskInput = "";
        private float buttonAnimationTime = 1f;
        private float buttonAnimationProgress = 0f;
        private bool isButtonAnimating = false;
        private Color buttonColor = Color.green;
        private bool isSettingsVisible = false;
        private string geminiApiKey = "";
        private string openaiApiKey = "";
        private string antrophicApiKey = "";
        private string groqApiKey = "";
        public static string GeminiProjectName = "";
        private bool _isAwaitingReply;
        private List<string> _ignoredFolders = new();
        private bool isGeneratingCode = false;
        private float generationProgress = 0f;
        private float lastProgressUpdateTime = 0f;
        private float targetProgress = 0f;
        private float progressSpeed = 0.00001f;
        private const string PLUGIN_NAME = "<color=#FFD700>Code Generator</color> ";
        private const string PROMPTS_SAVE_FOLDER = "Sanat/CodeGenerator/Prompts";
        public const string PREFS_GEMINI_PROJECT_NAME = "GeminiProjectName";
        private const string PREFS_KEY_TASK = "Task";
        private List<IncludedFolder> includedFolders = new();
        private const string INCLUDED_FOLDERS_PREFS_KEY = "IncludedFolders";
        public static IncludedFoldersManager IncludedFoldersManager;
        private CodeGeneratorBookmarks bookmarkManager;
        private Vector2 taskScrollPosition;

        [System.Serializable]
        private class IncludedFolder
        {
            public string path;
            public bool isEnabled;
        }

        [MenuItem("Tools/Sanat/CodeGenerator")]
        public static void ShowWindow()
        {
            GetWindow<CodeGenerator>("Code Generator");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Task Description", EditorStyles.boldLabel);
            
            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
            textAreaStyle.wordWrap = true;

            float textAreaHeight = 3 * EditorGUIUtility.singleLineHeight;
            
            taskScrollPosition = EditorGUILayout.BeginScrollView(taskScrollPosition, GUILayout.Height(textAreaHeight));
            
            taskInput = EditorGUILayout.TextArea(taskInput, textAreaStyle, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            GUILayout.Space(20);
            if (isSettingsVisible)
            {
                DrawSettingsFields();
            }
            EditorGUILayout.LabelField("Select Class Names:", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh Class List"))
            {
                RefreshClassList();
            }
            string settingsButtonLabel = isSettingsVisible ? "Close Settings" : "Settings";
            if (GUILayout.Button(settingsButtonLabel))
            {
                isSettingsVisible = !isSettingsVisible;
            }
            EditorGUILayout.Space();
            classNameInput = EditorGUILayout.TextField("Class Name", classNameInput);
            if (!string.IsNullOrEmpty(classNameInput))
            {
                List<string> filteredSuggestions = new List<string>();
                foreach (var kv in classToPath)
                {
                    if (!kv.Key.StartsWith(classNameInput, StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    if (_ignoredFolders.Any(ignoredFolder => kv.Value.Contains(ignoredFolder)))
                        continue;

                    filteredSuggestions.Add(kv.Key);
                }
                string[] suggestions = filteredSuggestions.OrderBy(c => c).ToArray();
                if (suggestions.Length > 0)
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                    foreach (var suggestion in suggestions)
                    {
                        if (GUILayout.Button(suggestion))
                        {
                            if (!selectedClassNames.Contains(suggestion))
                            {
                                selectedClassNames.Add(suggestion);
                            }
                            classNameInput = "";
                            GUI.FocusControl(null);
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            EditorGUILayout.Space();
            if (selectedClassNames.Count > 0)
            {
                if (GUILayout.Button("Clear all selected classes"))
                {
                    selectedClassNames.Clear();
                }
                GUILayout.Space(10);
            }
            if (selectedClassNames.Count > 4)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            }
            for (int i = 0; i < selectedClassNames.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("x", GUILayout.Width(40)))
                {
                    selectedClassNames.RemoveAt(i);
                    i--;
                    continue;
                }
                EditorGUILayout.LabelField(selectedClassNames[i]);
                EditorGUILayout.EndHorizontal();
            }
            if (selectedClassNames.Count > 4)
            {
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Prompt"))
            {
                generatedPrompt = "";
            }
            GUI.backgroundColor = buttonColor;
            if (GUILayout.Button("Generate Prompt"))
            {
                ExecGeneratePrompt();
            }

            if (isGeneratingCode)
            {
                Rect progressRect = GUILayoutUtility.GetRect(100, 20);
                EditorGUI.ProgressBar(progressRect, generationProgress, $"Generating... {generationProgress * 100:F0}%");
            }
            
            if (GUILayout.Button("Generate Code"))
            {
                ExecGenerateCode();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            bookmarkManager.DrawBookmarksUI(this);

            if (!string.IsNullOrEmpty(generatedPrompt))
            {
                GUILayout.Label("Generated Prompt:", EditorStyles.boldLabel);
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Screen.height * 0.4f));
                generatedPrompt = EditorGUILayout.TextArea(generatedPrompt, GUILayout.Height(20 * EditorGUIUtility.singleLineHeight));
                GUILayout.EndScrollView();
            }
            if (isButtonAnimating)
            {
                Repaint();
            }
        }

        #region Generation
        private void ExecGeneratePrompt()
        {
            Dictionary<string, string> projectCode = new Dictionary<string, string>();
            foreach (string className in selectedClassNames)
            {
                if (classToPath.TryGetValue(className, out string filePath))
                {
                    projectCode[filePath] = File.ReadAllText(filePath);
                }
            }
            
            var includedFolders = IncludedFoldersManager.GetEnabledFolders();
            foreach (var folder in includedFolders)
            {
                string[] files = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    projectCode[file] = File.ReadAllText(file);
                }
            }
            
            SaveTaskToPrefs();
            GeneratePrompt(projectCode, 3);
            SavePromptToFile();
            CopyPromptToClipboard();
            StartButtonAnimation();
        }

        private void SaveTaskToPrefs()
        {
            PlayerPrefs.SetString(PREFS_KEY_TASK, taskInput);
            PlayerPrefs.Save();
        }

        private void ExecGenerateCode()
        {
            isGeneratingCode = true;
            generationProgress = 0f;
            targetProgress = .33f;
            lastProgressUpdateTime = (float)EditorApplication.timeSinceStartup;
            EditorApplication.update += UpdateProgressBar;
            Dictionary<string, string> projectCode = new Dictionary<string, string>();
            foreach (string className in selectedClassNames)
            {
                if (classToPath.TryGetValue(className, out string filePath))
                {
                    projectCode[filePath] = File.ReadAllText(filePath);
                }
            }
            SaveTaskToPrefs();
            string[] projectCodePathes = projectCode.Keys.ToArray();
            GeneratePrompt(projectCode);
            SavePromptToFile();
            CopyPromptToClipboard();
            StartButtonAnimation();
            var includedCode = GenerateIncludedCode(projectCode, "", ref generatedPrompt);
            AbstractAgentHandler.ApiKeys apiKeys = new AbstractAgentHandler.ApiKeys(openaiApiKey, antrophicApiKey, groqApiKey, geminiApiKey);
            AgentCodeStrategyArchitector agentCodeStrategyArchitector = new AgentCodeStrategyArchitector(apiKeys, taskInput, includedCode);
            AbstractAgentHandler agentCodeArchitector = new AgentCodeArchitector(apiKeys, taskInput, includedCode);
            AbstractAgentHandler agentSolutionToDicts = new AgentSolutionToDict(apiKeys, projectCodePathes);
            AbstractAgentHandler agentCodeMerger = new AgentCodeMerger(apiKeys, projectCode);
            
            // agentCodeStrategyArchitector.OnComplete += (string result) =>
            // {
            //     Debug.Log($"{agentCodeStrategyArchitector.Name} OnComplete: {result.Substring(0,500)}");
            //     //agentCodeArchitector.Handle(result);
            // };
            // agentCodeStrategyArchitector.Handle("");
            // return;
            agentCodeMerger.OnComplete += (string result) =>
            {
                UpdateProgress(1f);
                isGeneratingCode = false;
                Repaint();
            };
            
            agentCodeArchitector.OnComplete += (string result) =>
            {
                Debug.Log($"{agentCodeArchitector.Name} OnComplete: {result.Substring(0,500)}");
                agentCodeArchitector.SaveResultToFile(result);
                AgentCodeValidator agentValidator = new AgentCodeValidator(apiKeys, taskInput, includedCode, result);
                agentValidator.OnComplete += (string validationResult) =>
                {
                    Debug.Log($"{agentValidator.Name} OnComplete: {validationResult}");
                    NextStepsAfterArchitector(validationResult, agentCodeMerger, 
                        apiKeys, result, agentSolutionToDicts, 
                        agentCodeArchitector as AgentCodeArchitector, includedCode);
                };
                agentValidator.Handle(result);
                UpdateProgress(0.33f);
            };
            agentCodeArchitector.SetNext(null);
            agentCodeArchitector.Handle(generatedPrompt);
        }

        private AbstractAgentHandler NextStepsAfterArchitector(string validationResult,
            AbstractAgentHandler agentCodeMerger, 
            AbstractAgentHandler.ApiKeys apiKeys, 
            string result, 
            AbstractAgentHandler agentSolutionToDicts,
            AgentCodeArchitector agentCodeArchitector,
            string includedCode) 
        {
            AgentCodeMerger agentCodeMergerDirect = agentCodeMerger as AgentCodeMerger;
            var firstRow = validationResult.Split('\n')[0];
            if (firstRow.Contains("1"))
            {
                Debug.Log($"Validation successful. Proceeding with code merging. validationResult: {validationResult}");
                agentCodeMergerDirect.InsertCode(result);
            }
            else
            {
                Debug.Log($"Validation failed. Returning to Architector. validationResult: {validationResult}");
                agentCodeArchitector.WorkWithFeedback(validationResult, result, (string feedbackResult) =>
                {
                    Debug.Log($"Feedback result: {feedbackResult}");
                    AgentCodeValidator agentValidator = new AgentCodeValidator(apiKeys, taskInput, includedCode, feedbackResult);
                    agentValidator.OnComplete += (string validationResult) =>
                    {
                        Debug.Log($"{agentValidator.Name} OnComplete: {validationResult}");
                        NextStepsAfterArchitector(validationResult, agentCodeMerger, 
                            apiKeys, result, agentSolutionToDicts, 
                            agentCodeArchitector, includedCode);
                    };
                    agentValidator.Handle(feedbackResult);
                });


                // agentSolutionToDicts.OnComplete += (string result) =>
                // {
                //     Debug.Log($"{agentSolutionToDicts.Name} result({result} chars): {result}");
                //     agentCodeMergerDirect.Handle(result);
                // };
                // agentSolutionToDicts.Handle(result);
            }

            return agentCodeMerger;
        }

        private string GeneratePrompt(Dictionary<string, string> projectCode, int rowsToRemove = 0)
        {
            string includedCode = "";
            string includedCodeRaw = "";
            includedCodeRaw = GenerateIncludedCode(projectCode, includedCodeRaw, ref includedCode);
            includedCode = Regex.Replace(includedCode, @"\s+", " ").Trim();
            string clearedCodeScriptForLLM = includedCodeRaw;
            string promptLocation = Application.dataPath + $"{AbstractAgentHandler.PROMPTS_FOLDER_PATH}Unity code architector.md";
            string loadedPrompt = AbstractAgentHandler.LoadPrompt(promptLocation);
            string newPrompt = CodeGeneratorExtensions.RemoveLastNRows(loadedPrompt, rowsToRemove);
            string prompt = newPrompt + $"\n# CODE: {clearedCodeScriptForLLM}\n\n# TASK: {taskInput}";
            generatedPrompt = prompt;
            return clearedCodeScriptForLLM;
        }

        private string GenerateIncludedCode(Dictionary<string, string> projectCode, string includedCodeRaw, ref string includedCode)
        {
            foreach (var kvPathToCode in projectCode)
            {
                foreach (string className in selectedClassNames)
                {
                    if (kvPathToCode.Key.Contains(className))
                    {
                        includedCodeRaw += $"// {kvPathToCode.Key}:\n {kvPathToCode.Value} \n\n";
                        includedCode += File.ReadAllText(kvPathToCode.Key).Replace("\n", "").Replace("\r", "") + "\n\n";
                    }
                }
            }
            
            var includedFolders = IncludedFoldersManager.GetEnabledFolders();
            foreach (var folder in includedFolders)
            {
                string[] files = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    includedCodeRaw += $"// {file}:\n {File.ReadAllText(file)} \n\n";
                    includedCode += File.ReadAllText(file).Replace("\n", "").Replace("\r", "") + "\n\n";
                }
            }
            
            return includedCodeRaw;
        }

        #endregion

        #region Progress Bar
        private void UpdateProgress(float progress)
        {
            targetProgress = progress;
            if (progress >= 1f)
            {
                isGeneratingCode = false;
                EditorApplication.update -= UpdateProgressBar;
                Repaint();
            }
        }

        private void UpdateProgressBar()
        {
            float currentTime = (float)EditorApplication.timeSinceStartup;
            if (currentTime - lastProgressUpdateTime >= 0.02f)
            {
                if (generationProgress < targetProgress)
                {
                    generationProgress = Mathf.Min(generationProgress + progressSpeed, targetProgress);
                    lastProgressUpdateTime = currentTime;
                    Repaint();
                }
            }
        }

        #endregion

        #region Settings
        private void DrawSettingsFields()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            InsertApiKeyRow("Gemini API Key", "GeminiApiKey", "https://aistudio.google.com/app/apikey", ref geminiApiKey);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            InsertApiKeyRow("OpenAI API Key", "OpenaiApiKey", "https://platform.openai.com/api-keys", ref openaiApiKey);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            InsertApiKeyRow("Groq API Key", "GroqApiKey", "https://console.groq.com/keys", ref groqApiKey);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            InsertApiKeyRow("Antrophic API Key", "AntrophicApiKey", "https://console.anthropic.com/settings/keys", ref antrophicApiKey);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ignored Folders:", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Ignored Folder"))
            {
                _ignoredFolders.Add("");
            }
            for (int i = 0; i < _ignoredFolders.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _ignoredFolders[i] = EditorGUILayout.TextField("Folder Path (Ex.: Assets\\Test)", _ignoredFolders[i], GUILayout.ExpandWidth(true));
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    _ignoredFolders.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gemini Project Name", GUILayout.Width(150));
            GeminiProjectName = EditorGUILayout.TextField(GeminiProjectName);
            PlayerPrefs.SetString("GeminiProjectName", GeminiProjectName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            IncludedFoldersManager.DrawIncludedFoldersUI();
            EditorGUILayout.EndVertical();
            PlayerPrefs.Save();
        }
        #endregion

        private void InsertApiKeyRow(string fieldName, string playerPrefsKey, string url, ref string apiKeyVariable)
        {
            EditorGUILayout.LabelField(fieldName, GUILayout.Width(150));
            apiKeyVariable = EditorGUILayout.PasswordField(apiKeyVariable);
            PlayerPrefs.SetString(playerPrefsKey, apiKeyVariable);
            if (GUILayout.Button("Get Key", GUILayout.Width(70)))
            {
                Application.OpenURL(url);
            }
        }

        private void RefreshClassList()
        {
            Debug.Log($"{PLUGIN_NAME}Refreshing class list...");
            classToPath.Clear();
            Debug.Log($"{PLUGIN_NAME}Current ignored folders count: {_ignoredFolders.Count}");
            foreach (var ignoredFolder in _ignoredFolders)
            {
                Debug.Log($"{PLUGIN_NAME}Ignored folder path: {ignoredFolder}");
            }

            string[] csFiles = Directory.GetFiles("Assets", "*.cs", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles("Packages", "*.cs", SearchOption.AllDirectories))
                .Where(filePath => !_ignoredFolders.Any(ignoredFolder => filePath.Contains(ignoredFolder)))
                .ToArray();
            Debug.Log($"{PLUGIN_NAME}Found {csFiles.Length} .cs files.");
            foreach (string filePath in csFiles)
            {
                string className = Path.GetFileNameWithoutExtension(filePath);
                classToPath[className] = filePath;
            }
            Debug.Log($"{PLUGIN_NAME}Class to path dictionary populated with {classToPath.Count} entries.");
        }

        private void SavePromptToFile()
        {
            string directoryPath = Path.Combine(Application.dataPath, PROMPTS_SAVE_FOLDER);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileName = $"prompt_{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.txt";
            string filePath = Path.Combine(directoryPath, fileName);
            File.WriteAllText(filePath, generatedPrompt);
            Debug.Log($"{PLUGIN_NAME}Prompt of Length = {generatedPrompt.Length} chars   saved to {filePath}");
        }

        private void CopyPromptToClipboard()
        {
            TextEditor te = new TextEditor();
            te.text = generatedPrompt;
            te.SelectAll();
            te.Copy();
        }

        private void StartButtonAnimation()
        {
            isButtonAnimating = true;
            buttonAnimationProgress = 0f;
            buttonColor = Color.yellow;
            EditorApplication.update += UpdateButtonAnimation;
        }

        private void UpdateButtonAnimation()
        {
            buttonAnimationProgress += Time.deltaTime / buttonAnimationTime;
            buttonColor = Color.Lerp(Color.red, Color.green, Mathf.PingPong(buttonAnimationProgress * 2, 1));
            if (buttonAnimationProgress >= 1f)
            {
                isButtonAnimating = false;
                EditorApplication.update -= UpdateButtonAnimation;
            }
        }

        private void OnEnable()
        {
            selectedClassNames = new List<string>(PlayerPrefs.GetString("SelectedClassNames", "").Split(',').Where(s => !string.IsNullOrEmpty(s)));
            geminiApiKey = PlayerPrefs.GetString("GeminiApiKey", "");
            openaiApiKey = PlayerPrefs.GetString("OpenaiApiKey", "");
            groqApiKey = PlayerPrefs.GetString("GroqApiKey", "");
            antrophicApiKey = PlayerPrefs.GetString("AntrophicApiKey", "");
            GeminiProjectName = PlayerPrefs.GetString(PREFS_GEMINI_PROJECT_NAME, "");
            _ignoredFolders = PlayerPrefs.GetString("IgnoredFolders", "").Split(',').Where(s => !string.IsNullOrEmpty(s)).ToList();
            includedFolders = JsonUtility.FromJson<List<IncludedFolder>>(PlayerPrefs.GetString(INCLUDED_FOLDERS_PREFS_KEY, "[]"));
            taskInput = PlayerPrefs.GetString(PREFS_KEY_TASK, "");
            isGeneratingCode = false;
            
            IncludedFoldersManager = new IncludedFoldersManager();
            RefreshClassList();
            CheckAndHandleFirstLaunch();
            bookmarkManager = new CodeGeneratorBookmarks();
            bookmarkManager.OnBookmarkLoaded += LoadBookmarkData;
            bookmarkManager.LoadBookmarksFromPrefs();
            var bookmarks = bookmarkManager.GetBookmarks();
            bookmarkManager.InitializeReorderableList();
        }

        private void OnDisable()
        {
            PlayerPrefs.SetString("SelectedClassNames", string.Join(",", selectedClassNames));
            PlayerPrefs.SetString("GeminiApiKey", geminiApiKey);
            PlayerPrefs.SetString("OpenaiApiKey", openaiApiKey);
            PlayerPrefs.SetString("GroqApiKey", groqApiKey);
            PlayerPrefs.SetString("AntrophicApiKey", antrophicApiKey);
            PlayerPrefs.SetString("GeminiProjectName", GeminiProjectName);
            PlayerPrefs.SetString("IgnoredFolders", string.Join(",", _ignoredFolders));
            PlayerPrefs.SetString(INCLUDED_FOLDERS_PREFS_KEY, JsonUtility.ToJson(includedFolders));
            bookmarkManager.SaveBookmarksToPrefs();
            PlayerPrefs.Save();
            bookmarkManager.OnBookmarkLoaded -= LoadBookmarkData;
        }
        
        private void LoadBookmarkData(CodeGeneratorBookmarks.Bookmark bookmark)
        {
            var bookmarks = bookmarkManager.LoadBookmarksFromPrefs();
            foreach (var bkm in bookmarks)
            {
                if (bkm.Name == bookmark.Name)
                {
                    selectedClassNames = bkm.SelectedClassNames;
                    taskInput = bkm.Task;
                    break;
                }
                
            }
        }

        private void CheckAndHandleFirstLaunch()
        {
            if (!IsFirstLaunch())
            {
                return;
            }

            Debug.Log($"{PLUGIN_NAME}First launch detected. Adding prompt save folder to .gitignore and ignore.conf files.");
            string assetsParentPath = Directory.GetParent(Application.dataPath).FullName;
            string gitignorePath = Path.Combine(assetsParentPath, ".gitignore");
            string ignoreConfPath = Path.Combine(assetsParentPath, "ignore.conf");
            string folderPathToAdd = Environment.NewLine + "/Assets/" + PROMPTS_SAVE_FOLDER + Environment.NewLine;
            string resultsPathToAdd = Environment.NewLine + "/Assets/" + AbstractAgentHandler.RESULTS_SAVE_FOLDER + Environment.NewLine;
            if (File.Exists(gitignorePath))
            {
                File.AppendAllText(gitignorePath, folderPathToAdd);
                File.AppendAllText(gitignorePath, resultsPathToAdd);
            }
            if (File.Exists(ignoreConfPath))
            {
                File.AppendAllText(ignoreConfPath, folderPathToAdd);
                File.AppendAllText(ignoreConfPath, resultsPathToAdd);
            }
        }

        private bool IsFirstLaunch()
        {
            const string key = "CODE_GENERATOR_FIRST_LAUNCH";
            bool isFirstLaunch = PlayerPrefs.GetInt(key, 1) == 1;
            if (isFirstLaunch)
            {
                PlayerPrefs.SetInt(key, 0);
                PlayerPrefs.Save();
            }
            return isFirstLaunch;
        }
    }
    
}