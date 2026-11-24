using System.IO;
using Evolution.Articulated.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Evolution.Articulated.EditorTools
{
    // Explicit user menu only. Never runs on project import, never rewrites Build Settings.
    public static class ArticulatedSceneBuilder
    {
        public const string DemoPath="Assets/Evolution/EVCP2/Scenes/EVCP2_Articulated.unity";
        public const string ProbePath="Assets/Evolution/EVCP2/Scenes/EVCP2_IsolatedJoint.unity";
        [MenuItem("Tools/Evolution/Create/Open EV-CP2 Demo")]
        public static void CreateOrOpenDemo() => CreateOrOpen(DemoPath,false);
        [MenuItem("Tools/Evolution/Create/Open EV-CP2 Isolated Joint Probe")]
        public static void CreateOrOpenProbe() => CreateOrOpen(ProbePath,true);
        private static void EnsureTimeControl(Scene scene,bool probe)
        {
            if(probe) return; // Isolated native gate stays at 1x unless the user explicitly changes time.
            ArticulatedDemo2D found=null;int count=0;
            foreach(var root in scene.GetRootGameObjects())
            foreach(var demo in root.GetComponentsInChildren<ArticulatedDemo2D>(true)) { found=demo;count++; }
            if(count!=1)
            { Debug.LogWarning("Expected one EV-CP2 demo bootstrap; existing scene left unchanged.");return; }
            if(found.GetComponent<SimulationTimeControl2D>()!=null) return;
            if(!EditorUtility.DisplayDialog("EV-CP2 time controls","Add missing time controls to this existing EV-CP2 bootstrap? No other objects/configuration will be replaced. Save with Ctrl+S afterwards.","Add","Open unchanged")) return;
            Undo.AddComponent<SimulationTimeControl2D>(found.gameObject);EditorSceneManager.MarkSceneDirty(scene);
        }
        private static void CreateOrOpen(string path,bool probe)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
            { EditorUtility.DisplayDialog("EV-CP2","Exit Play Mode before creating/opening a scene.","OK");return; }
            if(SceneManager.GetActiveScene().path==path)
            { EnsureTimeControl(SceneManager.GetActiveScene(),probe);return; }
            // Respect cancellation and unsaved edits in every open scene before switching.
            if(!EditorSceneManager.SaveModifiedScenesIfUserWantsTo()) return;
            if(File.Exists(path))
            {
                // Opening an existing user-edited demo must never regenerate or overwrite it.
                Scene existing=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
                EnsureTimeControl(existing,probe);return;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));AssetDatabase.Refresh();
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var bootstrap=new GameObject(probe ? "EVCP2 Isolated Joint Probe" : "EVCP2 Bootstrap");
            Undo.RegisterCreatedObjectUndo(bootstrap,"Create EV-CP2 bootstrap");
            if(probe) bootstrap.AddComponent<SingleJointProbe2D>();
            else
            { bootstrap.AddComponent<SimulationTimeControl2D>();bootstrap.AddComponent<ArticulatedDemo2D>(); }
            EditorSceneManager.MarkSceneDirty(scene);
            if(!EditorSceneManager.SaveScene(scene,path))
                throw new IOException("Could not save EV-CP2 scene: "+path);
            Selection.activeGameObject=bootstrap;
            Debug.Log("EV-CP2 scene saved by this Editor at "+path+". Commit its real generated .meta files; native acceptance still requires Play.");
        }
    }
}
