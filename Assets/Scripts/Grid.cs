using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyGraph;

public class Grid : MonoBehaviour
{
    [SerializeField]
    Vector2Int _gridDimensions = new Vector2Int(10, 15);
    //Vector3Int _gridSize = new Vector3Int(10, 3, 10);

    GameObject[,] _grid;
    Gradient _gradient = new Gradient();

    List<Edge<GameObject>> _edges;
    UndirecteGraph<GameObject, Edge<GameObject>> _undirectedGraph;
    List<GameObject> _edgeLines;
    Dijkstra<GameObject, Edge<GameObject>> _dijkstra;

    // Start is called before the first frame update
    void Start()
    {
        GameObject voxelPrefab = Resources.Load<GameObject>("Prefabs/Node");
        _grid = new GameObject[_gridDimensions.x, _gridDimensions.y];
        _edges = new List<Edge<GameObject>>();

        for (int x = 0; x < _gridDimensions.x; x++)
            for (int y = 0; y < _gridDimensions.y; y++)
            {
                _grid[x, y] = GameObject.Instantiate(voxelPrefab, new Vector3(x * 2, 0, y * 2), Quaternion.identity);
                if (x > 0) _edges.Add(new Edge<GameObject>(_grid[x, y], _grid[x - 1, y]));
                if (y > 0) _edges.Add(new Edge<GameObject>(_grid[x, y], _grid[x, y - 1]));
            }
        _undirectedGraph = new UndirecteGraph<GameObject, Edge<GameObject>>(_edges);
        _edgeLines = new List<GameObject>();
        ResetGraphLines();
        SetGradient();
    }

    void SetGradient()
    {

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        GradientColorKey[] colorKey = new GradientColorKey[2];
        colorKey[0].color = Color.green;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.red;
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 0.0f;
        alphaKey[1].time = 1.0f;

        _gradient.SetKeys(colorKey, alphaKey);
    }

    // Update is called once per frame
    void Update()
    {
        ResetGraphLines();

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0) && Physics.Raycast(ray, out hit))
        {
            GameObject objectHit = hit.transform.gameObject;
            if (objectHit.tag == "Node")
            {
                _dijkstra = new Dijkstra<GameObject, Edge<GameObject>>(_undirectedGraph);
                _dijkstra.DijkstraCalculateWeights(objectHit);
                ColourNodes();
            }
        }
    }

    void ResetGraphLines()
    {
        _edgeLines.ForEach(e => GameObject.Destroy(e));
        _edgeLines.Clear();
        List<Edge<GameObject>> edges = _undirectedGraph.GetEdges();
        foreach (var edge in edges)
        {
            GameObject edgeLine = new GameObject($"Edge {_edgeLines.Count}");
            LineRenderer renderer = edgeLine.AddComponent<LineRenderer>();
            renderer.SetPosition(0, edge.Source.transform.position);
            renderer.SetPosition(1, edge.Target.transform.position);
            renderer.startWidth = 0.2f;
            renderer.startColor = new Color(1f, 0f, 0f);
            renderer.endWidth = 0.2f;
            renderer.startColor = new Color(0f, 1f, 0f);
            _edgeLines.Add(edgeLine);
        }
    }



    void ColourNodes()
    {
        double maxWeight = _dijkstra.MaxDistance;
        //Debug.Log($"Max weight {maxWeight}");
        for (int x = 0; x < _gridDimensions.x; x++)
            for (int y = 0; y < _gridDimensions.y; y++)
            {
                double weight = _dijkstra.VertexWeight(_grid[x, y]);

                Material material = _grid[x, y].GetComponent<MeshRenderer>().material;

                material.color = _gradient.Evaluate((float)Remap(weight, 0, maxWeight, 0, 1));
            }
    }

    double Remap(double value, double oldMin, double oldMax, double newMin, double newMax)
    {
        var remappedValue = newMin + (value - oldMin) * (newMax - newMin) / (oldMax - oldMin);
        //Debug.Log($"Remapped value {remappedValue}, value: {value}, oldMin: {oldMin}, oldMax: {oldMax}, newMin: {newMax}, newMax: {newMax}");
        return remappedValue;
    }
}
