using UnityEngine;

namespace Evolution.Source.Unity
{
    /// <summary>
    /// Opt-in runtime fixture for later integration. Does not create/save scenes,
    /// modify project settings, auto-run on import, or depend on UnityEditor APIs.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EvolutionPrototype2D : MonoBehaviour
    {
        [SerializeField] private bool useKeyboardControls;
        private GameObject ownedRoot;
        private CreatureMotor2D motor;

        private void OnEnable()
        {
            if (ownedRoot == null) return;
            ownedRoot.SetActive(true);
            if (!useKeyboardControls) motor.SetMovementIntent(Vector2.right);
        }

        private void OnDisable()
        {
            if (ownedRoot != null) ownedRoot.SetActive(false);
        }

        private void Start()
        {
            ownedRoot = new GameObject("EV-CP1 Source Prototype");
            var cameraObject = new GameObject("Evolution Demo Camera");
            cameraObject.transform.SetParent(ownedRoot.transform, false);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.93f, .92f, .87f, 1f);

            var creature = new GameObject("Source Creature");
            creature.transform.SetParent(ownedRoot.transform, false);
            creature.transform.position = new Vector3(-3f, 0f, 0f);
            creature.AddComponent<CreatureBody2D>();
            motor = creature.AddComponent<CreatureMotor2D>();
            creature.AddComponent<MorphologyDemo2D>();

            var obstacle = new GameObject("Single Vector Obstacle");
            obstacle.transform.SetParent(ownedRoot.transform, false);
            obstacle.transform.position = new Vector3(.8f, 0f, 0f);
            obstacle.AddComponent<VectorObstacle2D>();

            if (useKeyboardControls) creature.AddComponent<CreatureKeyboardInput2D>();
            else motor.SetMovementIntent(Vector2.right);
            Debug.Log("Evolution source fixture: one creature, one obstacle. " +
                "Default movement pushes right into the obstacle for a contact test. " +
                "Morphology is a demonstration, not adaptation or learning.", this);
        }

        private void OnDestroy()
        {
            if (ownedRoot != null) Destroy(ownedRoot);
        }
    }
}
