using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sanat.CodeGenerator.Editor
{
    [Serializable]
    public class TabData
    {
        public string Name;
        public List<string> SelectedClassNames = new();
        public string TaskInput = "";
        public string GeneratedPrompt = "";
        public bool IsGeneratingCode;
        public float GenerationProgress;
        public Vector2 ScrollPosition;
        public Vector2 TaskScrollPosition;
        public string ClassNameInput = "";
        public bool IsSelectedClassesVisible = true;
        public bool IsGeneratedPromptVisible = true;
        public string Id;

        public TabData(string name)
        {
            Name = name;
            Id = Guid.NewGuid().ToString();
        }
    }
}