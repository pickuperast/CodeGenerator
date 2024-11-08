using System;
using System.Collections.Generic;

namespace Sanat.CodeGenerator.CodebaseRag
{
    public class CodeKnowledgeGraph
    {
        public class Node
        {
            public Guid Id { get; set; }
            public string Type { get; set; } // Class, Field, Method, etc.
            public Dictionary<string, object> Properties { get; set; }
            public List<float> Embedding { get; set; }
        }

        public class Edge  
        {
            public Guid Id { get; set; }
            public Guid SourceId { get; set; }
            public Guid TargetId { get; set; } 
            public string Type { get; set; }
            public Dictionary<string, object> Properties { get; set; }
        }
    }
}