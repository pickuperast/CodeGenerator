// Copyright (c) Sanat. All rights reserved.
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Sanat.CodeGenerator
{
    public class IncludedFoldersManager
    {
        private const string INCLUDED_FOLDERS_PREFS_KEY = "IncludedFolders";
        private List<IncludedFolder> includedFolders = new ();

        [System.Serializable]
        private class IncludedFolder
        {
            public string path;
            public bool isEnabled;
        }

        public IncludedFoldersManager()
        {
            LoadIncludedFolders();
        }

        public void DrawIncludedFoldersUI()
        {
            EditorGUILayout.LabelField("Included Folders:", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Included Folder"))
            {
                includedFolders.Add(new IncludedFolder { path = "", isEnabled = true });
            }

            for (int i = 0; i < includedFolders.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                includedFolders[i].path = EditorGUILayout.TextField("Folder Path (Ex.: Assets\\Test)", includedFolders[i].path, GUILayout.ExpandWidth(true));
                includedFolders[i].isEnabled = EditorGUILayout.Toggle(includedFolders[i].isEnabled, GUILayout.Width(20));
                if (GUILayout.Button("x", GUILayout.Width(20)))
                {
                    includedFolders.RemoveAt(i);
                }
                EditorGUILayout.EndHorizontal();
            }

            SaveIncludedFolders();
        }

        public List<string> GetEnabledFolders()
        {
            return includedFolders.Where(folder => folder.isEnabled).Select(folder => folder.path).ToList();
        }

        private void LoadIncludedFolders()
        {
            string json = PlayerPrefs.GetString(INCLUDED_FOLDERS_PREFS_KEY, "[]");
            
            IncludedFoldersList list = JsonUtility.FromJson<IncludedFoldersList>(json);
            includedFolders = list?.folders ?? new List<IncludedFolder>();
            
        }

        private void SaveIncludedFolders()
        {
            IncludedFoldersList list = new IncludedFoldersList { folders = includedFolders };
            string json = JsonUtility.ToJson(list);
            PlayerPrefs.SetString(INCLUDED_FOLDERS_PREFS_KEY, json);
            PlayerPrefs.Save();
        }

        [System.Serializable]
        private class IncludedFoldersList
        {
            public List<IncludedFolder> folders;
        }
    }
}