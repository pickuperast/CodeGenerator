using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Sanat.CodeGenerator.ApiComfiUI
{
[CreateAssetMenu(fileName = "ComfyUIImageProcessor", menuName = "ScriptableObjects/ComfyUIImageProcessor", order = 1)]
public class ComfyUIImageProcessor : ScriptableObject
{
    private static readonly string serverAddress = "127.0.0.1:8188";
    public static string comfiuiOutputFolder = "E:/Programs/ComfyUI_windows_portable/ComfyUI/output";
    private const string WORKFLOWS_FOLDER = "Sanat/CodeGenerator/ApiComfiUI/Workflows";
    private Guid clientId = Guid.NewGuid();

    private static readonly HttpClient httpClient = new HttpClient();

    [MenuItem("ComfyUI/Process Image")]
    public static async void ProcessImageWithComfyUI(string imagePath, string workflowName)
    {
        try
        {
            var workflowParams = LoadWorkflowParams(workflowName);
            string outputImagePath = await ProcessImage(imagePath, workflowParams);
            Debug.Log($"Image processed successfully. Output path: {outputImagePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error processing image: {ex.Message}");
        }
    }
    
    [MenuItem("ComfyUI/Generate Image")]
    public static async void GenerateImageWithComfyUI(string prompt, string workflowName, string outputFolderPath)
    {
        try
        {
            var workflowParams = LoadWorkflowParams(workflowName);
            string outputImagePath = await GenerateImage(prompt, workflowParams, outputFolderPath);
            Debug.Log($"Image processed successfully. Output path: {outputImagePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error processing image: {ex.Message}");
        }
    }
    
    private static async Task<string> GenerateImage(string prompt, WorkflowParams workflowParams, string outputFolderPath)
    {
        // Prepare the workflow JSON with the text prompt
        string workflowJson = PrepareWorkflowJsonWithPrompt(prompt, workflowParams);

        // Queue the prompt
        string promptId = await QueuePrompt(workflowJson);

        // Track progress and get images
        string outputImagePath = await GetOutputImage(promptId, workflowParams.OutputNodeIds);

        return outputImagePath;
    }

    private static async Task<string> ProcessImage(string imagePath, WorkflowParams workflowParams)
    {
        // Upload the image
        await UploadImage(imagePath);

        // Prepare the workflow JSON
        string workflowJson = PrepareWorkflowJson(imagePath, workflowParams);

        // Queue the prompt
        string promptId = await QueuePrompt(workflowJson);

        // Track progress and get images
        string outputImagePath = await GetOutputImage(promptId, workflowParams.OutputNodeIds);

        return outputImagePath;
    }

    private static async Task UploadImage(string imagePath)
    {
        var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(File.ReadAllBytes(imagePath)), "image", Path.GetFileName(imagePath));

        var response = await httpClient.PostAsync($"http://{serverAddress}/upload/image", form);
        response.EnsureSuccessStatusCode();
    }
    
    private static string PrepareWorkflowJsonWithPrompt(string prompt, WorkflowParams workflowParams)
    {
        // Load and modify the workflow JSON
        string workflowJsonPath = Path.Combine(Application.dataPath, WORKFLOWS_FOLDER, workflowParams.WorkflowName);
        string workflowJson = File.ReadAllText(workflowJsonPath);
        var workflow = JsonConvert.DeserializeObject<dynamic>(workflowJson);
        
        workflow[workflowParams.InputNodeId]["inputs"]["text"] = prompt;

        return workflow.ToString();
    }

    private static string PrepareWorkflowJson(string imagePath, WorkflowParams workflowParams)
    {
        // Load and modify the workflow JSON
        string workflowJsonPath = Path.Combine(Application.dataPath, WORKFLOWS_FOLDER, workflowParams.WorkflowName);
        string workflowJson = File.ReadAllText(workflowJsonPath);

        // Modify the JSON to include the image path and other parameters
        // This is a simplified example; you may need to use a JSON library to modify the JSON properly
        workflowJson = workflowJson.Replace("INPUT_IMAGE_PLACEHOLDER", Path.GetFileName(imagePath));

        return workflowJson;
    }

    private static async Task<string> QueuePrompt(string workflowJson)
    {
        var content = new StringContent(workflowJson, System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync($"http://{serverAddress}/prompt", content);
        response.EnsureSuccessStatusCode();

        // Extract the prompt ID from the response
        string responseBody = await response.Content.ReadAsStringAsync();
        // Assume the response contains a JSON object with a "prompt_id" field
        string promptId = ExtractPromptIdFromResponse(responseBody);

        return promptId;
    }

    private static async Task<string> GetOutputImage(string promptId, int[] outputNodeIds, string filename = "output_image.png")
    {
        // Wait for processing to complete and download the output image
        // This is a simplified example; you may need to implement WebSocket communication to track progress

        string outputImagePath = Path.Combine(comfiuiOutputFolder, filename);
        // Download the image from the server
        var response = await httpClient.GetAsync($"http://{serverAddress}/view?filename=output_image.png");
        response.EnsureSuccessStatusCode();

        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
        File.WriteAllBytes(outputImagePath, imageBytes);

        return outputImagePath;
    }

    private static string ExtractPromptIdFromResponse(string responseBody)
    {
        // Implement JSON parsing to extract the prompt ID
        // This is a placeholder implementation
        return "extracted_prompt_id";
    }

    public static WorkflowParams LoadWorkflowParams(string workflowName)
    {
        // Define your workflows here
        var workflows = new System.Collections.Generic.Dictionary<string, WorkflowParams>
        {
            { "img2img_realism_to_stylized_no_upscale", new WorkflowParams("img2img_realism_to_stylized_no_upscale.json", 14, new[] { 109 }) },
            { "img2img_upscaler", new WorkflowParams("upscaler_4x.json", 14, new[] { 136 }) },
            { "icons_generator_api", new WorkflowParams("icons_generator_api.json", 6, new []{ 9 })}
        };

        if (!workflows.ContainsKey(workflowName))
        {
            throw new System.Exception($"Workflow with name {workflowName} does not exist.");
        }

        return workflows[workflowName];
    }
}

public class WorkflowParams
{
    public string WorkflowName { get; }
    public int InputNodeId { get; }
    public int[] OutputNodeIds { get; }

    public WorkflowParams(string workflowName, int inputNodeId, int[] outputNodeIds)
    {
        WorkflowName = workflowName;
        InputNodeId = inputNodeId;
        OutputNodeIds = outputNodeIds;
    }
}
}