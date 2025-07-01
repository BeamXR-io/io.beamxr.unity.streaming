using BeamXR.Streaming.Core;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor
{
    public class PrefabAddStreamingPrefab : MonoBehaviour
    {
        [MenuItem("BeamXR/Prefabs/Core/Add BeamXR", priority = 0)]
        private static void AddStreamingPrefab()
        {
            SpawnPrefab("BeamXR", FindFirstObjectByType<BeamManager>(FindObjectsInactive.Include), specificType: typeof(BeamManager));
        }

        public static GameObject GetParentObject()
        {
            GameObject go = GameObject.Find("BeamXR Objects");
            if (go == null)
            {
                go = new GameObject("BeamXR Objects");
            }
            return go;
        }

        public static void SpawnPrefab(string prefabName, Object existing, Transform parent = null, System.Type specificType = null)
        {
            // Search for the prefab by name.
            string[] guids = AssetDatabase.FindAssets("l:BeamXR a:packages " + prefabName);
            if (guids.Length == 0)
            {
                Debug.LogError($"{prefabName} prefab not found!");
                return;
            }

            // If there's no instance of the prefab in the scene, instantiate it.
            if (existing == null)
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null && (specificType == null || prefab.GetComponentInChildren(specificType) != null))
                    {
                        Transform spawnParent = null;
                        if (parent != null)
                        {
                            spawnParent = parent;
                        }
                        else if (Selection.activeGameObject != null && Selection.activeGameObject.scene != null && Selection.activeGameObject.scene.isLoaded && Selection.activeGameObject.scene.name != null)
                        {
                            spawnParent = Selection.activeGameObject.transform;
                        }
                        Object spawnedObject = PrefabUtility.InstantiatePrefab(prefab, spawnParent);
                        EditorGUIUtility.PingObject(spawnedObject);
                        break;
                    }
                }
            }
            else
            {
                BeamLogger.LogWarning($"There is already an instance of the {prefabName} in the scene.", existing);
            }
        }
    }
}
