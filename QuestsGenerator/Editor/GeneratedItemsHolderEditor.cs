// Assets\Sanat\CodeGenerator\QuestsGenerator\Editor\GeneratedItemsHolderEditor.cs:
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GeneratedItemsHolder))]
public class GeneratedItemsHolderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GeneratedItemsHolder generatedItemsHolder = (GeneratedItemsHolder)target;

        if (GUILayout.Button("Clean Up Items without description"))
        {
            CleanUpItems(generatedItemsHolder);
        }

        if (GUILayout.Button("Export Icon Prompts to Text File"))
        {
            ExportIconPromptsToTextFile(generatedItemsHolder);
        }
    }

    private void CleanUpItems(GeneratedItemsHolder generatedItemsHolder)
    {
        if (!Application.isPlaying)
        {
            List<GeneratedItemDefinition> itemsToRemove = new List<GeneratedItemDefinition>();

            foreach (var item in generatedItemsHolder.GeneratedItems)
            {
                if (item == null)
                {
                    itemsToRemove.Add(item);
                    continue;
                }
                if (item.ItemDescription == null || item.ItemDescription.Length < 3)
                {
                    itemsToRemove.Add(item);
                }
            }

            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (var item in itemsToRemove)
                {
                    generatedItemsHolder.GeneratedItems.Remove(item);
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(item));
                }

                EditorUtility.SetDirty(generatedItemsHolder);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }
    }

    private void ExportIconPromptsToTextFile(GeneratedItemsHolder generatedItemsHolder)
    {
        if (!Application.isPlaying)
        {
            string path = AssetDatabase.GetAssetPath(generatedItemsHolder);
            string directory = Path.GetDirectoryName(path);
            string filePath = Path.Combine(directory, "IconPrompts.txt");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var item in generatedItemsHolder.GeneratedItems)
                {
                    if (item != null && !string.IsNullOrEmpty(item.IconPrompt))
                    {
                        writer.WriteLine($"positive: {item.IconPrompt}\n\nnegative:text, watermark\n----");
                    }
                }
            }
            
            File.WriteAllText(filePath, File.ReadAllText(filePath).TrimEnd('\n'));
            AssetDatabase.Refresh();
        }
    }
}