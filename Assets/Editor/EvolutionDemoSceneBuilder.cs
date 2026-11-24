#if UNITY_EDITOR
using Evolution.Creatures;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Evolution.EditorTools
{
    public static class EvolutionDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Evolution/Scenes/EV_MorphologyPrototype.unity";

        [MenuItem("Tools/Evolution/Create Morphology Demo")]
        public static void CreateMorphologyDemo()
        {
            EnsureFolder("Assets/Evolution");
            EnsureFolder("Assets/Evolution/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 2.0f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.93f, 0.92f, 0.87f, 1f);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            GameObject creatureObject = new GameObject("Procedural Creature");
            creatureObject.transform.position = Vector3.zero;
            creatureObject.AddComponent<ProceduralCreature2D>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = creatureObject;

            Debug.Log("Evolution: morphology demo created at " + ScenePath +
                ". Press Play to observe continuous procedural changes. This is not learning AI yet.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
#endif
