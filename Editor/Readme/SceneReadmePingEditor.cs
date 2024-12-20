using UnityEditor;
using BeamXR.Streaming.Utils;

namespace BeamXR.Streaming.Editor
{
    [CustomEditor(typeof(SceneReadmePing))]
    public class SceneReadmePingEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (!SceneReadmeEditor.SelectSceneReadme(true))
            {
                EditorGUILayout.HelpBox("No Readme currently exists for this scene.", MessageType.Warning);
            }
        }
    }
}