using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;


[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    public GameObject waterPlane;
    public GameObject roadObject;


    Vector3[] vertices;
    int[] triangles;

    public int gridWidth = 256;
    public int gridHeight = 256;
    public float amplitude = 4;
    public int meshResolution = 2;


    public float maskRadius = 3.0f;

    public float a1, a2, a3, a4;
    public float f1, f2, f3, f4;
    public float xOffset, zOffset;
    public float heightScale;

    public float waterLevel = 30.0f;

    public float slopeInfluence = 1.0f;
    public float distInfluence = 1.0f;
    public float waterInfluence = 0.01f;

    private int meshWidth;
    private int meshHeight;

    private List<TerrainNode> tempMask;
    private List<TerrainNode> tempPath;
    private LineRenderer pathRenderer;

    public int startX = 0, startY = 0;
    public int endX = 256, endY = 256;

    public int StartX { get { return startX; } set { startX = value; } }
    public int StartY { get { return startY; } set { startY = value; } }
    public int EndX { get { return endX; } set { endX = value; } }
    public int EndY { get { return endY; } set { endY = value; } }

    public void SetStartX(int x)
    {
        startX = x;
    }

    TerrainNode[,] nodes;
    Dictionary<TerrainNode, List<TerrainNode>> nodeMasks;

    Path roadPath;
    

    // Start is called before the first frame update
    void Start()
    {
        nodeMasks = new Dictionary<TerrainNode, List<TerrainNode>>();
        endX = gridWidth-1; endY = gridHeight-1;
        pathRenderer = GetComponent<LineRenderer>();
        meshWidth = gridWidth * meshResolution;
        meshHeight = gridHeight * meshResolution;
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
        
        tempMask = GenerateMask(7, 13);
        CalculateMasks();

        tempPath = FindPath(nodes[7, 13], nodes[63, 63]);
        pathRenderer.positionCount = tempPath.Count;
        
        roadPath = new Path(new Vector2(7,13));

        roadPath.IsClosed = false;
        roadPath.AutoSetControlPoints = false;
        for (int i = 0; i < tempPath.Count; i++)
        {
            roadPath.AddSegment(tempPath[i].gridPos);
        }
        roadPath.IsClosed = false;
        
        //roadPath.DeleteSegment(roadPath.NumPoints - 1);

        RoadCreator creator = roadObject.GetComponent<RoadCreator>();
        creator.SetPath(roadPath);
        creator.UpdateRoad();

        Vector3[] pathPositions = new Vector3[tempPath.Count];
        for (int i = 0; i < tempPath.Count; i++)
        {
            pathPositions[i] = tempPath[i].GetVertex();
        }

        pathRenderer.SetPositions(pathPositions);

        string path_out = "Path:";
        foreach (TerrainNode t in tempPath)
        {
            path_out += t.ToString() + "->";
        }
        Debug.Log(path_out);

        for (int i = 1; i < tempPath.Count; i++)
        {
            Debug.DrawLine(tempPath[i - 1].GetVertex(), tempPath[i].GetVertex(), Color.red);
        }
    }

    void CalculateMasks()
    {
        if (nodes == null)
        {
            return;
        }

        for (int z = 0; z <= gridHeight; z++)
        {
            for (int x = 0; x <= gridHeight; x++)
            {
                TerrainNode curr = nodes[x, z];
                nodeMasks.Add(curr, GenerateMask(x, z));
            }
        }
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


    public float GetHeight(int x, int z)
    {
        float xNrm = (1.0f * x + xOffset) / gridWidth;
        float zNrm = (1.0f * z + zOffset) / gridHeight;
        float h1 = Mathf.PerlinNoise(xNrm * f1, zNrm * f1) * a1;
        float h2 = Mathf.PerlinNoise(xNrm * f2, zNrm * f2) * a2;
        float h3 = Mathf.PerlinNoise(xNrm * f3, zNrm * f3) * a3;
        float h4 = Mathf.PerlinNoise(xNrm * f4, zNrm * f4) * a4;

        return h1 + h2 + h3 + h4;
    }

    public float GetHeight(float x, float z)
    {
        float xNrm = (1.0f * x + xOffset) / gridWidth;
        float zNrm = (1.0f * z + zOffset) / gridHeight;
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
                else if (x == 63 && z == 63)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(nodes[x, z].GetVertex(), 0.1f);
                }
                else if (tempMask.Contains(nodes[x,z]))
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(nodes[x, z].GetVertex(), 0.1f);
                }
                else if (tempPath.Contains(nodes[x,z]))
                {
                    Gizmos.color = Color.yellow;
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

    public List<TerrainNode> GenerateMask(int i, int j, float radius)
    {
        float radius2 = radius * radius;
        List<TerrainNode> mask = new List<TerrainNode>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (i == x && j == y)
                {
                    continue;
                }

                if (TerrainNode.Distance2(nodes[x, y], nodes[i, j]) < radius2)
                {
                    mask.Add(nodes[x, y]);
                }

            }
        }

        return mask;
    }

    List<TerrainNode> SlowFindPath(TerrainNode start, TerrainNode end)
    {
        List<TerrainNode> path = new List<TerrainNode>();

        // Add Start node to priority queue
        // Get mask at this location
        // Cost all nodes in the mask and add to priority queue
        // 
        // while pq not empty
        //  pop top
        //  check if goal, if yes, return
        //  check mask
        //  for each neighbor
        //    compute cost
        //    if not on open or closed add to pq
        //    if on closed or open and new is cheaper
        //      remove old and add new with lower cost
        //  put current node on closed list
        //  if open list is empty, no path found

        SearchNode goal = new SearchNode();
        goal.Row = end.Row;
        goal.Col = end.Col;

        List<SearchNode> open = new List<SearchNode>();
        List<SearchNode> closed = new List<SearchNode>();
        SearchNode front = new SearchNode();
        front.givenCost = 0;
        front.heuristicCost = TerrainNode.Distance(start, end);
        front.Row = start.Row;
        front.Col = start.Col;
        front.prev = null;
        open.Add(front);


        while (open.Count > 0)
        {
            // Find the lowest value
            float lowestCost = float.MaxValue;
            int lowestIndex = 0;
            for (int i = 0; i < open.Count; i++)
            {
                float currentCost = open[i].givenCost + open[i].heuristicCost;
                if (currentCost < lowestCost)
                {
                    lowestCost = currentCost;
                    lowestIndex = i;
                }
            }

            // Pop the top
            SearchNode curr = open[lowestIndex];
            open.RemoveAt(lowestIndex);


            // Check if its the goal
            if (curr.Equals(goal))
            {
                // If it is return the path
                // Back propogate and build the path

                //path.Insert(path.Count, );
                while (curr != null)
                {
                    path.Add(nodes[curr.Row, curr.Col]);
                    curr = curr.prev;
                }

                return path;
            }

            List<TerrainNode> mask;
            if (!nodeMasks.TryGetValue(nodes[curr.Row, curr.Col], out mask))
            {
                mask = GenerateMask(curr.Row, curr.Col);
            }

            foreach (TerrainNode t in mask)
            {
                SearchNode sn = new SearchNode();

                TerrainNode ct = nodes[curr.Row, curr.Col];
                TerrainNode tt = nodes[t.Row, t.Col];

                float distCost = TerrainNode.Distance(ct, tt);

                float slopeCost = SlopeCost(ct, tt);



                sn.givenCost = curr.givenCost + distCost + (slopeInfluence * slopeCost);


                sn.heuristicCost = TerrainNode.Distance(
                                    end,
                                    nodes[t.Row, t.Col]
                                );


                sn.Row = t.Row;
                sn.Col = t.Col;
                sn.prev = curr;

                bool onOpen = open.Contains(sn);
                bool onClosed = closed.Contains(sn);

                if (!onOpen && !onClosed)
                {                 
                    open.Add(sn);
                }

                if (onOpen || onClosed)
                {
                    if (onOpen)
                    {
                        for (int i = 0; i < open.Count; i++)
                        {


                            if (open[i].Equals(sn) && sn.Cost < open[i].Cost)
                            {
                                open[i].givenCost = sn.givenCost;
                                open[i].heuristicCost = sn.heuristicCost;
                                open[i].prev = sn.prev;
                            }
                        }
                    }
                    else if (onClosed)
                    {
                        for (int i = 0; i < closed.Count; i++)
                        {
                            if (closed[i].Equals(sn) && sn.Cost < closed[i].Cost)
                            {
                                closed.RemoveAt(i);
                                open.Add(sn);
                            }
                        }
                    }
                }

            }
            
            closed.Add(curr);

        }

        // If path is empty, no path found
        return path;
    }

    List<TerrainNode> FindPath(TerrainNode start, TerrainNode end)
    {
        List<TerrainNode> path = new List<TerrainNode>();

        float timeout = 2.0f;
        float start_time = Time.time;

        SearchNode goal = new SearchNode();
        goal.Row = end.Row;
        goal.Col = end.Col;

        List<SearchNode> open = new List<SearchNode>();
        List<SearchNode> closed = new List<SearchNode>();
        SearchNode front = new SearchNode();
        front.givenCost = 0;
        front.heuristicCost = TerrainNode.Distance(start, end);
        front.Row = start.Row;
        front.Col = start.Col;
        front.prev = null;
        
        

        SimplePriorityQueue<SearchNode> open_pq = new SimplePriorityQueue<SearchNode>();
        HashSet<SearchNode> closed_set = new HashSet<SearchNode>();
        open_pq.Enqueue(front, front.Cost);

        while (open_pq.Count > 0)
        {
            if (Time.time - start_time > timeout)
            {
                return path;
            }

            // Pop the top
            SearchNode curr = open_pq.Dequeue();

            // Check if its the goal
            if (curr.Equals(goal))
            {
                // If it is return the path
                // Back propogate and build the path
                
                //path.Insert(path.Count, );
                while (curr != null)
                {
                    path.Add(nodes[curr.Row, curr.Col]);
                    curr = curr.prev;
                }
                
                return path;
            }

            List<TerrainNode> mask;
            if (!nodeMasks.TryGetValue(nodes[curr.Row, curr.Col], out mask))
            {
                mask = GenerateMask(curr.Row, curr.Col);
            }
             
            foreach(TerrainNode t in mask)
            {

                if (Time.time - start_time > timeout)
                {
                    return path;
                }
                SearchNode sn = new SearchNode();

                TerrainNode ct = nodes[curr.Row, curr.Col];
                TerrainNode tt = nodes[t.Row, t.Col];

                float distCost = TerrainNode.Distance(ct, tt);

                float slopeCost = SlopeCost(ct, tt);

                float waterCost = WaterCost(tt);

                sn.givenCost = curr.givenCost + (distInfluence * distCost) + (slopeInfluence * slopeCost) + (waterInfluence * waterCost);
                                

                sn.heuristicCost = TerrainNode.Distance(
                                    end,
                                    nodes[t.Row, t.Col]
                                );


                sn.Row = t.Row;
                sn.Col = t.Col;
                sn.prev = curr;

                bool onOpen = open_pq.Contains(sn);
                bool onClosed = closed_set.Contains(sn);


                if (!onOpen && !onClosed)
                {
                    open_pq.Enqueue(sn, sn.Cost);
                }
                
                if (onOpen || onClosed)
                {
                    if (onOpen)
                    {

                        float cost = open_pq.GetPriority(sn);
                        if (sn.Cost < cost)
                        {
                            open_pq.Remove(sn);
                            open_pq.Enqueue(sn, sn.Cost);
                        }
                    }
                    else if (onClosed)
                    {
                        SearchNode outNode;
                        closed_set.TryGetValue(sn, out outNode);
                        float cost = outNode.Cost;

                        if (sn.Cost < cost)
                        {
                            closed_set.Remove(sn);
                            open_pq.Enqueue(sn, sn.Cost);
                        }

                    }
                }
                
            }
            closed_set.Add(curr);
            
        }

        // If path is empty, no path found
        return path;
    }

    float SlopeCost(TerrainNode curr, TerrainNode next)
    {
        float diffH = next.height - curr.height;
        float diffX = next.Row - curr.Row;
        float diffY = next.Col - curr.Col;

        float flatDist = Mathf.Sqrt((diffX * diffX) + (diffY * diffY));
        
        if (Mathf.Approximately(flatDist, 0.0f))
        {
            if (Mathf.Approximately(diffH, 0))
                return 0;
            else
                return float.MaxValue;
        }

        return diffH / flatDist;
    }

    float WaterCost(TerrainNode next)
    {
        float cost = Mathf.Max(waterLevel - next.height, 0.0f);
        return cost;
    }

    float FlatDistanceCost(TerrainNode curr, TerrainNode next)
    {
        float diffX = next.Row - curr.Row;
        float diffY = next.Col - curr.Col;
        float flatDist = Mathf.Sqrt((diffX * diffX) + (diffY * diffY));

        return flatDist;
    }



    public void GetPath()
    {
        tempPath.Clear();
        tempPath = FindPath(nodes[startX, startY], nodes[endX, endY]);

        if (tempPath.Count > 0)
        {
            roadPath = new Path(tempPath[0].gridPos);
            
            for (int i = 0; i < tempPath.Count; i++)
            {
                roadPath.AddSegment(tempPath[i].gridPos);
            }
            roadPath.IsClosed = false;
            roadPath.DeleteSegment(roadPath.NumPoints - 1);
            RoadCreator creator = roadObject.GetComponent<RoadCreator>();
            creator.SetPath(roadPath);
            creator.UpdateRoad();

        }
        // Process TempPath
        for (int i = 0; i < tempPath.Count; i++)
        {
            if (tempPath[i].GetVertex().y < waterLevel)
            {
                if (i > 0)
                {
                    tempPath[i].height = Mathf.Max(waterLevel, tempPath[i - 1].height);
                }
                else
                {
                    tempPath[i].height = waterLevel;
                }
            }
        }


        pathRenderer.positionCount = tempPath.Count;



        Vector3[] pathPositions = new Vector3[tempPath.Count];
        for (int i = 0; i < tempPath.Count; i++)
        {
            pathPositions[i] = tempPath[i].GetVertex();
        }

        pathRenderer.SetPositions(pathPositions);

        string path_out = "Path:";
        foreach (TerrainNode t in tempPath)
        {
            path_out += t.ToString() + "->";
        }
        Debug.Log(path_out);
    }

    public void GetTerrain()
    {
        CreateShape();
        UpdateMesh();
    }

    #region GettersAndSetters

    public void SetSlopeInfluence(float f)
    {
        slopeInfluence = f;
    }

    public void SetDistInfluence(float f)
    {
        distInfluence = f;
    }
    public void SetWaterInfluence(float f)
    {
        waterInfluence = f;
    }

    public void SetStartX(string val)
    {
        startX = int.Parse(val);
    }

    public void SetStartY(string val)
    {
        startY = int.Parse(val);
    }

    public void SetEndX(string val)
    {
        endX = int.Parse(val);
    }

    public void SetEndY(string val)
    {
        endY = int.Parse(val);
    }

    public void SetWaterLevel(float height)
    {
        waterLevel = height;
        Vector3 curr_pos = waterPlane.transform.position;
        curr_pos.y = waterLevel;
        waterPlane.transform.position = curr_pos;
    }


    public void SetNoise1Amplitude(float val)
    {
        a1 = val;
    }

    public void SetNoise2Amplitude(float val)
    {
        a2 = val;
    }
    public void SetNoise3Amplitude(float val)
    {
        a3 = val;
    }
    public void SetNoise4Amplitude(float val)
    {
        a4 = val;
    }

    public void SetNoise1Frequency(float val)
    {
        f1 = val;
    }

    public void SetNoise2Frequency(float val)
    {
        f2 = val;
    }
    public void SetNoise3Frequency(float val)
    {
        f3 = val;
    }
    public void SetNoise4Frequency(float val)
    {
        f4 = val;
    }

    public void SetNoiseXOffset(float val)
    {
        xOffset = val;
    }

    public void SetNoiseZOffset(float val)
    {
        zOffset = val;
    }

    #endregion
}

public class SearchNode : IComparable<SearchNode>
{
    public Vector2Int gridPos;
    public float givenCost;
    public float heuristicCost;
    public SearchNode prev;

    public int Row { get { return gridPos.x; } set { gridPos.x = value; } }
    public int Col { get { return gridPos.y; } set { gridPos.y = value; } }

    public float Cost { get { return givenCost + heuristicCost; } }

    public int CompareTo(SearchNode other)
    {
        float cost = (this.givenCost + this.heuristicCost) -
            (other.givenCost + other.heuristicCost);
        if (Mathf.Approximately(cost, 0))
        {
            return 0;
        }

        if (cost < 0)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj.GetType() == typeof(SearchNode))
        {
            SearchNode node = (SearchNode)obj;

            if (this.Row == node.Row && this.Col == node.Col)
            {
                return true;
            }
        }

        return false;
    }

    public override int GetHashCode()
    {
        return this.Row.GetHashCode() + (this.Col.GetHashCode() << 3);
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

    public static float Distance(TerrainNode t1, TerrainNode t2)
    {
        float diffX = t1.Row - t2.Row;
        float diffY = t1.Col - t2.Col;
        float diffH = t1.height - t2.height;

        return Mathf.Sqrt(diffX * diffX) + (diffY * diffY) + (diffH * diffH);
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

    public override string ToString()
    {
        return "[" + this.Row + ", " + this.Col + "]";
    }
}