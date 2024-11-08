// Assets/Sanat/CodeGenerator/Editor/CodeGeneratorPreparationHelper.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sanat.CodeGenerator;
using UnityEngine;

public class CodeGeneratorPreparationHelper
{
    public Dictionary<string, string> PrepareProjectCode(List<string> selectedClassNames, Dictionary<string, string> classToPath, List<string> ignoredFolders)
    {
        Dictionary<string, string> projectCode = new Dictionary<string, string>();

        foreach (string className in selectedClassNames)
        {
            if (classToPath.TryGetValue(className, out string filePath))
            {
                // Check if file path is not in ignored folders
                if (!ignoredFolders.Any(ignoredFolder => filePath.Contains(ignoredFolder)))
                {
                    try
                    {
                        string fileContent = File.ReadAllText(filePath);
                        projectCode[filePath] = fileContent;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error reading file {filePath}: {ex.Message}");
                    }
                }
            }
        }
        
        var additionalFiles = GetAdditionalIncludedFiles(CodeGenerator.IncludedFoldersManager.GetEnabledFolders());
        
        projectCode = MergeProjectCodeWithAdditionalFiles(projectCode, additionalFiles);

        return projectCode;
    }

    public string GenerateIncludedCode(Dictionary<string, string> projectCode, string includedCodeRaw = "")
    {
        string includedCode = includedCodeRaw;

        foreach (var kvPathToCode in projectCode)
        {
            // Add file path and content to the included code
            includedCode += $"// {kvPathToCode.Key}:\n{kvPathToCode.Value}\n\n";
        }

        // Remove newline characters and trim
        includedCode = includedCode.Replace("\n", "").Replace("\r", "").Trim();

        return includedCode;
    }

    public List<string> GetAdditionalIncludedFiles(List<string> includedFolderPaths)
    {
        List<string> additionalFiles = new List<string>();

        foreach (string folderPath in includedFolderPaths)
        {
            try
            {
                string[] csFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);
                additionalFiles.AddRange(csFiles);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting files from folder {folderPath}: {ex.Message}");
            }
        }

        return additionalFiles;
    }

    public Dictionary<string, string> MergeProjectCodeWithAdditionalFiles(
        Dictionary<string, string> projectCode, 
        List<string> additionalFiles)
    {
        Dictionary<string, string> mergedProjectCode = new Dictionary<string, string>(projectCode);

        foreach (string filePath in additionalFiles)
        {
            if (!mergedProjectCode.ContainsKey(filePath))
            {
                try
                {
                    string fileContent = File.ReadAllText(filePath);
                    mergedProjectCode[filePath] = fileContent;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error reading additional file {filePath}: {ex.Message}");
                }
            }
        }

        return mergedProjectCode;
    }
}