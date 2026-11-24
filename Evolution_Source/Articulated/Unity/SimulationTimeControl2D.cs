using Evolution.Articulated.Core;
using UnityEngine;

namespace Evolution.Articulated.Unity
{
    // One explicit demo time owner. IMGUI keyboard events work while timeScale=0, no Input package.
    [DisallowMultipleComponent]
    public sealed class SimulationTimeControl2D : MonoBehaviour
    {
        [SerializeField] private bool showControls=true;
        [SerializeField] private bool keyboardShortcuts=true;
        [SerializeField] private float measuredSpeed;
        [SerializeField] private float measuredStepsPerSecond;
        private static SimulationTimeControl2D owner;
        private readonly TimeScaleLease lease=new TimeScaleLease();
        private float lastRunningScale=1;
        private double sampleStart,advancedGameTime;
        private long sampleSteps;
        private static readonly float[] Modes={0,1,2,5,20};
        private static readonly string[] Labels={"II","1×","2×","5×","20×"};
        public long PhysicsSteps { get; private set; }
        public float MeasuredSpeed => measuredSpeed;
        public float MeasuredStepsPerSecond => measuredStepsPerSecond;
        public bool ShowControls { get => showControls;set => showControls=value; }
        public bool HasOwnership => owner==this && lease.Owns(Time.timeScale);
        private void OnEnable()
        {
            if(owner!=null && owner!=this)
            { Debug.LogWarning("EV-CP2 time control already owned; duplicate disabled.",this);enabled=false;return; }
            owner=this;lease.Acquire(Time.timeScale);Time.timeScale=1;lastRunningScale=1;
            PhysicsSteps=0;ResetMeasurement();
            if(Mathf.Abs(Time.fixedDeltaTime-.02f)>1e-6f)
                Debug.LogWarning("EV-CP2 expects 0.02 game-second fixed step. Current step is preserved, not overridden.",this);
        }
        public bool SelectScale(float scale)
        {
            if(owner!=this || !lease.TrySelect(Time.timeScale,scale)) return false;
            Time.timeScale=scale;if(scale>0) lastRunningScale=scale;
            ResetMeasurement();return true;
        }
        public void TogglePause() => SelectScale(Time.timeScale==0 ? lastRunningScale : 0);
        private void ResetMeasurement()
        { sampleStart=Time.realtimeSinceStartupAsDouble;advancedGameTime=0;sampleSteps=0;measuredSpeed=0;measuredStepsPerSecond=0; }
        private void FixedUpdate()
        {
            if(!HasOwnership) return;
            PhysicsSteps++;sampleSteps++;advancedGameTime+=Time.fixedDeltaTime;
        }
        private void Update()
        {
            if(owner!=this) return;
            if(!lease.Owns(Time.timeScale))
            {
                // A different writer wins. Do not fight it or restore over it.
                Debug.LogWarning("EV-CP2 time ownership lost to another writer; controls disabled without restoring over it.",this);
                enabled=false;return;
            }
            double elapsed=Time.realtimeSinceStartupAsDouble-sampleStart;
            if(elapsed<.5) return;
            measuredSpeed=(float)(advancedGameTime/elapsed);measuredStepsPerSecond=(float)(sampleSteps/elapsed);
            sampleStart=Time.realtimeSinceStartupAsDouble;advancedGameTime=0;sampleSteps=0;
        }
        private void OnGUI()
        {
            if(!HasOwnership) return;
            Event e=Event.current;
            if(keyboardShortcuts && e.type==EventType.KeyDown && !e.alt && !e.control && !e.command)
            {
                bool used=true;
                switch(e.keyCode)
                {
                    case KeyCode.Space:TogglePause();break;
                    case KeyCode.Alpha1:case KeyCode.Keypad1:SelectScale(1);break;
                    case KeyCode.Alpha2:case KeyCode.Keypad2:SelectScale(2);break;
                    case KeyCode.Alpha3:case KeyCode.Keypad3:SelectScale(5);break;
                    case KeyCode.Alpha4:case KeyCode.Keypad4:SelectScale(20);break;
                    default:used=false;break;
                }
                if(used) e.Use();
            }
            if(!showControls) return;
            float x=Mathf.Max(4,Screen.width-282);const float y=8;
            GUI.Box(new Rect(x,y,274,69),GUIContent.none);
            Color previous=GUI.backgroundColor;
            for(int i=0;i<Modes.Length;i++)
            {
                GUI.backgroundColor=Time.timeScale==Modes[i] ? new Color(.4f,.85f,.7f,1) : previous;
                if(GUI.Button(new Rect(x+5+i*53,y+4,49,23),Labels[i]))
                { if(i==0) TogglePause();else SelectScale(Modes[i]); }
            }
            GUI.backgroundColor=previous;
            GUI.Label(new Rect(x+7,y+28,262,19),$"Selected: {(Time.timeScale==0 ? "Pause" : Time.timeScale+"×")}   measured: {measuredSpeed:F1}×");
            GUI.Label(new Rect(x+7,y+46,262,19),$"{measuredStepsPerSecond:F0} steps/s   step {Time.fixedDeltaTime:F3} game s");
        }
        private void Release()
        {
            if(owner!=this) return;
            if(lease.TryRelease(Time.timeScale,out double restore)) Time.timeScale=(float)restore;
            owner=null;
        }
        private void OnDisable() => Release();
        private void OnDestroy() => Release();
    }
}
