using UnityEngine;
using BeamXR.Streaming.Core;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace BeamXR.Streaming.Example
{
    public class BeamInteractionPanel : MonoBehaviour
    {
        #region Fields
        private string _streamingUrl;
        private AuthenticationState _previousState;
        private StreamingState _knownStreamingState;
        private bool _knownRecordingState;
        private BeamManager _beamManager;
        private BeamUnityEvents _unityEvents;

        [SerializeField]
        private Text _authStatusText, _streamingStatusText;

        [SerializeField]
        private Text _infoText, _recordingText;

        [SerializeField]
        private CanvasGroup _streamingButtons, _recordingButtons;

        [SerializeField]
        private Button _streamPositiveButton, _streamNegativeButton;

        [SerializeField]
        private Button _recordPositiveButton, _recordNegativeButton;

        private Text _streamPositiveText, _streamNegativeText, _recordPositiveText, _recordNegativeText;

        #endregion Fields

        #region Game object lifecycle

        void Awake()
        {
            FindParts();
        }

        private void FindParts()
        {
            if (_beamManager == null)
            {
                _beamManager = FindFirstObjectByType<BeamManager>(FindObjectsInactive.Include);
            }
            if (_unityEvents == null)
            {
                _unityEvents = FindFirstObjectByType<BeamUnityEvents>(FindObjectsInactive.Include);
            }

            if (_streamPositiveText == null)
            {
                _streamPositiveText = _streamPositiveButton.GetComponentInChildren<Text>(true);
            }
            if (_streamNegativeText == null)
            {
                _streamNegativeText = _streamNegativeButton.GetComponentInChildren<Text>(true);
            }
            if (_recordPositiveText == null)
            {
                _recordPositiveText = _recordPositiveButton.GetComponentInChildren<Text>(true);
            }
            if (_recordNegativeText == null)
            {
                _recordNegativeText = _recordNegativeButton.GetComponentInChildren<Text>(true);
            }
        }

        private void OnEnable()
        {
            FindParts();

            if (_beamManager == null)
            {
                gameObject.SetActive(false);
                return;
            }

            if (_unityEvents != null)
            {
                BindEvents(true);
            }

            OnAuthenticationState(_beamManager.AuthState);
            OnStreamingState(_beamManager.StreamingState);
            OnRecordingState(CaptureType.Cloud, _beamManager.RecordingState);
        }

        private void OnDisable()
        {
            if (_unityEvents != null)
            {
                BindEvents(false);
            }
        }

        private void BindEvents(bool bind)
        {
            if (bind)
            {
                _unityEvents.OnAuthenticationChanged.AddListener(OnAuthenticationState);
                _unityEvents.OnStreamingStateChanged.AddListener(OnStreamingState);
                _unityEvents.OnRecordingStateChanged.AddListener(OnRecordingState);
            }
            else
            {
                _unityEvents.OnAuthenticationChanged.RemoveListener(OnAuthenticationState);
                _unityEvents.OnStreamingStateChanged.RemoveListener(OnStreamingState);
                _unityEvents.OnRecordingStateChanged.RemoveListener(OnRecordingState);
            }
        }

        private void OnAuthenticationState(AuthenticationState state)
        {
            _authStatusText.text = state.ToString();

            switch (state)
            {
                case AuthenticationState.Error:
                case AuthenticationState.NotAuthenticated:
                    if (_infoText != null)
                    {
                        _infoText.text = "Please log in to BeamXR to start streaming.\nYou can local record without logging in.";
                    }
                    SetRecordingButtons("Start Recording", () => _beamManager.StartRecording());

                    SetStreamingButtons("Log in to Beam", () =>
                    {
                        _beamManager.Authenticate();
                    });
                    break;
                case AuthenticationState.Loading:

                    if (_streamingButtons != null)
                    {
                        _streamingButtons.interactable = false;
                    }
                    if (_recordingButtons != null)
                    {
                        _recordingButtons.interactable = false;
                    }

                    if (_infoText != null)
                    {
                        _infoText.text = "Loading login...";
                    }

                    SetStreamingButtons();
                    break;
                case AuthenticationState.Authenticating:

                    if (_streamingButtons != null)
                    {
                        _streamingButtons.interactable = true;
                    }
                    if (_recordingButtons != null)
                    {
                        _recordingButtons.interactable = false;
                    }

                    if (_infoText != null)
                    {
                        _infoText.text = $"Please visit {_beamManager.DeviceFlowCode.VerificationUrl} and enter the code {_beamManager.DeviceFlowCode.UserCode}";
                    }

                    SetStreamingButtons("Open Browser", () =>
                    {
                        Application.OpenURL(_beamManager.DeviceFlowCode.VerificationUrlComplete);
                    });
                    break;
                case AuthenticationState.Authenticated:

                    if (_streamingButtons != null)
                    {
                        _streamingButtons.interactable = true;
                    }
                    if (_recordingButtons != null)
                    {
                        _recordingButtons.interactable = true;
                    }

                    StreamingButtonOnAuthenticated();
                    OnStreamingState(_beamManager.StreamingState);
                    break;
            }
        }

        private void OnStreamingState(StreamingState state)
        {
            if (_beamManager.AuthState != AuthenticationState.Authenticated)
            {
                return;
            }

            switch (state)
            {
                case StreamingState.Disconnected:
                    if (_infoText != null)
                    {
                        _infoText.text = $"Idle, ready to stream to {_beamManager.HostName}";
                    }
                    break;
                case StreamingState.Streaming:
                    if (_infoText != null)
                    {
                        if (_beamManager.HostName.Equals("Cloud", StringComparison.OrdinalIgnoreCase))
                        {
                            string platforms = "";

                            foreach (var item in _beamManager.StreamPlatforms)
                            {
                                if (item.IsStreaming)
                                {
                                    platforms += item.GetVisualPlatformName() + ", ";
                                }
                            }

                            if (platforms == "")
                            {
                                _infoText.text = $"Streaming to BeamXR, but not publicly viewable on any platform.";
                            }
                            else
                            {
                                platforms = platforms.Substring(0, platforms.Length - 3);
                                _infoText.text = $"Streaming to BeamXR, viewable on " + platforms + ".";
                            }
                        }
                        else
                        {
                            _infoText.text = $"Your stream is viewable in the app.";
                        }
                    }

                    OnRecordingState(CaptureType.Cloud, _beamManager.RecordingState);
                    break;
                case StreamingState.Error:
                    if (_infoText != null)
                    {
                        _infoText.text = "An error occurred. Please try again.";
                    }
                    break;
                default:
                    if (_infoText != null)
                    {
                        _infoText.text = state.ToString();
                    }
                    break;
            }
            if (_streamingStatusText != null)
            {
                _streamingStatusText.text = state.ToString();
            }
        }

        private void OnRecordingState(CaptureType capture, RecordingState state)
        {
            if (_streamingButtons != null)
            {
                switch (capture)
                {
                    case CaptureType.Local:
                        _streamingButtons.interactable = (int)state <= 0;
                        break;
                    case CaptureType.Cloud:
                        _streamingButtons.interactable = (int)state <= 0;
                        break;
                }
            }
            if (_recordingText != null)
            {
                _recordingText.text = (_beamManager.IsStreaming ? "Cloud" : "Local") + " Recording State - " + _beamManager.RecordingState;
            }
        }

        #endregion Game object lifecycle

        #region Methods

        private void StreamingButtonOnAuthenticated()
        {
            SetStreamingButtons("Start Streaming", () => _beamManager.StartStreaming(), "Stop Streaming", () => _beamManager.StopStreaming());

            if (_streamPositiveButton != null)
            {
                _streamPositiveButton.interactable = !_beamManager.IsStreaming;
            }
            if (_streamNegativeButton != null)
            {
                _streamNegativeButton.interactable = _beamManager.IsStreaming;
            }
        }

        private void SetRecordingButtons(string positive = "", System.Action positiveClick = null, string negative = "", System.Action negativeClick = null)
        {
            SetupButton(_recordPositiveText, _recordPositiveButton, positive, positiveClick);
            SetupButton(_recordNegativeText, _recordNegativeButton, negative, negativeClick);
        }

        private void SetStreamingButtons(string positive = "", System.Action positiveClick = null, string negative = "", System.Action negativeClick = null)
        {
            SetupButton(_streamPositiveText, _streamPositiveButton, positive, positiveClick);
            SetupButton(_streamNegativeText, _streamNegativeButton, negative, negativeClick);
        }

        private void SetupButton(Text textObject, Button button, string text, System.Action action)
        {
            if (button == null)
                return;

            button.gameObject.SetActive(action != null);
            if (action != null)
            {
                textObject.text = text;
                button.onClick.RemoveAllListeners();

                button.onClick.AddListener(() =>
                {
                    action?.Invoke();
                });
            }
        }
        #endregion Methods
    }

}