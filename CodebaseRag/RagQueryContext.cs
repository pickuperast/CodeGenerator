using System.Collections.Generic;

namespace Sanat.CodeGenerator.CodebaseRag
{
    public class RagQueryContext
    {
        public string Query { get; set; }
        public RagQueryOptimizer.QueryPlan Plan { get; set; }
        public List<string> RequiredContexts { get; set; }
        public List<string> OptionalContexts { get; set; }
        public float MinRelevanceScore { get; set; }
        public int MaxResults { get; set; }
        public bool IncludeRelatedCode { get; set; }
        public Dictionary<string, float> ContextWeights { get; set; }
    }
}