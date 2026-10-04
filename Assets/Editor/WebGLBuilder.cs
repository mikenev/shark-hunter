using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Builds the game for the web into /docs so GitHub Pages can serve it from main.
public static class WebGLBuilder
{
    const string OutputDir = "docs";

    [MenuItem("Shark Hunter/Build WebGL")]
    public static void Build()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("Shark Hunter", "No scenes in Build Settings. Run Shark Hunter > Build Scenes first.", "OK");
            return;
        }

        PlayerSettings.productName = "Shark Hunter";

        // GitHub Pages can't send the Content-Encoding headers Unity's .br/.gz files need,
        // so ship the build uncompressed.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;

        // Wipe the previous build so stale hashed files don't pile up.
        if (Directory.Exists(OutputDir)) Directory.Delete(OutputDir, true);

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputDir,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"WebGL build failed: {report.summary.result} ({report.summary.totalErrors} errors)");
            return;
        }

        // Stop Jekyll from touching the output.
        File.WriteAllText(Path.Combine(OutputDir, ".nojekyll"), "");

        Debug.Log($"WebGL build complete: {OutputDir}/ ({report.summary.totalSize / (1024 * 1024)} MB)");
    }
}
