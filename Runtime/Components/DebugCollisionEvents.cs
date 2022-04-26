using System;
using UnityEngine;

namespace Vertx.Debugging
{
    [AddComponentMenu("Debugging/Debug Collision Events")]
    public class DebugCollisionEvents : MonoBehaviour
    {
        [SerializeField] private Type _type = Type.Enter;
        [Pair(null, "Duration")]
        [SerializeField]
        private DebugComponentBase.ColorDurationPair _enter =
            new DebugComponentBase.ColorDurationPair(DebugUtils.HitColor, 0.1f);
        [Pair(null, "Duration")]
        [SerializeField]
        private DebugComponentBase.ColorDurationPair _stay =
            new DebugComponentBase.ColorDurationPair(DebugUtils.StartColor);
        [Pair(null, "Duration")]
        [SerializeField]
        private DebugComponentBase.ColorDurationPair _exit =
            new DebugComponentBase.ColorDurationPair(DebugUtils.EndColor, 0.1f);

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
                DebugUtils.DrawSurfacePoint(contact.point, contact.normal, _enter.Color, _enter.Duration);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if ((_type & Type.Stay) == 0) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                DebugUtils.DrawSurfacePoint(contact.point, contact.normal, _stay.Color, _stay.Duration);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if ((_type & Type.Exit) == 0) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                DebugUtils.DrawSurfacePoint(contact.point, contact.normal, _exit.Color, _exit.Duration);
            }
        }

#if VERTX_PHYSICS_2D
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if ((_type & Type.Enter) == 0) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                DebugUtils.DrawArrow2D(contact.point, contact.normal, _enter.Color, _enter.Duration);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if ((_type & Type.Stay) == 0) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                DebugUtils.DrawArrow2D(contact.point, contact.normal, _stay.Color, _stay.Duration);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if ((_type & Type.Exit) == 0) return;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint2D contact = collision.GetContact(i);
                DebugUtils.DrawArrow2D(contact.point, contact.normal, _exit.Color, _exit.Duration);
            }
        }
#endif
#endif
    }
}