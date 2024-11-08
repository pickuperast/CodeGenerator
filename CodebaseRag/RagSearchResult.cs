using System.Collections.Generic;

namespace Sanat.CodeGenerator.CodebaseRag
{
    public class RagSearchResult
    {
        public string CodeSnippet { get; set; }
        public string FilePath { get; set; }
        public string ClassName { get; set; }
        public string ElementName { get; set; }
        //TODO: Add CodeElementType
        //public CodeElementType ElementType { get; set; }
        public float RelevanceScore { get; set; }
        public List<string> Contexts { get; set; }
        public Dictionary<string, float> ContextScores { get; set; }
        public List<RagSearchResult> RelatedResults { get; set; }
    }
}