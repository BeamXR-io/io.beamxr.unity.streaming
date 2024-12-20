using UnityEngine;
using UnityEditor;
public class PlayerPrefsClearer : MonoBehaviour
{
    [MenuItem("BeamXR/Tools/Clear PlayerPrefs", false, 10)]
    private static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs cleared via menu.");
    }
}
