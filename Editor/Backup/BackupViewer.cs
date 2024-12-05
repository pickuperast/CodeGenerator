using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Sanat.CodeGenerator
{
    public class BackupViewer : EditorWindow
    {
        private Vector2 scrollPosition;
        private string selectedFilePath;
        private ScriptFileBackup[] backups;

        [MenuItem("Tools/Sanat/Backup Viewer")]
        public static void ShowWindow()
        {
            GetWindow<BackupViewer>("Script Backups");
        }

        private void OnEnable()
        {
            LoadBackups();
        }

        private void LoadBackups()
        {
            string metaFolder = "Assets/ScriptBackups/Meta";
            backups = AssetDatabase.FindAssets("t:ScriptFileBackup", new[] { metaFolder })
                .Select(guid => AssetDatabase.LoadAssetAtPath<ScriptFileBackup>(
                    AssetDatabase.GUIDToAssetPath(guid)))
                .OrderByDescending(b => b.dateTime)
                .ToArray();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var backup in backups)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"File: {Path.GetFileName(backup.filePath)}");
                EditorGUILayout.LabelField($"Date: {backup.dateTime}");
                EditorGUILayout.LabelField($"Version: {backup.version}");
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Restore", GUILayout.Width(100)))
                {
                    if (EditorUtility.DisplayDialog("Restore Backup",
                            $"Are you sure you want to restore {Path.GetFileName(backup.filePath)} to this version?",
                            "Yes", "No"))
                    {
                        RestoreBackup(backup);
                    }
                }

                if (GUILayout.Button("Compare", GUILayout.Width(100)))
                {
                    CompareWithCurrent(backup);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void RestoreBackup(ScriptFileBackup backup)
        {
            if (File.Exists(backup.filePath))
            {
                BackupManager.BackupScriptFile(backup.filePath, File.ReadAllText(backup.filePath));
                File.WriteAllText(backup.filePath, backup.content);
                AssetDatabase.Refresh();
                Debug.Log($"Restored backup for {backup.filePath}");
            }
        }

        private void CompareWithCurrent(ScriptFileBackup backup)
        {
            if (File.Exists(backup.filePath))
            {
                string currentContent = File.ReadAllText(backup.filePath);
                EditorGUIUtility.systemCopyBuffer = backup.content;
                Debug.Log($"Backup content copied to clipboard for comparison");
            }
        }
    }
}