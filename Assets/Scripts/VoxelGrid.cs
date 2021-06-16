using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using EasyGraph;

public enum LineType { Empty, Public, Private}
public class VoxelGrid
{
    #region Public fields

    public Vector3Int GridSize;
    public Voxel[,,] Voxels;
    public Corner[,,] Corners;
    public Face[][,,] Faces = new Face[3][,,];
    public Edge[][,,] Edges = new Edge[3][,,];
    public Vector3 Origin;
    public Vector3 Corner;
    public float VoxelSize { get; private set; }
    public UndirecteGraph<Voxel, Edge<Voxel>> VoxelGraph;
    
    Gradient _gradient = new Gradient();
    List<Edge<Voxel>> _edges;
    List<GameObject> _edgeLines;
    Dijkstra<Voxel, Edge<Voxel>> _dijkstra;
    public GameObject _line;

    #endregion

    #region Constructors
    public Voxel GetVoxelByIndex(Vector3Int index) => Voxels[index.x, index.y, index.z];

    /// <summary>
    /// Constructor for a basic <see cref="VoxelGrid"/>
    /// </summary>
    /// <param name="size">Size of the grid</param>
    /// <param name="origin">Origin of the grid</param>
    /// <param name="voxelSize">The size of each <see cref="Voxel"/></param>
    public VoxelGrid(Vector3Int size, Vector3 origin, float voxelSize)
    {
        GridSize = size;
        Origin = origin;
        VoxelSize = voxelSize;
        var prefab = Resources.Load<GameObject>("Prefabs/Node");
        
        Voxels = new Voxel[GridSize.x, GridSize.y, GridSize.z];

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    Voxels[x, y, z] = new Voxel(
                        new Vector3Int(x, y, z),
                        this,
                        prefab);
                }
            }
        }

        MakeFaces();
        MakeCorners();
        MakeEdges();
        CreateGraph();
    }


    public void CreateGraph()
    {
        _edges = new List<Edge<Voxel>>();

        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    if (x < GridSize.x - 1) _edges.Add(new Edge<Voxel>(Voxels[x, y, z], Voxels[x + 1, y, z]));
                    if (y < GridSize.y - 1) _edges.Add(new Edge<Voxel>(Voxels[x, y, z], Voxels[x, y + 1, z]));
                    if (z < GridSize.z - 1) _edges.Add(new Edge<Voxel>(Voxels[x, y, z], Voxels[x, y, z + 1]));
                }
            }
        }

        VoxelGraph = new UndirecteGraph<Voxel, Edge<Voxel>>(_edges);
        _edgeLines = new List<GameObject>();
        //Status = LineType.Empty;
        //ResetGraphLines();
    }

    //public LineType _lineType = LineType.Empty;

    //public LineType Status
    //{
    //    get
    //    {
    //        return _lineType;
    //    }
    //    set
    //    {
    //        _line.SetActive(value == LineType.Empty);
    //        _lineType = value;
    //    }
    //}

    //public void SetAsPublicLine()
    //{
    //    if (Status == LineType.Public)
    //    {     
    //        _line.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Public");
    //    }
    //}
    //public void SetAsPrivateLine()
    //{
    //    if (Status == LineType.Private)
    //    {

    //        _line.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Private");
    //    }
    //}


    public void ResetGraphLines()
    {
        // Adjust the Line Grid in here
        _edgeLines.ForEach(e => GameObject.Destroy(e));
        _edgeLines.Clear();
        List<Edge<Voxel>> edges = VoxelGraph.GetEdges();
        foreach (var edge in edges)
        {
            GameObject edgeLine = new GameObject($"Edge {_edgeLines.Count}");
            LineRenderer renderer = edgeLine.AddComponent<LineRenderer>();

            renderer.SetPosition(0, edge.Source._voxelGO.transform.position);
            renderer.SetPosition(1, edge.Target._voxelGO.transform.position);
            renderer.startWidth = 0.02f;
            renderer.startColor = new Color(0f, 0f, 0f);
            renderer.endWidth = 0.02f;
            renderer.startColor = new Color(0f, 0f, 0f);
            _edgeLines.Add(edgeLine);
        }

    }

    #endregion

    #region Grid elements constructors

    /// <summary>
    /// Creates the Faces of each <see cref="Voxel"/>
    /// </summary>
    private void MakeFaces()
    {
        // make faces
        Faces[0] = new Face[GridSize.x + 1, GridSize.y, GridSize.z];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[0][x, y, z] = new Face(x, y, z, Axis.X, this);
                }

        Faces[1] = new Face[GridSize.x, GridSize.y + 1, GridSize.z];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[1][x, y, z] = new Face(x, y, z, Axis.Y, this);
                }

        Faces[2] = new Face[GridSize.x, GridSize.y, GridSize.z + 1];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Faces[2][x, y, z] = new Face(x, y, z, Axis.Z, this);
                }
    }

    /// <summary>
    /// Creates the Corners of each Voxel
    /// </summary>
    private void MakeCorners()
    {
        Corner = new Vector3(Origin.x - VoxelSize / 2, Origin.y - VoxelSize / 2, Origin.z - VoxelSize / 2);

        Corners = new Corner[GridSize.x + 1, GridSize.y + 1, GridSize.z + 1];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Corners[x, y, z] = new Corner(new Vector3Int(x, y, z), this);
                }
    }

    /// <summary>
    /// Creates the Edges of each Voxel
    /// </summary>
    private void MakeEdges()
    {
        Edges[2] = new Edge[GridSize.x + 1, GridSize.y + 1, GridSize.z];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Edges[2][x, y, z] = new Edge(x, y, z, Axis.Z, this);
                }

        Edges[0] = new Edge[GridSize.x, GridSize.y + 1, GridSize.z + 1];

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[0][x, y, z] = new Edge(x, y, z, Axis.X, this);
                }

        Edges[1] = new Edge[GridSize.x + 1, GridSize.y, GridSize.z + 1];

        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[1][x, y, z] = new Edge(x, y, z, Axis.Y, this);
                }
    }

    #endregion

    #region Grid operations

    /// <summary>
    /// Get the Faces of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the faces</returns>
    public IEnumerable<Face> GetFaces()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Faces[n].GetLength(0);
            int ySize = Faces[n].GetLength(1);
            int zSize = Faces[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Faces[n][x, y, z];
                    }
        }
    }

    /// <summary>
    /// Get the Voxels of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Voxels</returns>
    public IEnumerable<Voxel> GetVoxels()
    {
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    yield return Voxels[x, y, z];
                }
    }

    /// <summary>
    /// Get the Corners of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Corners</returns>
    public IEnumerable<Corner> GetCorners()
    {
        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    yield return Corners[x, y, z];
                }
    }

    /// <summary>
    /// Get the Edges of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the edges</returns>
    public IEnumerable<Edge> GetEdges()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Edges[n].GetLength(0);
            int ySize = Edges[n].GetLength(1);
            int zSize = Edges[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Edges[n][x, y, z];
                    }
        }
    }

    #endregion


    #region Public Methods
    
    public List<Voxel> GetBoundaryVoxels()
    {
        List<Voxel> result = new List<Voxel>();
        foreach (var voxel in GetVoxels())
        {
            if (voxel.IsFacade) result.Add(voxel);
        }

        return result;
    }

    #endregion

}