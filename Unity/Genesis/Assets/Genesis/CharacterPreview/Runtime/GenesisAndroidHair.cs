using System;
using System.Collections.Generic;
using UnityEngine;

namespace Genesis
{
    // Only hair is replaced, and only in Android players. Imported VRM assets stay untouched.
    public sealed class GenesisAndroidHair : MonoBehaviour
    {
        private readonly Dictionary<Material, Material> replacements = new Dictionary<Material, Material>();
        private readonly Dictionary<Renderer, Material[]> originals = new Dictionary<Renderer, Material[]>();
        public int ReplacementCount => replacements.Count;

        public void Apply()
        {
            if (originals.Count > 0) return;
            Shader shader = Resources.Load<Shader>("Genesis/Shaders/AndroidHair");
            if (shader == null || !shader.isSupported)
            {
                Debug.LogError("ZendaGenesis Android hair shader unavailable; original materials retained.", this);
                return;
            }
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                Material[] source = renderer.sharedMaterials;
                Material[] result = (Material[])source.Clone();
                bool changed = false;
                for (int i = 0; i < source.Length; i++)
                {
                    Material material = source[i];
                    if (material == null || material.name.IndexOf("hair", StringComparison.OrdinalIgnoreCase) < 0 ||
                        material.shader == null || !material.shader.name.Contains("MToon")) continue;
                    // Do not turn genuinely blended materials opaque.
                    int alphaMode = material.HasProperty("_AlphaMode") ? material.GetInt("_AlphaMode") : 0;
                    if (alphaMode == 2) continue;
                    if (!replacements.TryGetValue(material, out Material replacement))
                    {
                        replacement = new Material(shader) { name = material.name + " / Android color-safe" };
                        replacement.SetTexture("_MainTex", material.GetTexture("_MainTex"));
                        replacement.SetTextureScale("_MainTex", material.GetTextureScale("_MainTex"));
                        replacement.SetTextureOffset("_MainTex", material.GetTextureOffset("_MainTex"));
                        replacement.SetColor("_Color", material.GetColor("_Color"));
                        replacement.SetFloat("_Cutoff", material.HasProperty("_Cutoff") ? material.GetFloat("_Cutoff") : 0.5f);
                        replacement.SetFloat("_UseCutoff", alphaMode == 1 ? 1 : 0);
                        replacements.Add(material, replacement);
                    }
                    result[i] = replacement;
                    changed = true;
                }
                if (!changed) continue;
                originals.Add(renderer, source);
                renderer.sharedMaterials = result;
            }
        }

        private void OnDestroy() { Restore(); }

        public void Restore()
        {
            foreach (var row in originals) if (row.Key != null) row.Key.sharedMaterials = row.Value;
            foreach (Material material in replacements.Values)
                if (Application.isPlaying) Destroy(material); else DestroyImmediate(material);
            originals.Clear();
            replacements.Clear();
        }
    }
}