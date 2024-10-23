using BeamXR.Streaming.Core;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor
{
    public class PrefabAddStreamingPrefab : MonoBehaviour
    {
        [MenuItem("BeamXR/Prefabs/Core/Add Streaming Prefab")]
        private static void AddStreamingPrefab()
        {
            // Search for the prefab by name.
            string[] guids = AssetDatabase.FindAssets("BeamXR-Streaming");
            if (guids.Length == 0)
            {
                Debug.LogError("BeamXR-Streaming prefab not found!");
                return;
            }

            // Load the first matching asset.
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            BeamStreamingManager existingManager = GameObject.FindAnyObjectByType<BeamStreamingManager>();

            // If there's no instance of the prefab in the scene, instantiate it.
            if (existingManager == null)
            {
                GameObject newPrefab = Instantiate(prefab);
                newPrefab.name = "BeamXR-Streaming";
                existingManager = GameObject.FindObjectOfType<BeamStreamingManager>();
                existingManager.GetComponent<BeamStreamingManager>()._targetResolution = Core.Media.StreamResolution.Resolution720p;
            }
            else
            {
                Debug.LogWarning("There is already an instance of the BeamStreamingManager in the scene.");
            }
        }

        [MenuItem("BeamXR/Prefabs/UI/Add Interaction Panel Prefab")]
        private static void AddInteractionPanelPrefab()
        {
            // Search for the prefab by name.
            string[] guids = AssetDatabase.FindAssets("BeamXR Interaction Panel");
            if (guids.Length == 0)
            {
                Debug.LogError("BeamXR Interaction Panel prefab not found!");
                return;
            }

            // Load the first matching asset.
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // Check to see if there's already an instance of the prefab in the scene.
            GameObject existingPrefab = GameObject.Find("BeamXR Interaction Panel");

            // If there's no instance of the prefab in the scene, instantiate it.
            if (existingPrefab == null)
            {
                GameObject newPrefab = Instantiate(prefab);
                newPrefab.name = "BeamXR Interaction Panel";
                existingPrefab = GameObject.Find("BeamXR Interaction Panel");
                existingPrefab.transform.position = new Vector3(0, 1.2f, 0.5f);
            }
            else
            {
                Debug.LogWarning("There is already an instance of the BeamXR Interaction Panel prefab in the scene.");
            }

            // Get the beam streaming manager.
            BeamStreamingManager beamStreamingManager = GameObject.FindAnyObjectByType<BeamStreamingManager>();

            // If the beam streaming manager is found, set the interaction panel as the _deviceFlowInstructionsManager property (private serializefield).
            if (beamStreamingManager != null)
            {
                var behaviour = existingPrefab.GetComponent<Gui.BeamInteractionPanel>();
                beamStreamingManager._deviceFlowInstructionsManager.Manager = behaviour;
                beamStreamingManager._streamStateDisplayManager.Manager = behaviour;
            }
        }
    }
}
