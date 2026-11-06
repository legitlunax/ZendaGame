using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Genesis
{
    // Original project geometry. Joint animation deliberately does not claim Humanoid retargeting.
    public sealed partial class GenesisFairy : MonoBehaviour
    {
        public static readonly string[] Reactions = { "Idle", "Happy", "Surprise", "Sorrow", "Casting", "Celebration" };
        public string Reaction { get; private set; } = "Idle";
        public string AppearanceId { get; private set; }
        public int HairStyle { get; private set; }
        public int ShoeStyle { get; private set; }
        public int WingStyle { get; private set; }
        private Transform[] nodes;
        private readonly List<Mesh> meshes = new List<Mesh>();
        private readonly List<Material> materials = new List<Material>();
        private readonly List<GameObject> shoes = new List<GameObject>();
        private float clock, remaining;
        private float wingScale = 1;
        private bool crown = true, jewelry = true;
        private static readonly int[] WingNodes = { 69, 142 };
        private static readonly int[] ArmNodes = { 56, 129 };
        private static readonly int[] ElbowNodes = { 59, 132 };

        private Vector3 ReadVector(BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        private void Awake()
        {
            TextAsset asset = Resources.Load<TextAsset>("Genesis/Character/fairy");
            Material template = Resources.Load<Material>("Genesis/DemoUnlit");
            if (asset == null || template == null) throw new InvalidOperationException("Fairy mesh/material missing.");
            using (BinaryReader reader = new BinaryReader(new MemoryStream(asset.bytes)))
            {
                if (reader.ReadInt32() != 0x31464e47) throw new InvalidDataException("Invalid fairy bundle.");
                nodes = new Transform[reader.ReadInt32()];
                int meshCount = reader.ReadInt32(), materialCount = reader.ReadInt32();
                int[] meshIds = new int[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    int parent = reader.ReadInt32();
                    meshIds[i] = reader.ReadInt32();
                    nodes[i] = new GameObject("Fairy joint " + i).transform;
                    nodes[i].SetParent(parent < 0 ? transform : nodes[parent], false);
                    nodes[i].localPosition = ReadVector(reader);
                    nodes[i].localRotation = new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    nodes[i].localScale = ReadVector(reader);
                }
                int[] materialIds = new int[meshCount];
                for (int i = 0; i < meshCount; i++)
                {
                    materialIds[i] = reader.ReadInt32();
                    Vector3[] vertices = new Vector3[reader.ReadInt32()];
                    int[] triangles = new int[reader.ReadInt32()];
                    Vector3[] normals = new Vector3[vertices.Length];
                    for (int v = 0; v < vertices.Length; v++) vertices[v] = ReadVector(reader);
                    for (int v = 0; v < normals.Length; v++) normals[v] = ReadVector(reader);
                    for (int v = 0; v < triangles.Length; v++) triangles[v] = reader.ReadInt32();
                    Mesh mesh = new Mesh { name = "Original fairy mesh " + i, indexFormat = IndexFormat.UInt32 };
                    mesh.vertices = vertices; mesh.normals = normals; mesh.triangles = triangles;
                    mesh.RecalculateBounds();
                    meshes.Add(mesh);
                }
                for (int i = 0; i < materialCount; i++)
                {
                    Material material = new Material(template) { name = "Fairy palette " + i };
                    material.color = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()).gamma;
                    material.SetFloat("_Cull", 0);
                    materials.Add(material);
                }
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (meshIds[i] < 0) continue;
                    nodes[i].gameObject.AddComponent<MeshFilter>().sharedMesh = meshes[meshIds[i]];
                    nodes[i].gameObject.AddComponent<MeshRenderer>().sharedMaterial = materials[materialIds[meshIds[i]]];
                }
            }
            nodes[0].name = "Original Fairy Queen";
            foreach (int index in WingNodes) nodes[index].name = "Separate wing attachment " + index;
            for (int i = 0; i < 2; i++)
            {
                GameObject shoe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                shoe.name = "Interchangeable shoe " + i;
                shoe.transform.SetParent(nodes[i == 0 ? 65 : 138], false);
                shoe.transform.localPosition = new Vector3(0, -0.68f, -0.045f);
                shoe.transform.localScale = new Vector3(0.14f, 0.09f, 0.24f);
                shoe.GetComponent<Renderer>().sharedMaterial = materials[1];
                Destroy(shoe.GetComponent<Collider>());
                shoes.Add(shoe);
            }
            BuildCelestialHeroine();
            Debug.Log("GENESIS_FAIRY_READY: original adult celestial heroine, separate wings, six reactions, live cosmetics.");
        }

        public void ApplyAppearance(WardrobeDefinition design)
        {
            if (design == null) throw new ArgumentNullException(nameof(design));
            materials[0].color = ParseColor(design.outfit);
            materials[1].color = ParseColor(design.accent);
            materials[6].color = materials[7].color = ParseColor(design.wings);
            wingScale = Mathf.Clamp((float)design.scale, 0.5f, 1.5f);
            AppearanceId = design.id;
            UpdateCosmetics();
        }

        private static Color ParseColor(string hex)
        {
            if (!ColorUtility.TryParseHtmlString("#" + hex.TrimStart('#'), out Color color))
                throw new FormatException("Invalid wardrobe color: " + hex);
            return color;
        }

        public void SetCosmetics(int hair, int shoe, int wing, bool showCrown, bool showJewelry)
        {
            HairStyle = Mathf.Clamp(hair, 0, 2); ShoeStyle = Mathf.Clamp(shoe, 0, 2);
            WingStyle = Mathf.Clamp(wing, 0, 2); crown = showCrown; jewelry = showJewelry;
            UpdateCosmetics();
        }

        private void UpdateCosmetics()
        {
            materials[8].color = HairStyle == 0 ? new Color(0.48f, 0.38f, 0.67f) :
                HairStyle == 1 ? new Color(0.94f, 0.81f, 0.55f) : new Color(0.75f, 0.87f, 1);
            nodes[214].gameObject.SetActive(HairStyle != 2);
            nodes[233].gameObject.SetActive(HairStyle != 2);
            nodes[203].gameObject.SetActive(crown || jewelry);
            foreach (int index in new[] { 204, 207, 208, 209, 210 }) nodes[index].gameObject.SetActive(crown);
            foreach (int index in new[] { 205, 206 }) nodes[index].gameObject.SetActive(jewelry);
            foreach (GameObject shoe in shoes)
            {
                shoe.SetActive(ShoeStyle != 0);
                shoe.transform.localScale = ShoeStyle == 2 ? new Vector3(0.15f, 0.28f, 0.24f) : new Vector3(0.14f, 0.09f, 0.24f);
            }
            foreach (int index in WingNodes)
            {
                nodes[index].gameObject.SetActive(WingStyle != 2);
                nodes[index].localScale = Vector3.one * wingScale * (WingStyle == 1 ? 0.72f : 1);
            }
            UpdateHeroineCosmetics();
        }

        public void PlayReaction(string reaction, float duration = 3)
        {
            if (Array.IndexOf(Reactions, reaction) < 0) throw new ArgumentException("Unknown fairy reaction.");
            Reaction = reaction;
            remaining = Mathf.Max(0, duration);
        }

        private void Update()
        {
            if (nodes == null) return;
            clock += Time.unscaledDeltaTime;
            remaining -= Time.unscaledDeltaTime;
            if (remaining <= 0) Reaction = "Idle";
            bool happy = Reaction == "Happy", dance = Reaction == "Celebration", sorrow = Reaction == "Sorrow";
            bool cast = Reaction == "Casting", surprise = Reaction == "Surprise";
            nodes[0].localPosition = new Vector3(0, Mathf.Sin(clock * 2) * 0.055f, 0);
            nodes[0].localRotation = Quaternion.Euler(0, -7 + (dance ? Mathf.Sin(clock * 2) * 18 : 0), dance ? Mathf.Sin(clock * 3) * 3 : 0);
            nodes[202].localRotation = Quaternion.Euler(sorrow ? -14 : surprise ? 8 : 0, 0, sorrow ? -4 : Mathf.Sin(clock * 1.7f) * 1.4f);
            for (int i = 0; i < 2; i++)
            {
                float side = i == 0 ? -1 : 1;
                float lift = happy ? 55 : cast ? 42 : surprise ? 34 : sorrow ? 5 : 11;
                if (dance) lift = 40 + Mathf.Sin(clock * 3 + i) * 22;
                nodes[ArmNodes[i]].localRotation = Quaternion.Euler(cast ? 28 : 0, 0, side * lift);
                nodes[ElbowNodes[i]].localRotation = Quaternion.Euler(happy || cast ? 48 : 10, 0, 0);
                nodes[WingNodes[i]].localRotation = Quaternion.Euler(0, side * Mathf.Sin(clock * (cast ? 9 : 5)) * 18, 0);
                int eye = i == 0 ? 219 : 238;
                float blink = clock % 4 > 3.84f ? 0.12f : happy ? 0.55f : sorrow ? 0.65f : 1;
                nodes[eye].localScale = new Vector3(1, blink, 1);
            }
            nodes[252].localScale = new Vector3(0.082f, surprise ? 0.065f : happy || dance ? 0.035f : 0.023f, 0.016f);
        }

        private void OnDestroy()
        {
            foreach (Mesh mesh in meshes) if (mesh != null) Destroy(mesh);
            foreach (Material material in materials) if (material != null) Destroy(material);
        }
    }
}