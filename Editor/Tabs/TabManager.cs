using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Sanat.CodeGenerator.Editor
{
    public class TabManager
    {
        public List<TabData> Tabs { get; private set; } = new() { new TabData("Main") };
        public int CurrentTabIndex { get; private set; }
        public TabData CurrentTab => Tabs.Count > 0 ? Tabs[CurrentTabIndex] : null;
        public event Action<TabData> OnTabChanged;
        public event Action<TabData> OnTabClosed;
        public event Action<TabData> OnTabAdded;

        public TabManager()
        {
            LoadTabs();
            if (Tabs.Count == 0)
            {
                AddTab("Main");
            }
        }
        public void AddTab(string name)
        {
            if (CurrentTab != null)
            {
                SaveCurrentTabState();
            }
            var tab = new TabData(name);
            Tabs.Add(tab);
            CurrentTabIndex = Tabs.Count - 1;
            SaveTabs();
            OnTabAdded?.Invoke(tab);
            OnTabChanged?.Invoke(CurrentTab);
        }
        public void CloseTab(int index)
        {
            if (index < 0 || index >= Tabs.Count) return;
            var closedTab = Tabs[index];
            Tabs.RemoveAt(index);
            if (Tabs.Count == 0)
            {
                AddTab("Main");
            }
            CurrentTabIndex = Mathf.Min(CurrentTabIndex, Tabs.Count - 1);
            SaveTabs();
            OnTabClosed?.Invoke(closedTab);
            OnTabChanged?.Invoke(CurrentTab);
        }
        public void SelectTab(int index)
        {
            if (index < 0 || index >= Tabs.Count) return;
            if (CurrentTab != null)
            {
                SaveCurrentTabState();
            }
            CurrentTabIndex = index;
            OnTabChanged?.Invoke(CurrentTab);
        }
        private void SaveCurrentTabState()
        {
            SaveTabs();
        }
        private void SaveTabs()
        {
            string json = JsonUtility.ToJson(new SerializableTabList { tabs = Tabs });
            EditorPrefs.SetString("CodeGeneratorTabs", json);
        }
        private void LoadTabs()
        {
            string json = EditorPrefs.GetString("CodeGeneratorTabs", "");
            if (!string.IsNullOrEmpty(json))
            {
                SerializableTabList loadedTabs = JsonUtility.FromJson<SerializableTabList>(json);
                Tabs = loadedTabs.tabs;
                if (Tabs.Count == 0)
                {
                    Tabs = new List<TabData>() { new TabData("Main") };
                }
            }
        }
        [Serializable]
        private class SerializableTabList
        {
            public List<TabData> tabs = new();
        }
    }
}