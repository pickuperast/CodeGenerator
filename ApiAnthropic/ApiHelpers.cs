// Copyright (c) Sanat. All rights reserved.
using System;

namespace Sanat.ApiAnthropic{
public static class ApiHelpers
{
    /// <summary>
    /// According https://www-files.anthropic.com/production/images/model_pricing_july2023.pdf <br/>
    /// </summary>
    /// <param name="modelId"></param>
    /// <param name="completionTokens"></param>
    /// <param name="promptTokens"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static double CalculatePriceInUsd(string modelId, int completionTokens, int promptTokens)
    {
        var promptPricePerToken = modelId switch
        {
            ApiAnthropicModels.Claude => 11.02,
            
            ApiAnthropicModels.ClaudeInstant => 1.63,
            
            _ => throw new NotImplementedException(),
        };
        var completionPricePerToken = modelId switch
        {
            ApiAnthropicModels.Claude => 32.68,
            
            ApiAnthropicModels.ClaudeInstant => 5.51,
            
            _ => throw new NotImplementedException(),
        } * 0.001 * 0.001;
        
        return completionTokens * completionPricePerToken +
               promptTokens * promptPricePerToken;
    }

    /// <summary>
    /// Calculates the maximum number of tokens possible to generate for a model. <br/>
    /// According https://docs.anthropic.com/claude/reference/selecting-a-model <br/>
    /// </summary>
    /// <param name="modelId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static int CalculateContextLength(string modelId)
    {
        return modelId switch
        {
            ApiAnthropicModels.Claude => 100_000,
            ApiAnthropicModels.ClaudeInstant => 100_000,
            
            _ => throw new NotImplementedException(),
        };
    }
}

}