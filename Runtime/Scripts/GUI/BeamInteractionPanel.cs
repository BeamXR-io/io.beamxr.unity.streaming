using BeamXR.Streaming.Core.Auth.DeviceFlow;
using BeamXR.Streaming.Core.StreamState;
using UnityEngine;
using TMPro;
using System.Linq;
using BeamXR.Streaming.Core;

namespace BeamXR.Streaming.Gui
{
    [RequireComponent(typeof(Canvas))]
    public class BeamInteractionPanel : MonoBehaviour, IDeviceFlowInstructionsManager, IStreamStateDisplayManager
    {
        #region Fields

        [SerializeField]
        private Color _positiveButtonColour = new Color(0.0f, (1.0f / 255.0f) * 173.0f, (1.0f / 255.0f) * 16.0f, 255.0f);

        [SerializeField]
        private Color _negativeButtonColour = new Color((1.0f / 255.0f) * 173.0f, (1.0f / 255.0f) * 2.0f, 0.0f, 255.0f);

        [SerializeField]
        private GameObject _parentContainer;

        [SerializeField]
        private GameObject _statusLabelContainer;

        [SerializeField]
        private GameObject _statusLabel;

        [SerializeField]
        private GameObject _bodyTextLabel;

        [SerializeField]
        private GameObject _bodyTextLabelContainer;

        [SerializeField]
        private GameObject _positiveButton;

        [SerializeField]
        private GameObject _negativeButton;

        [SerializeField]
        private GameObject _buttonContainer;

        [SerializeField]
        [Tooltip("A list of colliders which can interact with buttons by intersection. Attach small colliders to things like your controllers and index finger prefabs.")]
        private Collider[] _buttonInteractionColliders;

        private Canvas _canvas;
        private float _timeUntilNextClick = 10.0f;
        private string _streamingUrl;

        #endregion Fields

        #region Game object lifecycle

        void Start()
        {
            DontDestroyOnLoad(gameObject);

            _canvas = GetComponent<Canvas>();

            // Find every button in the children of this and add the CubeButton component.
            foreach (var button in GetComponentsInChildren<UnityEngine.UI.Button>())
            {
                var cubeButton = button.gameObject.AddComponent<BeamCubeButton>();
            }

            Hide();
        }

        void Awake()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (_timeUntilNextClick > 0.0f)
            {
                _timeUntilNextClick -= Time.deltaTime;
                return;
            }

            CheckCubeInteractions();
        }

        void CheckCubeInteractions()
        {
            // Get all the cubes (buttons) in the scene.
            var cubes = FindObjectsOfType<BeamCubeButton>();

            foreach (var cube in cubes)
            {
                // Get the cube collider.
                var cubeCollider = cube.GetCollider();

                if (_buttonInteractionColliders != null)
                {
                    foreach (var collider in _buttonInteractionColliders)
                    {
                        // Check the item is active.
                        if (!collider.gameObject.activeInHierarchy)
                        {
                            continue;
                        }

                        // Check to see if it is colliding with the cube.
                        if (cubeCollider.bounds.Intersects(collider.bounds))
                        {
                            // Log the name of the object.
                            Debug.Log($"Colliding with {collider.gameObject.name}");

                            // Get the button.
                            var button = cube.GetComponentInParent<UnityEngine.UI.Button>();

                            // See if the button is interactable and active.
                            if (button != null && button.interactable && button.gameObject.activeInHierarchy)
                            {
                                // Simulate a click.
                                button.onClick.Invoke();

                                _timeUntilNextClick = 3.0f;

                                return;
                            }
                        }
                    }
                }
            }
        }


        #endregion Game object lifecycle

        #region Methods

        void Hide()
        {
            _canvas.enabled = false;

            _statusLabelContainer.SetActive(false);

            _bodyTextLabelContainer.SetActive(false);

            _buttonContainer.SetActive(false);

            _positiveButton.SetActive(false);

            _negativeButton.SetActive(false);
        }

        void HideStatus()
        {
            _statusLabelContainer.SetActive(false);
        }

        void HideMessage()
        {
            _bodyTextLabelContainer.SetActive(false);
        }

        void HidePositiveButton()
        {
            _positiveButton.SetActive(false);
        }

        void HideNegativeButton()
        {
            _negativeButton.SetActive(false);
        }

        void ShowStatus(string status)
        {
            _statusLabelContainer.SetActive(true);

            _statusLabel.GetComponent<TMP_Text>().text = status;

            _canvas.enabled = true;
        }

        void ShowMessage(string message)
        {
            _bodyTextLabelContainer.SetActive(true);

            _bodyTextLabel.GetComponent<TMP_Text>().text = message;

            _canvas.enabled = true;
        }

        void ResetPositiveButtonColour()
        {
            _positiveButton.GetComponent<UnityEngine.UI.Button>().image.color = _positiveButtonColour;
        }

        void ResetNegativeButtonColour()
        {
            _negativeButton.GetComponent<UnityEngine.UI.Button>().image.color = _negativeButtonColour;
        }

        void SetPositiveButtonColour(Color colour)
        {
            _positiveButton.GetComponent<UnityEngine.UI.Button>().image.color = colour;
        }

        void SetNegativeButtonColour(Color colour)
        {
            _negativeButton.GetComponent<UnityEngine.UI.Button>().image.color = colour;
        }

        void ShowPositiveButton(string text, System.Action onClick)
        {
            _buttonContainer.SetActive(true);

            _positiveButton.SetActive(true);

            var textWithButtonIndicator = $"{text}";

            _positiveButton.GetComponentInChildren<TMP_Text>().text = textWithButtonIndicator;

            var button = _positiveButton.GetComponent<UnityEngine.UI.Button>();

            // Remnove all listeners
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() =>
            {
                _buttonContainer.SetActive(false);
                onClick?.Invoke();
            });

            ResetPositiveButtonColour();
        }

        void ShowNegativeButton(string text, System.Action onClick)
        {
            _buttonContainer.SetActive(true);

            _negativeButton.SetActive(true);

            var textWithButtonIndicator = $"{text}";

            _negativeButton.GetComponentInChildren<TMP_Text>().text = textWithButtonIndicator;

            var button = _negativeButton.GetComponent<UnityEngine.UI.Button>();

            // Remnove all listeners
            button.onClick.RemoveAllListeners();

            button.onClick.AddListener(() =>
            {
                _buttonContainer.SetActive(false);
                onClick?.Invoke();
            });

            ResetNegativeButtonColour();
        }

        string GetHumanReadableButtonName(KeyCode keyCode)
        {
            // Get the name of the key code depending on the platform.
            var buttonName = keyCode.ToString();

            // If the button name is a single letter then we can just return that.
            if (buttonName.Length == 1)
            {
                return buttonName;
            }

            // If the button name is a joystick button then we need to map it to the correct button depending on the platform (XBox, PS4, Meta Quest).
            if (buttonName.StartsWith("Joystick", System.StringComparison.OrdinalIgnoreCase))
            {
                // If the button name is a joystick button then we need to map it to the correct button depending on the platform (XBox, PS4, Meta Quest).
                if (buttonName.EndsWith("Button0"))
                {
                    buttonName = "A";
                }
                else if (buttonName.EndsWith("Button1"))
                {
                    buttonName = "B";
                }
                else if (buttonName.EndsWith("Button2"))
                {
                    buttonName = "X";
                }
                else if (buttonName.EndsWith("Button3"))
                {
                    buttonName = "Y";
                }
                else if (buttonName.EndsWith("Button4"))
                {
                    buttonName = "LB";
                }
                else if (buttonName.EndsWith("Button5"))
                {
                    buttonName = "RB";
                }
                else if (buttonName.EndsWith("Button6"))
                {
                    buttonName = "Back";
                }
                else if (buttonName.EndsWith("Button7"))
                {
                    buttonName = "Start";
                }
                else if (buttonName.EndsWith("Button8"))
                {
                    buttonName = "LS";
                }
                else if (buttonName.EndsWith("Button9"))
                {
                    buttonName = "RS";
                }
            }

            return buttonName;
        }

        #endregion Methods

        #region IDeviceFlowInstructionsManager implementation

        void IDeviceFlowInstructionsManager.ShowError(string error)
        {
            Hide();

            ShowStatus("Error");

            ShowMessage(error);

            ShowPositiveButton("Retry", () =>
            {
                // Try and get the beam streaming manager.
                var beamStreamingManager = FindObjectOfType<BeamStreamingManager>();

                if (beamStreamingManager != null)
                {
                    beamStreamingManager.Authenticate();
                }

                HidePositiveButton();
            });
        }

        void IDeviceFlowInstructionsManager.ShowSignout()
        {
            Hide();

            ShowStatus("Logged out");

            ShowPositiveButton("Sign in", () =>
            {
                // Try and get the beam streaming manager.
                var beamStreamingManager = FindObjectOfType<BeamStreamingManager>();

                if (beamStreamingManager != null)
                {
                    beamStreamingManager.Authenticate();
                }

                HidePositiveButton();
            });
        }

        void IDeviceFlowInstructionsManager.ShowSuccess(string message)
        {
            Hide();

            ShowStatus("Authenticated");
        }

        void IDeviceFlowInstructionsManager.ShowUserCode(string userCode, string verificationUri)
        {
            Hide();

            ShowStatus("Please sign in");

            ShowMessage($"Visit: \n{verificationUri}\n\nand enter code:\n{userCode}");
        }

        #endregion IDeviceFlowInstructionsManager implementation

        #region IStreamStateDisplayManager implementation

        void IStreamStateDisplayManager.ShowAvailableHosts(System.Collections.Generic.IEnumerable<BeamXR.Streaming.Core.StreamingHostSummary> hosts)
        {
            ShowStatus(hosts.Any() ? "Available hosts" : "No hosts available.");

            // Build a list of hosts as a string.
            var hostList = string.Empty;

            foreach (var host in hosts)
            {
                hostList += $"{host.hostName}\n";
            }

            ShowMessage(hosts.Any() ? $"The following hosts are available: {hostList}" : "Cloud streaming is available");
        }

        void IStreamStateDisplayManager.ShowRecordingStarted()
        {
            ShowStatus("Streaming + Recording");

            ShowMessage($"Now streaming and recording. To watch along visit \n{_streamingUrl}");

            ShowPositiveButton("Stop recording", () =>
            {
                // Try and get the beam streaming manager.
                var beamStreamingManager = FindObjectOfType<BeamStreamingManager>();

                if (beamStreamingManager != null)
                {
                    beamStreamingManager.StopRecording();
                }

                HidePositiveButton();
            });

            ShowNegativeButton("Stop streaming", () =>
            {
                // Try and get the beam streaming manager.
                var beamStreamingManager = FindObjectOfType<BeamStreamingManager>();

                if (beamStreamingManager != null)
                {
                    beamStreamingManager.StopStreaming();
                }

                HideNegativeButton();
                HidePositiveButton();
            });

            SetPositiveButtonColour(_negativeButtonColour);
        }

        void IStreamStateDisplayManager.ShowRecordingStopped()
        {
            ShowStatus("Streaming. Recording stopped.");

            ShowPositiveButton("Start recording", () =>
            {
                // Try and get the beam streaming manager.
                var beamStreamingManager = FindObjectOfType<BeamStreamingManager>();

                if (beamStreamingManager != null)
                {
                    beamStreamingManager.StartRecording();
                }

                HidePositiveButton();
            });

            ShowNegativeButton("Stop streaming", () =>
            {
                // Try and get the beam streaming manager.
                var beamStreamingManager = FindObjectOfType<BeamStreamingManager>();

                if (beamStreamingManager != null)
                {
                    beamStreamingManager.StopStreaming();
                }

                HideNegativeButton();
            });
        }

        void IStreamStateDisplayManager.ShowError(string error)
        {
            Hide();

            ShowStatus("Streaming error");

            ShowMessage(error);

            ShowPositiveButton("Retry", () =>
            {
                // Try and get the beam streaming manager.
                var beamStreamingManager = FindObjectOfType<BeamStreamingManager>();

                if (beamStreamingManager != null)
                {
                    beamStreamingManager.StartStreaming();
                }

                HidePositiveButton();
            });
        }

        void IStreamStateDisplayManager.ShowWatchUrl(string url)
        {
            _streamingUrl = url;

            Hide();

            ShowStatus("Streaming");

            ShowMessage($"Now streaming. To watch the stream go to:\n{url}");

            ShowPositiveButton("Start recording", () =>
            {
                // Try and get the beam streaming manager.
                var beamStreamingManager = FindObjectOfType<BeamStreamingManager>();

                if (beamStreamingManager != null)
                {
                    beamStreamingManager.StartRecording();
                }

                HidePositiveButton();
            });

            ShowNegativeButton("Stop streaming", () =>
            {
                // Try and get the beam streaming manager.
                var beamStreamingManager = FindObjectOfType<BeamStreamingManager>();

                if (beamStreamingManager != null)
                {
                    beamStreamingManager.StopStreaming();
                }

                HideNegativeButton();
            });
        }

        void IStreamStateDisplayManager.ShowStreamEnded()
        {
            Hide();

            ShowStatus("Stream ended");

            ShowMessage("To start streaming again please press below");

            ShowPositiveButton("Start streaming", () =>
            {
                // Try and get the beam streaming manager.
                var beamStreamingManager = FindObjectOfType<BeamStreamingManager>();

                if (beamStreamingManager != null)
                {
                    beamStreamingManager.StartStreaming();
                }

                HidePositiveButton();
            });
        }

        #endregion IStreamStateDisplayManager implementation
    }

}