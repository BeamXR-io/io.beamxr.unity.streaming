using BeamXR.Streaming.Core.Audio;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor.Audio
{
    [CustomEditor(typeof(BeamWwiseListener))]
    public class BeamWwiseListenerEditor : UnityEditor.Editor
    {
        private SerializedProperty _wwiseBufferMultiple;

        public override void OnInspectorGUI()
        {
#if BEAM_WWISE
            base.OnInspectorGUI();
            ShowBufferAdjustment();
#else
            ShowMissingWwise();
#endif
        }

        private void ShowMissingWwise()
        {
            EditorGUILayout.HelpBox("You must add BEAM_WWISE to your Scripting Define Symbols for this component to work correctly.", MessageType.Error);
            if(GUILayout.Button("Open Player Settings"))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }
        }

        private void ShowBufferAdjustment()
        {
            _wwiseBufferMultiple = serializedObject.FindProperty("_wwiseBufferMultiple");

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.IntField(new GUIContent($"Buffer Size", "The larger the buffer the more stable the audio stream will be, but uses more memory and may introduce unwanted audio latency. Android will generally require a longer buffer."), 2 << _wwiseBufferMultiple.intValue);

            if (GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.Width(24)))
            {
                _wwiseBufferMultiple.intValue = _wwiseBufferMultiple.intValue - 1;
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(24)))
            {
                _wwiseBufferMultiple.intValue = _wwiseBufferMultiple.intValue + 1;
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}