using Everett.Ebstorf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisualizeWorld : MonoBehaviour {

    private Mesh Mesh;
    private Dictionary<string, Cell> KnownCells;
    private Dictionary<string, Vector3> NodeIdToVector3Position;

    private bool MeshChanged = false;

    List<Vector3> vertices;
    List<int> triangles;
    List<Color> colours;
    List<Vector2> uvs;


    // Use this for initialization
    void Start () {
        NodeIdToVector3Position = new Dictionary<string, Vector3>();
        KnownCells = new Dictionary<string, Cell>();
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colours = new List<Color>();
        uvs = new List<Vector2>();
    }

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = Mesh = new Mesh();
        GetComponent<MeshCollider>().sharedMesh = Mesh;
    }

    // Update is called once per frame
    void Update () {
		if (MeshChanged)
        {
            Mesh.vertices = vertices.ToArray();
            Mesh.triangles = triangles.ToArray();
            Mesh.colors = colours.ToArray();
            Mesh.uv = uvs.ToArray();
            Mesh.RecalculateNormals();
            Mesh.RecalculateBounds();
            Mesh.RecalculateTangents();
            GetComponent<MeshCollider>().sharedMesh = Mesh;
            MeshChanged = false;
        }
	}

    private void OnDrawGizmos()
    {
        if (MeshChanged)
        {
            /*
            foreach (var vertex_index in NodeIdToVertexIndex.Values)
            {
                Gizmos.DrawSphere(vertices[vertex_index], 0.1f);
            }*/
            //MeshChanged = false;
        }
    }

    public void AddCell(Cell newCell) {
        KnownCells.Add(newCell.Id, newCell);
        if (KnownCells.Count % 100 == 0)
        {
            Debug.Log("Processed cells " + KnownCells.Count.ToString());
        }
        // Retrieve colour for cell
        float red = 0.0f;
        float green = 0.0f;
        float blue = 0.0f;
        foreach (var quantity in newCell.Quantities)
        {
            if (quantity.Key.Name.Equals("red"))
            {
                red = (float)quantity.Value / 255.0f;
            }
            else if (quantity.Key.Name.Equals("green"))
            {
                green = (float)quantity.Value / 255.0f;
            }
            else if (quantity.Key.Name.Equals("blue"))
            {
                blue = (float)quantity.Value / 255.0f;
            }
        }
        var vertexColour = new Color(red, green, blue);
        // Vertices
        var centreVertexIndex = vertices.Count;
        vertices.Add(NodeIdToVector3Position[newCell.Id]);
        colours.Add(vertexColour);
        // Create vertices for boundary nodes
        int[] boundaryVertexIndices = new int[newCell.Perimeter.Count];
        for (var i = 0; i < newCell.Perimeter.Count; i++)
        {
            int nodeVertexId = vertices.Count;
            vertices.Add(NodeIdToVector3Position[newCell.Perimeter[i].Id]);
            colours.Add(vertexColour);
            boundaryVertexIndices[i] = nodeVertexId;
        }
        // Create triangles
        for (var i = 0; i < boundaryVertexIndices.Length; i++)
        {
            int currentNodeVertexIndex = boundaryVertexIndices[i];
            int nextNodeVertexIndex = boundaryVertexIndices[(i + 1) % boundaryVertexIndices.Length];
            triangles.Add(currentNodeVertexIndex);
            triangles.Add(nextNodeVertexIndex);
            triangles.Add(centreVertexIndex);
        }
    }

    public void RedrawMesh()
    {
        MeshChanged = true;
    }

    public void AddNodeVertex(string nodeId, Vector3 vertex)
    {
        NodeIdToVector3Position.Add(nodeId, vertex);
    }

    public Cell GetContainingCell(Vector3 targetPoint)
    {
        float closestDistance = 20f;
        Cell closestCell = null;
        foreach (var cell in KnownCells.Values)
        {
            var distance = Vector3.Distance(targetPoint, NodeIdToVector3Position[cell.Id]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCell = cell;
            }
        }
        return closestCell;
    }
}
