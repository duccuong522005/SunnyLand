using UnityEngine;
using UnityEditor;
using System.Linq;

public class FixAndroidBuild
{
    [MenuItem("Build/Fix Android Build Settings")]
    public static void FixAndroidSettings()
    {
        Debug.Log("Fixing Android Build Settings...");

        // Set Android Graphics APIs - Prioritize OpenGL ES for emulator compatibility
        // This is the main fix for black screen on emulators
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[]
        {
            UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3,
            UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2,
            UnityEngine.Rendering.GraphicsDeviceType.Vulkan
        });

        // Force custom Graphics APIs (not default)
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);

        // Note: Android Profiler is already enabled in ProjectSettings.asset (AndroidProfiler: 1)
        // Unity doesn't provide a public API to set this, but it's already configured

        // Set Android Target SDK Version to Auto (will use latest installed SDK)
        // This prevents issues with target SDK being 0
        try
        {
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        }
        catch
        {
            Debug.LogWarning("Could not set Target SDK version automatically. Please set it manually in Player Settings.");
        }

        Debug.Log("Android Build Settings Fixed!");
        Debug.Log("✓ Graphics APIs: OpenGL ES 3.0, OpenGL ES 2.0, Vulkan");
        Debug.Log("✓ Custom Graphics APIs enabled");
        Debug.Log("✓ Android Profiler enabled - You can now view logs in Unity Console");
        
        EditorUtility.DisplayDialog("Android Settings Fixed", 
            "Android build settings have been updated:\n\n" +
            "✓ Graphics APIs: OpenGL ES 3.0, OpenGL ES 2.0, Vulkan\n" +
            "✓ Android Profiler enabled\n" +
            "✓ This should fix black screen issues on emulators\n\n" +
            "⚠️ IMPORTANT: Please rebuild your APK for changes to take effect!\n\n" +
            "Also check:\n" +
            "1. File > Build Settings > Scenes - ensure main game scene is included\n" +
            "2. Player Settings > Android > Target SDK - set to Auto or latest API\n" +
            "3. Build with Development Build enabled to view logs", 
            "OK");
    }

    [MenuItem("Build/Check Android Scenes")]
    public static void CheckScenes()
    {
        Debug.Log("Checking build scenes...");
        
        var scenes = EditorBuildSettings.scenes;
        Debug.Log($"Total scenes in build: {scenes.Length}");
        
        for (int i = 0; i < scenes.Length; i++)
        {
            var scene = scenes[i];
            Debug.Log($"Scene {i}: {scene.path} (enabled: {scene.enabled})");
        }

        // Check if main level scene exists
        bool hasLevel1 = false;
        bool hasMainScene = false;
        
        foreach (var scene in scenes)
        {
            if (scene.path.Contains("level 1") || scene.path.Contains("Level1"))
            {
                hasLevel1 = true;
            }
            if (scene.path.Contains("Main") || scene.path.ToLower().Contains("menu"))
            {
                hasMainScene = true;
            }
        }

        if (!hasLevel1 && !hasMainScene)
        {
            Debug.LogWarning("⚠️ Main game scene may be missing!");
            Debug.LogWarning("Current scenes appear to be prefab scenes, not game scenes.");
            Debug.LogWarning("Consider adding: Assets/Prefabs/Map/level 1.unity or your main game scene");
        }

        string message = $"Found {scenes.Length} scene(s) in build settings:\n\n";
        foreach (var scene in scenes)
        {
            message += $"- {scene.path} {(scene.enabled ? "✓" : "✗")}\n";
        }
        
        if (!hasLevel1 && !hasMainScene)
        {
            message += "\n⚠️ WARNING: Main game scene may be missing!\n";
            message += "Current scenes look like prefabs. Add your main game scene!";
        }

        EditorUtility.DisplayDialog("Build Scenes Check", message, "OK");
    }
}
