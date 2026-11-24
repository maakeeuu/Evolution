"""Independent references and source contracts; NOT Unity compilation or physics execution."""
from pathlib import Path
import json
import math
import unittest
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'Evolution_Source/Articulated'
UNITY=SOURCE/'Unity'
class ArticulatedChecks(unittest.TestCase):
    def test_complete_runtime_mirror(self):
        mirror=ROOT/'Assets/Evolution/EVCP2'
        expected={p.relative_to(SOURCE) for p in SOURCE.rglob('*') if p.is_file() and p.suffix!='.meta'}
        actual={p.relative_to(mirror) for p in mirror.rglob('*') if p.is_file() and p.suffix!='.meta' and 'Scenes' not in p.relative_to(mirror).parts}
        for p in (mirror/'Scenes').glob('*'):
            if p.is_file() and p.suffix!='.meta':
                self.assertIn(p.name,{'EVCP2_Articulated.unity','EVCP2_IsolatedJoint.unity','EVCP3_Rod.unity'})
        self.assertEqual(expected,actual)
        self.assertGreaterEqual(len(expected),11)
        for p in expected:self.assertEqual((SOURCE/p).read_bytes(),(mirror/p).read_bytes(),str(p))
    def test_default_tip_load_regression(self):
        for i in range(1,4):
            scale=.96**i;parent=.96**(i-1)
            length=.85*scale;radius=.14*scale;mass=.35*scale*scale
            rest=math.hypot(.25*.85*(scale+parent),.72*.14*(scale-parent))
            old=min(12,90*.3*rest)*radius*.72
            new=min(60,600*.3*.7*rest)*radius*.72
            gravity=mass*9.81*length*.5
            self.assertLess(old,gravity);self.assertGreater(new,3*gravity)
        s=(UNITY/'ArticulatedOrganism2D.cs').read_text()
        self.assertIn('MuscleMechanics.DemonstrationSettings',s)
    def test_probe_has_native_not_just_force_acceptance(self):
        s=(UNITY/'SingleJointProbe2D.cs').read_text()
        for token in ['jointAngle','maxComDrift','maxMomentum','angles[0]*angles[1]<0','Mathf.Abs(angles[side])>5','ConfigureIsolatedJoint','ClearCommands']:
            self.assertIn(token,s)
        d=(UNITY/'ArticulationDiagnostics2D.cs').read_text()
        for token in ['jointSpeed','jointAngle','GetAppliedForce','GetLengthRate','GetTargetLength','CenterOfMass','isActiveAndEnabled']:
            self.assertIn(token,d)
    def test_explicit_safe_scene_builder(self):
        s=(SOURCE/'Editor/ArticulatedSceneBuilder.cs').read_text()
        for token in ['Tools/Evolution/Create/Open EV-CP2 Demo','EVCP2_Articulated.unity','EVCP2_IsolatedJoint.unity','SaveModifiedScenesIfUserWantsTo','File.Exists(path)','SaveScene(scene,path)','AddComponent<SingleJointProbe2D>','AddComponent<ArticulatedDemo2D>']:
            self.assertIn(token,s)
        for forbidden in ['InitializeOnLoad','EditorBuildSettings','DeleteAsset','WriteAllText']:
            self.assertNotIn(forbidden,s)
        editor=json.loads((SOURCE/'Editor/Evolution.Articulated.Editor.asmdef').read_text())
        self.assertEqual(editor['includePlatforms'],['Editor'])
    def test_time_control_preserves_fixed_step_and_ownership(self):
        s=(UNITY/'SimulationTimeControl2D.cs').read_text()
        for token in ['Time.timeScale=1','Time.timeScale=scale','TryRelease','OnDisable','OnDestroy','KeyCode.Space','KeyCode.Alpha1','KeyCode.Alpha4','{0,1,2,5,20}','advancedGameTime/elapsed','Time.realtimeSinceStartupAsDouble']:
            self.assertIn(token,s)
        for forbidden in ['Time.fixedDeltaTime=','Time.fixedDeltaTime =','Time.maximumDeltaTime=','Input.GetKey','linearVelocity=','Physics2D.Simulate']:
            self.assertNotIn(forbidden,s)
        demo=(UNITY/'ArticulatedDemo2D.cs').read_text()
        self.assertNotIn('RequireComponent(typeof(SimulationTimeControl2D))',demo)
        self.assertIn('bootstrap.AddComponent<SimulationTimeControl2D>()',(SOURCE/'Editor/ArticulatedSceneBuilder.cs').read_text())
    def test_engine_independent_assembly(self):
        core=json.loads((SOURCE/'Core/Evolution.Articulated.Core.asmdef').read_text())
        self.assertTrue(core['noEngineReferences'])
        self.assertEqual(core['references'],['Evolution.Source.Core'])
        for p in (SOURCE/'Core').glob('*.cs'):self.assertNotIn('UnityEngine',p.read_text())
    def test_physical_joint_and_force_contract(self):
        s=(UNITY/'ArticulatedOrganism2D.cs').read_text()
        for expected in ['HingeJoint2D','joint.useLimits=true','joint.useMotor=false','joint.useConnectedAnchor=true',
                         'GetRelativePoint(new Vector2','GetPointVelocity','AddForceAtPosition(forceA[i],pointA[i],ForceMode2D.Force)',
                         'AddForceAtPosition(-forceA[i],pointB[i],ForceMode2D.Force)','Physics2D.IgnoreCollision']:
            self.assertIn(expected,s)
        fixed=s.split('private void FixedUpdate()')[1].split('private void Fail')[0]
        for forbidden in ['transform.position =','transform.position=','linearVelocity=','MovePosition','AddTorque']:
            self.assertNotIn(forbidden,fixed)
        self.assertNotIn('.GetPoint(',fixed)
        self.assertLess(fixed.index('MuscleMechanics.Tension'),fixed.index('AddForceAtPosition'))
    def test_render_copies_physical_polygon_and_shared_shader(self):
        s=(UNITY/'ArticulatedOrganismView2D.cs').read_text()
        self.assertIn('organism.GetCollider(i).GetPath(0)',s)
        self.assertIn('vertices[j+1]=new Vector3(contour[j].x,contour[j].y,0)',s)
        self.assertIn('Resources.Load<Shader>("EvolutionVector2D")',s)
        self.assertFalse(list(SOURCE.rglob('*.shader')))
        self.assertNotIn('new MaterialPropertyBlock();properties.SetColor',s)
    def test_antagonist_moment_arms_reference(self):
        # Independent lever mechanics across a hinge at origin, including equal opposite torque balance.
        for angle in [-.5,-.1,0,.1,.5]:
            for side in [-1,1]:
                a=(-.2,side*.1);local=(.2,side*.1)
                b=(local[0]*math.cos(angle)-local[1]*math.sin(angle),local[0]*math.sin(angle)+local[1]*math.cos(angle))
                dx,dy=b[0]-a[0],b[1]-a[1];length=math.hypot(dx,dy);f=(5*dx/length,5*dy/length)
                ta=a[0]*f[1]-a[1]*f[0];tb=b[0]*(-f[1])-b[1]*(-f[0])
                self.assertAlmostEqual(ta+tb,0,places=12)
                self.assertLess(ta*side,0)
    def test_activation_rate_reference(self):
        for hz in [30,50,60,144]:
            a=0
            for _ in range(hz):a+=(1-a)*(-math.expm1(-8/hz))
            self.assertAlmostEqual(a,1-math.exp(-8),places=12)
    def test_soil_render_strip_and_collider_area_agree(self):
        polygon=[(-12,-2),(12,-2),(12,-.1),(5,-.1),(2,.15),(0,-.15),(-3,-.15),(-12,-.4)]
        area=sum(polygon[i][0]*polygon[(i+1)%8][1]-polygon[i][1]*polygon[(i+1)%8][0] for i in range(8))*.5
        top=list(reversed(polygon[2:]));strip=sum((b[0]-a[0])*((a[1]+2)+(b[1]+2))*.5 for a,b in zip(top,top[1:]))
        self.assertGreater(area,0);self.assertAlmostEqual(area,strip,places=12)
    def test_test_tooling_covers_canonical_cp2(self):
        p=(ROOT/'Evolution_Source/Tests/AdapterSyntax/Program.cs').read_text()
        self.assertIn('Path.Combine(root,"Articulated","Unity")',p)
        self.assertIn('Path.Combine(root,"Tests","ArticulatedUnity")',p)
        workflow=(ROOT/'.github/workflows/ev-cp1-core.yml').read_text()
        self.assertIn('Tests/Articulated/Evolution.Articulated.Tests.csproj',workflow)
        self.assertIn('articulated-tests.log',workflow)
if __name__=='__main__':unittest.main()
