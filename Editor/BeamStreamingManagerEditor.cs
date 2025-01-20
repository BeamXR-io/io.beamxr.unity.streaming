using BeamXR.Streaming.Core;
using BeamXR.Streaming.Core.Models.StreamingHosts;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Gui
{
    [CustomEditor(typeof(BeamStreamingManager))]
    public class BeamStreamingManagerEditor : UnityEditor.Editor
    { 
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BeamStreamingManager beamManager = (BeamStreamingManager)target;

            // Divider line
            AddDivider();

            if (EditorApplication.isPlaying)
            {
                ShowUserState(beamManager);
                ShowAvailableHosts(beamManager);
                ShowStreamingState(beamManager);
            }
        }

        private void AddDivider()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void ShowUserState(BeamStreamingManager beamManager)
        {
            // Label for Streaming State
            EditorGUILayout.LabelField("BeamXR user", EditorStyles.boldLabel);

            if (beamManager.Me == null)
            {
                if (beamManager.AuthState == AuthenticationState.Error)
                {
                    ShowColouredBox("Error signing in", Color.red, Color.white);

                    if (GUILayout.Button("Sign in"))
                    {
                        beamManager.Authenticate();
                    }

                    return;
                }

                if (beamManager.AuthState == AuthenticationState.Authenticating)
                {
                    ShowColouredBox("Signing in...", Color.yellow, Color.black);
                }
                else
                {
                    ShowColouredBox("Not logged in", Color.red, Color.white);

                    if (GUILayout.Button("Sign in"))
                    {
                        beamManager.Authenticate();
                    }
                }

                if (beamManager.DeviceFlowCode != null)
                {
                    EditorGUILayout.LabelField("Device flow code", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Go to the following URL:");
                    ShowColouredBox(beamManager.DeviceFlowCode.VerificationUrl, Color.green, Color.black);
                    EditorGUILayout.LabelField($"Enter the following code:");
                    ShowColouredBox(beamManager.DeviceFlowCode.UserCode, Color.green, Color.black);

                    if (GUILayout.Button("Open browser"))
                    {
                        // Open the browser to the verification URL.
                        Application.OpenURL(beamManager.DeviceFlowCode.VerificationUrlComplete);
                    }
                }
            }
            else
            {
                ShowColouredBox(beamManager.Me.email, Color.green, Color.red);
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
            EditorGUILayout.LabelField("Available hosts", EditorStyles.boldLabel);

            if (beamManager.AvailableStreamingHosts == null || beamManager.AvailableStreamingHosts.Length == 0)
            {
                ShowColouredBox("No streaming hosts available - will stream to cloud", Color.red, Color.white);
            }
            else
            {
                foreach (var host in beamManager.AvailableStreamingHosts)
                {
                    ShowColouredBox(host.hostName, Color.gray, Color.black);
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
            EditorGUILayout.LabelField("Streaming state", EditorStyles.boldLabel);

            // Set the color based on the current status
            Color statusColor;
            Color statusTextColor;
            switch (beamManager.StreamingState)
            {
                case StreamingState.Disconnected:
                    statusTextColor = Color.black;
                    statusColor = Color.gray;
                    break;
                case StreamingState.CreatingSession:
                case StreamingState.Disconnecting:
                case StreamingState.Connecting:
                    statusTextColor = Color.black;
                    statusColor = Color.yellow;
                    break;
                case StreamingState.Error:
                    statusTextColor = Color.white;
                    statusColor = Color.red;
                    break;
                case StreamingState.Streaming:
                case StreamingState.Connected:
                    statusTextColor = Color.red;
                    statusColor = Color.green;
                    break;
                default:
                    statusColor = Color.white;
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

                if (GUILayout.Button("Start streaming"))
                {
                    var hostId = hostIds[selection];

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
                    beamManager.IsRecording ? Color.green : Color.grey,
                    beamManager.IsRecording ? Color.red : Color.black);

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

                    if (beamManager.SessionState.CanGoLiveServices != null && beamManager.SessionState.CanGoLiveServices.Count() > 0)
                    {
                        EditorGUILayout.LabelField("Can go live on services");

                        foreach (var service in beamManager.SessionState.CanGoLiveServices)
                        {
                            EditorGUILayout.LabelField(service.ToString(), EditorStyles.label);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Can go live on services: None", EditorStyles.label);
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

        private void ShowColouredBox(string text, Color backgroundColor = default, Color textColor = default)
        {
            if (backgroundColor == default)
            {
                backgroundColor = Color.gray;
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
                normal = { background = MakeTex(2, 2, backgroundColor) },
                active = { background = MakeTex(2, 2, backgroundColor) },
                hover = { background = MakeTex(2, 2, backgroundColor) },
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 5, 5)
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
