// Copyright (c) Sanat. All rights reserved.
using System;
using System.Collections.Generic;
using Sanat.ApiGemini;
using Sanat.ApiOpenAI;
using UnityEngine;

namespace Sanat.CodeGenerator.Agents
{
    public class AgentQuestWriter : AbstractAgentHandler
    {
        private string _prompt;

        protected override string PromptFilename() => "PromptGenerateQuestsForItemsTOOL.md";
        protected override Model GetModel() => Model.GPT4omini;
        protected override string GetGeminiModel() => ApiGeminiModels.Pro;
        private const string LOCAL_PROMPTS_FOLDER_PATH = "/Sanat/CodeGenerator/QuestsGenerator/Editor/Prompts/";
        
        private OpenAI.ToolFunction GetFunctionData_AddNewQuest()
        {
            string description = "Add new quest to the quest list.";
            string name = "AddNewQuest";

            OpenAI.Parameter parameters = new ();
            parameters.AddProperty("quest_giver_name", OpenAI.DataTypes.STRING, "The name of the quest giving subject");
            parameters.AddProperty("quest_giver_identity", OpenAI.DataTypes.NUMBER, "identity of quest giver. 0 - Male, 1 - Female");
            parameters.AddProperty("quest_giver_faction", OpenAI.DataTypes.STRING, "Faction in which quest giver is belong.");
            parameters.AddProperty("quest_description", OpenAI.DataTypes.STRING, "Description text of the quest, that will be shown to the player.");

            OpenAI.ToolFunction function = new OpenAI.ToolFunction(name, description, parameters);
            return function;
        }
        
        public AgentQuestWriter(ApiKeys apiKeys, string task, string alreadyExistingQuests)
        {
            Name = "AgentDescriptionToPrompt";
            Description = "Generates prompts";
            Temperature = .7f;
            StoreKeys(apiKeys);
            string promptLocation = Application.dataPath + $"{LOCAL_PROMPTS_FOLDER_PATH}{PromptFilename()}";
            Instructions = LoadPrompt(promptLocation);
            if (alreadyExistingQuests != String.Empty)
                alreadyExistingQuests = "# ALREADY EXISTING QUESTS: " + alreadyExistingQuests;
            _prompt = $"{Instructions} # TASK: {task}. {alreadyExistingQuests}";
            //_prompt = $"# TASK: {task}. {alreadyExistingQuests}";
            SelectedApiProvider = ApiProviders.OpenAI;
        }
        
        public override void Handle(string input)
        {
            Debug.Log($"<color=purple>{Name}</color> asking: {_prompt}");
            BotParameters botParameters = new BotParameters(_prompt, SelectedApiProvider, Temperature, null);
            GetResponse(_prompt, s =>
            {
                GeneratedItemDefinition.AddNewQuestData newQuestData =
                    JsonUtility.FromJson<GeneratedItemDefinition.AddNewQuestData>(s);
                Debug.Log($"<color=purple>{Name}</color> received: {newQuestData.QuestGiverName}");
            });

            //AskBot(botParameters);
        }
        
        async public void GetResponse(string input, Action<string> callback)
        {
            // try
            // {
            //     ChatCompletionsRequest request = new ChatCompletionsRequest();
            //     Message message = new(Roles.USER, input);
            //     request.AddMessage(message);
            //     request.AddFunction(GetFunctionData_AddNewQuest());
            //
            //     ChatCompletionsResponse res = await chatCompletionsApi.CreateChatCompletionsRequest(request);
            //
            //     Debug.Log(res.GetFunctionCallResponse().Arguments);
            //     GeneratedItemDefinition.AddNewQuestData newQuestData = JsonUtility.FromJson<GeneratedItemDefinition.AddNewQuestData>(res.GetFunctionCallResponse().Arguments);
            //     callback?.Invoke(res.GetFunctionCallResponse().Arguments);
            //     //{"quest_giver_name":"Elysia the Enchantress","quest_giver_identity":1,"quest_giver_faction":"Mages Guild","quest_description":"Elysia the Enchantress seeks brave adventurers to retrieve 5 Phoenix Feathers, rare and mystical items with the power of rebirth, to aid in her magical experiments. Will you answer the call?"}
            // }
            // catch (OpenAiRequestException exception)
            // {
            //     Debug.LogError(exception);
            // }
        }
    }
}