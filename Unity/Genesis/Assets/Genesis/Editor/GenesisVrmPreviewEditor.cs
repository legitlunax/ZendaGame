using System;
using System.Collections.Generic;
using System.IO;
using Genesis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniGLTF;
using UniVRM10;

public static class GenesisVrmPreviewEditor
{
    private const string ModelPath = "Assets/Genesis/CharacterPreview/Zenda.vrm";
    private const string ScenePath = "Assets/Genesis/CharacterPreview/ZendaPreview.unity";

    [MenuItem("Genesis/VRM/Validate idle in Play mode")]
    public static void ValidateIdle()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Enter Play mode in ZendaPreview first.");
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        if (!scene.IsValid() || !scene.isLoaded) throw new InvalidOperationException("ZendaPreview is not open.");
        GenesisVrmIdle idle = null;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            idle = root.GetComponentInChildren<GenesisVrmIdle>();
            if (idle != null) break;
        }
        if (idle == null || !idle.IsReady) throw new InvalidOperationException("Idle controller is not initialized.");
        Animator animator = idle.GetComponent<Animator>();
        Vrm10Instance vrm = idle.GetComponent<Vrm10Instance>();
        Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
        if (head == null) throw new InvalidOperationException("Head bone missing.");
        int checks = 0;
        string previousMood = idle.Mood;
        try
        {
            foreach (string mood in new[] { "happy", "relaxed", "sad", "angry", "surprised" })
            {
                if (!idle.SetMood(mood)) throw new InvalidOperationException("Missing expression: " + mood);
                idle.Sample(1);
                foreach (ExpressionPreset preset in new[] { ExpressionPreset.happy, ExpressionPreset.relaxed,
                    ExpressionPreset.sad, ExpressionPreset.angry, ExpressionPreset.surprised })
                {
                    float expected = preset.ToString() == mood ? GenesisVrmIdle.FacialMoodWeight(mood) : 0;
                    if (!Mathf.Approximately(vrm.Runtime.Expression.GetWeight(ExpressionKey.CreateFromPreset(preset)), expected))
                        throw new InvalidOperationException("Expression isolation failed: " + mood);
                    checks++;
                }
            }
            if (!idle.SetMood("Neutral") || idle.SetMood("invalid"))
                throw new InvalidOperationException("Mood validation failed.");
            checks += 2;
            ExpressionKey blink = ExpressionKey.CreateFromPreset(ExpressionPreset.blink);
            idle.Sample(3.48f);
            if (!Mathf.Approximately(vrm.Runtime.Expression.GetWeight(blink), 1))
                throw new InvalidOperationException("Blink peak missing.");
            idle.Sample(0.3f);
            if (!Mathf.Approximately(vrm.Runtime.Expression.GetWeight(blink), 0))
                throw new InvalidOperationException("Blink did not reopen.");
            checks += 2;
            idle.Sample(2);
            Quaternion pose = head.localRotation;
            for (int i = 0; i < 100; i++) idle.Sample(2);
            if (Quaternion.Angle(pose, head.localRotation) > 0.001f)
                throw new InvalidOperationException("Idle pose drifted.");
            checks++;
            idle.Sample(float.NaN);
            idle.Sample(float.PositiveInfinity);
            if (Quaternion.Angle(pose, head.localRotation) > 0.001f)
                throw new InvalidOperationException("Nonfinite input changed pose.");
            checks++;
            idle.Sample(3);
            if (Quaternion.Angle(pose, head.localRotation) < 0.01f)
                throw new InvalidOperationException("Head motion missing.");
            checks++;
            idle.enabled = false;
            if (idle.IsReady) throw new InvalidOperationException("Disable did not release preview state.");
            idle.enabled = true;
            if (!idle.IsReady) throw new InvalidOperationException("Re-enable failed.");
            checks += 2;
            string result = "Zenda idle smoke: " + checks + " checks passed.";
            Directory.CreateDirectory("Builds");
            File.WriteAllText("Builds/zenda-idle-validation.txt", result);
            Debug.Log(result);
        }
        finally { idle.SetMood(previousMood); }
    }

    [MenuItem("Genesis/VRM/Enable idle in open preview")]
    public static void EnableIdle()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before installing the preview controller.");
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        if (!scene.IsValid() || !scene.isLoaded)
            throw new InvalidOperationException("Open the isolated ZendaPreview scene first.");
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Vrm10Instance model = root.GetComponentInChildren<Vrm10Instance>();
            if (model == null) continue;
            if (model.GetComponent<GenesisVrmIdle>() == null)
                Undo.AddComponent<GenesisVrmIdle>(model.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Zenda idle installed. Save the preview scene, then press Play. Imported VRM unchanged.");
            return;
        }
        throw new InvalidOperationException("No VRM instance found in the isolated preview.");
    }

    [MenuItem("Genesis/VRM/Create isolated Zenda preview")]
    public static string CreatePreview()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before creating the preview.");
        VrmScriptedImporter importer = AssetImporter.GetAtPath(ModelPath) as VrmScriptedImporter;
        if (importer == null) throw new InvalidOperationException("VRM importer is not ready.");
        importer.RenderPipeline = ImporterRenderPipelineTypes.UniversalRenderPipeline;
        importer.SaveAndReimport();
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        if (asset == null) throw new InvalidOperationException("VRM model import failed.");
        Scene previous = SceneManager.GetActiveScene();
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        if (scene.IsValid() && scene.isLoaded)
            throw new InvalidOperationException("Preview is already open; close it before regenerating.");
        scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        try
        {
            SceneManager.SetActiveScene(scene);
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(asset, scene);
            model.name = "Zenda VRoid - local preview";
            model.AddComponent<GenesisVrmIdle>();
            Animator animator = model.GetComponent<Animator>();
            if (animator == null || animator.avatar == null || !animator.avatar.isValid || !animator.avatar.isHuman)
                throw new InvalidOperationException("Imported model does not have a valid Humanoid Avatar.");
            SkinnedMeshRenderer[] meshes = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (meshes.Length == 0) throw new InvalidOperationException("No skinned meshes imported.");
            Bounds bounds = meshes[0].bounds;
            int blendShapes = 0;
            int vertices = 0;
            HashSet<string> shaders = new HashSet<string>();
            foreach (SkinnedMeshRenderer mesh in meshes)
            {
                bounds.Encapsulate(mesh.bounds);
                if (mesh.sharedMesh == null || mesh.bones.Length == 0)
                    throw new InvalidOperationException("Missing mesh or skin bones: " + mesh.name);
                vertices += mesh.sharedMesh.vertexCount;
                blendShapes += mesh.sharedMesh.blendShapeCount;
                foreach (Material material in mesh.sharedMaterials)
                {
                    if (material == null || material.shader == null || !material.shader.isSupported)
                        throw new InvalidOperationException("Missing or unsupported material on " + mesh.name);
                    shaders.Add(material.shader.name);
                    if (ShaderUtil.ShaderHasError(material.shader))
                        throw new InvalidOperationException("Shader compile error: " + material.shader.name);
                }
            }
            GameObject cameraObject = new GameObject("Zenda preview camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.065f, 0.11f);
            camera.orthographic = true;
            camera.orthographicSize = bounds.extents.y * 1.18f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 50f;
            camera.transform.position = bounds.center + Vector3.forward * 4f;
            camera.transform.LookAt(bounds.center);
            AddLight("Preview key", new Vector3(35f, 145f, 0f), 1.1f, new Color(1f, 0.94f, 0.88f));
            AddLight("Preview fill", new Vector3(15f, -35f, 0f), 0.55f, new Color(0.77f, 0.85f, 1f));
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.6f);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Directory.CreateDirectory("Builds");
            Capture(camera, "Builds/zenda-full.png");
            Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
            camera.orthographicSize = bounds.size.y * 0.23f;
            Vector3 focus = head.position + Vector3.down * bounds.size.y * 0.055f;
            camera.transform.position = focus + Vector3.forward * 4f;
            camera.transform.LookAt(focus);
            Capture(camera, "Builds/zenda-portrait.png");
            Vrm10Instance vrm = model.GetComponent<Vrm10Instance>();
            if (vrm == null || vrm.Vrm == null) throw new InvalidOperationException("VRM expressions/runtime missing.");
            string report = "Humanoid Avatar: valid\nSkinned meshes: " + meshes.Length +
                "\nVertices: " + vertices + "\nBlend shapes (summed across meshes): " + blendShapes +
                "\nShaders: " + string.Join(", ", shaders) + "\nVRM runtime: present\n" +
                "Local preview only; distribution/modification permissions require confirmation.\n";
            File.WriteAllText("Builds/zenda-validation.txt", report);
            Selection.activeGameObject = model;
            return report;
        }
        finally
        {
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
        }
    }

    private static void AddLight(string name, Vector3 angles, float intensity, Color color)
    {
        Light light = new GameObject(name).AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(angles);
        light.intensity = intensity;
        light.color = color;
        light.shadows = LightShadows.None;
    }

    private static void Capture(Camera camera, string path)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture target = RenderTexture.GetTemporary(1024, 1024, 24, RenderTextureFormat.ARGB32);
        Texture2D image = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(image);
            RenderTexture.ReleaseTemporary(target);
        }
    }
}