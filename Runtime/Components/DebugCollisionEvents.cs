using System;
using UnityEngine;

namespace Vertx.Debugging
{
    #if UNITY_EDITOR
    /// <summary>
    /// This only exists to avoid an issue with enum fields of an underlying type: byte.
    /// https://issuetracker.unity3d.com/issues/none-or-mixed-option-gets-set-to-enumflagsfield-when-selecting-the-everything-option
    /// Which has also regressed in later versions of 2022.
    /// </summary>
    [UnityEditor.CustomEditor(typeof(DebugCollisionEvents))]
    internal sealed class DebugCollisionEventsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI() => DrawDefaultInspector();
    }
    #endif
    
    [AddComponentMenu("Debugging/Debug Collision Events")]
    public sealed class DebugCollisionEvents : MonoBehaviour
    {
        [SerializeField] private Type _type = Type.Enter;
        [PairWithEnabler(null, "Duration", nameof(_type), (int)Type.Enter)]
        [SerializeField]
        private DebugComponentBase.ColorDurationPair _enter =
            new DebugComponentBase.ColorDurationPair(Shapes.EnterColor, 0.1f);
        [PairWithEnabler(null, "Duration", nameof(_type), (int)Type.Stay)]
        [SerializeField]
        private DebugComponentBase.ColorDurationPair _stay =
            new DebugComponentBase.ColorDurationPair(Shapes.StayColor);
        [PairWithEnabler(null, "Duration", nameof(_type), (int)Type.Exit)]
        [SerializeField]
        private DebugComponentBase.ColorDurationPair _exit =
            new DebugComponentBase.ColorDurationPair(Shapes.ExitColor, 0.1f);

        [Flags]
        private enum Type : byte
        {
            None,
            Enter = 1,
            Stay = 1 << 1,
            Exit = 1 << 2
        }

#if UNITY_EDITOR
        private void OnCollisionEnter(Collision collision)
        {
            if ((_type & Type.Enter) == 0) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                D.raw(new Shapes.SurfacePoint(contact.point, contact.normal), _enter.Color, _enter.Duration);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if ((_type & Type.Stay) == 0) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                D.raw(new Shapes.SurfacePoint(contact.point, contact.normal), _stay.Color, _stay.Duration);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if ((_type & Type.Exit) == 0) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                D.raw(new Shapes.SurfacePoint(contact.point, contact.normal), _exit.Color, _exit.Duration);
            }
        }

#if VERTX_PHYSICS_2D
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if ((_type & Type.Enter) == 0) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                D.raw(new Shapes.Arrow2D(contact.point, contact.normal), _enter.Color, _enter.Duration);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if ((_type & Type.Stay) == 0) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                D.raw(new Shapes.Arrow2D(contact.point, contact.normal), _stay.Color, _stay.Duration);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if ((_type & Type.Exit) == 0) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                D.raw(new Shapes.Arrow2D(contact.point, contact.normal), _exit.Color, _exit.Duration);
            }
        }
#endif
#endif
    }
}