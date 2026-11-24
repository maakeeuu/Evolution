using Evolution.Source.Core;
using UnityEngine;

namespace Evolution.Source.Unity
{
    /// <summary>Explicitly a shape demonstration, NOT learning or long-term adaptation.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CreatureBody2D))]
    [DefaultExecutionOrder(-50)]
    public sealed class MorphologyDemo2D : MonoBehaviour
    {
        private CreatureBody2D body;
        private Morphology baseline;
        private double elapsed;

        private void Start()
        {
            body = GetComponent<CreatureBody2D>();
            baseline = body.CurrentMorphology;
        }

        private void FixedUpdate()
        {
            elapsed += Time.fixedDeltaTime;
            double t = elapsed * .22;
            body.SetMorphologyTarget(new Morphology(
                baseline.Width * (1 + .13 * System.Math.Sin(t)),
                baseline.Height * (1 + .12 * System.Math.Sin(t * .83 + 1.4)),
                baseline.ForwardBias + .075 * System.Math.Sin(t * .61),
                baseline.DorsalBias + .065 * System.Math.Sin(t * .74 + .9),
                baseline.LateralLobes + .055 * System.Math.Sin(t * .49 + 2.1)));
        }
    }
}
