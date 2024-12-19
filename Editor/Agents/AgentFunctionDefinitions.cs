using System.Collections.Generic;
using Sanat.ApiAnthropic;
using Sanat.ApiGemini;
using Sanat.ApiOpenAI;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentFunctionDefinitions
    {
        #region SplitTaskToSingleFiles
        public const string TOOL_NAME_SPLIT_TASK_TO_SINGLE_FILES = "SplitTaskToSingleFiles";
        private const string PROPERTY_SplitTaskToSingleFiles_TASK_FILEPATH = "FilePath";
        private const string PROPERTY_SplitTaskToSingleFiles_TASK_ID = "TaskId";
        private const string FUNCTION_SPLIT_TASK_TO_SINGLE_FILES_DESCRIPTION = "Splits the task into multiple files. It should be used when you want to split the task into multiple files.";
        private const string PROPERTY_SplitTaskToSingleFiles_TASK_FILEPATH_DESCRIPTION = "Filepath of the task file, e.g. Assets\\Path\\To\\TaskFile.json";
        private const string PROPERTY_SplitTaskToSingleFiles_TASK_DEFINITION_DESCRIPTION = "Should be integer number 0 for Modify or integer number 1 for Create.";

        public ApiGemini.FunctionDeclaration GetFunctionData_GeminiSplitTaskToSingleFiles()
        {
            ApiGemini.FunctionDeclarationSchema parameters = new ApiGemini.FunctionDeclarationSchema
            {
                type = ApiGemini.FunctionDeclarationSchemaType.OBJECT,
                properties = new Dictionary<string, ApiGemini.FunctionDeclarationSchemaProperty>
                {
                    { PROPERTY_SplitTaskToSingleFiles_TASK_FILEPATH, new ApiGemini.FunctionDeclarationSchemaProperty{ type = ApiGemini.FunctionDeclarationSchemaType.STRING, description = PROPERTY_SplitTaskToSingleFiles_TASK_FILEPATH_DESCRIPTION} },
                    { PROPERTY_SplitTaskToSingleFiles_TASK_ID, new ApiGemini.FunctionDeclarationSchemaProperty{ type = ApiGemini.FunctionDeclarationSchemaType.INTEGER, description = PROPERTY_SplitTaskToSingleFiles_TASK_DEFINITION_DESCRIPTION} }
                },
                required = new List<string> { PROPERTY_SplitTaskToSingleFiles_TASK_FILEPATH, PROPERTY_SplitTaskToSingleFiles_TASK_ID }
                
            };
            return new ApiGemini.FunctionDeclaration
            {
                name = TOOL_NAME_SPLIT_TASK_TO_SINGLE_FILES,
                description = FUNCTION_SPLIT_TASK_TO_SINGLE_FILES_DESCRIPTION,
                parameters = parameters
            };
        }
        
        public ApiOpenAI.ToolFunction GetFunctionData_OpenaiSplitTaskToSingleFiles()
        {
            ApiOpenAI.Parameter parameters = new ApiOpenAI.Parameter();
            parameters.AddProperty(PROPERTY_SplitTaskToSingleFiles_TASK_FILEPATH, ApiOpenAI.DataTypes.STRING, PROPERTY_SplitTaskToSingleFiles_TASK_FILEPATH_DESCRIPTION);
            parameters.AddProperty(PROPERTY_SplitTaskToSingleFiles_TASK_ID, ApiOpenAI.DataTypes.NUMBER, PROPERTY_SplitTaskToSingleFiles_TASK_DEFINITION_DESCRIPTION);
            parameters.Required.Add(PROPERTY_SplitTaskToSingleFiles_TASK_FILEPATH);
            parameters.Required.Add(PROPERTY_SplitTaskToSingleFiles_TASK_ID);

            ApiOpenAI.ToolFunction function = new ApiOpenAI.ToolFunction(TOOL_NAME_SPLIT_TASK_TO_SINGLE_FILES, FUNCTION_SPLIT_TASK_TO_SINGLE_FILES_DESCRIPTION, parameters);
            return function;
        }

        public ApiAnthropic.ApiAntrophicData.ToolFunction GetFunctionData_AntrophicSplitTaskToSingleFiles()
        {
            ApiAnthropic.ApiAntrophicData.InputSchema parameters = new ApiAnthropic.ApiAntrophicData.InputSchema();
            parameters.AddProperty(PROPERTY_SplitTaskToSingleFiles_TASK_FILEPATH, ApiAnthropic.ApiAntrophicData.DataTypes.STRING, PROPERTY_SplitTaskToSingleFiles_TASK_FILEPATH_DESCRIPTION);
            parameters.AddProperty(PROPERTY_SplitTaskToSingleFiles_TASK_ID, ApiAnthropic.ApiAntrophicData.DataTypes.NUMBER, PROPERTY_SplitTaskToSingleFiles_TASK_DEFINITION_DESCRIPTION);
            parameters.Required.Add(PROPERTY_SplitTaskToSingleFiles_TASK_FILEPATH);
            parameters.Required.Add(PROPERTY_SplitTaskToSingleFiles_TASK_ID);

            ApiAnthropic.ApiAntrophicData.ToolFunction function = new ApiAnthropic.ApiAntrophicData.ToolFunction(TOOL_NAME_SPLIT_TASK_TO_SINGLE_FILES, FUNCTION_SPLIT_TASK_TO_SINGLE_FILES_DESCRIPTION, parameters);
            return function;
        }
        
        #endregion

        #region Tool_ReplaceScriptFile
        public const string TOOL_NAME_REPLACE_SCRIPT_FILE = "ReplaceScriptFile";
        private const string PROPERTY_ReplaceScriptFile_FILEPATH = "FilePath";
        private const string PROPERTY_ReplaceScriptFile_CONTENT = "Content";
        private const string FUNCTION_REPLACE_SCRIPT_FILE_DESCRIPTION = "Fully replaces script file code content with new code content. It should be used when you want to replace the content of a script file with new content.";
        private const string FUNCTION_PROPERTY_ReplaceScriptFile_FILEPATH_DESCRIPTION = "Filepath of the code snippet, e.g. Assets\\Path\\To\\File.cs";
        private const string FUNCTION_PROPERTY_ReplaceScriptFile_CONTENT_DESCRIPTION = "FULL code for selected filepath, partial code snippets are NOT ALLOWED.";
        
        public ApiOpenAI.ToolFunction GetFunctionData_OpenaiSReplaceScriptFile()
        {
            ApiOpenAI.Parameter parameters = new ApiOpenAI.Parameter();
            parameters.AddProperty(PROPERTY_ReplaceScriptFile_FILEPATH, ApiOpenAI.DataTypes.STRING, FUNCTION_PROPERTY_ReplaceScriptFile_FILEPATH_DESCRIPTION);
            parameters.AddProperty(PROPERTY_ReplaceScriptFile_CONTENT, ApiOpenAI.DataTypes.STRING, FUNCTION_PROPERTY_ReplaceScriptFile_CONTENT_DESCRIPTION);
            parameters.Required.Add(PROPERTY_ReplaceScriptFile_FILEPATH);
            parameters.Required.Add(PROPERTY_ReplaceScriptFile_CONTENT);
            ApiOpenAI.ToolFunction function = new ApiOpenAI.ToolFunction(TOOL_NAME_REPLACE_SCRIPT_FILE, FUNCTION_REPLACE_SCRIPT_FILE_DESCRIPTION, parameters);
            return function;
        }

        public ApiGemini.FunctionDeclaration GetFunctionData_GeminiReplaceScriptFile()
        {
            ApiGemini.FunctionDeclarationSchema parameters = new ApiGemini.FunctionDeclarationSchema
            {
                type = ApiGemini.FunctionDeclarationSchemaType.OBJECT,
                properties = new Dictionary<string, ApiGemini.FunctionDeclarationSchemaProperty>
                 {
                    { PROPERTY_ReplaceScriptFile_FILEPATH, new ApiGemini.FunctionDeclarationSchemaProperty{ type = ApiGemini.FunctionDeclarationSchemaType.STRING, description = FUNCTION_PROPERTY_ReplaceScriptFile_FILEPATH_DESCRIPTION} },
                    { PROPERTY_ReplaceScriptFile_CONTENT, new ApiGemini.FunctionDeclarationSchemaProperty{ type = ApiGemini.FunctionDeclarationSchemaType.STRING, description = FUNCTION_PROPERTY_ReplaceScriptFile_CONTENT_DESCRIPTION} },
                },
                required = new List<string> { PROPERTY_ReplaceScriptFile_FILEPATH, PROPERTY_ReplaceScriptFile_CONTENT }
            };

            return new ApiGemini.FunctionDeclaration
            {
                name = TOOL_NAME_REPLACE_SCRIPT_FILE,
                description = FUNCTION_REPLACE_SCRIPT_FILE_DESCRIPTION,
                parameters = parameters
            };
        }

        public ApiAnthropic.ApiAntrophicData.ToolFunction GetFunctionData_AntrophicReplaceScriptFile()
        {
            ApiAnthropic.ApiAntrophicData.InputSchema parameters = new ApiAnthropic.ApiAntrophicData.InputSchema();
            parameters.AddProperty(PROPERTY_ReplaceScriptFile_FILEPATH, ApiAnthropic.ApiAntrophicData.DataTypes.STRING, FUNCTION_PROPERTY_ReplaceScriptFile_FILEPATH_DESCRIPTION);
            parameters.AddProperty(PROPERTY_ReplaceScriptFile_CONTENT, ApiAnthropic.ApiAntrophicData.DataTypes.STRING, FUNCTION_PROPERTY_ReplaceScriptFile_CONTENT_DESCRIPTION);
            parameters.Required.Add(PROPERTY_ReplaceScriptFile_FILEPATH);
            parameters.Required.Add(PROPERTY_ReplaceScriptFile_CONTENT);
            ApiAnthropic.ApiAntrophicData.ToolFunction function = new ApiAnthropic.ApiAntrophicData.ToolFunction(TOOL_NAME_REPLACE_SCRIPT_FILE, FUNCTION_REPLACE_SCRIPT_FILE_DESCRIPTION, parameters);
            return function;
        }
        #endregion
    }
}