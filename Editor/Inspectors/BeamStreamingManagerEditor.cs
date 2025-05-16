using BeamXR.Streaming.Core;
using BeamXR.Streaming.Core.Media;
using BeamXR.Streaming.Core.Models;
using BeamXR.Streaming.Core.Models.VFU;
using BeamXR.Streaming.Core.SessionManagement;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor
{
    [CustomEditor(typeof(BeamStreamingManager))]
    public class BeamStreamingManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _streamDevices;
        private SerializedProperty _experienceKey, _experienceSecret;
        private StreamAvailability _streamAvailability;

        private Color _green = new Color(0.157f, 0.6f, 0.31f), _red = new Color(0.68f, 0.11f, 0.11f);
        private Texture2D _grayBg, _greenBg, _yellowBg, _redBg;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BeamStreamingManager beamManager = (BeamStreamingManager)target;
            _streamAvailability = beamManager.StreamAvailability;

            FindParts();

            ShowDeviceOptions(beamManager);

            // Divider line
            AddDivider();

            if (EditorApplication.isPlaying)
            {
                ShowUserState(beamManager);
                ShowAvailableHosts(beamManager);
                ShowStreamingState(beamManager);
            }
        }

        private void FindParts()
        {
            BackgroundTextures();
            _streamDevices = serializedObject.FindProperty("_streamDevices");
            _experienceKey = serializedObject.FindProperty("_experienceKey");
            _experienceSecret = serializedObject.FindProperty("_experienceSecret");
        }

        private void BackgroundTextures()
        {
            if (_grayBg == null)
            {
                _grayBg = MakeTex(2, 2, Color.gray);
            }

            if (_greenBg == null)
            {
                _greenBg = MakeTex(2, 2, _green);
            }

            if (_yellowBg == null)
            {
                _yellowBg = MakeTex(2, 2, Color.yellow);
            }

            if (_redBg == null)
            {
                _redBg = MakeTex(2, 2, _red);
            }
        }

        private void ShowDeviceOptions(BeamStreamingManager beamManager)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent("Platform Settings", "Adjust stream quality settings for different devices"), EditorStyles.boldLabel);

            if (_streamDevices.arraySize == 0)
            {
                EditorGUILayout.LabelField("Add a platform to change specific streaming settings.");
            }

            SerializedProperty child;
            StreamDevice platform;

            GUIContent closeButton = EditorGUIUtility.IconContent("d_winbtn_win_close");

            for (int i = 0; i < _streamDevices.arraySize; i++)
            {
                child = _streamDevices.GetArrayElementAtIndex(i);
                platform = (StreamDevice)child.FindPropertyRelative("platform").intValue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();

                child.isExpanded = EditorGUILayout.Foldout(child.isExpanded, platform.ToString(), true);

                EditorGUI.BeginDisabledGroup(Application.isPlaying && beamManager.StreamingState != StreamingState.Disconnected);
                if (GUILayout.Button(closeButton, EditorStyles.miniButton, GUILayout.Width(30)))
                {
                    _streamDevices.DeleteArrayElementAtIndex(i);
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                if (child.isExpanded)
                {
                    EditorGUI.BeginDisabledGroup(Application.isPlaying && beamManager.StreamingState != StreamingState.Disconnected);
                    EditorGUILayout.PropertyField(child.FindPropertyRelative("platform"));
                    EditorGUILayout.PropertyField(child.FindPropertyRelative("frameRate"));
                    EditorGUILayout.PropertyField(child.FindPropertyRelative("bitRate"));
                    EditorGUILayout.PropertyField(child.FindPropertyRelative("scaleResolutionDownBy"));
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUI.BeginDisabledGroup(Application.isPlaying && beamManager.StreamingState != StreamingState.Disconnected);
            if (GUILayout.Button("+"))
            {
                _streamDevices.InsertArrayElementAtIndex(_streamDevices.arraySize);
                SerializedProperty newChild = _streamDevices.GetArrayElementAtIndex(_streamDevices.arraySize - 1);
                newChild.isExpanded = true;
                newChild.FindPropertyRelative("platform").intValue = 0;
                newChild.FindPropertyRelative("frameRate").intValue = 30;
                newChild.FindPropertyRelative("bitRate").intValue = 5000000;
                newChild.FindPropertyRelative("scaleResolutionDownBy").floatValue = 1f;
            }
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }

        private void AddDivider()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void ShowAvailabilityBox(string name, bool value)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(value ? EditorGUIUtility.IconContent("P4_CheckOutRemote@2x") : EditorGUIUtility.IconContent("P4_DeletedLocal@2x"));

            EditorGUILayout.EndVertical();
        }

        private void ShowUserState(BeamStreamingManager beamManager)
        {
            // Label for Streaming State
            EditorGUILayout.LabelField("BeamXR User", EditorStyles.boldLabel);

            if (beamManager.Me == null)
            {
                if (beamManager.AuthState == AuthenticationState.Error)
                {
                    ShowColouredBox("Error signing in", _redBg, Color.white);

                    if (GUILayout.Button("Sign in"))
                    {
                        beamManager.Authenticate();
                    }

                    return;
                }

                if (beamManager.AuthState == AuthenticationState.Authenticating)
                {
                    ShowColouredBox("Signing in...", _yellowBg, Color.black);
                }
                else
                {
                    ShowColouredBox("Not logged in", _redBg, Color.white);

                    if (_experienceKey.stringValue == "" || _experienceSecret.stringValue == "")
                    {
                        EditorGUILayout.LabelField("Please assign an experience key and secret before trying to authenticate.");
                    }
                    else
                    {
                        if (GUILayout.Button("Sign in"))
                        {
                            beamManager.Authenticate();
                        }
                    }
                }

                if (beamManager.DeviceFlowCode != null)
                {
                    EditorGUILayout.LabelField("Device flow code", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Go to the following URL:");
                    ShowColouredBox(beamManager.DeviceFlowCode.VerificationUrl, _greenBg, Color.white);
                    EditorGUILayout.LabelField($"Enter the following code:");
                    ShowColouredBox(beamManager.DeviceFlowCode.UserCode, _greenBg, Color.white);

                    if (GUILayout.Button("Open browser"))
                    {
                        // Open the browser to the verification URL.
                        Application.OpenURL(beamManager.DeviceFlowCode.VerificationUrlComplete);
                    }
                }
            }
            else
            {
                ShowColouredBox(beamManager.Me.email, _greenBg, Color.white);

                if (_streamAvailability != null)
                {
                    EditorGUILayout.BeginHorizontal();

                    ShowColouredBox(_streamAvailability.CanCloudStream ? $"Cloud Stream up to {_streamAvailability.CloudMaxResolution}" : "Unable to Cloud Stream", _streamAvailability.CanCloudStream ? _greenBg : _redBg, Color.white);
                    ShowColouredBox(_streamAvailability.CanLocalStream ? $"Local Stream up to {(BeamResolution)System.Enum.GetValues(typeof(BeamResolution)).Cast<int>().Max()} " : "Unable to Cloud Stream", _streamAvailability.CanLocalStream ? _greenBg : _redBg, Color.white);

                    EditorGUILayout.EndHorizontal();
                    if (_streamAvailability.ErrorCode != 0)
                    {
                        ShowColouredBox($"Error: {_streamAvailability.ErrorCode} - {_streamAvailability.ErrorReason}", _redBg, Color.white);
                    }
                }

                EditorGUI.BeginDisabledGroup(!(beamManager.StreamingState == StreamingState.Disconnected || beamManager.StreamingState == StreamingState.Error));
                if (GUILayout.Button("Sign out"))
                {
                    beamManager.SignOut();
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private void ShowAvailableHosts(BeamStreamingManager beamManager)
        {
            if (beamManager.Me == null)
            {
                return;
            }

            // Label for Streaming State
            EditorGUILayout.LabelField("Available Hosts", EditorStyles.boldLabel);

            if (beamManager.AvailableStreamingHosts == null || beamManager.AvailableStreamingHosts.Length == 0)
            {
                ShowColouredBox("No local stream apps - will stream to cloud", _grayBg, Color.black);
            }
            else
            {
                foreach (var host in beamManager.AvailableStreamingHosts)
                {
                    ShowColouredBox(host.hostName, _grayBg, Color.black);
                }
            }
        }

        private void ShowStreamingState(BeamStreamingManager beamManager)
        {
            if (beamManager.Me == null)
            {
                return;
            }

            // Label for Streaming State
            EditorGUILayout.LabelField("Streaming State", EditorStyles.boldLabel);

            // User/experience banned
            if (_streamAvailability != null && !_streamAvailability.CanLocalStream && !_streamAvailability.CanCloudStream)
                return;

            // Set the color based on the current status
            Texture2D statusColor;
            Color statusTextColor;
            switch (beamManager.StreamingState)
            {
                case StreamingState.Disconnected:
                    statusTextColor = Color.black;
                    statusColor = _grayBg;
                    break;
                case StreamingState.CreatingSession:
                case StreamingState.Disconnecting:
                case StreamingState.Connecting:
                    statusTextColor = Color.black;
                    statusColor = _yellowBg;
                    break;
                case StreamingState.Error:
                    statusTextColor = Color.white;
                    statusColor = _redBg;
                    break;
                case StreamingState.Streaming:
                case StreamingState.Connected:
                    statusTextColor = Color.white;
                    statusColor = _greenBg;
                    break;
                default:
                    statusColor = _grayBg;
                    statusTextColor = Color.black;
                    break;
            }

            ShowColouredBox(beamManager.StreamingState.ToString(), statusColor, statusTextColor);

            if (beamManager.StreamingState == StreamingState.Disconnected || beamManager.StreamingState == StreamingState.Error)
            {
                // Create a dropdown list of the available hosts + one more for cloud.
                var hosts = beamManager.AvailableStreamingHosts.ToList();

                var hostNames = hosts.Select(x => x.hostName).ToList();
                var hostIds = hosts.Select(x => x.id).ToList();

                hostNames.Add("Cloud");
                hostIds.Add("");

                int selection = hostIds.Count - 1;

                if (beamManager.PreferredStreamingHost != null)
                {
                    var preferredHostId = hostIds.IndexOf(beamManager.PreferredStreamingHost.id);
                    if (preferredHostId != -1)
                    {
                        selection = preferredHostId;
                    }
                }

                var newSelectionId = GUILayout.SelectionGrid(selection, hostNames.ToArray(), hostNames.Count);

                if (newSelectionId != selection)
                {
                    beamManager.SetPreferredStreamingHost(hostIds[newSelectionId]);
                    selection = newSelectionId;
                }

                var hostId = hostIds[selection];
                bool showButton = true;

                if(_streamAvailability == null || (hostId == "" && !_streamAvailability.CanCloudStream))
                {
                    showButton = false;
                }

                if (showButton && GUILayout.Button("Start streaming"))
                {
                    if (hostId == "")
                    {
                        beamManager.StartStreamingToCloud();
                    }
                    else
                    {
                        beamManager.StartStreaming(hostId);
                    }
                }
            }
            else if (beamManager.StreamingState == StreamingState.Streaming)
            {
                if (beamManager.SessionState.ActiveUrls != null && beamManager.SessionState.ActiveUrls.Count() > 0)
                {
                    EditorGUILayout.LabelField("Active URLs");

                    foreach (var url in beamManager.SessionState.ActiveUrls)
                    {
                        if (GUILayout.Button(url.Type))
                        {
                            Application.OpenURL(url.Url);
                        }
                    }
                }

                EditorGUILayout.LabelField("Stream control");

                if (GUILayout.Button("Stop streaming"))
                {
                    beamManager.StopStreaming();
                }

                EditorGUILayout.LabelField("Recording state", EditorStyles.boldLabel);

                ShowColouredBox(
                    beamManager.IsRecording ? "Recording" : "Not recording",
                    beamManager.IsRecording ? _greenBg : _grayBg,
                    beamManager.IsRecording ? _red : Color.black);

                if (beamManager.IsRecording)
                {
                    if (GUILayout.Button("Stop recording"))
                    {
                        beamManager.StopRecording();
                    }
                }
                else
                {
                    if (GUILayout.Button("Start recording"))
                    {
                        beamManager.StartRecording();
                    }
                }

                if (beamManager.SessionState != null)
                {
                    EditorGUILayout.LabelField("Session capabilities");

                    var unicodeTick = "Yes";
                    var unicodeCross = "No";

                    EditorGUILayout.LabelField($"Can record: {(beamManager.SessionState.CanRecord ? unicodeTick : unicodeCross)}", EditorStyles.label);
                    EditorGUILayout.LabelField($"Can view in portal: {(beamManager.SessionState.IsPortalVisibilityEnabled ? unicodeTick : unicodeCross)}", EditorStyles.label);
                    EditorGUILayout.LabelField($"Can view in go-live link: {(beamManager.SessionState.IsGoLiveEnabled ? unicodeTick : unicodeCross)}", EditorStyles.label);

                    if (beamManager.SessionState.StreamPlatforms != null && beamManager.SessionState.StreamPlatforms.Count() > 0)
                    {
                        EditorGUILayout.LabelField($"Available Streaming Platforms", EditorStyles.boldLabel);

                        foreach (var service in beamManager.SessionState.StreamPlatforms)
                        {
                            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                            EditorGUILayout.LabelField($"{service.GetVisualPlatformName()} ({(service.IsStreaming ? "Streaming" : "Offline")})");
                            if (GUILayout.Button((service.IsStreaming ? "Stop" : "Start") + $" {service.GetVisualPlatformName()} Stream"))
                            {
                                if (service.IsStreaming)
                                {
                                    beamManager.StopSocialStream(service);
                                }
                                else
                                {
                                    beamManager.StartSocialStream(service);
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("User has no connected streaming platforms", EditorStyles.label);
                    }

                    if (beamManager.SessionState.CanChatServices != null && beamManager.SessionState.CanChatServices.Count() > 0)
                    {
                        EditorGUILayout.LabelField("Can go chat on services");

                        foreach (var service in beamManager.SessionState.CanChatServices)
                        {
                            EditorGUILayout.LabelField(service.ToString(), EditorStyles.label);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Can go chat on services: None", EditorStyles.label);
                    }
                }
            }
        }

        private void ShowColouredBox(string text, Texture2D background = null, Color textColor = default)
        {
            if (background == null)
            {
                background = _grayBg;
            }

            if (textColor == default)
            {
                textColor = Color.black;
            }

            // Create a label style.
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = textColor },
                active = { textColor = textColor },
                hover = { textColor = textColor },
                alignment = TextAnchor.MiddleCenter
            };

            // Draw the colored box
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = background },
                active = { background = background },
                hover = { background = background },
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(4, 4, 4, 4)
            };

            // Group label and box together
            GUILayout.BeginVertical(boxStyle);
            GUILayout.Label(text, labelStyle);
            GUILayout.EndVertical();
        }

        // Helper method to create a texture of a single color
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
