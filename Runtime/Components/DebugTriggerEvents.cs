using System;
using UnityEngine;

namespace Vertx.Debugging
{
    [AddComponentMenu("Debugging/Debug Trigger Events")]
    public class DebugTriggerEvents : MonoBehaviour
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
        private void OnTriggerEnter(Collider collider)
        {
            if ((_type & Type.Enter) == 0) return;
            DebugUtils.DrawBounds(collider.bounds, _enter.Color, _enter.Duration);
        }

        private void OnTriggerStay(Collider collider)
        {
            if ((_type & Type.Stay) == 0) return;
            DebugUtils.DrawBounds(collider.bounds, _stay.Color, _stay.Duration);
        }

        private void OnTriggerExit(Collider collider)
        {
            if ((_type & Type.Exit) == 0) return;
            DebugUtils.DrawBounds(collider.bounds, _exit.Color, _exit.Duration);
        }


#if VERTX_PHYSICS_2D
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if ((_type & Type.Enter) == 0) return;
            DebugUtils.DrawBounds(collider.bounds, _enter.Color, _enter.Duration);
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if ((_type & Type.Stay) == 0) return;
            DebugUtils.DrawBounds(collider.bounds, _stay.Color, _stay.Duration);
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if ((_type & Type.Exit) == 0) return;
            DebugUtils.DrawBounds(collider.bounds, _exit.Color, _exit.Duration);
        }
#endif
#endif
    }
}