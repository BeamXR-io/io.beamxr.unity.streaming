using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BeamXR.Streaming.Core;

namespace BeamXR.Streaming.Editor
{
    [CustomPropertyDrawer(typeof(SingleLayer))]
    public class SingleLayerPropertyDrawer : PropertyDrawer
    {
        private Dictionary<int, GUIContent> _layers;
        private GUIContent[] _layerNames;
        private List<int> _layerValues;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            LayersInitialized();
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            SerializedProperty layerIndex = property.FindPropertyRelative("_layerIndex");
            int index = _layerValues.IndexOf(layerIndex.intValue);
            if (index < 0)
            {
                if (Application.isPlaying)
                {
                    layerIndex.intValue = EditorGUI.IntField(position, property.displayName, layerIndex.intValue);
                    return;
                }
                else
                {
                    layerIndex.intValue = 0;
                    index = 0;
                }
            }

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(position, label, index, _layerNames);
            if (EditorGUI.EndChangeCheck())
            {
                layerIndex.intValue = _layerValues[index];
            }

            EditorGUI.EndProperty();
        }

        private void LayersInitialized()
        {
            if (_layers == null)
            {
                Dictionary<int, GUIContent> valueToLayer = new Dictionary<int, GUIContent>();
                valueToLayer[-1] = new GUIContent("None");

                for (int i = 0; i < 32; i++)
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        valueToLayer[i] = new GUIContent(layerName);
                    }
                }

                _layerValues = valueToLayer.Keys.ToList();
                _layerNames = valueToLayer.Values.ToArray();
            }
        }
    }
}