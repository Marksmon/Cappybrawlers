#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Capybrawlers.Editor
{
    public static class BuildPipeline
    {
        private static readonly string[] Scenes =
        {
            "Assets/_Game/Scenes/Bootstrap.unity",
            "Assets/_Game/Scenes/MainMenu.unity",
            "Assets/_Game/Scenes/Battle.unity",
            "Assets/_Game/Scenes/Collection.unity",
        };

        public static void BuildAndroid()
        {
            ApplyAndroidKeystore();
            var report = UnityEditor.BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes               = Scenes,
                locationPathName     = "Builds/Capybrawlers.aab",
                target               = BuildTarget.Android,
                options              = BuildOptions.None,
            });
            ThrowOnFailure(report, "Android");
        }

        public static void BuildIOS()
        {
            var report = UnityEditor.BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes               = Scenes,
                locationPathName     = "Builds/Capybrawlers_iOS",
                target               = BuildTarget.iOS,
                options              = BuildOptions.None,
            });
            ThrowOnFailure(report, "iOS");
        }

        private static void ThrowOnFailure(BuildReport report, string platform)
        {
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception($"{platform} build failed: {report.summary.result}");
            Debug.Log($"{platform} build succeeded in {report.summary.totalTime.TotalSeconds:F1}s");
        }

        private static void ApplyAndroidKeystore()
        {
            var path = Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_PATH");
            if (string.IsNullOrEmpty(path)) return;

            PlayerSettings.Android.keystoreName = path;
            PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_PASS") ?? "";
            PlayerSettings.Android.keyaliasName = Environment.GetEnvironmentVariable("ANDROID_KEY_ALIAS") ?? "";
            PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("ANDROID_KEY_PASS") ?? "";
        }
    }
}
#endif
