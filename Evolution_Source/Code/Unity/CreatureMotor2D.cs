using System;
using Evolution.Source.Core;
using UnityEngine;

namespace Evolution.Source.Unity
{
    /// <summary>Temporary gravity-free XY prototype motor, not an AI controller.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(CreatureBody2D))]
    public sealed class CreatureMotor2D : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 6f)] private float speed = 2f;
        [SerializeField, Range(0.1f, 30f)] private float acceleration = 8f;
        private Rigidbody2D body;
        private CreatureBody2D shape;
        private Vector2 intent;

        private void Awake()
        {
            NormalizeSettings();
            body = GetComponent<Rigidbody2D>();
            shape = GetComponent<CreatureBody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.linearDamping = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public void SetMovementIntent(Vector2 value)
        {
            if (!Morphology.Finite(value.x) || !Morphology.Finite(value.y))
                throw new ArgumentOutOfRangeException(nameof(value));
            intent = new Vector2(Mathf.Clamp(value.x, -1f, 1f), Mathf.Clamp(value.y, -1f, 1f));
        }

        private void FixedUpdate()
        {
            // Cross-component readiness is checked after every Awake has had a chance to run.
            if (shape == null || !shape.IsInitialized)
            {
                enabled = false;
                return;
            }
            Point2 next = PlanarMotion.ApproachVelocity(
                new Point2(body.linearVelocity.x, body.linearVelocity.y),
                new Point2(intent.x, intent.y), speed, acceleration, Time.fixedDeltaTime);
            body.linearVelocity = new Vector2((float)next.X, (float)next.Y);
        }

        private void OnDisable()
        {
            intent = Vector2.zero;
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        private void OnValidate() => NormalizeSettings();
        private void NormalizeSettings()
        {
            speed = Morphology.Finite(speed) ? Mathf.Clamp(speed, 0.1f, 6f) : 2f;
            acceleration = Morphology.Finite(acceleration) ? Mathf.Clamp(acceleration, 0.1f, 30f) : 8f;
        }
    }
}
