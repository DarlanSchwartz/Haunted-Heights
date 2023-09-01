using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace Schwartz.MeshCreator
{
    #if UNITY_EDITOR

    public class MeshCreator
    {
        public static float width = 1f;
        public static float height = 1f;
        public static float polygonRadius = 1;
        public static int polygonVerticesCount = 30;

        [MenuItem("GameObject/3D Object/Triangle", false, 0)]
        public static void TriangleMesh()
        {
            GameObject temp = new GameObject("Triangle");
            MeshFilter meshFilter = temp.AddComponent(typeof(MeshFilter)) as MeshFilter;
            MeshCollider meshCollider = temp.AddComponent(typeof(MeshCollider)) as MeshCollider;
            MeshRenderer meshRenderer = temp.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            meshRenderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

            Mesh mesh = new Mesh();
            mesh.name = "Triangle";
            meshFilter.mesh = mesh;

            //Verticies
            Vector3[] verticies = new Vector3[3]
            {
        new Vector3(0,0,0), new Vector3(width, 0, 0), new Vector3(0, height, 0)
            };

            //Triangles
            int[] tri = new int[3];

            tri[0] = 0;
            tri[1] = 2;
            tri[2] = 1;

            //normals
            Vector3[] normals = new Vector3[3];

            normals[0] = -Vector3.forward;
            normals[1] = -Vector3.forward;
            normals[2] = -Vector3.forward;

            //UVs
            Vector2[] uv = new Vector2[3];

            uv[0] = new Vector2(0, 0);
            uv[0] = new Vector2(1, 0);
            uv[0] = new Vector2(0, 1);

            //initialise
            mesh.vertices = verticies;
            mesh.triangles = tri;
            mesh.normals = normals;
            mesh.uv = uv;

            meshCollider.sharedMesh = mesh;
        }
        [MenuItem("GameObject/3D Object/Circle", false, 0)]
        public static void PolyMesh()
        {
            GameObject temp = new GameObject("Circle");
            MeshFilter meshFilter = temp.AddComponent(typeof(MeshFilter)) as MeshFilter;
            MeshCollider meshCollider = temp.AddComponent(typeof(MeshCollider)) as MeshCollider;
            MeshRenderer meshRenderer = temp.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            meshRenderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");

            bool breakAfter = false;
            if (Selection.transforms.Length == 1)
            {
                temp.transform.SetParent(Selection.activeTransform);
            }
            else if (Selection.transforms.Length > 1)
            {
                for (int i = 0; i < Selection.transforms.Length; i++)
                {
                    if (Selection.transforms[i].childCount > 0)
                    {
                        for (int j = 0; j < Selection.transforms[i].childCount; j++)
                        {
                            if (Selection.transforms[i].GetChild(j).name != temp.name)
                            {
                                temp.transform.SetParent(Selection.transforms[i]);
                                breakAfter = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        temp.transform.SetParent(Selection.transforms[i]);
                        break;
                    }

                    if (breakAfter)
                    {
                        break;
                    }
                }
            }

            temp.transform.localPosition = Vector3.zero;

            Mesh mesh = new Mesh();
            mesh.name = "Circle";

            meshFilter.mesh = mesh;

            //verticies
            List<Vector3> verticiesList = new List<Vector3> { };
            float x;
            float y;
            for (int i = 0; i < polygonVerticesCount; i++)
            {
                x = polygonRadius * Mathf.Sin((2 * Mathf.PI * i) / polygonVerticesCount);
                y = polygonRadius * Mathf.Cos((2 * Mathf.PI * i) / polygonVerticesCount);
                verticiesList.Add(new Vector3(x, y, 0f));
            }
            Vector3[] verticies = verticiesList.ToArray();

            //triangles
            List<int> trianglesList = new List<int> { };
            for (int i = 0; i < (polygonVerticesCount - 2); i++)
            {
                trianglesList.Add(0);
                trianglesList.Add(i + 1);
                trianglesList.Add(i + 2);
            }
            int[] triangles = trianglesList.ToArray();

            //normals
            List<Vector3> normalsList = new List<Vector3> { };
            for (int i = 0; i < verticies.Length; i++)
            {
                normalsList.Add(-Vector3.forward);
            }
            Vector3[] normals = normalsList.ToArray();

            //initialise
            mesh.vertices = verticies;
            mesh.triangles = triangles;
            mesh.normals = normals;

            meshCollider.sharedMesh = mesh;
        }
        public static Mesh GetPolyMesh(int interactions, float polyRadius)
        {
            Mesh mesh = new Mesh();
            mesh.name = "Circle";

            //verticies
            List<Vector3> verticiesList = new List<Vector3> { };
            float x;
            float y;
            for (int i = 0; i < interactions; i++)
            {
                x = polyRadius * Mathf.Sin((2 * Mathf.PI * i) / interactions);
                y = polyRadius * Mathf.Cos((2 * Mathf.PI * i) / interactions);
                verticiesList.Add(new Vector3(x, y, 0f));
            }
            Vector3[] verticies = verticiesList.ToArray();

            //triangles
            List<int> trianglesList = new List<int> { };
            for (int i = 0; i < (interactions - 2); i++)
            {
                trianglesList.Add(0);
                trianglesList.Add(i + 1);
                trianglesList.Add(i + 2);
            }
            int[] triangles = trianglesList.ToArray();

            //normals
            List<Vector3> normalsList = new List<Vector3> { };
            for (int i = 0; i < verticies.Length; i++)
            {
                normalsList.Add(-Vector3.forward);
            }
            Vector3[] normals = normalsList.ToArray();

            //initialise
            mesh.vertices = verticies;
            mesh.triangles = triangles;
            mesh.normals = normals;

            return mesh;
        }
    }
    #endif
}

