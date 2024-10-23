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
            // Load the prefab.
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BeamXR.Streaming/Runtime/Prefabs/BeamXR-Streaming.prefab");

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
            // Load the prefab.
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BeamXR.Streaming/Runtime/Prefabs/BeamXR Interaction Panel.prefab");

            // Check to see if there's already an instance of the prefab in the scene.
            GameObject existingPrefab = GameObject.Find("BeamXR Interaction Panel");

            // If there's no instance of the prefab in the scene, instantiate it.
            if (existingPrefab == null)
            {
                GameObject newPrefab = Instantiate(prefab);
                newPrefab.name = "BeamXR Interaction Panel";
                existingPrefab = GameObject.Find("BeamXR Interaction Panel");
                existingPrefab.transform.transform.position = new Vector3(0, 1.2f, 0.5f);
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