using BeamXR.Streaming.Core;
using BeamXR.Streaming.Core.Media;
using BeamXR.Streaming.Core.Models;
using BeamXR.Streaming.Core.Settings;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor
{
    [CustomEditor(typeof(BeamManager))]
    public class BeamManagerEditor : UnityEditor.Editor
    {
        private StreamAvailability _streamAvailability;
        private SerializedObject _serializedBeamSettings;
        private BeamSettings _beamSettings;
        private bool _advanced = false;
        private static string[] _advancedSettings = { "_credentialsManager", "_deviceFlowInstructionsManager" };

        private Color _green = new Color(0.157f, 0.6f, 0.31f), _red = new Color(0.68f, 0.11f, 0.11f);
        private Texture2D _grayBg, _greenBg, _yellowBg, _redBg;

        private float _boxHeight = 0;

        public override void OnInspectorGUI()
        {
            BeamManager beamManager = (BeamManager)target;
            _streamAvailability = beamManager.StreamAvailability;

            FindParts();

            DrawElements();

            if (EditorApplication.isPlaying)
            {
                AddDivider();

                ShowButtons(beamManager);
                ShowLocalRecording(beamManager);
                ShowPhoto(beamManager);

                ShowUserState(beamManager);
                ShowAvailableHosts(beamManager);
                ShowStreamingState(beamManager);
                
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void FindParts()
        {
            BackgroundTextures();

            _serializedBeamSettings = BeamSettingsProvider.GetSerializedSettings();
            _beamSettings = BeamSettingsProvider.GetOrCreateSettings();

            serializedObject.FindProperty("_beamSettings").objectReferenceValue = _beamSettings;

            _advanced = _serializedBeamSettings.FindProperty("_advancedSettings").boolValue;
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

        private void DrawElements()
        {
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), GetType(), false);

            if (GUILayout.Button(new GUIContent("BeamXR Settings", "Configure your experience and device settings"), GUILayout.Width(114)))
            {
                SettingsService.OpenProjectSettings("Project/BeamXR");
            }
            EditorGUILayout.EndHorizontal();

            if (_beamSettings.ExperienceKey == "" || _beamSettings.ExperienceSecret == "")
            {
                EditorGUILayout.HelpBox("You must set both an experience key and secret from the Beam Developer Portal before you can stream or record. Navigate to the BeamXR Settings to fix this.", MessageType.Error);
            }

            SerializedProperty serializedProperty = serializedObject.GetIterator();
            serializedProperty.NextVisible(enterChildren: true);
            while (serializedProperty.NextVisible(enterChildren: false))
            {
                if (!_advanced)
                {
                    if (_advancedSettings.Contains(serializedProperty.name))
                        continue;
                }
                EditorGUILayout.PropertyField(serializedProperty);
            }
        }

        private void AddDivider()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
        
        private void ShowButtons(BeamManager beamManager)
        {
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

                EditorGUILayout.LabelField("Stream Destination", EditorStyles.boldLabel);

                var newSelectionId = GUILayout.SelectionGrid(selection, hostNames.ToArray(), hostNames.Count);

                if (newSelectionId != selection)
                {
                    beamManager.SetPreferredStreamingHost(hostIds[newSelectionId]);
                    selection = newSelectionId;
                }

                var hostId = hostIds[selection];
                bool showButton = true;

                if (_streamAvailability == null || (hostId == "" && !_streamAvailability.CanCloudStream))
                {
                    showButton = false;
                }

                EditorGUILayout.Space();

                if (showButton && GUILayout.Button("Start Streaming"))
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
                if (GUILayout.Button("Stop Streaming"))
                {
                    beamManager.StopStreaming();
                }

                if (beamManager.IsCloudRecording)
                {
                    if (GUILayout.Button("Stop Recording"))
                    {
                        beamManager.StopRecording();
                    }
                }
                else
                {
                    if (GUILayout.Button("Start Recording"))
                    {
                        beamManager.StartRecording();
                    }
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Loading...");
                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space();
        }


        private void ShowAvailabilityBox(string name, bool value)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(value ? EditorGUIUtility.IconContent("P4_CheckOutRemote@2x") : EditorGUIUtility.IconContent("P4_DeletedLocal@2x"));

            EditorGUILayout.EndVertical();
        }

        private void ShowUserState(BeamManager beamManager)
        {
            // Label for Streaming State
            EditorGUILayout.LabelField("BeamXR User", EditorStyles.boldLabel);

            if (beamManager.Me == null)
            {
                if (beamManager.AuthState == AuthenticationState.Error)
                {
                    ShowColouredBox("Error signing in", _redBg, Color.white);

                    if (GUILayout.Button("Sign In"))
                    {
                        beamManager.Authenticate();
                    }

                    return;
                }

                if ((int)beamManager.AuthState > 0)
                {
                    ShowColouredBox("Signing in...", _yellowBg, Color.black);
                }
                else
                {
                    ShowColouredBox("Not logged in", _redBg, Color.white);

                    if (_beamSettings.ExperienceKey == "" || _beamSettings.ExperienceSecret == "")
                    {
                        EditorGUILayout.LabelField("Please assign an experience key and secret before trying to authenticate.");
                    }
                    else
                    {
                        if (GUILayout.Button("Sign In"))
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

                    if (GUILayout.Button("Open Browser"))
                    {
                        // Open the browser to the verification URL.
                        Application.OpenURL(beamManager.DeviceFlowCode.VerificationUrlComplete);
                    }
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                ShowColouredBox(beamManager.Me.email, _greenBg, Color.white);

                EditorGUI.BeginDisabledGroup(!(beamManager.StreamingState == StreamingState.Disconnected || beamManager.StreamingState == StreamingState.Error));
                if (GUILayout.Button("Sign Out", GUILayout.Width(60), GUILayout.Height(_boxHeight)))
                {
                    beamManager.SignOut();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();

                if (_streamAvailability != null)
                {
                    EditorGUILayout.BeginHorizontal();

                    var label = _streamAvailability.CloudMaxResolution.ToString();
                    var match = Regex.Match(label, @"\d");
                    if (match.Success)
                    {
                        label = label.Substring(match.Index);
                    }
                    ShowColouredBox(_streamAvailability.CanCloudStream ? $"Cloud Stream up to {label}" : "Unable to Cloud Stream", _streamAvailability.CanCloudStream ? _greenBg : _redBg, Color.white);

                    label = ((BeamResolution)System.Enum.GetValues(typeof(BeamResolution)).Cast<int>().Max()).ToString();
                    match = Regex.Match(label, @"\d");
                    if (match.Success)
                    {
                        label = label.Substring(match.Index);
                    }
                    ShowColouredBox(_streamAvailability.CanLocalStream ? $"Local Stream up to {label} " : "Unable to Cloud Stream", _streamAvailability.CanLocalStream ? _greenBg : _redBg, Color.white);

                    EditorGUILayout.EndHorizontal();
                    if (_streamAvailability.ErrorCode != 0)
                    {
                        ShowColouredBox($"Error: {_streamAvailability.ErrorCode} - {_streamAvailability.ErrorReason}", _redBg, Color.white);
                    }
                }

                if (beamManager.StreamPlatforms != null && beamManager.StreamPlatforms.Count() > 0)
                {
                    EditorGUILayout.LabelField($"Available Streaming Platforms", EditorStyles.boldLabel);

                    foreach (var service in beamManager.StreamPlatforms)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        if (beamManager.IsStreaming)
                        {
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
                        }
                        else
                        {
                            EditorGUILayout.LabelField(service.GetVisualPlatformName());
                            if (GUILayout.Button(service.AutoStream ? "Disable" : "Enable", GUILayout.Width(100)))
                            {
                                beamManager.ChangePlatformAutoStream(service, !service.AutoStream, null);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("User has no connected streaming platforms", EditorStyles.label);
                }
            }
        }

        private void ShowAvailableHosts(BeamManager beamManager)
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

        private void ShowStreamingState(BeamManager beamManager)
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
                // Done in ShowButtons
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

                EditorGUILayout.LabelField("Recording state", EditorStyles.boldLabel);

                ShowColouredBox(
                    beamManager.IsCloudRecording ? "Recording" : "Not recording",
                    beamManager.IsCloudRecording ? _greenBg : _grayBg,
                    beamManager.IsCloudRecording ? _red : Color.black);

                if (beamManager.SessionState != null)
                {
                    EditorGUILayout.LabelField("Session Capabilities");

                    var unicodeTick = "Yes";
                    var unicodeCross = "No";

                    EditorGUILayout.LabelField($"Can record: {(beamManager.SessionState.CanRecord ? unicodeTick : unicodeCross)}", EditorStyles.label);
                    EditorGUILayout.LabelField($"Can view in portal: {(beamManager.SessionState.IsPortalVisibilityEnabled ? unicodeTick : unicodeCross)}", EditorStyles.label);
                    EditorGUILayout.LabelField($"Can view in go-live link: {(beamManager.SessionState.IsGoLiveEnabled ? unicodeTick : unicodeCross)}", EditorStyles.label);

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

        private void ShowLocalRecording(BeamManager beamManager)
        {
            EditorGUI.BeginDisabledGroup(!beamManager.LocalRecordingAllowed || beamManager.IsRecordingProcessing);

            if (GUILayout.Button(beamManager.IsLocalRecording ? "Stop Local Recording" : "Start Local Recording"))
            {
                if (beamManager.IsLocalRecording)
                {
                    beamManager.StopRecording();
                }
                else
                {
                    beamManager.StartRecording();
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private void ShowPhoto(BeamManager beamManager)
        {
            if (GUILayout.Button("Take Photo"))
            {
                beamManager.SavePhoto();
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

            if (_boxHeight == 0)
            {
                _boxHeight = labelStyle.CalcHeight(new GUIContent("test"), 40) + 8;
            }

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
