using UnityEditor;
using UnityEngine;

namespace Vertx.Debugging.Editor
{
    [CustomPropertyDrawer(typeof(PairAttribute))]
    internal sealed class PairDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);
            
            PairAttribute pairAttribute = (PairAttribute)attribute;
            position.width /= 2f;
            position.width -= 2;

            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Mathf.Min(position.width / 2f, oldLabelWidth);

            property.NextVisible(true);
            EditorGUI.PropertyField(position, property, pairAttribute.FirstLabel, true);
            property.NextVisible(true);
            position.x += position.width + 4;
            EditorGUI.PropertyField(position, property, pairAttribute.SecondLabel, true);
            
            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
    
    [CustomPropertyDrawer(typeof(PairWithEnablerAttribute))]
    internal sealed class PairWithEnablerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PairWithEnablerAttribute pairAttribute = (PairWithEnablerAttribute)attribute;
            
            SerializedProperty key = property.serializedObject.FindProperty(pairAttribute.ConstraintKey);

            if (key == null)
            {
                EditorGUI.HelpBox(position, $"Key {pairAttribute.ConstraintKey} could not be found on same ScriptableObject.", MessageType.Error);
                return;
            }

            using (new EditorGUI.DisabledScope((key.intValue & pairAttribute.ConstraintValue) == 0))
            {
                position = EditorGUI.PrefixLabel(position, label);

                position.width /= 2f;
                position.width -= 2;

                float oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = Mathf.Min(position.width / 2f, oldLabelWidth);

                property.NextVisible(true);
                EditorGUI.PropertyField(position, property, pairAttribute.FirstLabel, true);
                property.NextVisible(true);
                position.x += position.width + 4;
                EditorGUI.PropertyField(position, property, pairAttribute.SecondLabel, true);

                EditorGUIUtility.labelWidth = oldLabelWidth;
            }
        }
    }
}