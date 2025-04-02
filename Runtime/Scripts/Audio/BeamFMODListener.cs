using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BeamXR.Streaming.Core.Audio
{
    [HelpURL("http://docs.beamxr.io/sdk-guides/core/third-party-audio")]
    public class BeamFMODListener : BeamAudioListener
    {
#if BEAM_FMOD
        private FMOD.DSP_READ_CALLBACK _dspReadCallback;
        private FMOD.DSP _fmodDSP;
        private static int _fmodSampleRate = 0;
        private GCHandle _objHandle;
        private float[] _tempBuffer, _resampleBuffer;

        [AOT.MonoPInvokeCallback(typeof(FMOD.DSP_READ_CALLBACK))]
        static FMOD.RESULT CaptureDSPReadCallback(ref FMOD.DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint numFrames, int numChannels, ref int outchannels)
        {
            FMOD.DSP_STATE_FUNCTIONS functions = (FMOD.DSP_STATE_FUNCTIONS)Marshal.PtrToStructure(dsp_state.functions, typeof(FMOD.DSP_STATE_FUNCTIONS));

            IntPtr userData;
            functions.getuserdata(ref dsp_state, out userData);
            functions.getsamplerate(ref dsp_state, ref _fmodSampleRate);

            GCHandle objHandle = GCHandle.FromIntPtr(userData);
            BeamFMODListener beamFMOD = objHandle.Target as BeamFMODListener;

            beamFMOD._sampleRate = _fmodSampleRate;

            var numSamples = Math.Min(numFrames * numChannels, beamFMOD._bufferSize);

            Marshal.Copy(inbuffer, beamFMOD._tempBuffer, 0, (int)numSamples);
            Marshal.Copy(beamFMOD._tempBuffer, 0, outbuffer, (int)numSamples);

            if (beamFMOD.Capturing)
            {
                if(numChannels == beamFMOD.AudioChannels)
                {
                    beamFMOD.StoreAudio(beamFMOD._tempBuffer, (int)numSamples);
                }
                else if(numChannels == 6)
                {
                    int ind = 0;
                    try
                    {
                        for (int i = 0; i < numFrames; i++)
                        {
                            int baseIndex = i * numChannels;

                            float frontLeft = beamFMOD._tempBuffer[baseIndex];
                            float frontRight = beamFMOD._tempBuffer[baseIndex + 1];
                            float center = beamFMOD._tempBuffer[baseIndex + 2];
                            float backLeft = beamFMOD._tempBuffer[baseIndex + 4];
                            float backRight = beamFMOD._tempBuffer[baseIndex + 5];

                            float left = 0.707f * frontLeft + 0.5f * center + 0.354f * backLeft;
                            float right = 0.707f * frontRight + 0.5f * center + 0.354f * backRight;

                            beamFMOD._resampleBuffer[ind] = left;
                            ind++;
                            beamFMOD._resampleBuffer[ind] = right;
                            ind++;
                        }
                        beamFMOD.StoreAudio(beamFMOD._resampleBuffer, ind);
                    }
                    catch (Exception e)
                    {
                        BeamLogger.LogError("Issue resampling FMOD audio into the buffer. The data was likely larger than the buffer.");
                    }
                }
                else if (numChannels == 1)
                {
                    int ind = 0;
                    for (int i = 0; i < numFrames; i++)
                    {
                        beamFMOD._resampleBuffer[ind] = beamFMOD._tempBuffer[i];
                        ind++;
                        beamFMOD._resampleBuffer[ind] = beamFMOD._tempBuffer[i];
                        ind++;
                    }
                    beamFMOD.StoreAudio(beamFMOD._resampleBuffer, ind);
                }
                else
                {
                    BeamLogger.LogError($"Unsupported channel count ({numChannels}), Beam FMOD only supports Mono, Stereo, and 5.1 channels.");
                }
            }

            return FMOD.RESULT.OK;
        }

        private void Start()
        {
            _dspReadCallback = CaptureDSPReadCallback;

            uint bufferLength;
            int numBuffers;

            FMODUnity.RuntimeManager.CoreSystem.getDSPBufferSize(out bufferLength, out numBuffers);
            _audioChannels = 2;
            _tempBuffer = new float[bufferLength * numBuffers * 2];
            _resampleBuffer = new float[_tempBuffer.Length * 6 * 2];
            _bufferSize = _tempBuffer.Length;

            _objHandle = GCHandle.Alloc(this);
            if (_objHandle != null)
            {
                FMOD.DSP_DESCRIPTION desc = new FMOD.DSP_DESCRIPTION();
                desc.numinputbuffers = 1;
                desc.numoutputbuffers = 0;
                desc.read = _dspReadCallback;
                desc.userdata = GCHandle.ToIntPtr(_objHandle);

                FMOD.ChannelGroup masterCG;
                if (FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup(out masterCG) == FMOD.RESULT.OK)
                {
                    if (FMODUnity.RuntimeManager.CoreSystem.createDSP(ref desc, out _fmodDSP) == FMOD.RESULT.OK)
                    {
                        if (masterCG.addDSP(0, _fmodDSP) != FMOD.RESULT.OK)
                        {
                            BeamLogger.LogWarning("FMOD: Unable to add the DSP to the master channel group");
                        }
                        else
                        {
                            _hasSetup = true;
                        }
                    }
                    else
                    {
                        BeamLogger.LogWarning("FMOD: Unable to create a DSP");
                    }
                }
                else
                {
                    BeamLogger.LogWarning("FMOD: Unable to create a master channel group");
                }
            }
            else
            {
                BeamLogger.LogWarning("FMOD: Unable to create a GCHandle");
            }
        }

        private void OnDestroy()
        {
            if (_objHandle != null)
            {
                FMOD.ChannelGroup masterCG;
                if (FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup(out masterCG) == FMOD.RESULT.OK)
                {
                    if (_fmodDSP.hasHandle())
                    {
                        masterCG.removeDSP(_fmodDSP);
                        _fmodDSP.release();
                    }
                }
                _objHandle.Free();
            }
        }
#else
        public void Awake()
        {
            BeamLogger.LogError("You must add BEAM_FMOD to your Scripting Define Symbols to capture audio from FMOD", context: this);
        }
#endif
    }

}