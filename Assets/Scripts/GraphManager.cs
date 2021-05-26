using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyGraph;
using System.Linq;

public class GraphManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> _nodes;
    List<Material> _materials;
    List<Edge<GameObject>> _edges;
    UndirecteGraph<GameObject, Edge<GameObject>> _undirectedGraph;
    List<GameObject> _edgeLines;
    Dijkstra<GameObject, Edge<GameObject>> _dijkstra;
    // VoxelGrid and Voxel List
    VoxelGrid _voxelGrid;
    List<Voxel> _targets = new List<Voxel>();
    List<Voxel> publicPath = new List<Voxel>();
    List<Voxel> privatePath = new List<Voxel>();
    Voxel _start, _stop;

    RandomWalk randomWalk;
    List<Voxel> randomwalk = new List<Voxel>();

    // Start is called before the first frame update
    void Start()
    {
        _voxelGrid = new VoxelGrid(new Vector3Int(8, 3, 12), transform.position, 1f);
        _edges = new List<Edge<GameObject>>();

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
        if (Input.GetKeyDown(KeyCode.Z))
        {
            PrivateNodeToVoxel();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            ReturnPrivateNodeToVoxel();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RandomWalk(privatePath);
        }

    }

    // Should be layer by layer
    void CreatePaths()
    {
        Queue<Voxel> targetPool = new Queue<Voxel>(_targets);
        
        Dijkstra<Voxel, Edge<Voxel>> dijkstra = new EasyGraph.Dijkstra<Voxel, Edge<Voxel>>(_voxelGrid.VoxelGraph);
        publicPath.AddRange(dijkstra.GetShortestPath(targetPool.Dequeue(), targetPool.Dequeue()));
        while (targetPool.Count > 0)
        {
            //Get the distance from the next point to all the points in path
            //take the shortest distance
            //store the shortest path into path
            Voxel nextVoxel = targetPool.Dequeue();
            SetNextShortestPath(nextVoxel, publicPath, dijkstra);
        }
        Debug.Log("public " + publicPath.Count);
        foreach (var voxel in publicPath)
        {
            voxel.SetAsPath();
        }
    }

    void SetNextShortestPath(Voxel targetVoxel, List<Voxel> path, Dijkstra<Voxel, Edge<Voxel>> dijkstra)
    {
        //Set Next Path into X,Z Position
        for (int x = 0; x < _voxelGrid.GridSize.x; x++)
        {
            for (int z = 0; z < _voxelGrid.GridSize.z; z++)
            {
                dijkstra.DijkstraCalculateWeights(targetVoxel);
                Voxel closestVoxel = path.MinBy(v => dijkstra.VertexWeight(v));
                List<Voxel> newpath = new List<Voxel>();
                newpath.AddRange(dijkstra.GetShortestPath(targetVoxel, closestVoxel));
                newpath.Remove(closestVoxel);
                path.AddRange(newpath);
            }
        }
    }

    void RandomWalk(List<Voxel> path)
    {
        foreach (var voxel in path)
        {
            RandomWalk a = new RandomWalk();
            a.RunScript(5, voxel.Index);
            foreach (var index in a.PList)
            {
                if (Util.CheckBounds(index, _voxelGrid)) privatePath.Add(_voxelGrid.GetVoxelByIndex(index));
            }
        }
        Debug.Log("pirvate " + privatePath.Count);
        foreach (var voxel in privatePath)
        {
            voxel.SetAsRandomPath();
        }
    }

    //void RandomWalk(List<Voxel> path)
    //{
    //    foreach (var voxel in path)
    //    {
    //        RandomWalk a = new RandomWalk();
    //        a.RunScript(5, voxel.Index);
    //        foreach (var index in a.PList)
    //        {
    //            if (Util.CheckBounds(index, _voxelGrid)) privatePath.Add(_voxelGrid.GetVoxelByIndex(index));
    //        }
    //    }
    //    Debug.Log("pirvate " + privatePath.Count);
    //    foreach (var voxel in privatePath)
    //    {
    //        voxel.SetAsRandomPath();
    //    }
    //}


    void NodeToVoxel()
    {
        foreach (var voxel in publicPath)
        {
            voxel.VoxelGO.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    void ReturnNodeToVoxel()
    {
        foreach (var voxel in publicPath)
        {
            voxel.VoxelGO.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    
    void PrivateNodeToVoxel()
    {
        foreach (var voxel in privatePath)
        {
            voxel.VoxelGO.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    void ReturnPrivateNodeToVoxel()
    {
        foreach (var voxel in privatePath)
        {
            voxel.VoxelGO.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}
