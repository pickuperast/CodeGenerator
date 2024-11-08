// Assets\Sanat\CodeGenerator\CodebaseRag\RagProcessor.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using Sanat.ApiOpenAI;
using UnityEditor;

namespace Sanat.CodeGenerator.CodebaseRag
{
    public class RagProcessor
    {
        private SupabaseManager _supabaseManager;
        private string _openaiApiKey;
        private string _supabaseUrl;
        private string _supabaseKey;
        private string _tableName;
        private const string EMBEDDING_MODEL = "text-embedding-3-small";
        
        public async Task ProcessAllFiles(string inputFolder)
        {
#if UNITY_EDITOR
            _openaiApiKey = PlayerPrefs.GetString("OpenaiApiKey", "");
            _supabaseUrl = PlayerPrefs.GetString("SupabaseUrl", "");
            _supabaseKey = PlayerPrefs.GetString("SupabaseKey", "");
            _tableName = PlayerPrefs.GetString("TableName", "");

            if (string.IsNullOrEmpty(_openaiApiKey) || string.IsNullOrEmpty(_supabaseUrl) || 
                string.IsNullOrEmpty(_supabaseKey) || string.IsNullOrEmpty(_tableName))
            {
                Debug.LogError("Missing required API keys or configuration");
                return;
            }

            _supabaseManager = new SupabaseManager();
            //await _supabaseManager.Initialize(_supabaseUrl, _supabaseKey);

            string[] csharpFiles = Directory.GetFiles(inputFolder, "*.cs", SearchOption.AllDirectories);
            int totalFiles = csharpFiles.Length;
            int processedFiles = 0;
            int maxFiles = 5;

            foreach (string filePath in csharpFiles)
            {
                try
                {
                    await ProcessFile(filePath);
                    processedFiles++;
                    float progress = (float)processedFiles / totalFiles;
                    EditorUtility.DisplayProgressBar("Processing Files", 
                        $"Processing {Path.GetFileName(filePath)} ({processedFiles}/{totalFiles})", 
                        progress);
                    if (processedFiles >= maxFiles)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error processing file {filePath}: {e.Message}");
                }
            }
#endif
        }

        private async Task ProcessFile(string filePath)
        {
#if UNITY_EDITOR
            // string code = File.ReadAllText(filePath);
            // SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            // var root = await syntaxTree.GetRootAsync();
            // var functions = root.DescendantNodes()
            //     .OfType<MethodDeclarationSyntax>()
            //     .Select(method => new CodeFunction
            //     {
            //         Id = Guid.NewGuid(),
            //         FilePath = filePath,
            //         ClassName = method.Parent is ClassDeclarationSyntax classDeclaration ? classDeclaration.Identifier.Text : "",
            //         FunctionName = method.Identifier.Text,
            //         StartLine = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            //         EndLine = method.GetLocation().GetLineSpan().EndLinePosition.Line + 1,
            //         Parameters = JsonConvert.SerializeObject(method.ParameterList.Parameters.Select(p => new { Name = p.Identifier.Text, Type = p.Type.ToString() })),
            //         ReturnType = method.ReturnType.ToString(),
            //         Comments = method.GetLeadingTrivia().ToString(),
            //         CodeSnippet = method.ToFullString(),
            //         LastUpdated = DateTime.UtcNow,
            //         Fields = JsonConvert.SerializeObject(root.DescendantNodes().OfType<FieldDeclarationSyntax>().Select(field => new { Name = field.Declaration.Variables.First().Identifier.Text, Type = field.Declaration.Type.ToString() }))
            //     });
            //
            // foreach (var function in functions)
            // {
            //     function.Comments = await GenerateComments(function);
            //     function.Embedding = await GenerateEmbedding(function);
            //     await InsertIntoSupabase(function);
            // }
#endif
        }

        // private async Task<List<float>> GenerateEmbedding(CodeFunction function)
        // {
        //     TaskCompletionSource<List<float>> tcs = new TaskCompletionSource<List<float>>();
        //     
        //     string textToEmbed = $"{function.ClassName}.{function.FunctionName} {function.Comments} {function.CodeSnippet}";
        //     
        //     OpenAI.SubmitEmbeddingAsync(_openaiApiKey, textToEmbed, EMBEDDING_MODEL, (embedding) =>
        //     {
        //         if (embedding != null)
        //         {
        //             tcs.SetResult(embedding);
        //         }
        //         else
        //         {
        //             tcs.SetException(new Exception("Failed to generate embedding"));
        //         }
        //     });
        //
        //     return await tcs.Task;
        // }

        // private async Task InsertIntoSupabase(CodeFunction function)
        // {
        //     var client = _supabaseManager.GetClient();
        //     if (client == null)
        //     {
        //         throw new Exception("Supabase client not initialized");
        //     }
        //
        //     try
        //     {
        //         await client.From<CodeFunction>()
        //             .Insert(function);
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError($"Error inserting into Supabase: {e.Message}");
        //         throw;
        //     }
        // }
        //
        // private async Task<string> GenerateComments(CodeFunction function)
        // {
        //     string prompt = $"Generate documentation comments for this code that wil contain only necessary for RAG system keywords for " +
        //                     $"better searching in vector(1536) dimension, comments should be in JSON format:\n{function.CodeSnippet}";
        //     
        //     List<ChatMessage> messages = new List<ChatMessage>
        //     {
        //         new ChatMessage("system", "You are a code documentation expert. Generate concise, clear comments for the given code."),
        //         new ChatMessage("user", prompt)
        //     };
        //
        //     TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        //     
        //     OpenAI.SubmitChatAsync(_openaiApiKey, Model.GPT4omini, 0.7f, 150, messages, (response) =>
        //     {
        //         if (!string.IsNullOrEmpty(response))
        //         {
        //             tcs.SetResult(response);
        //             Debug.Log($"Generated comments for {function.ClassName}.{function.FunctionName}");
        //         }
        //         else
        //         {
        //             tcs.SetException(new Exception("Failed to generate comments"));
        //         }
        //     });
        //
        //     return await tcs.Task;
        // }
    }
}