using System.Collections.Generic;

namespace Sanat.CodeGenerator.CodebaseRag
{
    public class RagResultRanker
    {
        // public List<CodeSearchResult> RankResults(List<CodeSearchResult> results, RagQueryOptimizer.QueryPlan plan)
        // {
        //     return results
        //         .OrderByDescending(r => CalculateRelevanceScore(r, plan))
        //         .Take(plan.MaxResults)
        //         .ToList();
        // }
        //
        // private float CalculateRelevanceScore(CodeSearchResult result, RagQueryOptimizer.QueryPlan plan)
        // {
        //     float score = result.SemanticSimilarity;
        //
        //     // Boost score for direct field/method matches if requested
        //     if (plan.IncludeFields && result.Type == CodeElementType.Field)
        //         score *= 1.2f;
        //     if (plan.IncludeMethods && result.Type == CodeElementType.Method)
        //         score *= 1.2f;
        //
        //     // Consider relationship depth
        //     if (result.RelationshipDepth > 0)
        //         score *= (1.0f / (result.RelationshipDepth + 1));
        //
        //     return score;
        // }
    }
}