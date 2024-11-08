using System.Collections.Generic;
using System.Linq;

namespace Sanat.CodeGenerator.CodebaseRag
{
    public class RagUsageAnalyzer
    {
        private readonly Dictionary<string, int> _searchPatternFrequency = new();
        //private readonly Dictionary<CodeElementType, int> _resultTypeUsage = new();
        private readonly Dictionary<string, float> _contextScores = new();

        // public void TrackSearch(string query, RagQueryOptimizer.QueryPlan plan, List<CodeSearchResult> results)
        // {
        //     // Track search patterns
        //     var patterns = ExtractSearchPatterns(query);
        //     foreach (var pattern in patterns)
        //     {
        //         if (!_searchPatternFrequency.ContainsKey(pattern))
        //             _searchPatternFrequency[pattern] = 0;
        //         _searchPatternFrequency[pattern]++;
        //     }
        //
        //     // Track result type usage
        //     foreach (var result in results)
        //     {
        //         if (!_resultTypeUsage.ContainsKey(result.Type))
        //             _resultTypeUsage[result.Type] = 0;
        //         _resultTypeUsage[result.Type]++;
        //     }
        //
        //     // Track context effectiveness
        //     foreach (var result in results)
        //     {
        //         foreach (var context in result.Contexts)
        //         {
        //             if (!_contextScores.ContainsKey(context))
        //                 _contextScores[context] = 0;
        //             _contextScores[context] += result.RelevanceScore;
        //         }
        //     }
        // }
        //
        // public QueryOptimizationSuggestions GenerateOptimizations()
        // {
        //     return new QueryOptimizationSuggestions
        //     {
        //         PreferredElementTypes = _resultTypeUsage
        //             .OrderByDescending(kvp => kvp.Value)
        //             .Take(3)
        //             .Select(kvp => kvp.Key)
        //             .ToList(),
        //
        //         EffectiveContexts = _contextScores
        //             .OrderByDescending(kvp => kvp.Value)
        //             .Take(5)
        //             .Select(kvp => kvp.Key)
        //             .ToList(),
        //
        //         CommonPatterns = _searchPatternFrequency
        //             .OrderByDescending(kvp => kvp.Value)
        //             .Take(10)
        //             .Select(kvp => kvp.Key)
        //             .ToList()
        //     };
        // }
    }
}