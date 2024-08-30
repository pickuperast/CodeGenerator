using System.Collections.Generic;
using Sanat.CodeGenerator;

public class ToolGetWholeProjectStructure
{
    public static List<string> GetWholeProjectStructure()
    {
        List<string> projectStructure = CodeGenerator.IncludedFoldersManager.GetEnabledFolders();
        return projectStructure;
    }
}
