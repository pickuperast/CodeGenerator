// Assets\Sanat\CodeGenerator\QuestsGenerator\GeneratedItemsHolder.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEditor;

[CreateAssetMenu(fileName = "GeneratedItemsHolder", menuName = "ScriptableObjects/GeneratedItemsHolder", order = 1)]
public class GeneratedItemsHolder : ScriptableObject
{
    public List<GeneratedItemDefinition> GeneratedItems => _generatedItems;
    [SerializeField] private List<GeneratedItemDefinition> _generatedItems = new();
    
    public string GlobalPositivePromptSuffix;
    public string GlobalNegativePromptSuffix;
    public string ImagesFolder;

    /// <summary>
    /// Deletes items and their associated assets (icons, scriptable objects) from fromIndex to toIndex inclusive
    /// </summary>
    public void DeleteItemsRange(int fromIndex, int toIndex)
    {
#if UNITY_EDITOR
        if (fromIndex < 0 || toIndex >= _generatedItems.Count || fromIndex > toIndex)
        {
            Debug.LogError($"Invalid index range: from {fromIndex} to {toIndex}");
            return;
        }

        // Get the directory path of this scriptable object
        string holderPath = AssetDatabase.GetAssetPath(this);
        string directory = Path.GetDirectoryName(holderPath);
        
        // Start batch operation for better performance
        AssetDatabase.StartAssetEditing();
        
        try
        {
            // Process items in reverse order to avoid index shifting issues
            for (int i = toIndex; i >= fromIndex; i--)
            {
                var item = _generatedItems[i];
                if (item == null) continue;

                // Delete icon sprite if it exists
                if (item.Icon != null)
                {
                    string spritePath = AssetDatabase.GetAssetPath(item.Icon);
                    if (!string.IsNullOrEmpty(spritePath))
                    {
                        AssetDatabase.DeleteAsset(spritePath);
                    }
                }

                // Delete the item's scriptable object asset
                string itemPath = AssetDatabase.GetAssetPath(item);
                if (!string.IsNullOrEmpty(itemPath))
                {
                    AssetDatabase.DeleteAsset(itemPath);
                }

                // Remove from list
                _generatedItems.RemoveAt(i);
            }

            // Delete any orphaned icon files in the Images folder
            string imagesPath = Path.Combine(directory, "Images");
            if (Directory.Exists(imagesPath))
            {
                for (int i = fromIndex; i <= toIndex; i++)
                {
                    string iconPath = Path.Combine(imagesPath, $"item_icon_{i}.png").Replace("\\", "/");
                    if (File.Exists(iconPath))
                    {
                        string assetPath = "Assets" + iconPath.Substring(Application.dataPath.Length);
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                }
            }

            // Shift indices of remaining icons if needed
            ShiftRemainingIconsAndUpdateReferences(toIndex + 1, fromIndex);

            EditorUtility.SetDirty(this);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }

    private void ShiftRemainingIconsAndUpdateReferences(int startIndex, int newStartIndex)
    {
#if UNITY_EDITOR
        string holderPath = AssetDatabase.GetAssetPath(this);
        string directory = Path.GetDirectoryName(holderPath);
        string imagesPath = Path.Combine(directory, "Images");

        if (!Directory.Exists(imagesPath)) return;

        int offset = newStartIndex - startIndex;
        var iconMoves = new List<(string oldPath, string newPath)>();

        // First, collect all moves we need to make
        for (int i = startIndex; i < _generatedItems.Count; i++)
        {
            string oldIconPath = Path.Combine(imagesPath, $"item_icon_{i}.png").Replace("\\", "/");
            string newIconPath = Path.Combine(imagesPath, $"item_icon_{i + offset}.png").Replace("\\", "/");

            if (File.Exists(oldIconPath))
            {
                iconMoves.Add((oldIconPath, newIconPath));
            }
        }

        // Then perform the moves
        foreach (var move in iconMoves)
        {
            if (File.Exists(move.newPath))
            {
                File.Delete(move.newPath);
            }
            File.Move(move.oldPath, move.newPath);

            // Update asset database
            string oldAssetPath = "Assets" + move.oldPath.Substring(Application.dataPath.Length);
            string newAssetPath = "Assets" + move.newPath.Substring(Application.dataPath.Length);
            AssetDatabase.MoveAsset(oldAssetPath, newAssetPath);
        }

        // Update references in the items
        for (int i = startIndex; i < _generatedItems.Count; i++)
        {
            var item = _generatedItems[i];
            if (item != null && item.Icon != null)
            {
                string iconPath = Path.Combine(imagesPath, $"item_icon_{i + offset}.png").Replace("\\", "/");
                string assetPath = "Assets" + iconPath.Substring(Application.dataPath.Length);
                var newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (newSprite != null)
                {
                    item.Icon = newSprite;
                    EditorUtility.SetDirty(item);
                }
            }
        }
#endif
    }
}