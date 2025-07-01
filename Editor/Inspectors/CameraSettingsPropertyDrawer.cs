using BeamXR.Streaming.Core.Media;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor
{
    [CustomPropertyDrawer(typeof(CameraSettings))]
    public class CameraSettingsPropertyDrawer : PropertyDrawer
    {
        private SerializedProperty _fieldOfView;
        private SerializedProperty _cameraView, _lookType;
        private SerializedProperty _smoothingAmount;
        private SerializedProperty _yAngle, _zDistance, _zLookDistance;
        private SerializedProperty _cameraHeight, _headHeight;

        private CameraView _currentView;
        private CameraLookType _currentLookType;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            FindParts(property);

            EditorGUI.BeginProperty(position, label, property);

            position.height = EditorGUIUtility.singleLineHeight;

            if (property.depth > 0)
            {
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, new GUIContent(GetSummary()), true);
            }
            else
            {
                property.isExpanded = true;

                GUIContent link = new GUIContent("Camera Settings Guide", "View our docs to see what each of the camera settings do and suggested use-cases.");

                Vector2 size = EditorStyles.linkLabel.CalcSize(link);

                Rect linkRect = position;
                linkRect.x = position.x + position.width - size.x;
                linkRect.width = size.x;

                Rect labelRect = position;
                labelRect.width = position.width - size.x;

                EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);

                if (EditorGUI.LinkButton(linkRect, link))
                {
                    Application.OpenURL("https://docs.beamxr.io/sdk-guides/core/camera-settings");
                }
            }


            if (property.isExpanded)
            {
                position.y += EditorGUIUtility.singleLineHeight + 2;
                position.height = ((EditorGUIUtility.singleLineHeight + 2) * 9) + 8;
                EditorGUI.LabelField(position, "", GUI.skin.GetStyle("HelpBox"));

                position.height = EditorGUIUtility.singleLineHeight;
                position.y += 5;
                position.x += 4;
                position.width -= 8;

                DrawElement(ref position, _fieldOfView);
                DrawElement(ref position, _cameraView);
                DrawElement(ref position, _lookType, _currentView != CameraView.FirstPerson);
                DrawElement(ref position, _smoothingAmount);
                DrawElement(ref position, _yAngle, _currentView == CameraView.ThirdPerson);
                DrawElement(ref position, _zDistance, _currentView == CameraView.ThirdPerson);
                DrawElement(ref position, _zLookDistance, _currentLookType == CameraLookType.LookPosition);
                DrawElement(ref position, _cameraHeight, _currentView == CameraView.ThirdPerson);
                DrawElement(ref position, _headHeight, _currentView != CameraView.FirstPerson && _currentLookType != CameraLookType.Direction);
            }

            EditorGUI.EndProperty();
        }

        private void FindParts(SerializedProperty property)
        {
            _fieldOfView = property.FindPropertyRelative("fieldOfView");
            _cameraView = property.FindPropertyRelative("cameraView");
            _lookType = property.FindPropertyRelative("lookType");
            _smoothingAmount = property.FindPropertyRelative("smoothingAmount");
            _yAngle = property.FindPropertyRelative("yAngle");
            _zDistance = property.FindPropertyRelative("zDistance");
            _zLookDistance = property.FindPropertyRelative("zLookDistance");
            _cameraHeight = property.FindPropertyRelative("cameraHeight");
            _headHeight = property.FindPropertyRelative("headHeight");

            _currentView = (CameraView)_cameraView.intValue;
            _currentLookType = (CameraLookType)_lookType.intValue;

            if (_currentView == CameraView.ThirdPerson && _currentLookType == CameraLookType.Inverted)
            {
                _lookType.intValue = (int)CameraLookType.Direction;
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
            return _currentView.ToString() + " " + _currentLookType.ToString() + $" - FOV: {_fieldOfView.floatValue} Smoothing: {_smoothingAmount.floatValue}";
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            return ((EditorGUIUtility.singleLineHeight + 2) * 10) + 8;
        }
    }
}