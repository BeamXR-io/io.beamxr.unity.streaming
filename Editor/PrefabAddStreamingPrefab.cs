using BeamXR.Streaming.Core;
using BeamXR.Streaming.Core.Media;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor
{
    public class PrefabAddStreamingPrefab : MonoBehaviour
    {
        [MenuItem("BeamXR/Prefabs/Core/Add Streaming Prefab", priority = 0)]
        private static void AddStreamingPrefab()
        {
            SpawnPrefab("BeamXR-Streaming", FindFirstObjectByType<BeamStreamingManager>(FindObjectsInactive.Include));
        }

        [MenuItem("BeamXR/Prefabs/Core/Add Camera Controller", priority = 1)]
        private static void AddCameraControllerPrefab()
        {
            SpawnPrefab("BeamXR Camera Controller", FindFirstObjectByType<BeamCameraController>(FindObjectsInactive.Include), GetParentObject().transform);
        }

        [MenuItem("BeamXR/Prefabs/UI/Add Interaction Panel Prefab", priority = 1)]
        private static void AddInteractionPanelPrefab()
        {
            SpawnPrefab("BeamXR Interaction Panel", GameObject.Find("BeamXR Interaction Panel"));
        }

        public static GameObject GetParentObject()
        {
            GameObject go = GameObject.Find("BeamXR Objects");
            if(go == null)
            {
                go = new GameObject("BeamXR Objects");
            }
            return go;
        }

        public static void SpawnPrefab(string prefabName, Object existing, Transform parent = null)
        {
            // Search for the prefab by name.
            string[] guids = AssetDatabase.FindAssets(prefabName);
            if (guids.Length == 0)
            {
                Debug.LogError($"{prefabName} prefab not found!");
                return;
            }

            // If there's no instance of the prefab in the scene, instantiate it.
            if (existing == null)
            {
                // Load the first matching asset.
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                Transform spawnParent = null;
                if(parent != null)
                {
                    spawnParent = parent;
                }
                else if(Selection.activeGameObject != null && Selection.activeGameObject.scene != null)
                {
                    spawnParent = Selection.activeGameObject.transform;
                }
                PrefabUtility.InstantiatePrefab(prefab, spawnParent);
            }
            else
            {
                Debug.LogWarning($"There is already an instance of the {prefabName} in the scene.");
            }
        }
    }
}
