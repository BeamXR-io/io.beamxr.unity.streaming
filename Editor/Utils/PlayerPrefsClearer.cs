using UnityEngine;
using UnityEditor;
using BeamXR.Streaming.Core.Auth.Credentials;
using BeamXR.Streaming.Core;
using BeamXR.Streaming.Core.Media;
public class PlayerPrefsClearer : MonoBehaviour
{
    [MenuItem("BeamXR/Tools/Reset Beam Login", false, 300)]
    private static void ClearPlayerPrefs()
    {
        PlayerPrefsCredentialsManager.ClearPrefs();
        BeamLogger.Log("Beam login details have been cleared");
    }

    [MenuItem("BeamXR/Tools/Reset Beam Camera Player Presets", false, 301)]
    private static void ClearPlayerPresets()
    {
        BeamCamera.ResetAllPresets();
        BeamLogger.Log("Player Beam Camera presets have been reset to your defaults");
    }
}
