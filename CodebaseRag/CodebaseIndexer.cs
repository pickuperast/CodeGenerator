// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using System.Threading.Tasks;
// using Newtonsoft.Json;
// using Sanat.ApiOpenAI;
//
// namespace Sanat.CodeGenerator.CodebaseRag
// {
//     public class CodebaseIndexer
//     {
//         private readonly Queue<Type> _typeQueue = new Queue<Type>();
//         private readonly HashSet<Type> _processedTypes = new HashSet<Type>();
//         private readonly CodeKnowledgeGraph _graph = new();
//         private readonly SupabaseManager _supabaseDb;
//         //TODO: create semantic processor
//         //private readonly SemanticContextProcessor _semanticProcessor;
//
//         public async Task ProcessType(Type type)
//         {
//             _processedTypes.Add(type);
//
//             var classId = Guid.NewGuid();
//             var classNode = new CodeClass
//             {
//                 Id = classId,
//                 ClassName = type.Name,
//                 Namespace = type.Namespace,
//                 ParentClass = type.BaseType?.FullName,
//                 Interfaces = JsonConvert.SerializeObject(type.GetInterfaces().Select(i => i.FullName)),
//                 FilePath = GetSourceFilePath(type),
//                 EmbeddedSignature = await GenerateClassSignatureEmbedding(type)
//             };
//
//             await _supabaseDb.GetClient().From<CodeClass>().Insert(classNode);
//
//             var fieldsWithContext = new Dictionary<FieldInfo, string>();
//             foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
//             {
//                 var semanticContext = _semanticProcessor.GenerateFieldContext(field, type);
//                 fieldsWithContext[field] = semanticContext;
//                 
//                 var fieldNode = new CodeField
//                 {
//                     Id = Guid.NewGuid(),
//                     ClassId = classId,
//                     FieldName = field.Name,
//                     FieldType = field.FieldType.FullName,
//                     Modifiers = GetFieldModifiers(field),
//                     Documentation = GetFieldDocumentation(field),
//                     SemanticContext = semanticContext,
//                     Embedding = await GenerateFieldEmbedding(field, semanticContext)
//                 };
//
//                 await _supabaseDb.GetClient().From<CodeField>().Insert(fieldNode);
//
//                 // Track relationships
//                 if (!_processedTypes.Contains(field.FieldType) && !field.FieldType.IsPrimitive)
//                 {
//                     _typeQueue.Enqueue(field.FieldType);
//                     
//                     await _supabaseDb.GetClient().From<CodeRelationship>().Insert(new CodeRelationship
//                     {
//                         Id = Guid.NewGuid(),
//                         SourceId = classId,
//                         TargetId = GetOrCreateTypeId(field.FieldType),
//                         RelationType = "FIELD_TYPE",
//                         Metadata = JsonConvert.SerializeObject(new
//                         {
//                             FieldName = field.Name,
//                             IsCollection = IsCollectionType(field.FieldType)
//                         })
//                     });
//                 }
//             }
//
//             // Process inheritance relationships
//             if (type.BaseType != null && !type.BaseType.Equals(typeof(object)))
//             {
//                 await _supabaseDb.GetClient().From<CodeRelationship>().Insert(new CodeRelationship
//                 {
//                     Id = Guid.NewGuid(),
//                     SourceId = classId,
//                     TargetId = GetOrCreateTypeId(type.BaseType),
//                     RelationType = "INHERITS_FROM"
//                 });
//             }
//
//             // Process interface implementations
//             foreach (var iface in type.GetInterfaces())
//             {
//                 await _supabaseDb.GetClient().From<CodeRelationship>().Insert(new CodeRelationship
//                 {
//                     Id = Guid.NewGuid(), 
//                     SourceId = classId,
//                     TargetId = GetOrCreateTypeId(iface),
//                     RelationType = "IMPLEMENTS"
//                 });
//             }
//
//             // Process next type in queue
//             while (_typeQueue.Count > 0)
//             {
//                 var nextType = _typeQueue.Dequeue();
//                 if (!_processedTypes.Contains(nextType))
//                 {
//                     await ProcessType(nextType);
//                 }
//             }
//         }
//
//         private string GetSourceFilePath(Type type)
//         {
//             throw new NotImplementedException();
//         }
//
//         private async Task<List<float>> GenerateClassSignatureEmbedding(Type type)
//         {
//             var signatureText = $"{type.Namespace}.{type.Name} " +
//                                $"Base: {type.BaseType?.Name ?? "none"} " +
//                                $"Interfaces: {string.Join(",", type.GetInterfaces().Select(i => i.Name))}";
//             
//             return await GetEmbedding(signatureText);
//         }
//
//         private async Task<List<float>> GenerateFieldEmbedding(FieldInfo field, string semanticContext)
//         {
//             var fieldText = $"{field.Name} {field.FieldType.Name} {semanticContext}";
//             return await GetEmbedding(fieldText);
//         }
//
//         private async Task<List<float>> GetEmbedding(string text)
//         {
//             TaskCompletionSource<List<float>> tcs = new();
//             OpenAI.SubmitEmbeddingAsync(_openAiKey, text, "text-embedding-3-small", embedding =>
//             {
//                 tcs.SetResult(embedding);
//             });
//             return await tcs.Task;
//         }
//
//         private Guid GetOrCreateTypeId(Type type)
//         {
//             // Implementation to get or generate type ID
//             return Guid.NewGuid(); 
//         }
//     }
// }