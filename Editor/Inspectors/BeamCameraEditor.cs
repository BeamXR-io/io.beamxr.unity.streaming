using UnityEditor;
using UnityEngine;
using BeamXR.Streaming.Core.Media;

namespace BeamXR.Streaming.Editor
{
    [CustomEditor(typeof(BeamCamera))]
    public class BeamCameraEditor : UnityEditor.Editor
    {
        private BeamCamera _streamingCamera = null;

        private GUIStyle _boldFoldout;
        private CameraView _currentView;
        private bool _rendering = false, _streaming = false;
        private int _requests = 0;

        private SerializedProperty _cameraMethod, _inputRenderTexture;
        private SerializedProperty _defaultCameraSettings, _currentCameraSettings, _cameraPresets;
        private SerializedProperty _transformToFollow, _selfieTransform;
        private SerializedProperty _transformPriorities;
        private SerializedProperty _automaticHiddenLayer, _layersToHide, _specificBeamLayer, _addBeamLayerToMainCam;
        private SerializedProperty _cameraSettingsChanged, _transformToFollowChanged;

        private bool _renderering = false;

        public override void OnInspectorGUI()
        {
            FindParts();

            CameraMethod();

            if (_streamingCamera.CameraRenderMethod == BeamCamera.CameraMethod.RenderTexture)
            {
                EditorGUILayout.HelpBox("Most features are not applicable when using a custom render texture.", MessageType.Info);
            }
            else
            {
                SecondaryCamera();
                IgnoreLayers();
                Events();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            if (Application.isPlaying && !_renderering)
            {
                if(_streamingCamera == null)
                {
                    _streamingCamera = serializedObject.targetObject as BeamCamera;
                }
                if(_streamingCamera != null)
                {
                    _streamingCamera.RequestTemporaryCamera(serializedObject.targetObject);
                    _renderering = true;
                }
            }
        }

        private void OnDestroy()
        {
            if(Application.isPlaying && _renderering)
            {
                if (_streamingCamera == null)
                {
                    _streamingCamera = serializedObject.targetObject as BeamCamera;
                }
                if (_streamingCamera != null)
                {
                    _streamingCamera.ReleaseTemporaryCamera(serializedObject.targetObject);
                    _renderering = false;
                }
            }
        }

        private void FindParts()
        {
            _boldFoldout = new GUIStyle(EditorStyles.foldout);
            _boldFoldout.fontStyle = FontStyle.Bold;

            _streamingCamera = serializedObject.targetObject as BeamCamera;
            if (Application.isPlaying)
            {
                _streaming = _streamingCamera.Streaming;
                _rendering = _streamingCamera.Rendering;
                _requests = _streamingCamera.TempRequests;
            }
            serializedObject.FindFirstProperty(ref _cameraMethod, "_cameraMethod");
            serializedObject.FindFirstProperty(ref _inputRenderTexture, "_inputRenderTexture");

            serializedObject.FindFirstProperty(ref _selfieTransform, "_selfieTransform");
            serializedObject.FindFirstProperty(ref _transformPriorities, "_transformPriorities");
            serializedObject.FindFirstProperty(ref _transformToFollow, "_transformToFollow");

            serializedObject.FindFirstProperty(ref _defaultCameraSettings, "_defaultCameraSettings");
            serializedObject.FindFirstProperty(ref _currentCameraSettings, "_currentCameraSettings");
            serializedObject.FindFirstProperty(ref _cameraPresets, "_cameraPresets");

            if (Application.isPlaying)
            {
                _currentView = (CameraView)_currentCameraSettings.FindPropertyRelative("cameraView").intValue;
            }

            serializedObject.FindFirstProperty(ref _automaticHiddenLayer, "_automaticHiddenLayer");
            serializedObject.FindFirstProperty(ref _layersToHide, "_layersToHide");
            serializedObject.FindFirstProperty(ref _specificBeamLayer, "_specificBeamLayer");
            serializedObject.FindFirstProperty(ref _addBeamLayerToMainCam, "_addBeamLayerToMainCam");

            serializedObject.FindFirstProperty(ref _cameraSettingsChanged, "OnCameraSettingsChanged");
            serializedObject.FindFirstProperty(ref _transformToFollowChanged, "OnTransformToFollowChanged");
        }

        private void CameraMethod()
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.PropertyField(_cameraMethod);
            EditorGUI.EndDisabledGroup();
            if (_streamingCamera.CameraRenderMethod == BeamCamera.CameraMethod.RenderTexture)
            {
                EditorGUILayout.PropertyField(_inputRenderTexture);
            }
        }

        private void SecondaryCamera()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorStyles.label.richText = true;
                EditorGUILayout.LabelField($"Rendering: {(_rendering ? "<color=#28a13c>" : "<color=#d63722>")}{_rendering}</color> {(_rendering ? "" : "(camera will not update)")}");
                EditorGUILayout.LabelField($"Streaming: {_streaming} - Preview Requests: {_requests}");

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.PropertyField(_selfieTransform);
            EditorGUILayout.PropertyField(_transformToFollow);

            TransformPriorities();

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.PropertyField(_defaultCameraSettings);
            EditorGUI.EndDisabledGroup();

            if (Application.isPlaying)
            {
                EditorGUILayout.PropertyField(_currentCameraSettings);
                if (GUILayout.Button("Call Camera Update Event"))
                {
                    _streamingCamera.UpdateCameraSettings(_streamingCamera.CurrentCameraSettings);
                }
            }

            EditorGUILayout.PropertyField(_cameraPresets);
        }

        private void TransformPriorities()
        {
            if (Application.isPlaying)
            {
                if (_currentView == CameraView.FirstPerson || _currentView == CameraView.ThirdPerson)
                {
                    EditorGUILayout.LabelField("Camera is targetting player, assigned transforms will be ignored.");
                }
                else if (_transformPriorities.arraySize == 0)
                {
                    EditorGUILayout.LabelField("No transforms to follow.");
                }
                else
                {
                    EditorGUILayout.LabelField(new GUIContent("Current Transforms", "The currently active transforms, with their respective priority"), EditorStyles.boldLabel);
                    for (int i = 0; i < _transformPriorities.arraySize; i++)
                    {
                        var transform = _transformPriorities.GetArrayElementAtIndex(i).FindPropertyRelative("transform");
                        EditorGUILayout.PropertyField(transform, new GUIContent($"{transform.objectReferenceValue.name} ({_transformPriorities.GetArrayElementAtIndex(i).FindPropertyRelative("priority").intValue})"));
                    }
                }
            }
        }

        private void IgnoreLayers()
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            EditorGUILayout.PropertyField(_automaticHiddenLayer);
            EditorGUI.BeginDisabledGroup(_automaticHiddenLayer.boolValue);
            EditorGUILayout.PropertyField(_layersToHide);
            EditorGUILayout.PropertyField(_specificBeamLayer);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(_addBeamLayerToMainCam);

            EditorGUI.EndDisabledGroup();
        }

        private void Events()
        {
            EditorGUILayout.Space();
            _transformToFollowChanged.isExpanded = EditorGUILayout.Foldout(_transformToFollowChanged.isExpanded, "Events", true, _boldFoldout);
            if (_transformToFollowChanged.isExpanded)
            {
                EditorGUILayout.PropertyField(_cameraSettingsChanged);
                EditorGUILayout.PropertyField(_transformToFollowChanged);
            }
        }
    }
}
