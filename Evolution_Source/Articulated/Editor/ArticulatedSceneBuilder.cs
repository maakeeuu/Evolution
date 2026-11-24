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
        private static void CreateOrOpen(string path,bool probe)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
            { EditorUtility.DisplayDialog("EV-CP2","Exit Play Mode before creating/opening a scene.","OK");return; }
            if(SceneManager.GetActiveScene().path==path) return;
            // Respect cancellation and unsaved edits in every open scene before switching.
            if(!EditorSceneManager.SaveModifiedScenesIfUserWantsTo()) return;
            if(File.Exists(path))
            {
                // Opening an existing user-edited demo must never regenerate or overwrite it.
                EditorSceneManager.OpenScene(path,OpenSceneMode.Single);return;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));AssetDatabase.Refresh();
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var bootstrap=new GameObject(probe ? "EVCP2 Isolated Joint Probe" : "EVCP2 Bootstrap");
            Undo.RegisterCreatedObjectUndo(bootstrap,"Create EV-CP2 bootstrap");
            if(probe) bootstrap.AddComponent<SingleJointProbe2D>();
            else bootstrap.AddComponent<ArticulatedDemo2D>();
            EditorSceneManager.MarkSceneDirty(scene);
            if(!EditorSceneManager.SaveScene(scene,path))
                throw new IOException("Could not save EV-CP2 scene: "+path);
            Selection.activeGameObject=bootstrap;
            Debug.Log("EV-CP2 scene saved by this Editor at "+path+". Commit its real generated .meta files; native acceptance still requires Play.");
        }
    }
}
