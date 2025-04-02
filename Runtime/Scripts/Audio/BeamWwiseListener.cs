using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeamXR.Streaming.Core.Audio
{
    [HelpURL("http://docs.beamxr.io/sdk-guides/core/third-party-audio")]
    public class BeamWwiseListener : BeamAudioListener
    {
#if BEAM_WWISE
        [SerializeField, HideInInspector, Range(9, 16)]
        private int _wwiseBufferMultiple = 12;

        private ulong _wwiseDeviceId;
        private bool _wwiseBound = false;

        private IEnumerator Start()
        {
            while (!AkSoundEngine.IsInitialized())
            {
                yield return null;
            }

            AkOutputSettings outputSettings = new AkOutputSettings
            {
                channelConfig = AkChannelConfig.Standard(AkSoundEngine.AK_SPEAKER_SETUP_STEREO),
                idDevice = 0,
                audioDeviceShareset = AkSoundEngine.AK_INVALID_UNIQUE_ID,
                ePanningRule = AkPanningRule.AkPanningRule_Speakers
            };

            var outputResult = AkSoundEngine.AddOutput(outputSettings, out _wwiseDeviceId);
            if (outputResult == AKRESULT.AK_Success)
            {
                BeamLogger.Log($"Using Wwise device: {_wwiseDeviceId}");
            }
            else
            {
                BeamLogger.LogError($"Unable to use Wwise device. Error: {outputResult.ToString()}");
                yield return null;
            }

            AkAudioSettings audioSettings = new AkAudioSettings();
            while (AkSoundEngine.GetAudioSettings(audioSettings) != AKRESULT.AK_Success)
            {
                yield return null;
            }

            _sampleRate = (int)AkSoundEngine.GetSampleRate();

            _bufferSize = 2048;
            _levelBuffer = new float[_bufferSize];

            _bufferSize = 2 << _wwiseBufferMultiple;

            var audioSinkCapabilities = new Ak3DAudioSinkCapabilities();
            AkChannelConfig channelConfig = new AkChannelConfig();
            AkSoundEngine.GetOutputDeviceConfiguration(_wwiseDeviceId, channelConfig, audioSinkCapabilities);
            _audioChannels = (int)channelConfig.uNumChannels;

            BeamLogger.Log($"Wwise audio started. Sample rate: {_sampleRate}");

            _hasSetup = true;
        }

        protected override void OnCapturingChange(bool capturing)
        {
            if (capturing)
            {
                if (!_wwiseBound)
                    BindWwise();
            }
            else
            {
                if (_wwiseBound)
                    ReleaseWwise();
            }
        }

        private void BindWwise()
        {
            if (!_hasSetup)
                return;

            AkSoundEngine.ClearCaptureData();
            AkSoundEngine.StartDeviceCapture(_wwiseDeviceId);
#if UNITY_EDITOR
            // Ensure that the editor update does not call AkSoundEngine.RenderAudio().
            AkSoundEngineController.Instance.DisableEditorLateUpdate();
#endif
            _wwiseBound = true;
        }

        private void ReleaseWwise()
        {
            if (!_hasSetup)
                return;
            AkSoundEngine.StopDeviceCapture(_wwiseDeviceId);
#if UNITY_EDITOR
            // Bring back editor update calls to AkSoundEngine.RenderAudio().
            AkSoundEngineController.Instance.EnableEditorLateUpdate();
#endif
            _wwiseBound = false;
        }

        private void OnValidate()
        {
            if (_wwiseBufferMultiple < 9)
            {
                _wwiseBufferMultiple = 9;
            }
            if (_wwiseBufferMultiple > 16)
            {
                _wwiseBufferMultiple = 16;
            }
        }

        private void OnDisable()
        {
            if (_wwiseBound)
            {
                ReleaseWwise();
            }
        }

        public override bool GetAudioBufferReady()
        {
            if (!_wwiseBound)
            {
                BindWwise();
            }

            int samples = (int)AkSoundEngine.UpdateCaptureSampleCount(_wwiseDeviceId);
            if (samples > 0)
            {
                float[] newSamples = new float[samples];
                AkSoundEngine.GetCaptureSamples(_wwiseDeviceId, newSamples, (uint)samples);

                StoreAudio(newSamples);
                StoreAudioLevel(newSamples, AudioChannels);
            }

            return base.GetAudioBufferReady();
        }
#else
        public void Awake()
        {
            BeamLogger.LogError("You must add BEAM_WWISE to your Scripting Define Symbols to capture audio from Wwise", context:this);
        }
#endif
    }
}