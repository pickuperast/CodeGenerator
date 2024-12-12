// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sanat.ApiGemini;
using Sanat.ApiGroq;
using Sanat.ApiOpenAI;
using Sanat.CodeGenerator;
using Sanat.CodeGenerator.Editor;

namespace Sanat.CodeGenerator.Agents
{
	public class AgentCodeMerger : AbstractAgentHandler
	{
		protected List<FileContent> _projectCode = new ();
		protected const string PROMPT_FILE_PATH_EXTRACT = "PromptAgentCodeMergerFilePathExtract.md";
		protected const string PROMPT_MERGE_CODE = "PromptAgentCodeMergerMergeCode.md";
		string[] _projectCodePaths;
		
		protected override string PromptFilename() => "AgentCodeMerger.md";
		
		protected override Model GetModel() => Model.GPT4omini;
        
		protected override string GetGeminiModel() => ApiGeminiModels.Flash;

		public AgentCodeMerger() { }

		public AgentCodeMerger(ApiKeys apiKeys, List<FileContent> includedCodeFiles, string[] projectCodePaths)
		{
			Name = "Agent Code Merger";
			Description = "Merges code snippets";
			Temperature = .0f;
			string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PromptFilename()}";
			Instructions = LoadPrompt(promptLocation);
			StoreKeys(apiKeys);
			_projectCode = includedCodeFiles;
			_projectCodePaths = projectCodePaths;
			SelectedApiProvider = ApiProviders.Gemini;
		}
		
		public void InsertCode(string solutionInput)
		{
			GetFilePath(solutionInput, (result) =>
			{
				FileContent newQuestData = JsonUtility.FromJson<FileContent>(result);
				string[] filePathes = newQuestData.FilePath.Split(CSV_SEPARATOR);
				string[] fileContents = newQuestData.Content.Split(CSV_SEPARATOR);
				for (int i = 0; i < filePathes.Length; i++)
				{
					Debug.Log($"<color=cyan>{Name}</color> GetFilePath Result: {filePathes[i]}");
				}
				
				var splitted = result.Split(";");
				//var filePathesCleared = LeaveOnlyCsFiles(splitted);
				if (splitted.Length == 1)
				{
					Debug.Log($"<color=cyan>{Name}</color> working with single path: {result}");
					DirectInsertion(splitted[0], solutionInput);
				}else
				{
					for (int i = 0; i < splitted.Length; i++)
					{
						ExtractCodeContents(solutionInput, splitted[i]);
					}
				}
			});
		}

		protected void ExtractCodeContents(string solutionInput, string filePath)
		{
			Debug.Log($"<color=cyan>{Name}</color> ExtractCodeContents for path: {filePath}");
			AgentExtractCodeByFilepath agentExtractCodeByFilepath = new AgentExtractCodeByFilepath(Apikeys, filePath);
			agentExtractCodeByFilepath.SplitToFilePathContent(solutionInput, (result) =>
			{
				Debug.Log($"<color=cyan>{Name}</color> ExtractCodeContents Result: {result}");
				Debug.Log($"<color=cyan>{Name}</color> working with single path: {result}");
				DirectInsertion(filePath, solutionInput);
			});
		}

		protected void GetFilePath(string solutionInput, Action<string> callback)
		{
			string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PROMPT_FILE_PATH_EXTRACT}";
			string agentLogName = $"<color=cyan>{Name}</color>";
			string question = solutionInput;
			_modelName = Model.GPT4omini.Name;//ApiGroqModels.Llama3_70b_8192_tool.Name;
			BotParameters botParameters = new BotParameters(question, ApiProviders.OpenAI, .2f, callback, _modelName, true);
			var openaiTools = new ApiOpenAI.Tool[] { new ("function", GetFunctionData_OpenaiSplitCodeToFilePathes()) };
			botParameters.isToolUse = true;
			botParameters.openaiTools = openaiTools;
			botParameters.systemMessage = LoadPrompt(promptLocation);
			botParameters.onOpenaiChatResponseComplete += (response) =>
			{
				Debug.Log($"{agentLogName} GetFilePath Result: {response}");
				if (response.choices[0].finish_reason == "tool_calls")
				{
					ToolCalls[] toolCalls = response.choices[0].message.tool_calls;
					int filesAmount = toolCalls.Length;
					FileContent[] fileContents = new FileContent[filesAmount];
					string logFileNames = $"<color=cyan>{Name}</color>: ";
					for (int i = 0; i < filesAmount; i++)
					{
						fileContents[i] = JsonConvert.DeserializeObject<FileContent>(toolCalls[i].function.arguments);
						logFileNames += $"{fileContents[i].FilePath}, ";
						if (i == filesAmount - 1)
						{
							logFileNames = logFileNames.Remove(logFileNames.Length - 2);
						}
					}
					
					Debug.Log(logFileNames);
					for (int i = 0; i < filesAmount; i++)
					{
						Debug.Log($"{agentLogName} Writing file: {fileContents[i].FilePath}");
						DirectInsertion(fileContents[i].FilePath, fileContents[i].Content);
					}
				}
			};
			AskBot(botParameters);
		}

		public override void Handle(string input)
		{
			string clearedJson = ClearResult(input);
			Debug.Log($"<color=cyan>{Name}</color> working with this solution({clearedJson.Length} chars): {clearedJson}");
			List<FileContent> codeSnippets;
			try
			{
				codeSnippets = ExtractFileContents(clearedJson);
				foreach (FileContent snippet in codeSnippets)
				{
					string filePath = snippet.FilePath;
					string code = snippet.Content;
				
					Debug.Log("FilePath: " + filePath);
					Debug.Log("Content: " + code);
					if (code == String.Empty)
						continue;
				
					code = TranslateFromValidJson(code);
					DirectInsertion(filePath, code);
				}
			}catch(Exception ex)
			{
				Debug.Log($"<color=cyan>{Name}</color> <color=red>ERROR!</color> Could not parse the solution. Trying different approach. Exception: {ex.Message}");
				codeSnippets = ParseJson(clearedJson);
				foreach (FileContent snippet in codeSnippets)
				{
					string filePath = snippet.FilePath;
					string code = snippet.Content;
				
					Debug.Log("FilePath: " + filePath);
					Debug.Log($"Content({code.Length} chars): " + code);
					if (code == String.Empty)
						continue;
				
					string request = "Pretty print the code for direct insertion to file. Dont say anything, just provide the code. DONT SKIP ANY CODE OR KITTENS WILL DIE PLEASE PLEASE PLEASE. # CODE:";
					string prompt = $"{request}{code}";
					BotParameters botParameters = new BotParameters(prompt, SelectedApiProvider, Temperature, delegate(string result)
					{
						Debug.Log($"<color=purple>{Name}</color> result({result} chars): {result}");
					
						var match = Regex.Match(result, @"```\s*([\s\S]+)```");
						string formattedCode = match.Groups[1].Value;
						DirectInsertion(filePath, formattedCode);
					});
					AskBot(botParameters);
				}
			}

			if (_nextHandler != null)
			{
				_nextHandler.Handle(input);
			}
		}

		protected List<FileContent> ExtractFileContents(string clearedJson)
		{
			List<FileContent> codeSnippets;
			string convertedJson = TranslateToValidJson(clearedJson);
			var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml };
			codeSnippets = JsonConvert.DeserializeObject<List<FileContent>>(convertedJson, settings);
			return codeSnippets;
		}

		protected string TranslateToValidJson(string clearedJson)
		{
			string convertedJson = clearedJson.Replace("({", KEY_FIGURE_OPEN);
			convertedJson = convertedJson.Replace("(}", KEY_FIGURE_CLOSE);
			return convertedJson;
		}
		
		protected string TranslateFromValidJson(string clearedJson)
		{
			string convertedJson = clearedJson.Replace(KEY_FIGURE_OPEN, "({");
			convertedJson = convertedJson.Replace(KEY_FIGURE_CLOSE, "(}");
			return convertedJson;
		}

		public List<FileContent> ParseJson(string json)
		{
			string filePathPattern = @"""FilePath"":\s*""([^""]+)""";
			string contentPattern = @"""Content"":\s*""((?:[^""\\]|\\.)*)""";

			var filePathMatches = Regex.Matches(json, filePathPattern);
			var contentMatches = Regex.Matches(json, contentPattern);

			List<FileContent> filePathList = new List<FileContent>();
			
			for (int i = 0; i < filePathMatches.Count; i++)
			{
				string filePath = filePathMatches[i].Groups[1].Value;
				string content = contentMatches[i].Groups[1].Value;
				filePathList.Add(new FileContent { FilePath = filePath, Content = content });
			}

			return filePathList;
		}

		private bool CheckIfFileExists(string className)
		{
			for (int i = 0; i < _projectCodePaths.Length; i++)
			{
				if (_projectCodePaths[i].Contains(className))
				{
					return true;
				}
			}
			return false;
		}

		public async void MergeFiles(List<FileContent> fileContents)
		{
			var instructions = LoadPrompt(Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PROMPT_MERGE_CODE}");
			await Task.Delay(100);
			foreach(var fileContent in fileContents)
			{
				string className = fileContent.FilePath.Split("/").Last();
				bool fileExists = CheckIfFileExists(className);
				if (fileExists)
				{
					Debug.Log($"<color=cyan>{Name}</color> Merging code for: {fileContent.FilePath}");
					await MergeCodeWithLLM(fileContent);
				}
				else
				{
					AgentCodeMerger agentCodeMerger = new AgentCodeMerger();
					agentCodeMerger.DirectInsertion(fileContent.FilePath, fileContent.Content);
				}
			}
		}

		async Task MergeCodeWithLLM(FileContent fileContentToMerge)
		{
			string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PROMPT_MERGE_CODE}";
			string agentLogName = $"<color=cyan>{Name}</color>";
			string oldCode = _projectCode.FirstOrDefault(x => x.FilePath == fileContentToMerge.FilePath).Content;
			string question = $"Path: {fileContentToMerge.FilePath}. Here is the Old Code:\n" + oldCode + "\n\nHere is the New Code:\n" + fileContentToMerge.Content;
			Debug.Log($"{agentLogName} MergeCodeWithLLM asking: {question}");
			
			_modelName = Model.GPT4omini.Name;//ApiGroqModels.Llama3_70b_8192_tool.Name;
			BotParameters botParameters = new BotParameters(question, ApiProviders.OpenAI, .2f, null, _modelName, true);
			var openaiTools = new ApiOpenAI.Tool[] { new ("function", GetFunctionData_OpenaiSplitCodeToFilePathes()) };
			botParameters.isToolUse = true;
			botParameters.openaiTools = openaiTools;
			botParameters.systemMessage = LoadPrompt(promptLocation);
			botParameters.onOpenaiChatResponseComplete += (response) =>
			{
				Debug.Log($"{agentLogName} GetFilePath Result: {response}");
				SaveResultToFile(JsonConvert.SerializeObject(response));
				if (response.choices[0].finish_reason == "tool_calls")
				{
					ToolCalls[] toolCalls = response.choices[0].message.tool_calls;
					int filesAmount = toolCalls.Length;
					FileContent[] fileContents = new FileContent[filesAmount];
					string logFileNames = $"<color=cyan>{Name}</color>: ";
					for (int i = 0; i < filesAmount; i++)
					{
						fileContents[i] = JsonConvert.DeserializeObject<FileContent>(toolCalls[i].function.arguments);
						logFileNames += $"{fileContents[i].FilePath}, ";
						if (i == filesAmount - 1)
						{
							logFileNames = logFileNames.Remove(logFileNames.Length - 2);
						}
					}
					
					Debug.Log(logFileNames);
					for (int i = 0; i < filesAmount; i++)
					{
						Debug.Log($"{agentLogName} Writing file: {fileContents[i].FilePath}");
						DirectInsertion(fileContents[i].FilePath, fileContents[i].Content);
					}
				}
			};
			AskBot(botParameters);
		}
		
		public void DirectInsertion(string filePath, string code)
		{
			SaveResultToFile(code);
			filePath = filePath.Replace(":", string.Empty);
			CompareAndFixFilePath(ref filePath);
			if (File.Exists(filePath))
			{
				string originalContent = File.ReadAllText(filePath);
				BackupManager.BackupScriptFile(filePath, originalContent);
			}
            
			string directoryPath = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}
			if (!File.Exists(filePath))
			{
				File.Create(filePath).Dispose();
			}

			File.SetAttributes(filePath, FileAttributes.Normal);
			File.WriteAllText(filePath, code, Encoding.UTF8);
			OnComplete?.Invoke(code);
			CodeGeneratorFileOpener.OpenScript(filePath);
			Debug.Log($"<color=cyan>{Name}</color> <color=green>COMPLETED!</color> [{filePath}] Direct insertion:\n{code}");
		}

		private void CompareAndFixFilePath(ref string filePath)
		{
			var filename = Regex.Match(filePath, @"([^\\\/]+\.cs)");
			// try to find that filename in the project code
			if (filename.Success)
			{
				string filenameToFind = filename.Groups[1].Value;
				foreach (var projectFilePath in _projectCode)
				{
					if (projectFilePath.FilePath.Contains(filenameToFind))
					{
						filePath = projectFilePath.FilePath;
						break;
					}
				}
			}
		}

		protected ToolFunction GetFunctionData_OpenaiSplitCodeToFilePathes()
		{
			string description = "Inserts code into the file.";
			string name = "InsertCodeToPath";
			string propertyFilePathes = "FilePath";
			string propertyFileContents = "Content";

			Parameter parameters = new ();
			parameters.AddProperty(propertyFilePathes, DataTypes.STRING, $"AI must tell filepath of the code snippet");
			parameters.AddProperty(propertyFileContents, DataTypes.STRING, $"AI must provide FULL code snippet for selected filepath");
			parameters.Required.Add(propertyFilePathes);
			parameters.Required.Add(propertyFileContents);
			ToolFunction function = new ToolFunction(name, description, parameters);
			return function;
		}
	}
}