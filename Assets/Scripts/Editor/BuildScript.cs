using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Linq;

public class BuildScript
{
    private static string BuildPathPC = "Builds/PC";
    private static string BuildPathAndroid = "Builds/Android";

    [MenuItem("Build/Build Android APK")]
    public static void BuildAndroid()
    {
        // Build Android APK with Development Build enabled for viewing logs
        EditorUserBuildSettings.buildAppBundle = false;
        BuildOptions devOptions = BuildOptions.Development | 
                                 BuildOptions.AllowDebugging | 
                                 BuildOptions.ConnectWithProfiler;
        
        Debug.Log("Building Android APK with Development Build enabled...");
        Debug.Log("✓ Android Profiler is enabled (set in Project Settings)");
        Debug.Log("✓ Script Debugging enabled");
        Debug.Log("✓ Autoconnect Profiler enabled");
        Debug.Log("You can now view logs in Unity Console after installing this APK");
        
        BuildPlayer(BuildPathAndroid, BuildTarget.Android, devOptions);
    }

    [MenuItem("Build/Build PC (Windows)")]
    public static void BuildPC()
    {
        BuildPlayer(BuildPathPC, BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

    [MenuItem("Build/Build Android App Bundle")]
    public static void BuildAAB()
    {
        EditorUserBuildSettings.buildAppBundle = true;
        string aabPath = BuildPathAndroid + "/SunnyLand.aab";
        BuildPlayer(aabPath, BuildTarget.Android, BuildOptions.None);
        EditorUserBuildSettings.buildAppBundle = false;
    }

    private static void BuildPlayer(string buildPath, BuildTarget buildTarget, BuildOptions buildOptions)
    {
        // Ensure output directory exists
        string directory = Path.GetDirectoryName(buildPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Get all enabled scenes from build settings
        string[] scenes = GetEnabledScenes();

        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes found in build settings! Please add scenes in File > Build Settings");
            return;
        }

        // Set output file name and path
        string outputPath;
        if (buildTarget == BuildTarget.Android)
        {
            // Check if building AAB or APK based on path
            if (buildPath.EndsWith(".aab"))
            {
                outputPath = buildPath;
            }
            else
            {
                outputPath = Path.Combine(buildPath, "SunnyLand.apk");
            }
        }
        else
        {
            outputPath = Path.Combine(buildPath, "SunnyLand.exe");
        }

        // Build player
        Debug.Log($"Building for {buildTarget}...");
        Debug.Log($"Output path: {outputPath}");
        Debug.Log($"Scenes: {string.Join(", ", scenes)}");

        BuildReport report = BuildPipeline.BuildPlayer(scenes, outputPath, buildTarget, buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.outputPath}");
            Debug.Log($"Build size: {summary.totalSize / (1024 * 1024)} MB");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed!");
            EditorApplication.Exit(1);
        }
    }

    private static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
    }

    // Command line build methods (called with -executeMethod flag)
    public static void BuildPCCommandLine()
    {
        BuildPC();
        EditorApplication.Exit(0);
    }

    public static void BuildAndroidCommandLine()
    {
        BuildAndroid();
        EditorApplication.Exit(0);
    }

    public static void BuildAABCommandLine()
    {
        BuildAAB();
        EditorApplication.Exit(0);
    }
}

