#if USING_OCULUS_RUNTIME
using Unity.XR.Oculus;
using UnityEditor.XR.Management;
#endif
using System.Linq;
using UnityEditor;
using UnityEngine;
using BeamXR.Streaming.Core;

namespace BeamXR.Streaming.Editor
{
    public static class BeamEditorUtils
    {
        public static void FindFirstProperty(this SerializedObject serializedObject,ref SerializedProperty property, string name)
        {
            if (property != null)
                return;

            property = serializedObject.FindProperty(name);
        }
    }
}