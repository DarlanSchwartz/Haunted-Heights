using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectBaker : MonoBehaviour
{
#if UNITY_EDITOR
    public void Bake()
    {
        // Get the MeshFilter component
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider collider = GetComponent<MeshCollider>();

        // Create a new instance of the shared mesh
        Mesh newMesh = Instantiate(meshFilter.sharedMesh);
        meshFilter.mesh = newMesh;

        // Get the current scale of the transform
        Vector3 currentScale = transform.localScale;

        // Scale the mesh vertices by the inverse of the current scale
        Vector3[] vertices = newMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].x *= currentScale.x;
            vertices[i].y *= currentScale.y;
            vertices[i].z *= currentScale.z;
        }
        newMesh.vertices = vertices;

        // Set the scale of the transform to (1, 1, 1)
        transform.localScale = Vector3.one;
        collider.sharedMesh = newMesh;
    }


    [MenuItem("GameObject/Bake", false, 10)]
    static void BakeGameObject(MenuCommand menuCommand)
    {
        // Get the selected game object
        GameObject selectedGameObject = menuCommand.context as GameObject;

        // Get or add an ObjectBaker component
        ObjectBaker baker = selectedGameObject.GetComponent<ObjectBaker>();
        if (baker == null)
        {
            baker = selectedGameObject.AddComponent<ObjectBaker>();
        }

        // Call the Bake method on the ObjectBaker component
        baker.Bake();
    }
#endif
}
