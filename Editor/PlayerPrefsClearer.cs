using UnityEngine;
using UnityEditor;
public class PlayerPrefsClearer : MonoBehaviour
{
    [MenuItem("BeamXR/Tools/Clear PlayerPrefs")]
    private static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs cleared via menu.");
    }
}
