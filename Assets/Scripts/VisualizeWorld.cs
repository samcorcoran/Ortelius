using Everett.Ebstorf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisualizeWorld : MonoBehaviour {

    private Mesh Mesh;
    private Dictionary<string, Cell> KnownCells;
    private Dictionary<string, int> NodeIdToVertexIndex;


    private bool MeshChanged = false;

    List<Vector3> vertices;
    List<int> triangles;
    List<Color> colours;
    List<Vector2> uvs;


    // Use this for initialization
    void Start () {
        NodeIdToVertexIndex = new Dictionary<string, int>();
        KnownCells = new Dictionary<string, Cell>();
        // Mesh = GetComponent<Mesh>();
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colours = new List<Color>();
        uvs = new List<Vector2>();

    }

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = Mesh = new Mesh();
    }

        // Update is called once per frame
    void Update () {
		if (MeshChanged)
        {
            Mesh.triangles = triangles.ToArray();
            Mesh.vertices = vertices.ToArray();
            Mesh.colors = colours.ToArray();
            Mesh.uv = uvs.ToArray();
            Mesh.RecalculateNormals();
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

    // TODO: Function to receive new data through
    public void AddCell(Cell newCell) {
        KnownCells.Add(newCell.Id, newCell);
        var centreVertexIndex = NodeIdToVertexIndex[newCell.Id];
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
        for (var i = 0; i < newCell.Perimeter.Count; i++)
        {
            triangles.Add(NodeIdToVertexIndex[newCell.Perimeter[(i + 1) % newCell.Perimeter.Count].Id]);
            triangles.Add(NodeIdToVertexIndex[newCell.Perimeter[i].Id]);
            triangles.Add(centreVertexIndex);
            colours[centreVertexIndex] = vertexColour;
            colours[NodeIdToVertexIndex[newCell.Perimeter[i].Id]] = vertexColour;
            colours[NodeIdToVertexIndex[newCell.Perimeter[(i + 1) % newCell.Perimeter.Count].Id]] = vertexColour;
        }
        //AddTriangle();
        MeshChanged = true;
    }

    public void AddNodeVertex(string nodeId, Vector3 vertex)
    {
        // Get node verts and store them
        var nextIndex = vertices.Count;
        if (nextIndex % 100 == 0)
        {
            Debug.Log("Total vertices: " + nextIndex.ToString());
        }
        vertices.Add(vertex);
        colours.Add(Color.blue);
        uvs.Add(new Vector2(vertex.x, vertex.y));
        NodeIdToVertexIndex.Add(nodeId, nextIndex);
        MeshChanged = true;
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

}
