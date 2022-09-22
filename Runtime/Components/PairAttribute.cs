using System;
using UnityEngine;

namespace Vertx.Debugging
{
    public sealed class PairAttribute : PropertyAttribute
    {
        public readonly GUIContent FirstLabel, SecondLabel;

        public PairAttribute(string firstLabel, string secondLabel)
        {
            FirstLabel = string.IsNullOrEmpty(firstLabel) ? GUIContent.none : new GUIContent(firstLabel);
            SecondLabel = string.IsNullOrEmpty(secondLabel) ? GUIContent.none : new GUIContent(secondLabel);
        }
    }
    
    public sealed class PairWithEnablerAttribute : PropertyAttribute
    {
        public readonly GUIContent FirstLabel, SecondLabel;
        public readonly string ConstraintKey;
        public readonly int ConstraintValue;

        public PairWithEnablerAttribute(string firstLabel, string secondLabel, string constraintKey, int constraintValue)
        {
            ConstraintKey = constraintKey;
            ConstraintValue = constraintValue;
            FirstLabel = string.IsNullOrEmpty(firstLabel) ? GUIContent.none : new GUIContent(firstLabel);
            SecondLabel = string.IsNullOrEmpty(secondLabel) ? GUIContent.none : new GUIContent(secondLabel);
        }
    }
}