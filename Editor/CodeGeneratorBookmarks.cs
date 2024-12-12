using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
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
        private ReorderableList bookmarkList;
        private Texture2D bookmarkIcon;
        CodeGenerator codeGenerator;
        private string _className;
        public string ClassName {
            get {
                if (string.IsNullOrEmpty(_className)) _className = $"<color=#56dd12>CodeGeneratorBookmarks</color>";
                return _className;
            }
        }

        public event Action OnBookmarkSaved;
        public event System.Action<Bookmark> OnBookmarkLoaded;

        public List<Bookmark> GetBookmarks()
        {
            return new List<Bookmark>(bookmarks);
        }

        public void DrawBookmarksUI(CodeGenerator codeGeneratorEditorWindow)
        {
            codeGenerator = codeGeneratorEditorWindow;
            if (!codeGenerator.IsSettingsLoaded)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            showBookmarks = EditorGUILayout.Foldout(showBookmarks, "Bookmarks", true);
            if (showBookmarks)
            {
                EditorGUILayout.Space();
                DrawBookmarkCreation();
                DrawBookmarkSearch();
                DrawBookmarkList();
            }
            EditorGUILayout.EndVertical();
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
            if (bookmarks == null || bookmarks.Count == 0)
            {
                EditorGUILayout.LabelField("No bookmarks saved.");
                return;
            }

            bookmarkScrollPosition = EditorGUILayout.BeginScrollView(bookmarkScrollPosition, GUILayout.Height(200));
            if (bookmarkList != null)
            {
                try
                {
                    bookmarkList.DoLayoutList();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{ClassName} Error in bookmark list: {e.Message}. Reinitializing...");
                    InitializeReorderableList();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        public void InitializeReorderableList()
        {
            if (bookmarks == null)
            {
                bookmarks = new List<Bookmark>();
            }

            if (bookmarks.Count == 0)
            {
                LoadBookmarksFromPrefs();
            }

            bookmarkList = new ReorderableList(bookmarks, typeof(Bookmark), true, true, false, false);
            bookmarkList.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Bookmarks");
            bookmarkList.drawElementCallback = DrawBookmarkElement;
            bookmarkList.onReorderCallback = (ReorderableList list) => SaveBookmarksToPrefs();
        }

        private void DrawBookmarkElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (index < 0 || index >= bookmarks.Count)
            {
                Debug.LogWarning($"Invalid bookmark index: {index}. Total bookmarks: {bookmarks.Count}");
                return;
            }

            var bookmark = bookmarks[index];
            if (bookmark == null)
            {
                Debug.LogWarning($"Null bookmark at index {index}");
                return;
            }

            if (!string.IsNullOrEmpty(bookmarkSearchQuery) &&
                !bookmark.Name.ToLower().Contains(bookmarkSearchQuery.ToLower()))
            {
                return;
            }

            float iconWidth = 20;
            float buttonWidth = 60;
            float spacing = 5;
            string tooltipSeparator = bookmark.SelectedClassNames.Count > 10 ? ", " : "\n";
            string tooltip = $"[{bookmark.SelectedClassNames.Count}]: ";
            tooltip += string.Join(tooltipSeparator, bookmark.SelectedClassNames);
            EditorGUI.LabelField(
                new Rect(rect.x + iconWidth + spacing, rect.y, rect.width - iconWidth - buttonWidth * 3 - spacing * 4, rect.height),
                new GUIContent(bookmark.Name, tooltip)
            );

            if (GUI.Button(new Rect(rect.xMax - buttonWidth * 3 - spacing * 2, rect.y, buttonWidth, rect.height), "Add"))
            {
                AddBookmark(bookmark);
            }

            if (GUI.Button(new Rect(rect.xMax - buttonWidth * 2 - spacing, rect.y, buttonWidth, rect.height), "Load"))
            {
                LoadBookmark(bookmark);
            }

            if (GUI.Button(new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, rect.height), "Delete"))
            {
                if (EditorUtility.DisplayDialog("Confirm Deletion",
                    $"Are you sure you want to delete the bookmark '{bookmark.Name}'?", "Yes", "No"))
                {
                    DeleteBookmark(bookmark);
                    InitializeReorderableList(); // Reinitialize the list after deletion
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

            bookmark.Task = codeGenerator.taskInput;

            bookmarks.Add(bookmark);
            SaveBookmarksToPrefs();

            Debug.Log($"{ClassName} New bookmark saved: {bookmark.Name}. "+
                      $"Selected Classes ({bookmark.SelectedClassNames.Count}): "+
                      $"{string.Join(", ", bookmark.SelectedClassNames)}; "+
                      $"Task: {bookmark.Task}; Category: {bookmark.Category}");

            OnBookmarkSaved?.Invoke();

            InitializeReorderableList();

            if (codeGenerator != null)
            {
                codeGenerator.Repaint();
            }
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

        public void AddBookmark(Bookmark bookmark)
        {
            if (bookmark != null)
            {
                foreach (var bkm in bookmarks)
                {
                    if (bkm.Name == bookmark.Name)
                    {
                        var selectedClasses = codeGenerator.selectedClassNames;
                        var oldAmount = selectedClasses.Count;
                        var oldClasses = selectedClasses;
                        selectedClasses.AddRange(bkm.SelectedClassNames);
                        codeGenerator.selectedClassNames = new List<string>(selectedClasses.Distinct());
                        selectedClasses = codeGenerator.selectedClassNames;
                        var newAmount = selectedClasses.Count;
                        var addedClasses = selectedClasses.Except(oldClasses).ToList();
                        Debug.Log($"{ClassName} Added {newAmount-oldAmount} classes to selection: {string.Join(", ", addedClasses)}");
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