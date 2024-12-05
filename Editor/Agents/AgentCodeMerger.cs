// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sanat.ApiGemini;
using Sanat.ApiGroq;
using Sanat.ApiOpenAI;
using Sanat.CodeGenerator;
using Sanat.CodeGenerator.Editor;

namespace Sanat.CodeGenerator.Agents
{
	public class AgentCodeMerger : AbstractAgentHandler
	{
		protected Dictionary<string, string> _projectCode = new ();
		protected const string PROMPT_FILE_PATH_EXTRACT = "PromptAgentCodeMergerFilePathExtract.md";
		
		protected override string PromptFilename() => "AgentCodeMerger.md";
		
		protected override Model GetModel() => Model.GPT4omini;
        
		protected override string GetGeminiModel() => ApiGeminiModels.Flash;

		public AgentCodeMerger(ApiKeys apiKeys, Dictionary<string, string> projectCode)
		{
			Name = "Agent Code Merger";
			Description = "Merges code snippets";
			Temperature = .0f;
			string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PromptFilename()}";
			Instructions = LoadPrompt(promptLocation);
			StoreKeys(apiKeys);
			_projectCode = projectCode;
			SelectedApiProvider = ApiProviders.Gemini;
		}

		public AgentCodeMerger(ApiKeys apiKeys, string singelFileSolution)
		{
			Name = "Agent Code Merger";
			Description = "Merges code snippets";
			Temperature = .0f;
			string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PromptFilename()}";
			Instructions = LoadPrompt(promptLocation);
			StoreKeys(apiKeys);
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

		protected List<string> LeaveOnlyCsFiles(string[] splitted)
		{
			List<string> cleared = new List<string>();
			for (int i = 0; i < splitted.Length; i++)
			{
				if (splitted[i].EndsWith(".cs"))
				{
					cleared.Add(splitted[i]);
				}
			}

			return cleared;
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

		protected void ExtractCodeContentsOLD(string solutionInput, string[] splitted)
		{
			AgentSolutionToDict agentSolutionToDict = new AgentSolutionToDict(Apikeys, splitted);
			agentSolutionToDict.SplitToFilePathContent(solutionInput, (result) =>
			{
				Debug.Log($"<color=cyan>{Name}</color> ExtractCodeContents Result: {result}");
				var clearedJson = ClearResult(result);
				List<FileContent> fileContents = ExtractFileContents(clearedJson);
				foreach (FileContent snippet in fileContents)
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
			});
		}

		protected void GetFilePath(string solutionInput, Action<string> callback)
		{
			string promptLocation = Application.dataPath + $"{PROMPTS_FOLDER_PATH}{PROMPT_FILE_PATH_EXTRACT}";
			string agentLogName = $"<color=cyan>{Name}</color>";
			string question = solutionInput;
			_modelName = Model.GPT4omini.Name;//ApiGroqModels.Llama3_70b_8192_tool.Name;
			BotParameters botParameters = new BotParameters(question, ApiProviders.OpenAI, .2f, callback, _modelName, true);
			var openaiTools = new OpenAI.Tool[] { new ("function", GetFunctionData_OpenaiSplitCodeToFilePathes()) };
			botParameters.isToolUse = true;
			botParameters.openaiTools = openaiTools;
			botParameters.systemMessage = LoadPrompt(promptLocation);
			botParameters.onOpenaiChatResponseComplete += (response) =>
			{
				Debug.Log($"{agentLogName} GetFilePath Result: {response}");
				if (response.choices[0].finish_reason == "tool_calls")
				{
					OpenAI.ToolCalls[] toolCalls = response.choices[0].message.tool_calls;
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
		
		protected void DirectInsertion(string filePath, string code)
		{
			SaveResultToFile(code);
			filePath = filePath.Replace(":", string.Empty);
			CompareAndFixFilePath(ref filePath);
			if (File.Exists(filePath))
			{
				string originalContent = File.ReadAllText(filePath);
				BackupManager.BackupScriptFile(filePath, originalContent);
			}
			
			var match = Regex.Match(code, @"```csharp\s*([\s\S]*?)```");
			if (match.Success)
			{
				code = match.Groups[1].Value.Trim();
			}
			else
			{
				var lines = code.Split('\n');
				if (lines[lines.Length - 1] == "```")
				{
					var list = lines.ToList();
					list = list.GetRange(0, list.Count - 1);
                
					if (lines[1] == "```")
					{
						list = list.GetRange(1, list.Count - 1);
					}
                
					if (list[0].StartsWith("//"))
					{
						list = list.GetRange(1, list.Count - 1);
					}

					if (lines[0].StartsWith("```csharp"))
					{
						list = list.GetRange(1, list.Count - 1);
					}
					code = string.Join("\n", list);
				}
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
				foreach (var projectFilePath in _projectCode.Keys)
				{
					if (projectFilePath.EndsWith(filenameToFind))
					{
						filePath = projectFilePath;
						break;
					}
				}
			}
		}

		protected void MergeCode(string filePath, string solutionInput)
		{
			ParsedCodeData bakedCode = new ParsedCodeData();
			if (_projectCode.ContainsKey(filePath))
			{
				bakedCode = ToCustomJson(filePath, _projectCode[filePath]);
			}
			else
			{
				string slashesFixed = filePath.Replace("/", @"\");
				bakedCode = ToCustomJson(filePath, _projectCode[slashesFixed]);
			}
			string newInstructions = Instructions.Replace("@CODE@", JsonConvert.SerializeObject(bakedCode, Formatting.Indented));
			newInstructions = newInstructions.Replace("@DELTA@", solutionInput);
			Debug.Log($"<color=cyan>{Name}</color> asking: {newInstructions}");
			AskAntrophic(ApiAnthropic.Model.Haiku35Latest, newInstructions, .0f, (jsonResponse) =>
			{
				Debug.Log($"<color=cyan>{Name}</color> result: {jsonResponse}");
        
				string clearedJson = ClearResult(jsonResponse, Brackets.curly);
				ParsedCodeData parsedCodeData = JsonConvert.DeserializeObject<ParsedCodeData>(clearedJson);
        
				string mergedCode = MergeCodeParts(bakedCode, parsedCodeData);

				File.WriteAllText(filePath, mergedCode);
				Debug.Log($"<color=cyan>{Name}</color> <color=green>COMPLETED!</color> {filePath} Merged code:\n{mergedCode}");
				OnComplete?.Invoke(mergedCode);
			});
		}
		
		protected string MergeCodeParts(ParsedCodeData oldCode, ParsedCodeData newCode)
		{
			StringBuilder mergedCode = new StringBuilder();
			
			string preparedLibraries = MergeRows(oldCode.Libraries, newCode.Libraries);
			mergedCode.AppendLine(preparedLibraries);
			mergedCode.AppendLine();
			Debug.Log($"<color=cyan>{Name}</color> Merged libraries: \n{mergedCode}");
			
			bool isNamespaceExist = false;
			if (!string.IsNullOrEmpty(oldCode.Namespace))
			{
				mergedCode.AppendLine($"namespace {oldCode.Namespace}");
				mergedCode.AppendLine("{");
				isNamespaceExist = true;
			}
			Debug.Log($"<color=cyan>{Name}</color> Merged namespace: \n{mergedCode}");
			
			mergedCode.AppendLine(oldCode.ClassDeclaration);
			mergedCode.AppendLine("{");
			Debug.Log($"<color=cyan>{Name}</color> Merged class declaration: \n{mergedCode}");
			
			string preparedFields = MergeRows(oldCode.Fields, newCode.Fields);
			mergedCode.AppendLine(preparedFields);
			mergedCode.AppendLine();
			Debug.Log($"<color=cyan>{Name}</color> Merged fields: \n{mergedCode}");
			
			Dictionary<string, string> parsedMethods = newCode.Methods.ToDictionary(m => m.MethodName, m => m.MethodBody);

			UpdateOldCodeMethodsFromNewCode(oldCode, parsedMethods, mergedCode);
			Debug.Log($"<color=cyan>{Name}</color> Merged methods: \n{mergedCode}");
			
			AddOldMethodsNotInNewCode(oldCode, mergedCode);
			Debug.Log($"<color=cyan>{Name}</color> Merged old methods: \n{mergedCode}");
			
			mergedCode.AppendLine("}");
			if (isNamespaceExist)
			{
				mergedCode.AppendLine("}");
			}
			
			return mergedCode.ToString();
		}

		protected void AddOldMethodsNotInNewCode(ParsedCodeData oldCode, StringBuilder mergedCode)
		{
			foreach (var method in oldCode.Methods)
			{
				AddMethodToMergedCode(mergedCode, method.MethodBody);
				mergedCode.AppendLine();
			}
		}

		protected static void UpdateOldCodeMethodsFromNewCode(ParsedCodeData oldCode, Dictionary<string, string> parsedMethods, StringBuilder mergedCode)
		{
			List<MethodData> updatedMethods = new List<MethodData>(oldCode.Methods);
			foreach (var method in updatedMethods)
			{
				if (parsedMethods.TryGetValue(method.MethodName, out string updatedMethodBody))
				{
					// If the method exists in parsedCodeData, use the updated method body
					AddMethodToMergedCode(mergedCode, updatedMethodBody);
					var foundMethod = oldCode.Methods.Find(m => m.MethodName == method.MethodName);
					if (foundMethod != null)
					{
						oldCode.Methods.Remove(foundMethod);
					}
				}
				else
				{
					// If the method doesn't exist in parsedCodeData, use the original method body
					mergedCode.AppendLine(method.MethodBody);
				}

				mergedCode.AppendLine();
			}
		}

		protected OpenAI.ToolFunction GetFunctionData_OpenaiSplitCodeToFilePathes()
		{
			string description = "Inserts code into the file.";
			string name = "InsertCodeToPath";
			string propertyFilePathes = "FilePath";
			string propertyFileContents = "Content";

			OpenAI.Parameter parameters = new ();
			parameters.AddProperty(propertyFilePathes, OpenAI.DataTypes.STRING, $"AI must tell filepath of the code snippet");
			parameters.AddProperty(propertyFileContents, OpenAI.DataTypes.STRING, $"AI must provide FULL code snippet for selected filepath");
			parameters.Required.Add(propertyFilePathes);
			parameters.Required.Add(propertyFileContents);
			OpenAI.ToolFunction function = new OpenAI.ToolFunction(name, description, parameters);
			return function;
		}

		protected static void AddMethodToMergedCode(StringBuilder mergedCode, string updatedMethodBody)
		{
			mergedCode.AppendLine($"\r{updatedMethodBody}");
		}

		protected string MergeRows(string rowsA, string rowsB)
		{
			if (rowsB == null || rowsB.Length == 0)
			{
				return rowsA;
			}
			
			string[] bakedLibraries = rowsA.Split('\n');
			string[] newLibraries = rowsB.Split('\n');
			HashSet<string> mergedLibraries = new HashSet<string>(bakedLibraries);
			mergedLibraries.UnionWith(newLibraries);
			string mergedRows = string.Join("\n", mergedLibraries);
			return mergedRows;
		}

		protected ParsedCodeData ToCustomJson(string filePath, string codeRaw)
		{
			string libraries = ParseLibraries(codeRaw);
			string nameSpace = ParseNamespace(codeRaw);
			string classDeclaration = ParseClassName(codeRaw);
			string fields = ParseFields(codeRaw);
			List<MethodData> methods = ParseMethods(codeRaw);

			return new ParsedCodeData
			{
				FilePath = filePath,
				Libraries = libraries,
				Namespace = nameSpace,
				ClassDeclaration = classDeclaration,
				Fields = fields,
				Methods = methods
			};
		}

		protected string ParseNamespace(string codeRaw)
		{
			string pattern = @"namespace\s+([\w.]+)\s*\{";
			Match match = Regex.Match(codeRaw, pattern);

			if (match.Success)
			{
				return match.Groups[1].Value.Trim();
			}

			return string.Empty;
		}

		#region Parsers
		protected string ParseClassName(string codeRaw)
		{
			string pattern = @"(?:public|private|protected|internal)?\s*(sealed|abstract|static)?\s*class\s+(\w+)\s*(:\s*[\w,\s<>]+)?";
			Match match = Regex.Match(codeRaw, pattern);
    
			if (match.Success)
			{
				string modifiers = match.Groups[1].Value.Trim();
				string className = match.Groups[2].Value.Trim();
				string inheritance = match.Groups[3].Value.Trim();

				string fullClassDeclaration = $"public {modifiers} class {className} {inheritance}".Trim();
				return fullClassDeclaration.Replace("  ", " ");
			}
    
			return string.Empty;
		}

		protected List<MethodData> ParseMethods(string codeRaw)
		{
			List<MethodData> methods = new List<MethodData>();
			string pattern = @"(?:public|private|protected|internal|static)?\s+(?:void|[\w<>[\]]+)\s+(\w+)\s*\([^)]*\)\s*(?:where\s+[^{]+)?\s*\{(?:[^{}]|\{(?:[^{}]|\{(?:[^{}]|\{[^{}]*\})*\})*\})*\}";

			MatchCollection matches = Regex.Matches(codeRaw, pattern, RegexOptions.Singleline);

			foreach (Match match in matches)
			{
				string methodName = match.Groups[1].Value;
				string methodBody = match.Value.Trim();
				methods.Add(new MethodData { MethodName = methodName, MethodBody = methodBody });
			}

			return methods;
		}

		protected string ParseFields(string codeRaw)
		{
			List<string> fields = new List<string>();
			string pattern = @"(?:public|private|protected|internal|static)?\s+(?:readonly\s+)?(?!using|return|class|void|enum)[\w<>[\]]+\s+\w+(?:\s*=\s*(?![^;]*\()[^;]+)?;";
			
			MatchCollection matches = Regex.Matches(codeRaw, pattern);
    
			foreach (Match match in matches)
			{
				fields.Add(match.Value.Trim());
			}
    
			return string.Join("\n", fields);
		}

		protected string ParseLibraries(string codeRaw)
		{
			List<string> libraries = new List<string>();
			string pattern = @"using\s+[\w.]+;";
    
			MatchCollection matches = Regex.Matches(codeRaw, pattern);
    
			foreach (Match match in matches)
			{
				libraries.Add(match.Value.Trim());
			}
    
			return string.Join("\n", libraries);
		}
		#endregion
		
		[Serializable]
		public class ParsedCodeData
		{
			public string Namespace { get; set; }
			public string FilePath { get; set; }
			public string Libraries { get; set; }
			public string ClassDeclaration { get; set; }
			public string Fields { get; set; }
			public List<MethodData> Methods { get; set; }
		}
		
		[Serializable]
		public class MethodData
		{
			public string MethodName { get; set; }
			public string MethodBody { get; set; }
		}
		
		[Serializable]
		public class FileContent
		{
			public string FilePath { get; set; }
			public string Content { get; set; }
		}
	}
}