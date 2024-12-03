using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;

namespace Sanat.CodeGenerator.Bookmarks
{
    public class CodeGeneratorBookmarks
    {
        private List<Bookmark> bookmarks = new List<Bookmark>();
        private string newBookmarkName = "";
        private string bookmarkSearchQuery = "";
        private Vector2 bookmarkScrollPosition;
        private bool showBookmarks = false;
        private ReorderableList bookmarkList = new ReorderableList(new List<Bookmark>(), typeof(Bookmark), true, true, false, false);
        private Texture2D bookmarkIcon;
        CodeGenerator codeGenerator;
        public event Action OnBookmarkSaved;

        public event System.Action<Bookmark> OnBookmarkLoaded;
        
        public List<Bookmark> GetBookmarks()
        {
            return new List<Bookmark>(bookmarks);
        }
        
        public void DrawBookmarksUI(CodeGenerator codeGeneratorEdotorWindow)
        {
            codeGenerator = codeGeneratorEdotorWindow;
            if (!codeGenerator.IsSettingsLoaded)
                return;
            
            showBookmarks = EditorGUILayout.Foldout(showBookmarks, "Bookmarks", true);
            if (showBookmarks)
            {
                EditorGUILayout.Space();
                DrawBookmarkCreation();
                DrawBookmarkSearch();
                DrawBookmarkList();
            }
        }

        private void DrawBookmarkCreation()
        {
            EditorGUILayout.BeginHorizontal();
            newBookmarkName = EditorGUILayout.TextField("New Bookmark Name", newBookmarkName);
            string[] categories = { "General", "UI", "Gameplay", "Audio" };
            int selectedCategory = EditorGUILayout.Popup("Category", 0, categories);
            if (GUILayout.Button("Save Bookmark", GUILayout.Width(120)))
            {
                SaveBookmark(new Bookmark(newBookmarkName, codeGenerator.selectedClassNames, selectedCategory, ""));
                newBookmarkName = "";
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBookmarkSearch()
        {
            bookmarkSearchQuery = EditorGUILayout.TextField("Search Bookmarks", bookmarkSearchQuery);
        }

        private void DrawBookmarkList()
        {
            if (bookmarks.Count == 0)
            {
                EditorGUILayout.LabelField("No bookmarks saved.");
                return;
            }
            bookmarkScrollPosition = EditorGUILayout.BeginScrollView(bookmarkScrollPosition, GUILayout.Height(200));
            bookmarkList.DoLayoutList();
            EditorGUILayout.EndScrollView();
        }

        public void InitializeReorderableList()
        {
            if (bookmarks.Count == 0)
            {
                LoadBookmarksFromPrefs();
                return;
            }
            bookmarkList = new ReorderableList(bookmarks, typeof(Bookmark), true, true, false, false);
            bookmarkList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Bookmarks");
            bookmarkList.drawElementCallback = DrawBookmarkElement;
            bookmarkList.onReorderCallback = (ReorderableList list) => SaveBookmarksToPrefs();
        }

        private void DrawBookmarkElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var bookmark = bookmarks[index];
            if (!string.IsNullOrEmpty(bookmarkSearchQuery) && 
                !bookmark.Name.ToLower().Contains(bookmarkSearchQuery.ToLower()))
            {
                return;
            }

            float iconWidth = 20;
            float buttonWidth = 60;
            float spacing = 5;

            // Draw icon
            //EditorGUI.DrawTextureTransparent(new Rect(rect.x, rect.y, iconWidth, rect.height), bookmarkIcon);

            // Draw name
            EditorGUI.LabelField(new Rect(rect.x + iconWidth + spacing, rect.y, rect.width - iconWidth - buttonWidth * 2 - spacing * 3, rect.height), 
                new GUIContent(bookmark.Name, bookmark.Task));

            // Draw load button
            if (GUI.Button(new Rect(rect.xMax - buttonWidth * 2 - spacing, rect.y, buttonWidth, rect.height), "Load"))
            {
                LoadBookmark(bookmark);
            }

            // Draw delete button
            if (GUI.Button(new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height), "Delete"))
            {
                if (EditorUtility.DisplayDialog("Confirm Deletion", 
                    $"Are you sure you want to delete the bookmark '{bookmark.Name}'?", "Yes", "No"))
                {
                    DeleteBookmark(bookmark);
                }
            }
        }

        public void SaveBookmark(Bookmark bookmark)
        {
            if (string.IsNullOrEmpty(bookmark.Name))
            {
                Debug.LogWarning("Bookmark name cannot be empty.");
                return;
            }

            bookmarks.Add(bookmark);
            SaveBookmarksToPrefs();
        }

        public void LoadBookmark(Bookmark bookmark)
        {
            if (bookmark != null)
            {
                foreach (var bkm in bookmarks)
                {
                    if (bkm.Name == bookmark.Name)
                    {
                        OnBookmarkLoaded?.Invoke(bkm);
                        break;
                    }
                }
            }
        }

        public void DeleteBookmark(Bookmark bookmark)
        {
            bookmarks.Remove(bookmark);
            SaveBookmarksToPrefs();
        }

        public void SaveBookmarksToPrefs()
        {
            string json = JsonUtility.ToJson(new SerializableBookmarkList { bookmarks = bookmarks });
            EditorPrefs.SetString("CodeGeneratorBookmarks", json);
        }

        public List<Bookmark> LoadBookmarksFromPrefs()
        {
            string json = EditorPrefs.GetString("CodeGeneratorBookmarks", "");
            if (!string.IsNullOrEmpty(json))
            {
                SerializableBookmarkList loadedBookmarks = JsonUtility.FromJson<SerializableBookmarkList>(json);
                bookmarks = loadedBookmarks.bookmarks;
                return bookmarks;
            }

            return new List<Bookmark>();
        }

        public void ExportBookmarks()
        {
            string json = JsonUtility.ToJson(new SerializableBookmarkList { bookmarks = bookmarks });
            string path = EditorUtility.SaveFilePanel("Export Bookmarks", "", "bookmarks.json", "json");
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, json);
            }
        }

        public void ImportBookmarks()
        {
            string path = EditorUtility.OpenFilePanel("Import Bookmarks", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                string json = System.IO.File.ReadAllText(path);
                SerializableBookmarkList importedBookmarks = JsonUtility.FromJson<SerializableBookmarkList>(json);
                bookmarks.AddRange(importedBookmarks.bookmarks);
                SaveBookmarksToPrefs();
            }
        }

        [System.Serializable]
        private class SerializableBookmarkList
        {
            public List<Bookmark> bookmarks;
        }

        [System.Serializable]
        public class Bookmark
        {
            public string Name;
            public List<string> SelectedClassNames;
            public string Task;
            public int Category;

            public Bookmark(string name, List<string> selectedClassNames, int category, string task)
            {
                this.Name = name;
                this.SelectedClassNames = selectedClassNames;
                this.Task = task;
                this.Category = category;
            }
        }
    }
}