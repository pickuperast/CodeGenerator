// Copyright (c) Sanat. All rights reserved.
namespace Sanat.ApiAnthropic{

    /// <summary>
    /// According https://docs.anthropic.com/claude/reference/selecting-a-model <br/>
    /// We currently offer two families of models, both of which support 100,000 token context windows.
    /// </summary>
    public static class ApiAnthropicModels
    { 
        /// <summary>
        /// Superior performance on tasks that require complex reasoning. <br/>
        /// Max tokens: 100,000 tokens <br/>
        /// Training data: Up to February 2023 <br/>
        /// </summary>
        public const string Claude = "claude-2";
        
        /// <summary>
        /// Low-latency, high throughout. <br/>
        /// Max tokens: 100,000 tokens <br/>
        /// Training data: Up to February 2023 <br/>
        /// </summary>
        public const string ClaudeInstant = "claude-instant-1";
        public const string Claude35 = "claude-3-5-sonnet-20240620";
        public const string Claude35latest = "claude-3-5-sonnet-20241022";
    }
}