using UnityEngine;
using System;

namespace Sanat.CodeGenerator
{
    [CreateAssetMenu(fileName = "ScriptFileBackup", menuName = "CodeGenerator/ScriptFileBackup")]
    public class ScriptFileBackup : ScriptableObject
    {
        public string filePath;
        public string dateTime;
        public string content;
        public string version;
        public string comment;
        public string modifiedBy;
        public string originalHash;
        public string backupType;
    }
}