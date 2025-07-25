using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using BeamXR.Streaming.Core;
using BeamXR.Streaming.Core.Settings;

namespace BeamXR.Streaming.Editor
{
    public class BeamBuildUtils : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report)
        {
#if UNITY_2022_3_OR_NEWER
            if (report.summary.platform == BuildTarget.Android && PlayerSettings.graphicsJobs && PlayerSettings.graphicsJobMode != GraphicsJobMode.Legacy)
            {
                BeamLogger.LogWarning("Graphics Jobs mode must be set to Legacy, otherwise streaming will cause crashes on Android. This has been automatically changed.");
                PlayerSettings.graphicsJobMode = GraphicsJobMode.Legacy;
            }
#else
            if (report.summary.platform == BuildTarget.Android && PlayerSettings.graphicsJobs)
            {
                BeamLogger.LogWarning("Graphics Jobs cannot be used in Unity versions pre 2022.3.35f1 when targetting Android. Streaming will cause crashes. Graphics Jobs have been disabled.");
                PlayerSettings.graphicsJobs = false;
            }
#endif
            bool isDevelopmentBuild = (report.summary.options & BuildOptions.Development) != 0;

            if (!isDevelopmentBuild)
            {
                var beamSettings = BeamSettingsProvider.GetSerializedSettings();
                var devLogs = beamSettings.FindProperty("_developerLogs");
                if (devLogs.boolValue)
                {
                    BeamLogger.LogWarning("Developer logs are not allowed during a release build and will be automatically disabled.");
                }
            }
        }
    }
}