using BeamXR.Streaming.Core.Audio;
using UnityEditor;
using UnityEngine;

namespace BeamXR.Streaming.Editor.Audio
{
    [CustomEditor(typeof(BeamFMODListener))]
    public class BeamFMODListenerEditor : UnityEditor.Editor
    {
        private SerializedProperty _wwiseBufferMultiple;

        public override void OnInspectorGUI()
        {
#if BEAM_FMOD
            base.OnInspectorGUI();
#else
            ShowMissingFMOD();
#endif
        }

        private void ShowMissingFMOD()
        {
            EditorGUILayout.HelpBox("You must add BEAM_FMOD to your Scripting Define Symbols for this component to work correctly.", MessageType.Error);
            if(GUILayout.Button("Open Player Settings"))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }
        }
    }
}