using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Genesis
{
    public sealed partial class GenesisFairy
    {
        // Original adult (25+) design: celestial corset, petal skirt, comet hair and star crown.
        // No reference-image pixels, franchise logos, named-character costumes or external meshes.
        private readonly List<GameObject> heroineBoots = new List<GameObject>();
        private readonly List<GameObject> heroineJewels = new List<GameObject>();
        private readonly List<Transform> cometLocks = new List<Transform>();
        private Transform starCrown;
        public string DesignName => "Astra / Celestial Fairy (adult 25+)";

        private GameObject Shape(string shapeName, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject shape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shape.name = shapeName;
            shape.transform.SetParent(parent, false);
            shape.transform.localPosition = position;
            shape.transform.localScale = scale;
            shape.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(shape.GetComponent<Collider>());
            return shape;
        }

        // Ring entries: radius X, height, radius Z. Mesh is closed at both ends.
        private GameObject Profile(string shapeName, Transform parent, Vector3[] rings, Material material, float pleats = 0)
        {
            const int segments = 64;
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            for (int row = 0; row < rings.Length; row++)
            {
                Vector3 ring = rings[row];
                for (int segment = 0; segment <= segments; segment++)
                {
                    float angle = segment * Mathf.PI * 2 / segments;
                    float wave = 1 + pleats * Mathf.Cos(angle * 12);
                    vertices.Add(new Vector3(Mathf.Sin(angle) * ring.x * wave, ring.y, Mathf.Cos(angle) * ring.z * wave));
                    if (row == 0 || segment == segments) continue;
                    int a = (row - 1) * (segments + 1) + segment, b = row * (segments + 1) + segment;
                    triangles.Add(a); triangles.Add(b); triangles.Add(a + 1);
                    triangles.Add(a + 1); triangles.Add(b); triangles.Add(b + 1);
                }
            }
            int bottom = vertices.Count;
            vertices.Add(new Vector3(0, rings[0].y, 0));
            int top = vertices.Count;
            vertices.Add(new Vector3(0, rings[rings.Length - 1].y, 0));
            for (int segment = 0; segment < segments; segment++)
            {
                triangles.Add(bottom); triangles.Add(segment + 1); triangles.Add(segment);
                int last = (rings.Length - 1) * (segments + 1) + segment;
                triangles.Add(top); triangles.Add(last); triangles.Add(last + 1);
            }
            return MeshObject(shapeName, parent, vertices, triangles, material);
        }

        private GameObject MeshObject(string shapeName, Transform parent, List<Vector3> vertices, List<int> triangles, Material material)
        {
            Mesh mesh = new Mesh { name = shapeName, indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            meshes.Add(mesh);
            GameObject shape = new GameObject(shapeName);
            shape.transform.SetParent(parent, false);
            shape.AddComponent<MeshFilter>().sharedMesh = mesh;
            shape.AddComponent<MeshRenderer>().sharedMaterial = material;
            return shape;
        }

        private GameObject Star(string shapeName, Transform parent, Vector3 center, float radius, Material material)
        {
            List<Vector3> vertices = new List<Vector3> { center + new Vector3(0, 0, -0.025f) };
            List<int> triangles = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                float angle = i * Mathf.PI / 5;
                float r = radius * (i % 2 == 0 ? 1 : 0.43f);
                vertices.Add(center + new Vector3(Mathf.Sin(angle) * r, Mathf.Cos(angle) * r, 0));
                triangles.Add(0); triangles.Add(i + 1); triangles.Add((i + 1) % 10 + 1);
            }
            return MeshObject(shapeName, parent, vertices, triangles, material);
        }

        private void BuildCelestialHeroine()
        {
            // Retain stable animation joints; replace the old long gown and bow silhouette.
            nodes[1].gameObject.SetActive(false);
            foreach (int index in new[] { 49, 50, 51, 52, 53, 54, 55 }) nodes[index].gameObject.SetActive(false);
            Transform body = nodes[0];
            Profile("Astra / opaque fitted celestial bodice", body, new[] {
                new Vector3(0.31f,1.77f,0.225f), new Vector3(0.315f,1.88f,0.24f),
                new Vector3(0.225f,2.06f,0.18f), new Vector3(0.255f,2.22f,0.205f),
                new Vector3(0.32f,2.37f,0.24f), new Vector3(0.30f,2.49f,0.19f)
            }, materials[0]);
            Profile("Astra / gold waist band", body, new[] {
                new Vector3(0.245f,1.995f,0.197f), new Vector3(0.237f,2.055f,0.192f)
            }, materials[2]);
            Profile("Astra / opaque skirt undershell", body, new[] {
                new Vector3(0.54f,1.40f,0.36f), new Vector3(0.47f,1.62f,0.31f),
                new Vector3(0.32f,1.91f,0.245f)
            }, materials[1], 0.045f);
            Profile("Astra / layered pleated overskirt", body, new[] {
                new Vector3(0.535f,1.53f,0.37f), new Vector3(0.48f,1.67f,0.32f),
                new Vector3(0.325f,1.94f,0.25f)
            }, materials[0], 0.09f);
            for (int i = 0; i < 12; i++)
            {
                float angle = i * Mathf.PI / 6;
                Shape("Skirt gold hem bead", body, new Vector3(Mathf.Sin(angle) * 0.578f, 1.53f, Mathf.Cos(angle) * 0.403f),
                    Vector3.one * 0.024f, materials[2]);
            }
            Star("Astra / chest star gold setting", body, new Vector3(0, 2.36f, -0.263f), 0.14f, materials[2]);
            Star("Astra / chest amethyst", body, new Vector3(0, 2.36f, -0.293f), 0.087f, materials[5]);
            Star("Astra / waist star", body, new Vector3(0, 2.025f, -0.215f), 0.075f, materials[2]);

            // Long gloves and shaped tall boots remain children of the existing limb pivots.
            for (int i = 0; i < 2; i++)
            {
                int calf = i == 0 ? 65 : 138;
                Profile("Astra / long white glove", nodes[ElbowNodes[i]], new[] {
                    new Vector3(0.059f,-0.32f,0.064f), new Vector3(0.07f,-0.18f,0.073f),
                    new Vector3(0.071f,0.06f,0.073f)
                }, materials[0]);
                Profile("Astra / glove gold cuff", nodes[ElbowNodes[i]], new[] {
                    new Vector3(0.075f,0.045f,0.077f), new Vector3(0.075f,0.08f,0.077f)
                }, materials[2]);
                GameObject boot = new GameObject("Astra / removable tall boot " + i);
                boot.transform.SetParent(nodes[calf], false);
                Profile("Sculpted boot shaft", boot.transform, new[] {
                    new Vector3(0.085f,-0.68f,0.095f), new Vector3(0.085f,-0.52f,0.10f),
                    new Vector3(0.105f,-0.25f,0.108f), new Vector3(0.12f,0.08f,0.122f)
                }, materials[0]);
                Profile("Boot gold top", boot.transform, new[] {
                    new Vector3(0.124f,0.055f,0.126f), new Vector3(0.124f,0.09f,0.126f)
                }, materials[2]);
                Shape("Boot pointed toe", boot.transform, new Vector3(0,-0.685f,-0.075f), new Vector3(0.18f,0.13f,0.36f), materials[0]);
                Shape("Boot gold heel", boot.transform, new Vector3(0,-0.74f,0.06f), new Vector3(0.065f,0.13f,0.07f), materials[2]);
                Star("Boot star", boot.transform, new Vector3(0,0.015f,-0.135f),0.065f,materials[2]);
                heroineBoots.Add(boot);
                GameObject earring = Star("Astra / falling star earring", nodes[202], new Vector3(i == 0 ? -0.245f : 0.245f,-0.15f,-0.07f),0.045f,materials[2]);
                heroineJewels.Add(earring);
            }

            // Distinct asymmetric comet crown instead of paired buns or a franchise emblem.
            starCrown = new GameObject("Astra / comet crown").transform;
            starCrown.SetParent(nodes[202], false);
            for (int i = -2; i <= 2; i++)
            {
                float x = i * 0.076f;
                Shape("Crown crystal spire", starCrown, new Vector3(x,0.27f + (2-Mathf.Abs(i))*0.022f,-0.08f),
                    new Vector3(0.027f,0.11f + (2-Mathf.Abs(i))*0.035f,0.04f),materials[2]);
            }
            Star("Astra / crown north star",starCrown,new Vector3(0.035f,0.37f,-0.11f),0.073f,materials[5]);
            // Face remains recognizably anime, with a smaller nose and less round eye proportions.
            nodes[213].localScale = new Vector3(0.031f,0.059f,0.045f);
            nodes[202].localScale = new Vector3(0.96f,1.01f,1);
            foreach (int index in new[] { 214, 233 })
            {
                Transform lockRoot = new GameObject("Astra / flowing comet hair").transform;
                lockRoot.SetParent(nodes[202], false);
                float side = index == 214 ? -1 : 1;
                lockRoot.localPosition = new Vector3(side * 0.205f,0.12f,0.045f);
                for (int strand = 0; strand < 5; strand++)
                {
                    GameObject lockShape = Profile("Sculpted tapered hair lock", lockRoot, new[] {
                        new Vector3(0.006f,-1.34f,0.008f),new Vector3(0.045f,-1.12f,0.035f),
                        new Vector3(0.064f,-0.65f,0.052f),new Vector3(0.065f,-0.22f,0.06f),
                        new Vector3(0.045f,0.05f,0.04f)
                    },materials[8]);
                    lockShape.transform.localPosition = new Vector3(side * strand * 0.037f, strand * -0.025f,strand * 0.029f);
                    lockShape.transform.localRotation = Quaternion.Euler(0,0,side * (7 + strand * 3));
                }
                cometLocks.Add(lockRoot);
            }
            UpdateHeroineCosmetics();
        }

        private void UpdateHeroineCosmetics()
        {
            if (starCrown == null) return;
            nodes[203].gameObject.SetActive(false); // The new crown/jewelry replaces the original combined slot.
            nodes[214].gameObject.SetActive(false);
            nodes[233].gameObject.SetActive(false);
            starCrown.gameObject.SetActive(crown);
            foreach (GameObject jewel in heroineJewels) jewel.SetActive(jewelry);
            foreach (Transform lockRoot in cometLocks) lockRoot.gameObject.SetActive(HairStyle != 2);
            foreach (GameObject shoe in shoes) shoe.SetActive(ShoeStyle == 1);
            foreach (GameObject boot in heroineBoots) boot.SetActive(ShoeStyle == 2);
            // The original gold necklace remains separate from crown state.
            nodes[47].gameObject.SetActive(jewelry); nodes[48].gameObject.SetActive(jewelry);
        }
    }
}