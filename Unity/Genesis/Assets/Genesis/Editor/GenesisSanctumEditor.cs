using System;
using System.IO;
using Genesis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GenesisSanctumEditor
{
    private const string Path = "Assets/Genesis/Scenes/ZendaSanctum.unity";

    [MenuItem("Genesis/Open goddess Sanctum")]
    public static void OpenSanctum()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
        Scene existing = SceneManager.GetSceneByPath(Path);
        if (existing.IsValid() && existing.isLoaded) { SceneManager.SetActiveScene(existing); return; }
        if (File.Exists(Path))
        {
            SceneManager.SetActiveScene(EditorSceneManager.OpenScene(Path, OpenSceneMode.Additive));
            return;
        }
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Genesis/CharacterPreview/Zenda.vrm");
        if (asset == null) throw new InvalidOperationException("Zenda VRM import missing.");
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        GameObject root = new GameObject("Zenda Sanctum / local gameplay design");
        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
        model.name = "Zenda / main goddess";
        model.transform.SetParent(root.transform, false);
        model.transform.localPosition = new Vector3(2000, 0, 0);
        model.AddComponent<GenesisVrmIdle>().ShowControls = false;
        SkinnedMeshRenderer[] renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (renderers.Length == 0) throw new InvalidOperationException("Zenda has no skinned meshes.");
        Bounds bounds = renderers[0].bounds;
        foreach (SkinnedMeshRenderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
        Camera camera = new GameObject("Sanctum goddess camera").AddComponent<Camera>();
        camera.transform.SetParent(root.transform, false);
        camera.transform.position = bounds.center + Vector3.forward * 4;
        camera.transform.LookAt(bounds.center);
        camera.orthographic = true;
        camera.orthographicSize = bounds.extents.y * 1.6f;
        camera.rect = new Rect(0, 0, 0.578125f, 1);
        camera.backgroundColor = new Color(0.055f, 0.065f, 0.12f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 12;
        camera.depth = 20;
        AddLight(root.transform, "Goddess key", new Vector3(25, 150, 0), 1.15f, new Color(1, 0.91f, 0.78f));
        AddLight(root.transform, "Goddess fill", new Vector3(10, -35, 0), 0.6f, new Color(0.65f, 0.77f, 1));
        root.AddComponent<GenesisSanctum>();
        EditorSceneManager.SaveScene(scene, Path);
        Selection.activeGameObject = root;
        Debug.Log("ZENDA_SANCTUM_READY / Main goddess gameplay scene. Other scenes left open and unsaved; no build created.");
    }

    private static void AddLight(Transform parent, string name, Vector3 angles, float intensity, Color color)
    {
        Light light = new GameObject(name).AddComponent<Light>();
        light.transform.SetParent(parent, false);
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(angles);
        light.intensity = intensity;
        light.color = color;
        light.shadows = LightShadows.None;
    }
}