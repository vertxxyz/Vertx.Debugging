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
}