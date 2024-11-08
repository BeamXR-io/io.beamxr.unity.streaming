using UnityEngine;
using TMPro;
using BeamXR.Streaming.Core;
using System.Linq;
using System.Collections.Generic;

namespace BeamXR.Streaming.Gui
{
    [RequireComponent(typeof(Canvas))]
    public class BeamInteractionPanel : MonoBehaviour
    {
        #region Fields

        private Canvas _canvas;
        private float _timeUntilNextClick = 2.0f;
        private string _streamingUrl;
        private AuthenticationState _knownAuthState;
        private StreamingState _knownStreamingState;
        private bool _knownRecordingState;
        private bool _initialised = false;

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
        private string _knownAvailableHosts;

        #endregion Fields

        #region Game object lifecycle

        void Start()
        {
            DontDestroyOnLoad(gameObject);

            _canvas = GetComponent<Canvas>();
        }

        void Awake()
        {
            HidePositiveButton();
            HideNegativeButton();
            HideMessage();
            HideStatus();
            _initialised = false;
        }

        // Update is called once per frame
        void Update()
        {
            var beamStreamingManager = FindObjectOfType<BeamStreamingManager>();

            if (beamStreamingManager == null)
            {
                // Hide all the buttons.
                Hide();
                return;
            }

            _canvas.enabled = true;

            _timeUntilNextClick -= Time.deltaTime;

            CheckButtonInteractions();

            bool requiresUpdate = false;

            if (!_initialised)
            {
                _initialised = true;
                requiresUpdate = true;
            }

            // If the auth state or streaming state has changed, update the UI.
            if (beamStreamingManager.AuthState != _knownAuthState)
            {
                requiresUpdate = true;
                _knownAuthState = beamStreamingManager.AuthState;
            }

            if (beamStreamingManager.StreamingState != _knownStreamingState || beamStreamingManager.SessionState?.IsRecording != _knownRecordingState)
            {
                requiresUpdate = true;
                _knownStreamingState = beamStreamingManager.StreamingState;
                _knownRecordingState = beamStreamingManager.SessionState?.IsRecording ?? false;
            }

            var beamHostsIds = new List<string>();

            if (beamStreamingManager.AvailableStreamingHosts != null && beamStreamingManager.AvailableStreamingHosts.Count() > 0)
            {
                beamHostsIds = beamStreamingManager.AvailableStreamingHosts.Select(x => x.id).OrderBy(x => x).ToList();
            }

            var joinedHosts = string.Join(",", beamHostsIds);

            if (joinedHosts != _knownAvailableHosts)
            {
                requiresUpdate = true;
                _knownAvailableHosts = joinedHosts;
            }

            HidePositiveButton();
            HideNegativeButton();
            HideMessage();
            HideStatus();

            if (beamStreamingManager.AuthState == AuthenticationState.NotAuthenticated || beamStreamingManager.AuthState == AuthenticationState.Error)
            {
                ShowStatus("You'll need to log in to Beam to start streaming.");

                ShowMessage("Please use the button below to log in to Beam.");

                ShowPositiveButton("Log in to Beam", () =>
                {
                    beamStreamingManager.Authenticate();
                });

                return;
            }

            // Set which buttons are available based on the state.
            if (beamStreamingManager.AuthState == AuthenticationState.Authenticating)
            {
                ShowStatus("Authenticating");

                if (beamStreamingManager.DeviceFlowCode != null)
                {
                    ShowMessage($"Please visit {beamStreamingManager.DeviceFlowCode.VerificationUrl} and enter the code {beamStreamingManager.DeviceFlowCode.UserCode}");
                }
                else
                {
                    ShowMessage("Please wait while we authenticate you.");
                }

                return;
            }

            // We're authenticated. Branch the logic on the streaming state.
            ShowMessage(beamStreamingManager.GetStreamerInstructions());

            switch (beamStreamingManager.StreamingState)
            {
                case StreamingState.Disconnected:
                    {
                        ShowStatus("Offline");
                        
                        ShowPositiveButton("Start streaming", () =>
                        {
                            beamStreamingManager.StartStreaming();
                        });
                    }
                    break;
                case StreamingState.Error:
                    {
                        ShowStatus("Error");
                        ShowPositiveButton("Retry", () =>
                        {
                            beamStreamingManager.StartStreaming();
                        });
                    }
                    break;
                case StreamingState.Streaming:
                    {
                        // Check if we're recording.
                        if (beamStreamingManager.SessionState?.IsRecording == true)
                        {
                            ShowStatus("Recording");
                            ShowNegativeButton("Stop recording", () =>
                            {
                                beamStreamingManager.StopRecording();
                            });
                        }
                        else
                        {
                            ShowStatus("Streaming");
                            
                            if (beamStreamingManager.SessionState.CanRecord)
                            {
                                ShowPositiveButton("Start recording", () =>
                                {
                                    beamStreamingManager.StartRecording();
                                });
                            }

                            ShowNegativeButton("Stop streaming", () =>
                            {
                                beamStreamingManager.StopStreaming();
                            });
                        }
                    }
                    break;
                case StreamingState.CreatingSession:
                case StreamingState.Connecting:
                    ShowStatus("Connecting");
                    break;
                case StreamingState.Connected:
                    ShowStatus("Connected");
                    break;
                case StreamingState.Disconnecting:
                    ShowStatus("Disconnecting ");
                    break;
                default:
                    break;
            }
        }

        void CheckButtonInteractions()
        {
            var positiveButtonCollider = _positiveButton.GetComponent<BoxCollider>();
            var negativeButtonCollider = _negativeButton.GetComponent<BoxCollider>();

            if (positiveButtonCollider == null)
            {
                positiveButtonCollider = _positiveButton.AddComponent<BoxCollider>();
            }

            if (negativeButtonCollider == null)
            {
                negativeButtonCollider = _negativeButton.AddComponent<BoxCollider>();
            }

            if (_positiveButton.activeInHierarchy && positiveButtonCollider != null)
            {
                positiveButtonCollider.size = new Vector2(_positiveButton.GetComponent<RectTransform>().rect.width, _positiveButton.GetComponent<RectTransform>().rect.height);

                // Check to see if the button is being interacted with by any collider.
                if (_buttonInteractionColliders.Any(x => x.bounds.Intersects(positiveButtonCollider.bounds)))
                {
                    if (_timeUntilNextClick <= 0)
                    {
                        SetPositiveButtonColour(new Color(0.0f, (1.0f / 255.0f) * 173.0f, (1.0f / 255.0f) * 16.0f, 255.0f));
                        _timeUntilNextClick = 2.0f;
                        
                        var button = _positiveButton.GetComponent<UnityEngine.UI.Button>();
                        button.onClick.Invoke();
                    }
                }
                else
                {
                    ResetPositiveButtonColour();
                }
            }

            if (_negativeButton.activeInHierarchy && negativeButtonCollider != null)
            {
                negativeButtonCollider.size = new Vector2(_negativeButton.GetComponent<RectTransform>().rect.width, _negativeButton.GetComponent<RectTransform>().rect.height);

                // Check to see if the button is being interacted with by any collider (not just the interaction collider).
                if (_buttonInteractionColliders.Any(x => x.bounds.Intersects(negativeButtonCollider.bounds)))
                {
                    if (_timeUntilNextClick <= 0)
                    {
                        SetNegativeButtonColour(new Color(0.0f, (1.0f / 255.0f) * 173.0f, (1.0f / 255.0f) * 16.0f, 255.0f));
                        _timeUntilNextClick = 2.0f;

                        var button = _negativeButton.GetComponent<UnityEngine.UI.Button>();
                        button.onClick.Invoke();
                    }
                }
                else
                {
                    ResetNegativeButtonColour();
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

        #endregion Methods
    }

}