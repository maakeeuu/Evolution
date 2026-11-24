"""Independent numerical reference + source contracts, never Unity/native execution."""
from pathlib import Path
import hashlib, math, struct, unittest
ROOT=Path(__file__).resolve().parents[2]
SOURCE=ROOT/'Evolution_Source/Articulated'
def f(x):return struct.unpack('f',struct.pack('f',x))[0]
def cross(a,b,c):return (b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0])
def reference(count,angles,length=.85,radius=.14,taper=.96):
    lengths=[length*taper**i for i in range(count)];radii=[radius*taper**i for i in range(count)]
    axis=[(0,0)];ds=[];theta=0
    for i,L in enumerate(lengths):
        if i:theta+=math.radians(angles[i-1])
        d=(math.cos(theta),math.sin(theta));ds.append(d);axis.append((axis[-1][0]+d[0]*L,axis[-1][1]+d[1]*L))
    sections=[]
    for phi in [5,22,39,56,73]:
        x=-radii[0]*math.cos(math.radians(phi));w=radii[0]*math.sin(math.radians(phi));sections.append(((x,0),(0,w)))
    for i,c in enumerate(axis):
        if i in (0,count):d=ds[0 if i==0 else -1];w=radii[0 if i==0 else -1]
        else:
            a,b=ds[i-1],ds[i];n=math.hypot(a[0]+b[0],a[1]+b[1]);d=((a[0]+b[0])/n,(a[1]+b[1])/n)
            w=max(radii[i-1],radii[i])/math.cos(math.radians(angles[i-1]/2))
        sections.append((c,(-d[1]*w,d[0]*w)))
    for phi in [73,56,39,22,5]:
        x=radii[-1]*math.cos(math.radians(phi));w=radii[-1]*math.sin(math.radians(phi));d=ds[-1];a=axis[-1]
        sections.append(((a[0]+x*d[0],a[1]+x*d[1]),(-d[1]*w,d[0]*w)))
    vertices=[];triangles=[]
    for c,n in sections:vertices.extend([(f(c[0]-n[0]),f(c[1]-n[1])),(f(c[0]+n[0]),f(c[1]+n[1]))])
    for v in range(0,len(vertices)-2,2):triangles.extend([(v,v+2,v+1),(v+1,v+2,v+3)])
    return vertices,triangles,vertices[::2]+list(reversed(vertices[1::2]))
class RodChecks(unittest.TestCase):
    def test_reference_straight_c_s_and_parameter_extremes(self):
        cases=0
        for count in range(2,9):
            shapes=[[0]*(count-1),[12]*(count-1),[-12]*(count-1),[40*(-1)**i for i in range(count-1)],[-40*(-1)**i for i in range(count-1)]]
            for angles in shapes:
                v,t,p=reference(count,angles);cases+=1
                self.assertEqual((v,t,p),reference(count,angles))
                self.assertEqual(len(v),2*(count+11))
                for a,b,c in t:self.assertGreater(cross(v[a],v[b],v[c]),1e-12)
                for i in range(len(p)):
                    a,b=p[i],p[(i+1)%len(p)]
                    for j in range(i+2,len(p)):
                        if i==0 and j==len(p)-1:continue
                        c,d=p[j],p[(j+1)%len(p)]
                        self.assertFalse(cross(a,b,c)*cross(a,b,d)<0 and cross(c,d,a)*cross(c,d,b)<0)
            for mask in range(8):
                v,t,p=reference(count,[60*(-1)**i for i in range(count-1)],.5 if mask&1==0 else 1.5,.08 if mask&2==0 else .2,.94 if mask&4==0 else 1);cases+=1
                for a,b,c in t:self.assertGreater(cross(v[a],v[b],v[c]),1e-12)
        self.assertEqual(cases,91)
    def test_local_width_not_bent_bounding_box(self):
        for turn in [-40,0,40]:
            vertices,_,_=reference(4,[turn,turn,turn])
            for i in range(5):
                width=math.dist(vertices[2*(i+5)],vertices[2*(i+5)+1])
                radius=.14*.96**max(0,i-1)
                ratio=1 if i in (0,4) else 1/math.cos(math.radians(turn/2))
                self.assertAlmostEqual(width,2*radius*ratio,places=6)
        self.assertGreater(sum(.85*.96**i for i in range(4))/.28,11)
    def test_physics_files_are_exact_baseline_blobs(self):
        expected={'Unity/ArticulatedOrganism2D.cs':'81db739d0a8ab6a993e78197f7c933701dfba07c','Core/MuscleMechanics.cs':'749b20e99c045efcbae5c88b22afe3585323689e','Core/ChainDefinition.cs':'7e03ffecaae3d8b4d66a35ce239913fd8fab193d','Core/SegmentContour.cs':'df6d5516fcff393852bee31fc670c9c2f31db968'}
        for name,sha in expected.items():
            data=(SOURCE/name).read_bytes();self.assertEqual(hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest(),sha)
    def test_evcp1_tree_is_exactly_preserved(self):
        def tree(folder):
            entries=[]
            for p in sorted(folder.iterdir(),key=lambda p:p.name+('/' if p.is_dir() else '')):
                if p.is_dir():mode=b'40000';digest=tree(p)
                else:
                    data=p.read_bytes();mode=b'100644';digest=hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).digest()
                entries.append(mode+b' '+p.name.encode()+b'\0'+digest)
            data=b''.join(entries)
            return hashlib.sha1(b'tree '+str(len(data)).encode()+b'\0'+data).digest()
        self.assertEqual(tree(ROOT/'Assets/Evolution/EVCP1').hex(),'379cde3c17ac8dafcd0e5d9a6f12edfd46f33f13')
    def test_skin_read_only_and_no_radial_body(self):
        core=(SOURCE/'Core/RodSkinGeometry.cs').read_text();adapter=(SOURCE/'Unity/ArticulatedRodSkin2D.cs').read_text()
        for forbidden in ['RadialGeometry','CreateFan','UnityEngine','Random']:self.assertNotIn(forbidden,core)
        for forbidden in ['AddForce','SetPath(','MovePosition','MoveRotation','linearVelocity=','angularVelocity=','gravityScale=','mass=','SetActivationCommand','transform.position=']:
            self.assertNotIn(forbidden,adapter)
        for needed in ['GetJoint(i-1).anchor','GetJoint(i).connectedAnchor','body.TransformPoint','surface.Update(starts,ends)','mesh.vertices=vertices','collider.offset','MeasureCoverage(paths,8)']:
            self.assertIn(needed,adapter)
        view=(SOURCE/'Unity/ArticulatedOrganismView2D.cs').read_text();self.assertNotIn('Envelope patch',view)
    def test_baseline_and_modes_safe_scene_path(self):
        demo=(SOURCE/'Unity/RodDemo2D.cs').read_text();builder=(SOURCE/'Editor/ArticulatedSceneBuilder.cs').read_text()
        for token in ['SkeletonColliders','JointsMuscles','ShellOnly','Normal']:
            self.assertIn(token,(SOURCE/'Unity/ArticulatedOrganismView2D.cs').read_text())
        for token in ['GetRootGameObjects','ProceduralCreature2D','BLOCKED','InitialWaveDrive=false','skin.Status']:self.assertIn(token,demo)
        self.assertIn('EVCP3_Rod.unity',builder);self.assertIn('Create/Open EV-CP3 Rod Demo',builder)
        self.assertLess(builder.index('if(File.Exists(path))'),builder.index('NewScene(NewSceneSetup.EmptyScene'))
    def test_actual_csharp_and_pending_native_gates_wired(self):
        runner=(ROOT/'Evolution_Source/Tests/Articulated/Program.cs').read_text();self.assertIn('RodSkinTests.Run()',runner)
        native=(ROOT/'Evolution_Source/Tests/ArticulatedUnity/RodSkinIntegrationTests.cs').read_text()
        for token in ['SkinRefreshDoesNotMutateNativePhysics','ActualMuscleBendingChangesPublishedSkin','jointAngle','CollectionAssert.AreEqual','SetActivationCommand']:
            self.assertIn(token,native)
        workflow=(ROOT/'.github/workflows/ev-cp1-core.yml').read_text();self.assertIn('EV-CP3 assertions',workflow)
if __name__=='__main__':unittest.main()
