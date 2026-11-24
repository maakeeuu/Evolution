using UnityEngine;

namespace Evolution.Source.Unity
{
    // Optional adapter: no new Input System package or project-setting changes.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CreatureMotor2D))]
    public sealed class CreatureKeyboardInput2D : MonoBehaviour
    {
        private CreatureMotor2D motor;
        private void Awake() => motor = GetComponent<CreatureMotor2D>();

        private void OnEnable()
        {
#if !ENABLE_LEGACY_INPUT_MANAGER
            Debug.LogWarning("Evolution keyboard adapter needs legacy input enabled. " +
                "No settings were changed. Use CreatureMotor2D.SetMovementIntent with your input adapter.", this);
            enabled = false;
#endif
        }

        private void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            // Direct keys avoid a dependency on named Input Manager axes.
            float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f)
                - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            float y = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f)
                - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
            motor.SetMovementIntent(new Vector2(x, y));
#endif
        }

        private void OnDisable()
        {
            if (motor != null) motor.SetMovementIntent(Vector2.zero);
        }
    }
}
