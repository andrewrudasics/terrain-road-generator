using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int gridWidth = 128;
    public int gridHeight = 128;
    public float amplitude = 4;

    public float maskRadius = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
    }


    void CreateShape()
    {
        vertices = new Vector3[(gridWidth + 1) * (gridHeight + 1)];

        
        for (int i = 0, z = 0; z <= gridHeight; z++)
        {
            for (int x = 0; x <= gridHeight; x++)
            {
                float y = Mathf.PerlinNoise(x * 0.3f, z * 0.3f) * amplitude;
                y = Mathf.Min((Mathf.PerlinNoise((x+0.5f) * 0.01f, (z+0.5f) * 0.01f) * amplitude), y);
                y += Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 10.0f;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[gridWidth * gridHeight * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < gridHeight; z++)
        {

            for (int x = 0; x < gridWidth; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + gridWidth + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + gridWidth + 1;
                triangles[tris + 5] = vert + gridWidth + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], 0.1f);
        }
    }
}
