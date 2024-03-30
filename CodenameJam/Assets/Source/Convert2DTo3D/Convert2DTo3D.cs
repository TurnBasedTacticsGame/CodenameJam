using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Source.Convert2DTo3D
{
    public class Convert2DTo3D : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Collider cylinderPrefab;
        [SerializeField] private Collider cubePrefab;
        [SerializeField] private Material defaultMeshMaterial;

        private List<Mesh> meshes = new();

        // Start is called before the first frame update
        private void Start()
        {
            foreach (var collider in FindObjectsOfType<Collider2D>())
            {
                switch (collider)
                {
                    case CircleCollider2D circleCollider:
                    {
                        var cylinder = Instantiate(cylinderPrefab, collider.transform.position, Quaternion.identity);
                        var diameter = circleCollider.radius * 2;
                        cylinder.transform.localScale = new Vector3(diameter, 0.5f, diameter); // Cylinder has height 1 up, 1 down.
                        cylinder.transform.Rotate(new Vector3(90, 0, 0), Space.Self);

                        cylinder.transform.SetParent(transform);

                        break;
                    }
                    case BoxCollider2D boxCollider:
                    {
                        var cube = Instantiate(cubePrefab, collider.transform.position, Quaternion.identity);
                        cube.transform.localScale = new Vector3(boxCollider.size.x, 1, boxCollider.size.y); // Cube has height 0.5 up, 0.5 down
                        cube.transform.Rotate(new Vector3(90, 0, 0), Space.Self);

                        cube.transform.SetParent(transform);

                        break;
                    }
                    case PolygonCollider2D polygonCollider:
                    {
                        var meshObject = new GameObject("MeshCollider3D");
                        var meshFilter = meshObject.AddComponent<MeshFilter>();

                        var meshData = new MeshData();

                        // Triangulate
                        {
                            for (var i = 0; i < polygonCollider.points.Length; i++)
                            {
                                var start = (Vector3)polygonCollider.points[i];
                                var end = (Vector3)polygonCollider.points[(i + 1) % polygonCollider.points.Length];

                                var quad = new Quad
                                {
                                    TopLeft = end + Vector3.forward * 0.5f,
                                    TopRight = start + Vector3.forward * 0.5f,
                                    BottomLeft = end + Vector3.back * 0.5f,
                                    BottomRight = start + Vector3.back * 0.5f,
                                };

                                quad.Triangulate(meshData);
                                quad.Reverse();
                                quad.Triangulate(meshData);
                            }
                        }

                        var mesh = meshData.ToMesh();
                        meshFilter.sharedMesh = mesh;
                        meshes.Add(mesh);

                        var meshRenderer = meshObject.AddComponent<MeshRenderer>();
                        meshRenderer.sharedMaterial = defaultMeshMaterial;

                        meshObject.transform.position = collider.transform.position;
                        meshObject.transform.SetParent(transform);

                        break;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // Cleanup meshes, not part of prefab
            for (var i = meshes.Count - 1; i > 0; i--)
            {
                Destroy(meshes[i]);
            }
        }

        private class MeshData
        {
            public List<Vector3> Vertices { get; } = new();
            public List<Vector3> Normals { get; } = new();
            public List<Vector2> Uv { get; } = new();
            public List<int> Triangles { get; } = new();

            public Mesh ToMesh()
            {
                return new Mesh
                {
                    vertices = Vertices.ToArray(),
                    normals = Normals.ToArray(),
                    triangles = Triangles.ToArray(),
                    uv = Uv.ToArray(),
                };
            }
        }

        private struct Quad
        {
            public Vector3 BottomLeft;
            public Vector3 TopLeft;
            public Vector3 TopRight;
            public Vector3 BottomRight;

            public Vector2 UvOffset;
            public Vector2 UvSize;

            public void Triangulate(MeshData meshData)
            {
                var verticesStartIndex = meshData.Vertices.Count;

                meshData.Vertices.Add(BottomLeft);
                meshData.Uv.Add(UvOffset);

                meshData.Vertices.Add(TopLeft);
                meshData.Uv.Add(UvOffset + Vector2.up * UvSize.y);

                meshData.Vertices.Add(TopRight);
                meshData.Uv.Add(UvOffset + Vector2.right * UvSize.x + Vector2.up * UvSize.y);

                meshData.Vertices.Add(BottomRight);
                meshData.Uv.Add(UvOffset + Vector2.right * UvSize.x);

                // Triangle 1
                meshData.Triangles.Add(verticesStartIndex + 0);
                meshData.Triangles.Add(verticesStartIndex + 1);
                meshData.Triangles.Add(verticesStartIndex + 2);

                // Triangle 2
                meshData.Triangles.Add(verticesStartIndex + 2);
                meshData.Triangles.Add(verticesStartIndex + 3);
                meshData.Triangles.Add(verticesStartIndex + 0);

                var normal = Vector3.Cross(TopRight - TopLeft, BottomLeft - TopLeft);
                for (var i = 0; i < 4; i++)
                {
                    meshData.Normals.Add(normal);
                }
            }

            public void Reverse()
            {
                var temp = BottomLeft;
                BottomLeft = BottomRight;
                BottomRight = temp;

                temp = TopLeft;
                TopLeft = TopRight;
                TopRight = temp;
            }
        }
    }
}