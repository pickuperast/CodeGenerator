using System;
using System.Collections.Generic;
// using Supabase.Postgrest.Attributes;
// using Supabase.Postgrest.Models;

namespace Sanat.CodeGenerator.CodebaseRag
{
    /// <summary>
    /// CREATE TABLE code_functions (
    ///     id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    ///     file_path TEXT NOT NULL,
    ///     class_name TEXT NOT NULL,
    ///     function_name TEXT NOT NULL,
    ///     start_line INTEGER NOT NULL,
    ///     end_line INTEGER NOT NULL,
    ///     parameters JSONB,      -- Stores function parameters as JSON
    ///     return_type TEXT,      -- Stores return type of the function
    ///     comments TEXT,         -- Stores comments or docstrings associated with the function
    ///     code_snippet TEXT NOT NULL, -- The original code snippet of the function
    ///     embedding vector(1536) NOT NULL,    -- Embedding vector for similarity searches
    ///     last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- Last modified timestamp of the file
    ///     fields JSONB           -- Stores fields for MonoBehaviour classes or other metadata
    /// );
    /// </summary>
    // [Table("code_functions")]
    // public class CodeFunction : BaseModel
    // {
    //     [PrimaryKey("id")]
    //     public Guid Id { get; set; }
    //
    //     [Column("file_path")]
    //     public string FilePath { get; set; }
    //
    //     [Column("class_name")]
    //     public string ClassName { get; set; }
    //
    //     [Column("function_name")]
    //     public string FunctionName { get; set; }
    //
    //     [Column("start_line")]
    //     public int StartLine { get; set; }
    //
    //     [Column("end_line")] public int EndLine { get; set; }
    //     
    //     [Column("parameters")]
    //     public string Parameters { get; set; }
    //
    //     [Column("return_type")]
    //     public string ReturnType { get; set; }
    //
    //     [Column("comments")]
    //     public string Comments { get; set; }
    //
    //     [Column("code_snippet")]
    //     public string CodeSnippet { get; set; }
    //
    //     [Column("embedding")]
    //     public List<float> Embedding { get; set; }
    //
    //     [Column("last_updated")]
    //     public DateTime LastUpdated { get; set; }
    //
    //     [Column("fields")]
    //     public string Fields { get; set; }
    // }
}