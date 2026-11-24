using System;
using UnityEngine;

namespace Evolution.Articulated.Unity
{
    // Explicit open-loop excitation for inspecting real actuators. NOT learning or locomotion AI.
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ArticulatedOrganism2D))]
    public sealed class MuscleWaveDriver2D : MonoBehaviour
    {
        [SerializeField,Range(0f,1f)] private float amplitude=.8f;
        [SerializeField,Range(.05f,2f)] private float frequency=.55f;
        private ArticulatedOrganism2D organism;
        private double phase;
        private void Awake() => organism=GetComponent<ArticulatedOrganism2D>();
        private void FixedUpdate()
        {
            if(organism==null || !organism.IsInitialized || !organism.enabled) return;
            if(float.IsNaN(amplitude) || float.IsInfinity(amplitude) || float.IsNaN(frequency) || float.IsInfinity(frequency))
            { enabled=false;Debug.LogError("Invalid excitation settings.",this);return; }
            phase=(phase+2*Math.PI*Mathf.Clamp(frequency,.05f,2f)*Time.fixedDeltaTime)%(2*Math.PI);
            for(int joint=0;joint<organism.MuscleCount/2;joint++)
            {
                double wave=Math.Sin(phase-joint*.9)*Mathf.Clamp01(amplitude);
                organism.SetActivationCommand(joint*2,Math.Max(0,wave));
                organism.SetActivationCommand(joint*2+1,Math.Max(0,-wave));
            }
        }
        private void OnDisable() { if(organism!=null) organism.ClearCommands(); }
    }
}
