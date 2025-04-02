using BeamXR.Streaming.Core;
using System.Xml;
#if USING_OCULUS_RUNTIME
using Unity.XR.Oculus;
using UnityEditor.XR.Management;
using System.Linq;
#endif
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor
{
    public class BeamXRProjectSetupChecklist : EditorWindow
    {
        private const string AndroidManifestPath = "Assets/Plugins/Android/AndroidManifest.xml";
        private const string PackageManifestPath = "Packages/manifest.json";

        private const string checkmark = "✓";
        private const string cross = "✗";

        private static readonly string[] RequiredPermissions = {
        "android.permission.INTERNET",
        "android.permission.RECORD_AUDIO",
        "android.permission.RECORD_VIDEO"
    };

        private static Texture2D icon;
        private GUIContent infoIcon;
        private GUIContent checkmarkIcon;
        private GUIContent errorIcon;
        private GUIContent greenStatusIcon;
        private GUIContent redStatusIcon;
        private GUIContent yellowStatusIcon;
        private GUIContent orangeStatusIcon;
        private Vector2 scrollPosition;
        private GUIStyle invalidStyle;
        private GUIStyle validStyle;
        private GUIStyle fixButtonStyle;

        [MenuItem("BeamXR/BeamXR Project Setup Checklist")]
        public static void ShowWindow()
        {
            GetWindow<BeamXRProjectSetupChecklist>("BeamXR Project Setup Window");
        }

        private void SetStyles()
        {
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/io.beamxr.unity.streaming/Editor/BeamXr-Full-White.png");
            infoIcon = EditorGUIUtility.IconContent("console.infoicon");
            checkmarkIcon = EditorGUIUtility.IconContent("sv_icon_dot0_sml");
            errorIcon = EditorGUIUtility.IconContent("console.erroricon");
            greenStatusIcon = EditorGUIUtility.IconContent("sv_icon_name3");
            redStatusIcon = EditorGUIUtility.IconContent("sv_icon_name6");
            yellowStatusIcon = EditorGUIUtility.IconContent("sv_icon_name4");
            orangeStatusIcon = EditorGUIUtility.IconContent("sv_icon_name5");

            invalidStyle = new GUIStyle
            {
                normal = { textColor = Color.red },
                fontSize = 35,
                stretchWidth = false
            };

            validStyle = new GUIStyle
            {
                normal = { textColor = Color.green },
                fontSize = 35,
                stretchWidth = false
            };

            fixButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                hover = { textColor = Color.white },
                active = { textColor = Color.white },
                padding = new RectOffset(10, 10, 5, 5)
            };
        }

        private void OnGUI()
        {
            SetStyles();

            GUILayout.Space(10);

            // Begin a horizontal group to stretch the full width of the window.
            GUILayout.BeginHorizontal();

            if (icon != null)
            {
                GUILayout.Label(new GUIContent(icon), GUILayout.Height(25), GUILayout.Width(167));
                GUILayout.Space(10); // Add space between the icon and the text
                GUILayout.Label("Project setup checklist", EditorStyles.boldLabel);
            }
            else
            {
                GUILayout.Label("BeamXR project setup checklist", EditorStyles.boldLabel);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            CheckExperienceKeyAndSecret();

            GUILayout.Space(10);

            CheckPermissionsGui();

            GUILayout.EndScrollView();
        }

        private void CheckExperienceKeyAndSecret()
        {
            GUILayout.Label("Experience Key and Secret", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            // Iterate through the current scene and see if there's a BeamStreamingManager.
            var beamStreamingManager = FindFirstObjectByType<BeamStreamingManager>(FindObjectsInactive.Include);

            if (beamStreamingManager != null)
            {
                // Use reflection to get the _experienceKey and _experienceSecret fields.
                var experienceKeyField = beamStreamingManager.GetType().GetField("_experienceKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var experienceSecretField = beamStreamingManager.GetType().GetField("_experienceSecret", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                // Get the values of the fields.
                var experienceKey = experienceKeyField.GetValue(beamStreamingManager) as string;
                var experienceSecret = experienceSecretField.GetValue(beamStreamingManager) as string;

                // Check that the Experience Key and Secret are set.
                if (string.IsNullOrEmpty(experienceKey) || string.IsNullOrEmpty(experienceSecret))
                {
                    GUILayout.Label(cross, invalidStyle);

                    EditorGUILayout.HelpBox("Experience Key and Secret are not set. Please set them in the BeamStreamingManager component.", MessageType.Error);

                    if (GUILayout.Button("Fix", GUILayout.Width(50), GUILayout.Height(36)))
                    {
                        // Highlight the object.
                        EditorGUIUtility.PingObject(beamStreamingManager);
                        Selection.activeObject = beamStreamingManager;
                    }
                }
                else
                {
                    GUILayout.Label(checkmark, validStyle);

                    EditorGUILayout.HelpBox("Experience Key and Secret are set.", MessageType.Info);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CheckPermissionsGui()
        {
            GUILayout.Label("Permissions", EditorStyles.boldLabel);

            if (!System.IO.File.Exists(AndroidManifestPath))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(cross, invalidStyle);

                EditorGUILayout.HelpBox("AndroidManifest.xml file not found. Please create one in the Assets/Plugins/Android folder.", MessageType.Error);

                if (GUILayout.Button("Fix", GUILayout.Width(50), GUILayout.Height(36)))
                {
                    CreateDefaultAndroidManifest();
                }

                GUILayout.EndHorizontal();

                return;
            }
            else
            {
                XmlDocument manifestDoc = new XmlDocument();

                manifestDoc.Load(AndroidManifestPath);

                bool[] permissionsPresent = new bool[RequiredPermissions.Length];
                for (int i = 0; i < RequiredPermissions.Length; i++)
                {
                    permissionsPresent[i] = IsPermissionPresent(manifestDoc, RequiredPermissions[i]);
                    EditorGUILayout.BeginHorizontal();

                    if (permissionsPresent[i])
                    {
                        GUILayout.Label(checkmark, validStyle);

                        EditorGUILayout.HelpBox($"Android Manifest {RequiredPermissions[i]} is present and correct.", MessageType.Info);
                    }
                    else
                    {
                        GUILayout.Label(cross, invalidStyle);
                        EditorGUILayout.HelpBox($"Android Manifest {RequiredPermissions[i]} is required for standalone VR.", MessageType.Warning);
                        if (GUILayout.Button("Fix", GUILayout.Width(50), GUILayout.Height(36)))
                        {
                            AddPermissionToManifest(manifestDoc, RequiredPermissions[i]);
                            manifestDoc.Save(AndroidManifestPath);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void TextureCompressionAndGraphicsApiGui()
        {
            // Check and fix texture compression format
            GUILayout.Label("Texture compression and graphics API", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (!IsGraphicsApiSetToOpenGLES3Only() || IsAutoGraphicsApiEnabled())
            {
                GUILayout.Label(cross, invalidStyle);

                EditorGUILayout.HelpBox("The graphics API should be set to OpenGLES3 only. Vulkan currently contains issues preventing streaming of frames and so BeamXR streaming will not function correctly.", MessageType.Error);

                if (GUILayout.Button("Fix", GUILayout.Width(50), GUILayout.Height(36)))
                {
                    SetGraphicsApiToOpenGLES3Only();
                    SetAutoGraphicsApiEnabled(false);
                }
            }
            else
            {
                GUILayout.Label(checkmark, validStyle);

                EditorGUILayout.HelpBox("Graphics API is set to OpenGLES3.", MessageType.Info);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (!IsTextureCompressionSet())
            {
                GUILayout.Label(cross, invalidStyle);

                EditorGUILayout.HelpBox("Texture compression format should be set to ETC2 (GLES 3.0). Failing to do so will result in streaming video issues.", MessageType.Error);

                if (GUILayout.Button("Fix", GUILayout.Width(50), GUILayout.Height(36)))
                {
                    SetTextureCompressionFormat();
                }
            }
            else
            {
                GUILayout.Label(checkmark, validStyle);

                EditorGUILayout.HelpBox("Texture compression format is set to ETC2.", MessageType.Info);
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool IsPermissionPresent(XmlDocument manifestDoc, string permission)
        {
            XmlNodeList permissionNodes = manifestDoc.GetElementsByTagName("uses-permission");
            foreach (XmlNode node in permissionNodes)
            {
                XmlAttribute nameAttribute = node.Attributes["android:name"];
                if (nameAttribute != null && nameAttribute.Value == permission)
                {
                    return true;
                }
            }
            return false;
        }

        private void AddPermissionToManifest(XmlDocument manifestDoc, string permission)
        {
            XmlElement manifestElement = manifestDoc.DocumentElement;

            XmlElement permissionElement = manifestDoc.CreateElement("uses-permission");
            XmlAttribute nameAttribute = manifestDoc.CreateAttribute("android", "name", "http://schemas.android.com/apk/res/android");
            nameAttribute.Value = permission;
            permissionElement.Attributes.Append(nameAttribute);

            manifestElement.AppendChild(permissionElement);

            Debug.Log($"Added {permission} to AndroidManifest.xml");
        }

        private void CreateDefaultAndroidManifest()
        {
            string defaultManifest = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?>
            <manifest xmlns:android=""http://schemas.android.com/apk/res/android"" xmlns:tools=""http://schemas.android.com/tools"" android:installLocation=""auto"">
              <application android:label=""@string/app_name"" android:icon=""@mipmap/app_icon"" android:allowBackup=""false"">
                <activity android:theme=""@android:style/Theme.Black.NoTitleBar.Fullscreen"" android:configChanges=""locale|fontScale|keyboard|keyboardHidden|mcc|mnc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|touchscreen|uiMode"" android:launchMode=""singleTask"" android:name=""com.unity3d.player.UnityPlayerActivity"" android:excludeFromRecents=""true"" android:exported=""true"">
                  <intent-filter>
                    <action android:name=""android.intent.action.MAIN"" />
                    <category android:name=""android.intent.category.LAUNCHER"" />
                    <category android:name=""com.oculus.intent.category.VR"" />
                  </intent-filter>
                  <meta-data android:name=""com.oculus.vr.focusaware"" android:value=""true"" />
                </activity>
                <meta-data android:name=""unityplayer.SkipPermissionsDialog"" android:value=""false"" />
                <meta-data android:name=""com.samsung.android.vr.application.mode"" android:value=""vr_only"" />
                <meta-data android:name=""com.oculus.ossplash.background"" android:value=""black"" />
                <meta-data android:name=""com.oculus.supportedDevices"" android:value=""quest|quest2|questpro|eureka"" />
              </application>
              <uses-feature android:name=""android.hardware.vr.headtracking"" android:version=""1"" android:required=""true"" />
              <uses-permission android:name=""com.oculus.permission.USE_ANCHOR_API"" />
              <uses-permission android:name=""com.oculus.permission.USE_SCENE"" />
              <uses-permission android:name=""android.permission.INTERNET"" />
              <uses-permission android:name=""android.permission.RECORD_VIDEO"" />
              <uses-permission android:name=""android.permission.RECORD_AUDIO"" />
            </manifest>";

            if (!System.IO.Directory.Exists("Assets/Plugins/Android"))
            {
                System.IO.Directory.CreateDirectory("Assets/Plugins/Android");
            }

            System.IO.File.WriteAllText(AndroidManifestPath, defaultManifest);
            AssetDatabase.Refresh();
            Debug.Log("Created default AndroidManifest.xml");
        }

        private bool IsGraphicsApiSetToOpenGLES3Only()
        {
            var currentGraphicsApi = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);

            if (currentGraphicsApi == null)
            {
                return false;
            }

            if (currentGraphicsApi.Length == 0)
            {
                return false;
            }

            if (currentGraphicsApi.Length > 1)
            {
                return false;
            }

            bool isGLES3 = currentGraphicsApi[0] == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3;

            return isGLES3;
        }

        private void SetGraphicsApiToOpenGLES3Only()
        {
            var currentGraphicsApi = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);

            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });

            Debug.Log("Graphics API set to OpenGLES3.");
        }

        private bool IsTextureCompressionSet()
        {
            var currentTextureCompression = EditorUserBuildSettings.androidBuildSubtarget;

            bool isETC2 = currentTextureCompression == MobileTextureSubtarget.ETC2;

            return isETC2;
        }

        private void SetTextureCompressionFormat()
        {
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC2;

            Debug.Log("Texture compression format set to ETC2.");
        }

        private bool IsAutoGraphicsApiEnabled()
        {
            return PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android);
        }

        private void SetAutoGraphicsApiEnabled(bool enabled)
        {
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, enabled);
        }
    }
}