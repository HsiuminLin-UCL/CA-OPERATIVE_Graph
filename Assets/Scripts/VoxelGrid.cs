using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using EasyGraph;

public class VoxelGrid
{
    #region Fields
    public float VoxelSize { get; private set; }

    public Voxel[,,] Voxels;
    public Vector3 Origin;
    public Vector3 Corner;
    public Vector3Int GridSize;
    public UndirecteGraph<Voxel, Edge<Voxel>> VoxelGraph;

    Gradient _gradient = new Gradient();
    List<Edge<Voxel>> _edges;
    List<GameObject> _edgeLines;
    Dijkstra<Voxel, Edge<Voxel>> _dijkstra;

    #endregion

    public VoxelGrid(Vector3Int size, Vector3 origin, float voxelSize)
    {
        GridSize = size;
        Origin = origin;
        VoxelSize = voxelSize;

        //Create Prefab in Unity
        var prefab = Resources.Load<GameObject>("Prefabs/Node");
        //GameObject voxelPrefab = Resources.Load<GameObject>("Prefabs/Node");

        //Initiate the Voxel array
        Voxels = new Voxel[GridSize.x, GridSize.y, GridSize.z];
        

        //Populate the array with the new Voxels
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                    Voxels[x, y, z] = new Voxel(new Vector3Int(x, y, z), this, prefab);

        CreateGraph();
    }

    void CreateGraph()
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
        ResetGraphLines();
        //SetGradient();
    }

    //void SetGradient()
    //{

    //    // Populate the color keys at the relative time 0 and 1 (0 and 100%)
    //    GradientColorKey[] colorKey = new GradientColorKey[2];
    //    colorKey[0].color = Color.green;
    //    colorKey[0].time = 0.0f;
    //    colorKey[1].color = Color.red;
    //    colorKey[1].time = 1.0f;

    //    // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
    //    GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
    //    alphaKey[0].alpha = 1.0f;
    //    alphaKey[0].time = 0.0f;
    //    alphaKey[1].alpha = 0.0f;
    //    alphaKey[1].time = 1.0f;

    //    _gradient.SetKeys(colorKey, alphaKey);
    //}

    void ResetGraphLines()
    {
        // Adjust the Line Grid in here
        _edgeLines.ForEach(e => GameObject.Destroy(e));
        _edgeLines.Clear();
        List<Edge<Voxel>> edges = VoxelGraph.GetEdges();
        foreach (var edge in edges)
        {
            GameObject edgeLine = new GameObject($"Edge {_edgeLines.Count}");
            LineRenderer renderer = edgeLine.AddComponent<LineRenderer>();
            renderer.SetPosition(0, edge.Source.VoxelGO.transform.position);
            renderer.SetPosition(1, edge.Target.VoxelGO.transform.position);
            renderer.startWidth = 0.05f;
            renderer.startColor = new Color(1f, 0f, 0f);
            renderer.endWidth = 0.05f;
            renderer.startColor = new Color(0f, 1f, 0f);
            _edgeLines.Add(edgeLine);
        }
    }

    //void ColourNodes()
    //{
    //    double maxWeight = _dijkstra.MaxDistance;
    //    //Debug.Log($"Max weight {maxWeight}");
    //    for (int x = 0; x < GridSize.x; x++)
    //        for (int y = 0; y < GridSize.y; y++)
    //            for (int z = 0; z < GridSize.z; z++)
    //            {
    //                double weight = _dijkstra.VertexWeight(Voxels[x, y, z]);
    //                Material material = Voxels[x, y, z].VoxelGO.GetComponent<MeshRenderer>().material;
    //                material.color = _gradient.Evaluate((float)Remap(weight, 0, maxWeight, 0, 1));
    //            }
    //}

    //double Remap(double value, double oldMin, double oldMax, double newMin, double newMax)
    //{
    //    var remappedValue = newMin + (value - oldMin) * (newMax - newMin) / (oldMax - oldMin);
    //    //Debug.Log($"Remapped value {remappedValue}, value: {value}, oldMin: {oldMin}, oldMax: {oldMax}, newMin: {newMax}, newMax: {newMax}");
    //    return remappedValue;
    //}

}
