using BeamXR.Streaming.Core;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor
{
    [CustomEditor(typeof(BeamUnityEvents))]
    public class BeamUnityEventsEditor : UnityEditor.Editor
    {
        private GUIStyle _foldOut, _text;

        private SerializedProperty _showTooltips;
        private SerializedProperty _streamingStateChanged, _previousStreamingState;
        private SerializedProperty _onStreamStarted, _onStreamEnded, _onSessionError;

        private SerializedProperty _onRecordingStarted, _onRecordingEnded, _onRecordingStateChanged, _previousRecordingState;

        private SerializedProperty _onPhotoCaptureResult, _onPhotoCaptured;

        private SerializedProperty _onCapturingStarted, _onCapturingEnded;
        private SerializedProperty _onAspectRatio, _onResolution;

        private SerializedProperty _onAuthenticationChanged, _onStreamAvailabilityError;
        private SerializedProperty _onStreamPlatformState;

        public override void OnInspectorGUI()
        {
            FindParts();

            EditorGUILayout.PropertyField(_showTooltips);
            BuildGroup("Streaming", _showTooltips.boolValue, _streamingStateChanged, _onStreamStarted, _onStreamEnded, _onSessionError);

            BuildGroup("Recording", _showTooltips.boolValue, _onRecordingStateChanged, _onRecordingStarted, _onRecordingEnded);

            BuildGroup("Photos", _showTooltips.boolValue, _onPhotoCaptureResult, _onPhotoCaptured);

            BuildGroup("Capturing and Visuals", _showTooltips.boolValue, _onCapturingStarted, _onCapturingEnded, _onAspectRatio, _onResolution);

            BuildGroup("Account and Platforms", _showTooltips.boolValue, _onAuthenticationChanged, _onStreamAvailabilityError, _onStreamPlatformState);
            
            serializedObject.ApplyModifiedProperties();
        }

        private void FindParts()
        {
            _foldOut = new GUIStyle(EditorStyles.foldout);

            _foldOut.fontStyle = FontStyle.Bold;

            _text = new GUIStyle(EditorStyles.label);
            _text.wordWrap = true;

            _showTooltips = serializedObject.FindProperty("_showTooltips");
            _streamingStateChanged = serializedObject.FindProperty("OnStreamingStateChanged");
            _previousStreamingState = serializedObject.FindProperty("_previousStreamingState");
            _onStreamStarted = serializedObject.FindProperty("OnStreamStarted");
            _onStreamEnded = serializedObject.FindProperty("OnStreamEnded");
            _onSessionError = serializedObject.FindProperty("OnSessionError");

            _onRecordingStarted = serializedObject.FindProperty("OnRecordingStarted");
            _onRecordingEnded = serializedObject.FindProperty("OnRecordingEnded");
            _onRecordingStateChanged = serializedObject.FindProperty("OnRecordingStateChanged");
            _previousRecordingState = serializedObject.FindProperty("_previousRecordingState");

            _onPhotoCaptureResult = serializedObject.FindProperty("OnPhotoCapturedResult");
            _onPhotoCaptured = serializedObject.FindProperty("OnPhotoCaptured");

            _onCapturingStarted = serializedObject.FindProperty("OnCapturingStarted");
            _onCapturingEnded = serializedObject.FindProperty("OnCapturingEnded");
            _onAspectRatio = serializedObject.FindProperty("OnAspectRatioChanged");
            _onResolution = serializedObject.FindProperty("OnResolutionChanged");

            _onAuthenticationChanged = serializedObject.FindProperty("OnAuthenticationChanged");
            _onStreamAvailabilityError = serializedObject.FindProperty("OnStreamAvailabilityError");

            _onStreamPlatformState = serializedObject.FindProperty("OnStreamPlatformStateChanged");
        }

        private void BuildGroup(string header, bool showTooltips = true, params SerializedProperty[] properties)
        {
            if (properties.Length > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                properties[0].isExpanded = EditorGUILayout.Foldout(properties[0].isExpanded, header, true, _foldOut);
                EditorGUI.indentLevel--;
                if (properties[0].isExpanded)
                {
                    for (int i = 0; i < properties.Length; i++)
                    {
                        if (showTooltips)
                        {
                            EditorGUILayout.LabelField(properties[i].tooltip, _text);
                        }
                        EditorGUILayout.PropertyField(properties[i]);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}