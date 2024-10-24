using BeamXR.Streaming.Core;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Gui
{
    [CustomEditor(typeof(BeamStreamingManager))]
    public class BeamStreamingManagerEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            if (target == null)
            {
                return;
            }

            Repaint();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BeamStreamingManager beamManager = (BeamStreamingManager)target;

            // Divider line
            AddDivider();

            if (EditorApplication.isPlaying)
            {
                ShowUserState(beamManager);
                ShowArchitectureState(beamManager);
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
                ShowColouredBox("Not logged in", Color.red, Color.white);
                if (GUILayout.Button("Sign in"))
                {
                    beamManager.Authenticate();
                }
            }
            else
            {
                ShowColouredBox(beamManager.Me.email, Color.green, Color.red);
                if (GUILayout.Button("Sign out"))
                {
                    beamManager.SignOut();
                }
            }
        }

        private void ShowArchitectureState(BeamStreamingManager beamManager)
        {
            // Label for Streaming State
            EditorGUILayout.LabelField("Architecture", EditorStyles.boldLabel);

            var architecture = "LAN Streaming Client";

            if (beamManager.StreamingArchitecture == StreamingArchitecture.StageBasedWebRTC)
            {
                architecture = "Cloud Stage";
            }

            ShowColouredBox(architecture);
        }

        private void ShowStreamingState(BeamStreamingManager beamManager)
        {
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
                if (GUILayout.Button("Start streaming"))
                {
                    beamManager.StartStreaming();
                }
            }
            else if (beamManager.StreamingState == StreamingState.Streaming)
            {
                if (GUILayout.Button("Stop streaming"))
                {
                    beamManager.StopStreaming();
                }

                EditorGUILayout.LabelField("Recording state", EditorStyles.boldLabel);

                ShowColouredBox(
                    beamManager.RecordingState == RecordingState.Recording ? "Recording" : "Not recording",
                    beamManager.RecordingState == RecordingState.Recording ? Color.green : Color.grey,
                    beamManager.RecordingState == RecordingState.Recording ? Color.red : Color.black);

                if (beamManager.RecordingState == RecordingState.Recording)
                {
                    if (GUILayout.Button("Stop recording"))
                    {
                        beamManager.StopRecording();
                    }
                }

                if (beamManager.RecordingState == RecordingState.NotRecording)
                {
                    if (GUILayout.Button("Start recording"))
                    {
                        beamManager.StartRecording();
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
