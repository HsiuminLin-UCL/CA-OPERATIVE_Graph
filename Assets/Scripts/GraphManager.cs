using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyGraph;
using System.Linq;

public class GraphManager : MonoBehaviour
{
    #region Fields
    [SerializeField]
    List<GameObject> _nodes;
    List<Material> _materials;
    List<Edge<GameObject>> _edges;
    UndirecteGraph<GameObject, Edge<GameObject>> _undirectedGraph;
    List<GameObject> _edgeLines;
    Dijkstra<GameObject, Edge<GameObject>> _dijkstra;
    
    // VoxelGrid and Voxel List
    VoxelGrid _voxelGrid;
    List<Voxel> boundary = new List<Voxel>();
    List<Voxel> _targets = new List<Voxel>();

    List<Voxel> publicPath = new List<Voxel>();
    List<Voxel> privatePath = new List<Voxel>();
    List<Voxel> privateNode = new List<Voxel>();
    List<Voxel> privateVoxel = new List<Voxel>();
    List<Voxel> semiVoxel = new List<Voxel>();
    List<Voxel> publicVoxel = new List<Voxel>();

    Voxel _start, _stop;
    Voxel _voxel;

    //RandomWalk randomWalk;
    //List<Voxel> randomwalk = new List<Voxel>();
    //List<Voxel> PList = new List<Voxel>();
    #endregion

    #region Unity Method
    // Start is called before the first frame update
    void Start()
    {
        _voxelGrid = new VoxelGrid(new Vector3Int(8, 3, 12), transform.position, 1f);
        _edges = new List<Edge<GameObject>>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetClickedAsTarget();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CreatePaths();
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
            GetBoundaryNode();
        }
    }

    #endregion


    #region Public Method
    public void DrawVoxelGridRange()
    {
        //Draw the Grid Range to Define the Bounding of our Building

        //Not Sure How to Do in Here!

        //Get the mesh from the Site model
        //Create the VoxelGrid with site mesh by 4*4(m) grid
        //Draw the line to define the ragne of VoxelGrid
        //Set up the layers by we want
    }

    public void GetBoundaryNode()
    {
        //Voxel boundarynode = new Voxel();
        
        var neighbours = _voxel.GetFaceNeighboursArray();

        // If Voxel less than 5 neighbour, into the boundary list
        for (int i = 0; i < neighbours.Length; i++)
        {

            if (neighbours[5] != null)
            {
                if (neighbours[5].IsActive)
                {
                    foreach (var voxel in boundary)
                    {
                        Debug.Log("boundary " + boundary.Count);
                    }
                }
            }
        }
    }

    public void RamdomTargetNode()
    {
        //Call the random node in the boundary List
        foreach (var voxel in boundary)
        {
            //Set up the random range
            //var random node to voxel.IsTarget   


            //if (voxel.IsTarget)
            //{
            //    _targets.Remove(voxel);
            //    voxel.IsTarget = false;
            //}
            //else
            //{
            //    _targets.Add(voxel);
            //    voxel.IsTarget = true;
            //}

        }
    }

    public void CreatePaths()
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

    public void SetNextShortestPath(Voxel targetVoxel, List<Voxel> path, Dijkstra<Voxel, Edge<Voxel>> dijkstra)
    {
        //Set Next Path into X,Z Position
        dijkstra.DijkstraCalculateWeights(targetVoxel);
        //Voxel closestVoxel = path.MinBy(v => dijkstra.VertexWeight(v));
        Voxel closestVoxel;
        if (path.Any(p => p.Index.y == targetVoxel.Index.y))
        {
            var layerVoxels = path.Where(p => p.Index.y == targetVoxel.Index.y);
            closestVoxel = layerVoxels.MinBy(v => dijkstra.VertexWeight(v));
        }
        else closestVoxel = path.MinBy(v => dijkstra.VertexWeight(v));

        List<Voxel> newpath = new List<Voxel>();
        newpath.AddRange(dijkstra.GetShortestPath(targetVoxel, closestVoxel));
        newpath.Remove(closestVoxel);
        path.AddRange(newpath);
    }

    public void PrivatePath(Voxel targetVoxel, List<Voxel> path, Dijkstra<Voxel, Edge<Voxel>> dijkstra)
    {
        
        //Get the boundary List
        foreach (var voxel in boundary)
        {
            //Remove the node occupied by the publicPath
            //Create the random _target in the boundary List
            //var the random _target into priaveNode List

            //foreach (var voxel in privateNode)
            //{

            //}
        }

        //Connect the privateNode to the publicPath
        //Use ShortestPath method
        dijkstra.DijkstraCalculateWeights(targetVoxel);
        Voxel closestVoxel = path.MinBy(v => dijkstra.VertexWeight(v));


        //var the path into privatePath List

        foreach (var voxel in privatePath)
        {
            voxel.SetAsPrivatePath();
        }

    }

    public void PrivateVoxel(List<Voxel> privateNode)
    {
        
        //Get the privateNode List
        //Generate the voxel group with 2,3,4 different volume voxel with the privateNode List (ref M2C3??)
        //var the voxel into privateVoxel List
        foreach (var voxel in privateVoxel)
        {
            voxel.SetAsPrivatePath();
        }

    }


    public void SemiPublicVoxel(List<Voxel> privatePath, List<Voxel> privateVoxel)
    {
        //Get the List from privatePath
        //Remove the node occupied by the privateVoxel List
        List<Voxel> semi = new List<Voxel>();
        //semi.Remove();


        //var voxel in semipublic List
        foreach (var voxel in semiVoxel)
        {
            voxel.SetAsSemi();
        }
    }

    public void FillUpPublicVoxel()
    {
        //Fill up the public voxel by remained node
        //Get the all List (publicPath,privatePath,privateNode,semiVoxel)
        //Fill up the voxel by random.range (Combination WS?? M2C3??)
        //var fill up voxel and publicPath into publicVoxel List

    }

    #region RandomWalk
    //void RandomWalk(Voxel targetVoxel, List<Voxel> path, Dijkstra<Voxel, Edge<Voxel>> dijkstra)
    //{
    //    //Queue<Voxel> boundary = new Queue<Voxel>(_boundary);
    //    Queue<Voxel> boundary = new Voxel[5];

    //    Voxel closestVoxel;
    //    if (path.Any(p => p.Index.y == targetVoxel.Index.y))
    //    {
    //        var boundVoxel = path.Where(p => p.Index.y++ == targetVoxel.Index.y++);
    //        closestVoxel = boundVoxel.MinBy(v => dijkstra.VertexWeight(v));
    //    }
    //    else closestVoxel = path.MinBy(v => dijkstra.VertexWeight(v));

    //    Debug.Log("pirvate " + privatePath.Count);
    //    foreach (var voxel in privatePath)
    //    {
    //        voxel.SetAsRandomPath();
    //    }

    //}

    //void RandomWalk(List<Voxel> path)
    //{
    //    foreach (var voxel in path)
    //    {
    //        RandomWalk a = new RandomWalk();
    //        a.RunScript(3, voxel.Index);
    //        foreach (var index in a.PList)
    //        {
    //            if (Util.CheckBounds(index, _voxelGrid)) privatePath.Add(_voxelGrid.GetVoxelByIndex(index));
    //        }
    //    }
    //    Debug.Log("pirvate " + privatePath.Count);
    //    foreach (var voxel in privatePath)
    //    {
    //        voxel.SetAsPrivatePath();
    //    }
    //}
    #endregion

    public void StartAnimation()
    {
        //StartCoroutine();
    }

    #endregion

    #region Private Method

    private void SetClickedAsTarget()
    {
        
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
    }


    private void NodeToVoxel()
    {
        foreach (var voxel in publicPath)
        {
            voxel._voxelGO.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    private void ReturnNodeToVoxel()
    {
        foreach (var voxel in publicPath)
        {
            voxel._voxelGO.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private void PrivateNodeToVoxel()
    {
        foreach (var voxel in privatePath)
        {
            voxel._voxelGO.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
    private void ReturnPrivateNodeToVoxel()
    {
        foreach (var voxel in privatePath)
        {
            voxel._voxelGO.transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    //private void SemiNodeToVoxel()
    //{
    //    foreach (var voxel in semiPath)
    //    {
    //        voxel._voxelGO.transform.GetChild(2).gameObject.SetActive(true);
    //    }
    //}
    //private void ReturnSemiNodeToVoxel()
    //{
    //    foreach (var voxel in semiPath)
    //    {
    //        voxel._voxelGO.transform.GetChild(2).gameObject.SetActive(false);
    //    }
    //}

    #endregion
}


