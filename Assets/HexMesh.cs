using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {
    Mesh hexMesh;
    List<Vector3> vertices;
    List<int> triangles;
    List<Color> colors;
    MeshCollider meshCollider;

    public static class HexMetrics
    {
        public const float outerRadius = 10f;
        public const float innerRadius = outerRadius * 0.866025404f;
        public const float solidFactor = 0.75f;
        public const float blendFactor = 1f - solidFactor;

        public static Vector3[] corners = {
            new Vector3(0f, 0f, outerRadius),
            new Vector3(innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius),
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(0f, 0f, outerRadius)
        };
        public static Vector3 GetFirstCorner(HexDirection direction)
        {
            return corners[(int)direction];
        }

        public static Vector3 GetSecondCorner(HexDirection direction)
        {
            return corners[(int)direction + 1];
        }

        public static Vector3 GetFirstSolidCorner(HexDirection direction)
        {
            return corners[(int)direction] * solidFactor;
        }

        public static Vector3 GetSecondSolidCorner(HexDirection direction)
        {
            return corners[(int)direction + 1] * solidFactor;
        }

        public static Vector3 GetBridge(HexDirection direction)
        {
            return (corners[(int)direction] + corners[(int)direction + 1]) *
                blendFactor;
        }
    }

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
        vertices = new List<Vector3>();
        colors = new List<Color>();
        triangles = new List<int>();
    }

    public void Triangulate(HexCell[] cells)
    {
        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }
        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        AddTriangle(center, v1, v2);
        AddTriangleColor(cell.color);
        if (direction <= HexDirection.SE)
        {
            TriangulateConnection(direction, cell, v1, v2);
        }
        /*
        Vector3 bridge = HexMetrics.GetBridge(direction);

        //Vector3 v3 = center + HexMetrics.GetFirstCorner(direction);
        //Vector3 v4 = center + HexMetrics.GetSecondCorner(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;

        AddQuad(v1, v2, v3, v4);

        //AddTriangle(
        //    center,
        //    center + HexMetrics.GetFirstSolidCorner(direction),
        //    center + HexMetrics.GetSecondSolidCorner(direction));

        HexCell neighbor = cell.GetNeighbor(direction) ?? cell;
        //Color edgeColor = (cell.color + neighbor.color) * 0.5f;
        HexCell prevNeighbor = cell.GetNeighbor(direction.Previous()) ?? cell;
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next()) ?? cell;
        //AddTriangleColor(
        //    cell.color,
        //    (cell.color + prevNeighbor.color + neighbor.color) / 3f,
        //    (cell.color + neighbor.color + nextNeighbor.color) / 3f);
        //AddQuadColor(
        //    cell.color, 
        //    cell.color,
        //    (cell.color + prevNeighbor.color + neighbor.color) / 3f,
        //    (cell.color + neighbor.color + nextNeighbor.color) / 3f);

        Color bridgeColor = (cell.color + neighbor.color) * 0.5f;
        AddQuadColor(
            cell.color, bridgeColor);

        AddTriangle(v1, center + HexMetrics.GetFirstCorner(direction), v3);
        AddTriangleColor(
            cell.color,
            (cell.color + prevNeighbor.color + neighbor.color) / 3f,
            bridgeColor);

        AddTriangle(v2, v4, center + HexMetrics.GetSecondCorner(direction));
        AddTriangleColor(
            cell.color,
            bridgeColor,
            (cell.color + neighbor.color + nextNeighbor.color) / 3f);
            */
    }

    void TriangulateConnection(
        HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
    {

        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null)
        {
            return;
        }

        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;
        AddQuad(v1, v2, v3, v4);
        AddQuadColor(cell.color, neighbor.color);

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (nextNeighbor != null)
        {
            AddTriangle(v2, v4, v2);
            AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
        }
        if (direction <= HexDirection.E && nextNeighbor != null)
        {
            AddTriangle(v2, v4, v2 + HexMetrics.GetBridge(direction.Next()));
            AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
        }
    }

    void AddTriangleColor(Color c)
    {
        colors.Add(c);
        colors.Add(c);
        colors.Add(c);
    }

    void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }
    void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }

    void AddQuadColor(Color c1, Color c2)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
