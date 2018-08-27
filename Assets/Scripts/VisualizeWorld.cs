using Everett.Ebstorf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisualizeWorld : MonoBehaviour {

    private PanelInfoController panelInfoController;

    private float WorldScaleFactor = 10f;

    private static Color HOVERED_CELL_COLOUR = Color.magenta;
    private static Color SELECTED_CELL_COLOUR = Color.red;

    private Mesh Mesh;
    private Dictionary<string, Cell> KnownCells;
    private Dictionary<string, Vector3> NodeIdToVector3Position;
    private Dictionary<string, List<int>> CellIdToVertexVectors;

    private Cell hoveredCell = null;
    private Cell selectedCell = null;
    
    private bool VerticesChanged = false;
    private bool ColoursChanged = false;

    List<Vector3> vertices;
    List<int> triangles;
    List<Color> colours;
    List<Vector2> uvs;

    // Use this for initialization
    void Start () {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colours = new List<Color>();
        uvs = new List<Vector2>();
    }

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = Mesh = new Mesh();
        GetComponent<MeshCollider>().sharedMesh = Mesh;

        KnownCells = new Dictionary<string, Cell>();
        NodeIdToVector3Position = new Dictionary<string, Vector3>();
        CellIdToVertexVectors = new Dictionary<string, List<int>>();
    }

    // Update is called once per frame
    void Update () {
		if (VerticesChanged)
        {
            Mesh.vertices = vertices.ToArray();
            Mesh.triangles = triangles.ToArray();
            Mesh.uv = uvs.ToArray();
            Mesh.RecalculateNormals();
            Mesh.RecalculateBounds();
            Mesh.RecalculateTangents();
            GetComponent<MeshCollider>().sharedMesh = Mesh;
            VerticesChanged = false;
        }
        if (ColoursChanged)
        {
            Mesh.colors = colours.ToArray();
            ColoursChanged = false;
        }
	}

    public void SetWorldScaleFactor(float worldRadiusKm)
    {
        WorldScaleFactor = worldRadiusKm / 1000f;
    }

    public void HoverCell(Cell cell)
    {
        if (hoveredCell == cell)
        {
            return;
        }
        else if (hoveredCell != null)
        {
            if (hoveredCell == selectedCell)
            {
                SetSelectedColourForCell(hoveredCell);
            }
            else
            {
                RestoreColorForCell(hoveredCell);
            }
        }
        // Highlight newly selected cell's vertices
        SetHoveredColourForCell(cell);
        hoveredCell = cell;
        ColoursChanged = true;
    }

    public void SelectCell(Cell cell)
    {
        if (selectedCell == cell)
        {
            return;
        }
        else if (selectedCell != null)
        {
            RestoreColorForCell(selectedCell);
        }
        // Highlight newly selected cell's vertices
        SetSelectedColourForCell(cell);
        selectedCell = cell;
        ColoursChanged = true;
    }

    private void RestoreColorForCell(Cell cell)
    {
        Color normalCellColour = ColorOfCell(cell);
        SetColourForCell(cell, normalCellColour);
    }

    private void SetHoveredColourForCell(Cell cell)
    {
        SetColourForCell(cell, HOVERED_CELL_COLOUR);
    }

    private void SetSelectedColourForCell(Cell cell)
    {
        SetColourForCell(cell, SELECTED_CELL_COLOUR);
    }

    private void SetColourForCell(Cell cell, Color colour)
    {
        foreach (var vertexIndex in CellIdToVertexVectors[cell.Id])
        {
            SetColorForVertex(vertexIndex, colour);
        }
    }

    private void SetColorForVertex(int vertexIndex, Color colour)
    {
        colours[vertexIndex] = colour;
    }

    private void RestoreColorForVertex(int vertexIndex)
    {
        Mesh.colors.SetValue(colours[vertexIndex], vertexIndex);
    }

    private void OnDrawGizmos()
    {
        if (VerticesChanged)
        {
            /*
            foreach (var vertex_index in NodeIdToVertexIndex.Values)
            {
                Gizmos.DrawSphere(vertices[vertex_index], 0.1f);
            }*/
            //MeshChanged = false;
        }
    }

    private Color ColorOfCell(Cell cell)
    {
        // Retrieve colour for cell
        float red = 0.0f;
        float green = 0.0f;
        float blue = 0.0f;
        foreach (var quantity in cell.Quantities)
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
        return new Color(red, green, blue);
    }

    public void AddCell(Cell newCell) {
        KnownCells.Add(newCell.Id, newCell);
        CellIdToVertexVectors.Add(newCell.Id, new List<int>());
        if (KnownCells.Count % 100 == 0)
        {
            Debug.Log("Processed cells " + KnownCells.Count.ToString());
        }
        var vertexColour = ColorOfCell(newCell);
        // Vertices
        var centreVertexIndex = vertices.Count;
        vertices.Add(NodeIdToVector3Position[newCell.Id]);
        // Remember cell centre vector for this cell
        CellIdToVertexVectors[newCell.Id].Add(centreVertexIndex);
        colours.Add(vertexColour);
        // Create vertices for boundary nodes
        int[] boundaryVertexIndices = new int[newCell.Perimeter.Count];
        for (var i = 0; i < newCell.Perimeter.Count; i++)
        {
            int nodeVertexId = vertices.Count;
            vertices.Add(NodeIdToVector3Position[newCell.Perimeter[i].Id]);
            // Remember boundary node vectors for this cell
            CellIdToVertexVectors[newCell.Id].Add(nodeVertexId);
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

    public void MeshFinished()
    {
        VerticesChanged = true;
        ColoursChanged = true;
    }

    public void AddNodeVertex(string nodeId, float x, float y, float z)
    {
        // Scale from unit sphere positions to visualized scale
        // Switch z and y values to change coordinate system from z-up to y-up
        NodeIdToVector3Position.Add(nodeId, new Vector3(x*WorldScaleFactor, z*WorldScaleFactor, y * WorldScaleFactor));
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
