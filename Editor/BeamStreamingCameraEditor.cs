using UnityEditor;
using UnityEngine;
using BeamXR.Streaming.Core.Media;

namespace BeamXR.Streaming.Editor
{
    [CustomEditor(typeof(BeamStreamingCamera))]
    public class BeamStreamingCameraEditor : UnityEditor.Editor
    {
        private BeamCameraController _controller = null;

        public override void OnInspectorGUI()
        {
            if(_controller == null)
            {
                _controller = FindFirstObjectByType<BeamCameraController>(FindObjectsInactive.Include);
            }

            if(_controller == null)
            {
                EditorGUILayout.HelpBox("Add the BeamCameraController to your scene for more complex camera control.", MessageType.Info);
            }

            DrawDefaultInspector();
        }

    }
}
