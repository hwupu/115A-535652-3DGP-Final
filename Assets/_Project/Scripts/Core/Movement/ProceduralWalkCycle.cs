using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project3DGP.Core.Movement
{
    /// <summary>
    /// A walk animation made in code (no animation clips): the legs swing forward and back like pendulums
    /// while the character moves, and settle back to standing when it stops.
    ///   angle of a leg = swingAngle * weight * sin(2 pi * (phase + leg offset))
    ///   phase  grows with the DISTANCE walked (one full step cycle per strideLength metres), so faster walking
    ///          gives faster steps and the feet don't seem to slide
    ///   weight fades from 0 (standing) to 1 (walking), so starting and stopping are smooth
    /// The legs offset by half a cycle (0 and 0.5) move opposite to each other.
    /// It measures the speed itself from how far this object moved, so it works for any movement: keyboard
    /// walking, an automatic walk (e.g. to the exit gate) or an AI. Each leg turns around its hip, the top of
    /// its mesh, so the legs need no extra pivot objects.
    /// If the model has an Animator with a controller, the clips animate it instead and this turns itself off.
    /// </summary>
    public class ProceduralWalkCycle : MonoBehaviour
    {
        [Serializable]
        public class Leg
        {
            public Transform part;
            [Tooltip("Where in the step cycle this leg is (0..1). Use 0 and 0.5 for a left / right pair.")]
            [Range(0f, 1f)] public float phaseOffset;

            [NonSerialized] public Vector3 restPosition;     // local pose when standing
            [NonSerialized] public Quaternion restRotation;
            [NonSerialized] public Vector3 hip;              // the pivot, in the parent's local space
            [NonSerialized] public Vector3 axis;             // the swing axis, in the parent's local space
        }

        [SerializeField] List<Leg> legs = new();
        [Tooltip("How far each leg swings forward and back (degrees).")]
        [SerializeField] float swingAngle = 35f;
        [Tooltip("Metres walked per full step cycle (left + right step). Smaller = quicker little steps.")]
        [SerializeField] float strideLength = 0.5f;
        [Tooltip("Below this speed (m/s) the character counts as standing.")]
        [SerializeField] float walkingThreshold = 0.1f;
        [Tooltip("How quickly the swing fades in when walking starts and out when it stops.")]
        [SerializeField] float blendSpeed = 8f;
        [Tooltip("A jump of more than this in one frame (m) is a teleport, not walking.")]
        [SerializeField] float teleportDistance = 0.5f;

        float _phase;          // in cycles: 1.0 = one full left + right step
        float _weight;         // 0 = standing pose, 1 = full swing
        Vector3 _lastPosition;

        /// <summary>Measured horizontal speed (m/s).</summary>
        public float Speed { get; private set; }
        public bool IsWalking => Speed > walkingThreshold;

        void Awake()
        {
            var animator = GetComponentInChildren<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                enabled = false;   // a real model with clips: leave the legs to the Animator
                return;
            }
            foreach (var leg in legs)
                Prepare(leg);
            _lastPosition = transform.position;
        }

        // Remember the standing pose and find the hip: the top centre of the leg's mesh.
        void Prepare(Leg leg)
        {
            if (leg.part == null)
                return;
            var parent = leg.part.parent;
            leg.restPosition = leg.part.localPosition;
            leg.restRotation = leg.part.localRotation;
            var renderer = leg.part.GetComponent<Renderer>();
            var hipWorld = renderer != null
                ? renderer.bounds.center + Vector3.up * renderer.bounds.extents.y
                : leg.part.position;
            leg.hip = parent != null ? parent.InverseTransformPoint(hipWorld) : hipWorld;
            // Swing around the character's right axis (forward / back), expressed in the leg's parent space.
            leg.axis = parent != null ? parent.InverseTransformDirection(transform.right) : transform.right;
        }

        // LateUpdate: after this frame's movement, whoever moved the character.
        void LateUpdate()
        {
            var dt = Time.deltaTime;
            if (dt <= 0f)
                return;   // paused

            // 1. How far did we move on the ground this frame?
            var moved = transform.position - _lastPosition;
            moved.y = 0f;
            _lastPosition = transform.position;
            var distance = moved.magnitude;
            if (distance > teleportDistance)
                distance = 0f;   // a teleport (e.g. into the shooting stance) is not a step
            Speed = distance / dt;

            // 2. Advance the step cycle by the distance walked; fade the swing in or out.
            _phase = (_phase + distance / Mathf.Max(0.01f, strideLength)) % 1f;
            var target = IsWalking ? 1f : 0f;
            _weight = Mathf.MoveTowards(_weight, target, blendSpeed * dt);

            // 3. Turn every leg around its hip: rotate its rest offset from the hip, then its rest rotation.
            foreach (var leg in legs)
            {
                if (leg.part == null)
                    continue;
                var angle = swingAngle * _weight * Mathf.Sin(2f * Mathf.PI * (_phase + leg.phaseOffset));
                var swing = Quaternion.AngleAxis(angle, leg.axis);
                leg.part.localPosition = leg.hip + swing * (leg.restPosition - leg.hip);
                leg.part.localRotation = swing * leg.restRotation;
            }
        }

        /// <summary>For setup tools: the legs to swing.</summary>
        public void SetLegs(IEnumerable<Leg> newLegs) => legs = new List<Leg>(newLegs);
    }
}
