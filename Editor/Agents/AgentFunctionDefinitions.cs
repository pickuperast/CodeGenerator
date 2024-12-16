using System.Collections.Generic;
using Sanat.ApiAnthropic;
using Sanat.ApiGemini;
using Sanat.ApiOpenAI;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentFunctionDefinitions
    {
        #region ReplaceScriptFile
        private const string TOOL_NAME_REPLACE_SCRIPT_FILE = "ReplaceScriptFile";
        private const string PROPERTY_ReplaceScriptFile_FILEPATH = "FilePath";
        private const string PROPERTY_ReplaceScriptFile_CONTENT = "Content";
        private const string FUNCTION_REPLACE_SCRIPT_FILE_DESCRIPTION = "Fully replaces script file code content with new code content. It should be used when you want to replace the content of a script file with new content.";
        private const string FUNCTION_PROPERTY_ReplaceScriptFile_FILEPATH_DESCRIPTION = "Filepath of the code snippet, e.g. Assets\\Path\\To\\File.cs";
        private const string FUNCTION_PROPERTY_ReplaceScriptFile_CONTENT_DESCRIPTION = "FULL code for selected filepath, partial code snippets are NOT ALLOWED.";
        #endregion

        #region Tool_ReplaceScriptFile
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