using UnityEditor;

namespace BeamXR.Streaming.Editor
{
    public static class BeamEditorUtils
    {
        public static void FindFirstProperty(this SerializedObject serializedObject, ref SerializedProperty property, string name)
        {
            if (property != null)
                return;

            property = serializedObject.FindProperty(name);
        }
    }
}