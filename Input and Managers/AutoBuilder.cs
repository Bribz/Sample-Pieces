using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public static class AutoBuilder
{
    static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        return s[s.Length - 2];
    }

    public static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Where(p => p.enabled).Count()];
        //scenes[0] = "C:\ProjectName\Assets\Scenes\TitleScene.unity";
        //return scenes;

        int iterator = 0;
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            if (!EditorBuildSettings.scenes[i].enabled)
                continue;

            scenes[iterator] = EditorBuildSettings.scenes[i].path;
            iterator++;
        }

        return scenes;
    }

    [MenuItem("Build/Debug/Client", false, 1)]
    public static void BuildClient_Debug()
    {
        AssignScriptDefines.AssignDebug();
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        BuildPipeline.BuildPlayer(GetScenePaths(),
            "../Builds/Debug_" + DateTime.Now.ToString("MM-dd-yyyy_H-mm") + "/Debug_" + DateTime.Now.ToString("MM-dd-yyyy_H-mm") + ".exe",
            BuildTarget.StandaloneWindows64, BuildOptions.Development | BuildOptions.AllowDebugging);
    }

    [MenuItem("Build/Debug/Server", false, 2)]
    public static void BuildServer_Debug()
    {
        AssignScriptDefines.AssignServerDebugBuild();
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        BuildPipeline.BuildPlayer(GetScenePaths(),
            "../Builds/ServerDebug_" + DateTime.Now.ToString("MM-dd-yyyy_H-mm") + "/ServerDebug_" + DateTime.Now.ToString("MM-dd-yyyy_H-mm") + ".exe",
            BuildTarget.StandaloneWindows64, BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.EnableHeadlessMode);
    }
}