using BeamXR.Streaming.Core;
using BeamXR.Streaming.Core.Audio;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor.Audio
{
    [CustomEditor(typeof(BeamNativeMicrophone))]
    public class BeamNativeMicrophoneEditor : UnityEditor.Editor
    {
        private SerializedProperty _manualOutputSampleRate;
        private SerializedProperty _outputSampleRate, _microphoneSampleRate, _micBufferSize, _lastSamples;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            FindParts();
            EditorGUILayout.PropertyField(_manualOutputSampleRate);
            if (_manualOutputSampleRate.boolValue)
            {
                EditorGUILayout.PropertyField(_outputSampleRate);
            }
            EditorGUILayout.PropertyField(_microphoneSampleRate);
            EditorGUILayout.PropertyField(_micBufferSize);

            if (Application.isPlaying && BeamStreamingManager.Instance.IsStreaming)
            {
                EditorGUI.BeginDisabledGroup(true);

                EditorGUILayout.PropertyField(_lastSamples);

                EditorGUI.EndDisabledGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void FindParts()
        {
            _manualOutputSampleRate = serializedObject.FindProperty("_manualOutputSampleRate");
            _outputSampleRate = serializedObject.FindProperty("_outputSampleRate");
            _microphoneSampleRate = serializedObject.FindProperty("_microphoneSampleRate");
            _micBufferSize = serializedObject.FindProperty("_micBufferSize");
            _lastSamples = serializedObject.FindProperty("_lastSamples");
        }
    }
}