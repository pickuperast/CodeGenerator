using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GeneratedItemsHolder))]
public class GeneratedItemsHolderEditor : Editor
{
    private int _fromIndex;
    private int _toIndex;
    private Vector2 _scrollPosition;
    private const float IconSize = 48f;
    private const int ItemsPerPage = 10;
    private int _currentPage;
    private int _totalPages;
    private bool _showQuestGiverFaction = true;

    public override void OnInspectorGUI()
    {
        GeneratedItemsHolder generatedItemsHolder = (GeneratedItemsHolder)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Global Prompt Suffixes", EditorStyles.boldLabel);

        generatedItemsHolder.GlobalPositivePromptSuffix = EditorGUILayout.TextField("Global Positive Suffix", generatedItemsHolder.GlobalPositivePromptSuffix);
        generatedItemsHolder.GlobalNegativePromptSuffix = EditorGUILayout.TextField("Global Negative Suffix", generatedItemsHolder.GlobalNegativePromptSuffix);
        generatedItemsHolder.ImagesFolder = EditorGUILayout.TextField("Images Folder", generatedItemsHolder.ImagesFolder);

        EditorGUILayout.Space();
        DrawPaginationControls(generatedItemsHolder);

        _showQuestGiverFaction = EditorGUILayout.Foldout(_showQuestGiverFaction, "Quest Giver Faction");

        if (_showQuestGiverFaction)
        {
            DrawItemsTableWithFaction(generatedItemsHolder);
        }
        else
        {
            DrawItemsTable(generatedItemsHolder);
        }

        DrawPaginationControls(generatedItemsHolder);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        _fromIndex = Mathf.Max(0, EditorGUILayout.IntField("From Index", _fromIndex));
        int maxIndex = generatedItemsHolder.GeneratedItems.Count - 1;
        _toIndex = EditorGUILayout.IntSlider("To Index", _toIndex, _fromIndex, maxIndex);
        EditorGUILayout.EndHorizontal();

        if (_toIndex < _fromIndex)
        {
            _toIndex = _fromIndex;
        }

        if (GUILayout.Button("Clean Up Items without description"))
        {
            CleanUpItems(generatedItemsHolder);
        }

        if (GUILayout.Button("Export Icon Prompts to Text File"))
        {
            ExportIconPromptsToTextFile(generatedItemsHolder);
        }

        if (GUILayout.Button("Copy and Apply Images"))
        {
            CopyAndApplyImages(generatedItemsHolder);
        }

        if (GUILayout.Button("Delete Items Range"))
        {
            DeleteItemsRange(generatedItemsHolder);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(generatedItemsHolder);
        }
    }

    private void DrawItemsTableWithFaction(GeneratedItemsHolder holder)
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        int startIndex = _currentPage * ItemsPerPage;
        int endIndex = Mathf.Min(startIndex + ItemsPerPage, holder.GeneratedItems.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            EditorGUILayout.BeginHorizontal();
            GeneratedItemDefinition item = null;
            if (GUILayout.Button(i.ToString(), GUILayout.Width(30)))
            {
                item = holder.GeneratedItems[i];
                if (item != null)
                {
                    EditorGUIUtility.PingObject(item);
                    Selection.activeObject = item;
                }
            }

            item = holder.GeneratedItems[i];
            if (item != null)
            {
                item.Icon = (Sprite)EditorGUILayout.ObjectField(item.Icon, typeof(Sprite), false, GUILayout.Width(IconSize), GUILayout.Height(IconSize));

                EditorGUILayout.BeginVertical();
                item.ItemName = EditorGUILayout.TextField(item.ItemName, GUILayout.Width(IconSize * 4));
                item.ItemDescription = EditorGUILayout.TextArea(item.ItemDescription, GUILayout.Width(IconSize * 4), GUILayout.Height(IconSize - EditorGUIUtility.singleLineHeight), GUILayout.ExpandHeight(true));
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Faction:", EditorStyles.boldLabel);
                item.QuestGiverFaction = EditorGUILayout.TextField(item.QuestGiverFaction, GUILayout.Width(IconSize * 3));
                EditorGUILayout.EndVertical();
                
                if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    DeleteGeneratedItem(holder, item, i);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawPaginationControls(GeneratedItemsHolder holder)
    {
        _totalPages = Mathf.CeilToInt(holder.GeneratedItems.Count / (float)ItemsPerPage);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("<<", GUILayout.Width(30)))
        {
            _currentPage = 0;
        }

        if (GUILayout.Button("<", GUILayout.Width(30)))
        {
            _currentPage = Mathf.Max(0, _currentPage - 1);
        }
        GUILayout.Label($"Page {_currentPage + 1} of {_totalPages}", EditorStyles.centeredGreyMiniLabel);
        if (GUILayout.Button(">", GUILayout.Width(30)))
        {
            _currentPage = Mathf.Min(_totalPages - 1, _currentPage + 1);
        }

        if (GUILayout.Button(">>", GUILayout.Width(30)))
        {
            _currentPage = _totalPages - 1;
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawItemsTable(GeneratedItemsHolder holder)
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        int startIndex = _currentPage * ItemsPerPage;
        int endIndex = Mathf.Min(startIndex + ItemsPerPage, holder.GeneratedItems.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(30));

            var item = holder.GeneratedItems[i];
            if (item != null)
            {
                item.Icon = (Sprite)EditorGUILayout.ObjectField(item.Icon, typeof(Sprite), false, GUILayout.Width(IconSize), GUILayout.Height(IconSize));
                EditorGUILayout.BeginVertical();
                item.ItemName = EditorGUILayout.TextField(item.ItemName, GUILayout.Width(IconSize * 4));
                item.ItemDescription = EditorGUILayout.TextArea(item.ItemDescription, GUILayout.Width(IconSize * 4), GUILayout.Height(IconSize - EditorGUIUtility.singleLineHeight), GUILayout.ExpandHeight(true));
                EditorGUILayout.EndVertical();
                if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
                {
                    DeleteGeneratedItem(holder, item, i);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DeleteGeneratedItem(GeneratedItemsHolder holder, GeneratedItemDefinition item, int index)
    {
        if (!Application.isPlaying)
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                if (item.Icon != null)
                {
                    string spritePath = AssetDatabase.GetAssetPath(item.Icon);
                    if (!string.IsNullOrEmpty(spritePath))
                    {
                        AssetDatabase.DeleteAsset(spritePath);
                    }
                }
                string itemPath = AssetDatabase.GetAssetPath(item);
                if (!string.IsNullOrEmpty(itemPath))
                {
                    AssetDatabase.DeleteAsset(itemPath);
                }
                holder.GeneratedItems.RemoveAt(index);
                int maxPage = Mathf.CeilToInt(holder.GeneratedItems.Count / (float)ItemsPerPage) - 1;
                _currentPage = Mathf.Min(_currentPage, maxPage);
                            
                EditorUtility.SetDirty(holder);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }
    }

    private async void CopyAndApplyImages(GeneratedItemsHolder generatedItemsHolder)
    {
        if (Application.isPlaying) return;
        string holderPath = AssetDatabase.GetAssetPath(generatedItemsHolder);
        string targetDirectory = Path.Combine(Path.GetDirectoryName(holderPath), "Images").Replace("\\", "/");
        string sourceDirectory = generatedItemsHolder.ImagesFolder.Replace("\\", "/");
        if (!Directory.Exists(sourceDirectory))
        {
            Debug.LogError($"Source directory not found: {sourceDirectory}");
            return;
        }
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
            AssetDatabase.Refresh();
        }
        var validExtensions = new[] { ".png", ".jpg", ".jpeg" };
        int imagesCount = _toIndex - _fromIndex + 1;
        
        var imageFiles = Directory.GetFiles(sourceDirectory)
            .Where(f => validExtensions.Contains(Path.GetExtension(f).ToLower()))
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.CreationTime)
            .Take(imagesCount)
            .ToList();
        if (!imageFiles.Any())
        {
            Debug.LogError("No valid image files found in source directory");
            return;
        }
        string[] existingFiles = Directory.GetFiles(targetDirectory, "item_icon_*.*");
        foreach (string file in existingFiles)
        {
            string assetPath = file.Replace("\\", "/");
            if (assetPath.StartsWith(Application.dataPath))
            {
                assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
                AssetDatabase.DeleteAsset(assetPath);
            }
        }
        AssetDatabase.Refresh();
        
        Dictionary<string, (string itemPath, string sourceFile)> fileMappings = new ();
        
        CopyImages(generatedItemsHolder, imageFiles, targetDirectory, fileMappings);
        await ChangeTextureToSprite(fileMappings);
        await ApplySpritesToImages(fileMappings);
        Debug.Log("Image import process completed. If sprites are still not loading correctly, try closing and reopening Unity.");
    }

    private async Task ApplySpritesToImages(Dictionary<string, (string itemPath, string sourceFile)> fileMappings)
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var mapping in fileMappings)
            {
                string assetPath = mapping.Key;
                string itemPath = mapping.Value.itemPath;
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite == null)
                {
                    Debug.LogError($"Failed to load sprite from file: {assetPath}");
                    Debug.LogError($"Absolute path: {Path.GetFullPath(assetPath)}");
                    Debug.LogError($"Source file: {mapping.Value.sourceFile}");
                    continue;
                }
                var item = AssetDatabase.LoadAssetAtPath<GeneratedItemDefinition>(itemPath);
                if (item == null)
                {
                    Debug.LogError($"Failed to load item from file: {itemPath}");
                    continue;
                }
                item.Icon = sprite;
                EditorUtility.SetDirty(item);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private void CopyImages(GeneratedItemsHolder generatedItemsHolder, List<FileInfo> imageFiles, string targetDirectory,
        Dictionary<string, (string itemPath, string sourceFile)> fileMappings)
    {
        int processedImages = 0;
        int currentIndex = 0;
        imageFiles.Reverse();
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var item in generatedItemsHolder.GeneratedItems)
            {
                if (currentIndex >= _fromIndex && currentIndex <= _toIndex && item != null)
                {
                    if (processedImages < imageFiles.Count)
                    {
                        var sourceFile = imageFiles[processedImages].FullName;
                        string fileName = $"item_icon_{currentIndex}.png";
                        string targetFile = Path.Combine(targetDirectory, fileName).Replace("\\", "/");
                        
                        if (File.Exists(targetFile))
                        {
                            File.Delete(targetFile);
                        }
                        
                        File.Copy(sourceFile, targetFile, true);
                        
                        string assetPath = targetFile;
                        if (assetPath.StartsWith(Application.dataPath))
                        {
                            assetPath = "Assets" + assetPath.Substring(Application.dataPath.Length);
                        }
                        
                        fileMappings[assetPath] = (AssetDatabase.GetAssetPath(item), sourceFile);
                        processedImages++;
                    }
                }
                currentIndex++;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh();
    }

    private async Task ChangeTextureToSprite(Dictionary<string, (string itemPath, string sourceFile)> fileMappings)
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var mapping in fileMappings)
            {
                string assetPath = mapping.Key;
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.maxTextureSize = 512;
                    importer.compressionQuality = 100;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = true;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.isReadable = false;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings
                    {
                        maxTextureSize = 512,
                        format = TextureImporterFormat.RGBA32,
                        textureCompression = TextureImporterCompression.Uncompressed,
                        overridden = true
                    };
                    importer.SetPlatformTextureSettings(settings);
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
                else
                {
                    Debug.LogError($"Failed to get TextureImporter for: {assetPath}");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
            AssetDatabase.StartAssetEditing();
            try
            {
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
            string fileName = $"IconPrompts_{_fromIndex}-{_toIndex}.txt";
            string filePath = Path.Combine(directory, fileName);
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                int currentIndex = 0;
                foreach (var item in generatedItemsHolder.GeneratedItems)
                {
                    if (currentIndex >= _fromIndex && currentIndex <= _toIndex && item != null)
                    {
                        if (!string.IsNullOrEmpty(item.IconPrompt))
                        {
                            string positivePrompt = item.IconPrompt;
                            string negativePrompt = "text, watermark, human face, human";
                            
                            if (!string.IsNullOrEmpty(generatedItemsHolder.GlobalPositivePromptSuffix))
                            {
                                positivePrompt += ", " + generatedItemsHolder.GlobalPositivePromptSuffix;
                            }
                            if (!string.IsNullOrEmpty(generatedItemsHolder.GlobalNegativePromptSuffix))
                            {
                                negativePrompt += ", " + generatedItemsHolder.GlobalNegativePromptSuffix;
                            }
                            writer.WriteLine($"positive: {positivePrompt}\n\nnegative: {negativePrompt}\n----");
                        }
                    }
                    currentIndex++;
                }
            }
            
            File.WriteAllText(filePath, File.ReadAllText(filePath).TrimEnd('\n'));
            AssetDatabase.Refresh();
        }
    }

    private void DeleteItemsRange(GeneratedItemsHolder generatedItemsHolder)
    {
        if (!Application.isPlaying)
        {
            generatedItemsHolder.DeleteItemsRange(_fromIndex, _toIndex);
            EditorUtility.SetDirty(generatedItemsHolder);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}