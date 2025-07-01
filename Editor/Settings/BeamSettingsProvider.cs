using System.IO;
using UnityEditor;
using UnityEngine;
using BeamXR.Streaming.Core.Settings;
using UnityEngine.UIElements;
using BeamXR.Streaming.Core;
using BeamXR.Streaming.Core.Media;
using System;

namespace BeamXR.Streaming.Editor
{
    public class BeamSettingsProvider : SettingsProvider
    {
        private SerializedObject _beamSettings;
        // Do not directly reference
        private SerializedObject _cachedStreamingManager;
        private SerializedObject _beamManager
        {
            get
            {
                if (_cachedStreamingManager != null)
                {
                    return _cachedStreamingManager;
                }

                var streamingManager = GameObject.FindAnyObjectByType<BeamManager>();

                if (streamingManager != null)
                {
                    _cachedStreamingManager = new SerializedObject(streamingManager);
                    return _cachedStreamingManager;
                }
                return null;
            }
        }

        // Do not directly reference
        private SerializedObject _cachedStreamingCamera;
        private SerializedObject _beamCamera
        {
            get
            {
                if (_cachedStreamingCamera != null)
                {
                    return _cachedStreamingCamera;
                }

                var streamingCamera = GameObject.FindAnyObjectByType<BeamCamera>();

                if (streamingCamera != null)
                {
                    _cachedStreamingCamera = new SerializedObject(streamingCamera);
                    return _cachedStreamingCamera;
                }
                return null;
            }
        }

        private SerializedProperty _experienceKey, _experienceSecret;
        private SerializedProperty _beamEnvironment, _developerLogs;
        private SerializedProperty _localFolderName, _localFilePrefix, _localFileTimestamp;
        private SerializedProperty _defaultBeamOptions, _deviceOverrides;

        public BeamSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) { }

        public static bool IsSettingsAvailable()
        {
            return File.Exists(BeamSettings.SETTINGS_FOLDER + BeamSettings.SETTINGS_FILE);
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _beamSettings = GetSerializedSettings();
            if (_beamManager != null)
            {
                _beamManager.FindProperty("_beamSettings").objectReferenceValue = _beamSettings.targetObject;
            }
        }

        public override void OnGUI(string searchContext)
        {
            FindParts();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(14);
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (_beamManager != null)
            {
                if (GUILayout.Button("Beam Manager"))
                {
                    Selection.activeObject = _beamManager.targetObject;
                }
            }
            if (_beamCamera != null)
            {
                if (GUILayout.Button("Beam Camera"))
                {
                    Selection.activeObject = _beamCamera.targetObject;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_beamSettings.FindProperty("_advancedSettings").boolValue)
            {
                AdvancedSettings();
            }

            ExperienceSettings();
            FileSettings();
            PlatformSettings();
            _beamSettings.ApplyModifiedProperties();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void FindParts()
        {
            _experienceKey = _beamSettings.FindProperty("_experienceKey");
            _experienceSecret = _beamSettings.FindProperty("_experienceSecret");
            _beamEnvironment = _beamSettings.FindProperty("_beamEnvironment");
            _developerLogs = _beamSettings.FindProperty("_developerLogs");
            _localFolderName = _beamSettings.FindProperty("_localFolderName");
            _localFilePrefix = _beamSettings.FindProperty("_localFilePrefix");
            _localFileTimestamp = _beamSettings.FindProperty("_localFileTimestamp");
            _defaultBeamOptions = _beamSettings.FindProperty("_defaultBeamOptions");
            _deviceOverrides = _beamSettings.FindProperty("_modelDeviceOverrides");
        }

        private void AdvancedSettings()
        {
            EditorGUILayout.PropertyField(_beamEnvironment);
            EditorGUILayout.PropertyField(_developerLogs);
        }

        private void ExperienceSettings()
        {
            EditorGUILayout.LabelField("Experience", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_experienceKey);
            EditorGUILayout.PropertyField(_experienceSecret);
            if (_experienceKey.stringValue == "" || _experienceSecret.stringValue == "")
            {
                EditorGUILayout.HelpBox("You must set both an experience key and secret from the Beam Developer Portal before you can stream or record.", MessageType.Error);
            }
        }

        private void FileSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Files", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_localFolderName);
            EditorGUILayout.PropertyField(_localFilePrefix);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_localFileTimestamp);
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh"), GUILayout.Width(30)))
            {
                _localFileTimestamp.stringValue = BeamSettings.DEFAULT_TIMESTAMP;
            }
            _localFileTimestamp.stringValue = BeamSettings.SanitizeStrictDateTimeFormat(_localFileTimestamp.stringValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField($"Example filename: {_localFolderName.stringValue}/{_localFilePrefix.stringValue}-{DateTime.Now.ToString(_localFileTimestamp.stringValue)}.mp4");
        }

        private void PlatformSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_defaultBeamOptions);

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Specific Device Overrides", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                GetOrCreateSettings().AddNewDeviceOption();
                _beamSettings.Update();
            }
            EditorGUILayout.EndHorizontal();

            if (_deviceOverrides.arraySize == 0)
            {
                EditorGUILayout.LabelField("No current device overrides.");
            }
            else
            {
                for (int i = 0; i < _deviceOverrides.arraySize; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.PropertyField(_deviceOverrides.GetArrayElementAtIndex(i));
                    EditorGUILayout.EndVertical();
                }
            }
        }

        public override void OnTitleBarGUI()
        {
            GUIContent label = new GUIContent($"v{BeamManager.BEAM_VERSION}");
            GUIStyle labelStyle = EditorStyles.label;
            Vector2 labelSize = labelStyle.CalcSize(label);


            EditorGUILayout.LabelField(label, GUILayout.Width(labelSize.x), GUILayout.Height(labelSize.y + 6));

            if (EditorGUILayout.LinkButton("Developer Portal"))
            {
                Application.OpenURL("https://developers.beamxr.io/");
            }

            if (EditorGUILayout.LinkButton("Documentation"))
            {
                Application.OpenURL("https://docs.beamxr.io/");
            }

            GUIStyle style = EditorStyles.iconButton;
            style.margin = new RectOffset(0, 3, 6, 0);
            style.contentOffset = Vector2.zero;
            if (GUILayout.Button(EditorGUIUtility.IconContent("_Menu"), style))
            {
                var menu = new GenericMenu();

                if (_beamSettings == null)
                {
                    _beamSettings = GetSerializedSettings();
                }
                var advanced = _beamSettings.FindProperty("_advancedSettings");

                menu.AddItem(
                    new GUIContent("Advanced Settings"),
                    advanced.boolValue,
                    () =>
                    {
                        advanced.boolValue = !advanced.boolValue;
                        _beamSettings.ApplyModifiedPropertiesWithoutUndo();
                    }
                );

                menu.DropDown(new Rect(Event.current.mousePosition, Vector2.zero));
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateBeamSettingsProvider()
        {
            if (GetSerializedSettings() != null)
            {
                var provider = new BeamSettingsProvider("Project/BeamXR", SettingsScope.Project);

                // Automatically extract all keywords from the Styles.
                provider.keywords = GetSearchKeywordsFromSerializedObject(GetSerializedSettings());
                return provider;
            }

            // Settings Asset doesn't exist yet; no need to display anything in the Settings window.
            return null;
        }

        [MenuItem("BeamXR/BeamXR Settings")]
        private static void OpenSettings()
        {
            SettingsService.OpenProjectSettings("Project/BeamXR");
        }

        public static BeamSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<BeamSettings>(BeamSettings.SETTINGS_FOLDER + BeamSettings.SETTINGS_FILE);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<BeamSettings>();
                AssetDatabase.SetLabels(settings, new string[] { "BeamXR" });
                if (!AssetDatabase.IsValidFolder(BeamSettings.SETTINGS_FOLDER))
                {
                    AssetDatabase.CreateFolder("Assets", "Settings");
                }
                AssetDatabase.CreateAsset(settings, BeamSettings.SETTINGS_FOLDER + BeamSettings.SETTINGS_FILE);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        public static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }
}