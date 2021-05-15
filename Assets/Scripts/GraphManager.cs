using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyGraph;
using System.Linq;

public class GraphManager : MonoBehaviour
{

    List<GameObject> _nodes;
    [SerializeField]
    List<Material> _materials;

    List<Edge<GameObject>> _edges;
    UndirecteGraph<GameObject, Edge<GameObject>> _undirectedGraph;
    List<GameObject> _edgeLines;
    Dijkstra<GameObject, Edge<GameObject>> _dijkstra;

    // VoxelGrid and Voxel List
    VoxelGrid _voxelGrid;
    List<Voxel> _targets = new List<Voxel>();
    List<Voxel> path = new List<Voxel>();

    List<Voxel> allpath = new List<Voxel>();


    Voxel _start, _stop;




    // Start is called before the first frame update
    void Start()
    {
        _voxelGrid = new VoxelGrid(new Vector3Int(6, 3, 10), transform.position, 1f);

        _edges = new List<Edge<GameObject>>();

        //ResetGraphLines();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreatePaths();
        }

        //ResetGraphLines();

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;

            if (objectHit.CompareTag("Node") || objectHit.CompareTag("TargetNode"))
            {
                Voxel voxel = objectHit.GetComponentInParent<VoxelTrigger>().ConnectedVoxel;

                if (voxel.IsTarget)
                {
                    _targets.Remove(voxel);
                    voxel.IsTarget = false;
                }
                else
                {
                    _targets.Add(voxel);
                    voxel.IsTarget = true;
                }

            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            NodeToVoxel();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ReturnNodeToVoxel();
        }
    }

    void CreatePaths()
    {
        Queue<Voxel> targetPool = new Queue<Voxel>(_targets);
        
        Dijkstra<Voxel, Edge<Voxel>> dijkstra = new EasyGraph.Dijkstra<Voxel, Edge<Voxel>>(_voxelGrid.VoxelGraph);
        path.AddRange(dijkstra.GetShortestPath(targetPool.Dequeue(), targetPool.Dequeue()));
        //Debug.Log(path.Count);
        while (targetPool.Count > 0)
        {
            //Get the distance from the next point to all the points in path
            //take the shortest distance
            //store the shortest path into path
            Voxel nextVoxel = targetPool.Dequeue();
            SetNextShortestPath(nextVoxel, path, dijkstra);

        }
        Debug.Log(path.Count);
        foreach (var voxel in path)
        {
            voxel.SetAsPath();
        }
    }

    void SetNextShortestPath(Voxel targetVoxel, List<Voxel> path, Dijkstra<Voxel, Edge<Voxel>> dijkstra)
    {
        dijkstra.DijkstraCalculateWeights(targetVoxel);
        Voxel closestVoxel = path.MinBy(v => dijkstra.VertexWeight(v));
        List<Voxel> newpath = new List<Voxel>();
        newpath.AddRange(dijkstra.GetShortestPath(targetVoxel, closestVoxel));
        newpath.Remove(closestVoxel);
        path.AddRange(newpath);
    }

    void GetNeighborVoxel(Voxel targetVoxel, List<Voxel> path, List<Voxel> newpath)
    {
        //Get the All Path in List? or Array?
        List<Voxel> allpath = new List<Voxel>();


    }


    void NodeToVoxel()
    {
        foreach (var voxel in path)
        {
            voxel.VoxelGO.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    void ReturnNodeToVoxel()
    {
        foreach (var voxel in path)
        {
            voxel.VoxelGO.transform.GetChild(0).gameObject.SetActive(false);
        }
    }


    //void ResetGraphLines()
    //{
    //    _edgeLines.ForEach(e => GameObject.Destroy(e));
    //    _edgeLines.Clear();
    //    List<Edge<GameObject>> edges = _undirectedGraph.GetEdges();
    //    foreach (var edge in edges)
    //    {
    //        GameObject edgeLine = new GameObject($"Edge {_edgeLines.Count}");
    //        LineRenderer renderer = edgeLine.AddComponent<LineRenderer>();
    //        renderer.SetPosition(0, edge.Source.transform.position);
    //        renderer.SetPosition(1, edge.Target.transform.position);
    //        renderer.startWidth = 0.2f;
    //        renderer.startColor = new Color(1f, 0f, 0f);
    //        renderer.endWidth = 0.2f;
    //        renderer.startColor = new Color(0f, 1f, 0f);
    //        _edgeLines.Add(edgeLine);
    //    }
    //}
}
