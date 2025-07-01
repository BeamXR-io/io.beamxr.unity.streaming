using UnityEngine;
using UnityEditor;
using BeamXR.Streaming.Core.Settings;

namespace BeamXR.Streaming.Editor
{
    [CustomEditor(typeof(BeamSettings))]
    public class BeamSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Please use the BeamXR settings page to edit your settings file.");
            if (GUILayout.Button("Open BeamXR Settings"))
            {
                SettingsService.OpenProjectSettings("Project/BeamXR");
            }
        }
    }
}