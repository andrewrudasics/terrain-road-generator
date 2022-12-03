using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    public int gridWidth = 256;
    public int gridHeight = 256;
    public float amplitude = 4;
    public int meshResolution = 2;


    public float maskRadius = 3.0f;

    public float a1, a2, a3, a4;
    public float f1, f2, f3, f4;
    public float heightScale;

    private int meshWidth;
    private int meshHeight;

    private List<TerrainNode> tempMask;

    TerrainNode[,] nodes;


    // Start is called before the first frame update
    void Start()
    {
        meshWidth = gridWidth * meshResolution;
        meshHeight = gridHeight * meshResolution;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
        tempMask = GenerateMask(7, 13);
        
    }


    void CreateShape()
    {
        vertices = new Vector3[(meshWidth + 1) * (meshHeight + 1)];
        nodes = new TerrainNode[(gridWidth + 1), (gridHeight + 1)];
        
        for (int i = 0, z = 0; z <= gridHeight; z++)
        {
            for (int x = 0; x <= gridHeight; x++)
            {
                //float y = Mathf.PerlinNoise(x * 0.3f, z * 0.3f) * amplitude;
                //y = Mathf.Min((Mathf.PerlinNoise((x+0.5f) * 0.01f, (z+0.5f) * 0.01f) * amplitude), y);
                //y += Mathf.PerlinNoise(x * 0.05f, z * 0.05f) * 10.0f;
                float y = GetHeight(x, z) * heightScale;
                //vertices[i] = new Vector3(x, y, z);
                nodes[x, z] = new TerrainNode(x, z, y);
                i++;
            }
        }

        for (int i = 0, z = 0; z <= meshWidth; z++)
        {
            for (int x = 0; x <= meshHeight; x++)
            {
                float xPos = (float)x / (float)meshWidth * gridWidth;
                float zPos = (float)z / (float)meshHeight * gridHeight;

                float y = GetHeight(xPos, zPos) * heightScale;
                vertices[i] = new Vector3(xPos, y, zPos);
                i++;
            }
        }



        triangles = new int[meshWidth * meshHeight * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < meshHeight; z++)
        {

            for (int x = 0; x < meshWidth; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + meshWidth + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + meshWidth + 1;
                triangles[tris + 5] = vert + meshWidth + 2;

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

    private float GetHeight(int x, int z)
    {
        float xNrm = (1.0f * x) / gridWidth;
        float zNrm = (1.0f * z) / gridHeight;
        float h1 = Mathf.PerlinNoise(xNrm * f1, zNrm * f1) * a1;
        float h2 = Mathf.PerlinNoise(xNrm * f2, zNrm * f2) * a2;
        float h3 = Mathf.PerlinNoise(xNrm * f3, zNrm * f3) * a3;
        float h4 = Mathf.PerlinNoise(xNrm * f4, zNrm * f4) * a4;

        return h1 + h2 + h3 + h4;
    }

    private float GetHeight(float x, float z)
    {
        float xNrm = (1.0f * x) / gridWidth;
        float zNrm = (1.0f * z) / gridHeight;
        float h1 = Mathf.PerlinNoise(xNrm * f1, zNrm * f1) * a1;
        float h2 = Mathf.PerlinNoise(xNrm * f2, zNrm * f2) * a2;
        float h3 = Mathf.PerlinNoise(xNrm * f3, zNrm * f3) * a3;
        float h4 = Mathf.PerlinNoise(xNrm * f4, zNrm * f4) * a4;

        return h1 + h2 + h3 + h4;
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;


        for (int i = 0, z = 0; z <= gridHeight; z++)
        {
            for (int x = 0; x <= gridHeight; x++)
            {
                if (x == 7 && z == 13)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawSphere(nodes[x, z].GetVertex(), 0.1f);
                }
                else if (tempMask.Contains(nodes[x,z]))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(nodes[x, z].GetVertex(), 0.1f);
                }
                else
                {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawSphere(nodes[x, z].GetVertex(), 0.1f);
                }
                i++;
            }
        }

        //for (int i = 0; i < vertices.Length; i++)
        //{
            


        //    Gizmos.DrawSphere(vertices[i], 0.1f);

        //    Gizmos.color = Color.gray;
        //}
    }

    public List<TerrainNode> GenerateMask(int i, int j)
    {
        float radius2 = maskRadius * maskRadius;
        List<TerrainNode> mask = new List<TerrainNode>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (i == x && j == y)
                {
                    continue;
                }

                if (TerrainNode.Distance2(nodes[x,y], nodes[i,j]) < radius2)
                {
                    mask.Add(nodes[x,y]);
                }
                
            }
        }

        return mask;
    }
}

public class TerrainNode : IComparable<TerrainNode>
{
    public Vector2Int gridPos;
    public float height;

    public int Row { get { return gridPos.x; } set { gridPos.x = value; } }
    public int Col { get { return gridPos.y; } set { gridPos.y = value; } }

    public TerrainNode(int x, int z, float height)
    {
        this.gridPos = new Vector2Int(x, z);
        this.height = height;
    }

    public Vector3 GetVertex()
    {
        return new Vector3(Row, height, Col);
    }

    public static float Distance2(TerrainNode t1, TerrainNode t2)
    {
        float diffX = t1.Row - t2.Row;
        float diffY = t1.Col - t2.Col;
        float diffH = t1.height - t2.height;

        return (diffX * diffX) + (diffY * diffY) + (diffH * diffH);
    }

    public int CompareTo(TerrainNode other)
    {
        int rowDiff = this.Row - other.Row;
        int colDiff = this.Col - other.Col;

        if (rowDiff < 0)
        {
            return -2;
        }
        if (rowDiff > 0)
        {
            return 2;
        }
        if (colDiff < 0)
        {
            return -1;
        }
        if (colDiff > 0)
        {
            return 1;
        }

        return 0;
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() == typeof(TerrainNode))
        {
            return (this.CompareTo((TerrainNode)obj) == 0);
        }

        return false;
    }

}