using UnityEngine;
using UnityEditor;

namespace Sanat.CodeGenerator.Editor
{
    public static class CodeGeneratorFileOpener
    {
        public static void OpenScript(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError("File path is null or empty.");
                return;
            }

            string relativePath = filePath.Replace(Application.dataPath, "Assets");
            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);
            if (asset != null)
            {
                EditorGUIUtility.PingObject(asset);
                AssetDatabase.OpenAsset(asset);
            }
            else
            {
                Debug.Log($"Could not find script at path: {relativePath}. Failed to open not cached file. Try recompile solution.");
            }
        }
    }
}