using BeamXR.Streaming.Core.SessionManagement;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor
{
    [CustomPropertyDrawer(typeof(BeamOptions), true)]
    public class BeamOptionsPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty _frameRate;
        private SerializedProperty _bitRate, _scaleResolutionDownBy;
        private SerializedProperty _microphoneType;
        private SerializedProperty _microphoneScale, _microphoneSampleRate, _manualMicrophoneOutputRate, _microphoneOutputRate;
        private SerializedProperty _platform, _deviceModels;

        private bool _isModelDevice = false, _manualMic = false;

        private int _elements = 7;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            FindParts(property);

            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;

            if (property.depth > 0)
            {
                Rect fold = position;
                fold.width -= 30;
                property.isExpanded = EditorGUI.Foldout(fold, property.isExpanded, _isModelDevice ? new GUIContent(GetSummary()) : label, true);
                Rect r = position;
                r.x = position.x + position.width - 30;
                r.width = 30;
                if (GUI.Button(r, "-"))
                {
                    property.DeleteCommand();
                    return;
                }
            }
            else
            {
                property.isExpanded = true;
                EditorGUI.LabelField(position, label, EditorStyles.boldLabel);
            }

            if (property.isExpanded)
            {
                position.y += EditorGUIUtility.singleLineHeight + 2;
                position.height = ((EditorGUIUtility.singleLineHeight + 2) * ((_isModelDevice ? 2 : 0) + (_manualMic ? 1 : 0) + _elements)) + 8;
                EditorGUI.LabelField(position, "", GUI.skin.GetStyle("HelpBox"));

                position.height = EditorGUIUtility.singleLineHeight;
                position.y += 5;
                position.x += 4;
                position.width -= 8;

                if (_isModelDevice)
                {
                    DrawElement(ref position, _platform);
                    DrawElement(ref position, _deviceModels);
                }

                DrawElement(ref position, _frameRate);
                DrawElement(ref position, _bitRate);
                DrawElement(ref position, _scaleResolutionDownBy);
                DrawElement(ref position, _microphoneType);
                DrawElement(ref position, _microphoneScale);
                DrawElement(ref position, _manualMicrophoneOutputRate);
                if (_manualMic)
                {
                    DrawElement(ref position, _microphoneOutputRate);
                }
                DrawElement(ref position, _microphoneSampleRate);
            }

            EditorGUI.EndProperty();
        }

        private void FindParts(SerializedProperty property)
        {
            _isModelDevice = fieldInfo.FieldType.ToString().Contains(typeof(ModelDeviceOptions).ToString());

            _frameRate = property.FindPropertyRelative("frameRate");
            _bitRate = property.FindPropertyRelative("bitRate");
            _scaleResolutionDownBy = property.FindPropertyRelative("scaleResolutionDownBy");
            _microphoneType = property.FindPropertyRelative("microphoneType");
            _microphoneScale = property.FindPropertyRelative("microphoneScale");
            _microphoneSampleRate = property.FindPropertyRelative("microphoneSampleRate");
            _manualMicrophoneOutputRate = property.FindPropertyRelative("manualMicrophoneOutputRate");
            _microphoneOutputRate = property.FindPropertyRelative("microphoneOutputRate");

            _manualMic = _manualMicrophoneOutputRate.boolValue;

            if (_isModelDevice)
            {
                _platform = property.FindPropertyRelative("platform");
                _deviceModels = property.FindPropertyRelative("deviceModels");
            }
        }

        private void DrawElement(ref Rect position, SerializedProperty property, bool enabled = true)
        {
            bool oldEnabled = GUI.enabled;

            if (!enabled)
            {
                GUI.enabled = enabled;
            }

            EditorGUI.PropertyField(position, property);

            position.y += EditorGUIUtility.singleLineHeight + 2;

            GUI.enabled = oldEnabled;
        }

        private string GetSummary()
        {
            if (_platform == null)
                return "";

            return ((DevicePlatform)_platform.intValue) + (_deviceModels.stringValue.Length > 0 ? $" ({_deviceModels.stringValue})" : "");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            return ((EditorGUIUtility.singleLineHeight + 2) * ((_isModelDevice ? 2 : 0) + (_manualMic ? 1 : 0) + _elements + 1)) + 8;
        }
    }
}