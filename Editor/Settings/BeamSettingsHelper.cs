using BeamXR.Streaming.Core;
using UnityEditor;

namespace BeamXR.Streaming.Editor
{
    [InitializeOnLoad]
    static class BeamSettingsHelper
    {
        static BeamSettingsHelper()
        {
            if (!SessionState.GetBool("BeamFirstInitDone", false))
            {
                var settings = BeamSettingsProvider.GetOrCreateSettings();

                if(settings.ExperienceKey == "" || settings.ExperienceSecret == "")
                {
                    BeamLogger.LogError("Your BeamXR Experience key and secret are not set. You will not be able to stream or record.");
                }

                SessionState.SetBool("BeamFirstInitDone", true);
            }
        }
    }
}