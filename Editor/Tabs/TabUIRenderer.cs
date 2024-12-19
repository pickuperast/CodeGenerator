using UnityEngine;
using UnityEditor;

namespace Sanat.CodeGenerator.Editor
{
    public class TabUIRenderer
    {
        private const float TAB_HEIGHT = 25f;
        private const float NEW_TAB_BUTTON_WIDTH = 60f;
        private GUIStyle tabStyle;
        private GUIStyle selectedTabStyle;
        private GUIStyle closeButtonStyle;
        private CodeGenerator codeGenerator;
        private bool stylesInitialized;

        public TabUIRenderer(CodeGenerator codeGenerator)
        {
            this.codeGenerator = codeGenerator;
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;
            
            if (EditorStyles.toolbarButton == null) return;
            
            tabStyle = new GUIStyle(EditorStyles.toolbarButton);
            selectedTabStyle = new GUIStyle(EditorStyles.toolbarButton);
            selectedTabStyle.normal = selectedTabStyle.active;
            
            closeButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0)
            };
            
            stylesInitialized = true;
        }

        public void DrawTabs(TabManager tabManager, ref string newTabName)
        {
            if (!stylesInitialized)
            {
                InitializeStyles();
                if (!stylesInitialized) // If still not initialized, draw basic layout
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Loading...");
                    EditorGUILayout.EndHorizontal();
                    return;
                }
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (tabManager == null || tabManager.Tabs == null)
            {
                EditorGUILayout.EndHorizontal();
                return;
            }

            for (int i = 0; i < tabManager.Tabs.Count; i++)
            {
                var tab = tabManager.Tabs[i];
                bool isSelected = i == tabManager.CurrentTabIndex;
                
                if (DrawTab(tab, isSelected))
                {
                    tabManager.SelectTab(i);
                }
            }

            // New tab button and input field
            newTabName = EditorGUILayout.TextField(newTabName, GUILayout.Width(100));
            if (GUILayout.Button("+", GUILayout.Width(NEW_TAB_BUTTON_WIDTH)))
            {
                if (!string.IsNullOrEmpty(newTabName))
                {
                    tabManager.AddTab(newTabName);
                    newTabName = "New Tab";
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private bool DrawTab(TabData tab, bool isSelected)
        {
            if (!stylesInitialized) return false;
            
            bool clicked = false;
            
            EditorGUILayout.BeginHorizontal(GUILayout.Height(TAB_HEIGHT));
            
            if (GUILayout.Toggle(isSelected, tab.Name, isSelected ? selectedTabStyle : tabStyle))
            {
                if (!isSelected)
                {
                    clicked = true;
                }
            }

            if (GUILayout.Button("×", closeButtonStyle, GUILayout.Width(20)))
            {
                //codeGenerator.TabManager.CloseTab(codeGenerator.TabManager.Tabs.IndexOf(tab));
            }
            
            EditorGUILayout.EndHorizontal();
            
            return clicked;
        }
    }
}