using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyGraph;
using System.Linq;
using Unity.MLAgents;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    #region Fields
    [SerializeField]

    List<GameObject> _nodes;
    List<GameObject> _line;
    List<Material> _materials;
    List<Edge<GameObject>> _edges;
    UndirecteGraph<GameObject, Edge<GameObject>> _undirectedGraph;
    List<GameObject> _edgeLines;
    Dijkstra<GameObject, Edge<GameObject>> _dijkstra;

    // VoxelGrid and Voxel List
    Vector3Int _gridSize;
    public VoxelGrid _voxelGrid { get; private set; }

    List<Voxel> _boundary = new List<Voxel>();
    List<Voxel> _targets = new List<Voxel>();

    List<Voxel> _publicPath = new List<Voxel>();
    List<Voxel> _privatePath = new List<Voxel>();
    List<Voxel> _privateNode = new List<Voxel>();
    List<Voxel> _privateVoxels = new List<Voxel>();
    List<Voxel> _semiVoxel = new List<Voxel>();
    List<Voxel> _publicVoxel = new List<Voxel>();

   
    Voxel _start, _stop;
    Voxel _voxel;

    public int _targetPrivateAmount = 20;

    //PrivateVoxel[,,] _privatevoxelgroup;
    //PrivateVoxel _selected;

    //RandomWalk randomWalk;
    //List<Voxel> randomwalk = new List<Voxel>();
    //List<Voxel> PList = new List<Voxel>();
    #endregion

    #region Unity Method
    // Start is called before the first frame update
    void Start()
    {
        _gridSize = new Vector3Int(8, 3, 12);
        _voxelGrid = new VoxelGrid(_gridSize, transform.position, 1f);

        //_voxelGrid = new VoxelGrid(new Vector3Int(8, 3, 12), transform.position, 1f);

        _edges = new List<Edge<GameObject>>();
        _boundary = _voxelGrid.GetBoundaryVoxels();



        
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
        if (Input.GetKeyDown(KeyCode.C))
        {
            ResetEnvironment();
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
            GeneratePrivatePaths();
        }

        //Test
        if (Input.GetKeyDown(KeyCode.T))
        {
            //RandomPrivate();
        }



    }

    #endregion

    
    #region Public Method

    public Voxel GetRandomBoundaryVoxel()
    {
        var shuffledBoundary = _boundary.OrderBy(v => Random.value);
        return shuffledBoundary.First(v => !_publicPath.Contains(v));
    }

    public void CreatePaths()
    {
        // targetPool = Queue<Voxel>(_targets) object
        Queue<Voxel> targetPool = new Queue<Voxel>(_targets);

        Dijkstra<Voxel, Edge<Voxel>> dijkstra = new EasyGraph.Dijkstra<Voxel, Edge<Voxel>>(_voxelGrid.VoxelGraph);
        _publicPath.AddRange(dijkstra.GetShortestPath(targetPool.Dequeue(), targetPool.Dequeue()));

        while (targetPool.Count > 0)
        {
            //Get the distance from the next point to all the points in path
            //take the shortest distance
            //store the shortest path into path
            Voxel nextVoxel = targetPool.Dequeue();
            SetNextShortestPath(nextVoxel, _publicPath, dijkstra);
        }
        foreach (var voxel in _publicPath)
        {
            voxel.SetAsPublicPath();
            voxel.SetAsPublicVoxel();
            Debug.Log("public " + _publicPath.Count);
        }
        foreach (var edge in _publicPath)
        {
            _voxelGrid.SetAsPublicLine();
        }

        //_voxelGrid.SetAsPublicLine();
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

    public void GeneratePrivatePaths()
    {
        Dijkstra<Voxel, Edge<Voxel>> dijkstra = new EasyGraph.Dijkstra<Voxel, Edge<Voxel>>(_voxelGrid.VoxelGraph);
        int createdPaths = 0;
        while (createdPaths < _targetPrivateAmount)
        {
            // get a random voxel for boundary
            var origin = GetRandomBoundaryVoxel();

            //_privateNode.AddRange(dijkstra.DijkstraCalculateWeights(origin));
            //var origin = GetRandomBoundaryVoxel(_privateNode);
            //var origin = _privateNode.GetRandomBoundaryVoxel().ToList();

            dijkstra.DijkstraCalculateWeights(origin);

            // try to connect to the closest point in the public path of this layer
            // if none available, use the closest point

            Voxel target;
            var targetsOnLayer = _publicPath.Where(v => v.Index.y == origin.Index.y).ToList();
            if (targetsOnLayer.Count > 0) target = targetsOnLayer.MinBy(v => dijkstra.VertexWeight(v));
            else target = _publicPath.MinBy(v => dijkstra.VertexWeight(v));

            var path = dijkstra.GetShortestPath(origin, target);
            foreach (var voxel in path)
            {
                if (!_publicPath.Contains(voxel))
                {
                    voxel.SetAsPrivatePath();
                    //voxel.SetAsPrivateVoxel();
                    if (!_privatePath.Contains(voxel)) _privatePath.Add(voxel);            
                }
            }
            createdPaths++; // createdPaths = createdPaths + 1;
            Debug.Log("private path " + _privatePath.Count);
        }
    }

    public void RandomWalk(int time, Vector3Int Start, List<Voxel> path)
    {
        List<Voxel> randomwalker = new List<Voxel>();
        //Ramdom random = new Ramdom(seed);

        int walkers = 0;
        int x;
        int y;
        int z;
        int step (int rnd)
        {
            int choice = rnd;
            if (choice == 0)
            {
                x++;
            }
            else if (choice == 1)
            {
                x--;
            }
            else if (choice == 2)
            {
                y++;
            }
            else if (choice == 3)
            {
                y--;
            }
            else if (choice == 4)
            {
                z++;
            }
            else if (choice == 5)
            {
                z--;
            }
            return choice;
        }

        //while (walkers < _targetPrivateAmount)
        //{
        //    var origin = _publicPath.Where(v => _publicPath.Contains(v));
        //    var end = _boundary;
        //    var randomwalk = new Voxel();

        //    // var voxel = from origin to end 
        //    // if the step between 3 to 5 and on the boundary = true, else false
        //    // if = true, the first step(voxel) = semi voxel
        //    // change step 3,4,5 voxels to different private voxel color.

        //    if (randomwalker == _boundary)
        //    {
        //        if (//step run in 3 times.)
        //        {
        //            //Change voxel state
        //        }

        //        if (//step run in 4 times)
        //        {
        //            //change voxel state
        //        }

        //        if (//step run in 5 times)
        //        {
        //            //change voxel state
        //        }
        //        else return;
        //    }

        //    foreach (var voxel in path)
        //    {
        //        if (!_publicPath.Contains(voxel))
        //        {
        //            voxel.SetAsPrivatePath();
        //            //voxel.SetAsPrivateVoxel();
        //            if (!_privatePath.Contains(voxel)) _privatePath.Add(voxel);
        //        }
        //    }
        //    walkers++;
        //    Debug.Log("private path " + _privatePath.Count);
        //}

    }

    public void SemiPublicVoxel(List<Voxel> privatePath, List<Voxel> privateVoxel)
    {
        //Get the List from privatePath
        //Remove the node occupied by the privateVoxel List
        List<Voxel> semi = new List<Voxel>();
        //semi.Remove();


        //var voxel in semipublic List
        foreach (var voxel in _semiVoxel)
        {
            voxel.SetAsSemiVoxel();
        }
    }

    public void FillUpPublicVoxel()
    {
        //Fill up the public voxel by remained node
        //Get the all List (publicPath,privatePath,privateNode,semiVoxel)
        //Fill up the voxel by random.range (Combination WS?? M2C3??)
        //var fill up voxel and publicPath into publicVoxel List

    }


    #endregion

    #region IEnumerator
    public void StartCreatePublicPath()
    {
        StartCoroutine(CreatePathAnimated());
    }
    public void StartGeneratePrivatePaths()
    {
        StartCoroutine(GeneratePrivatePathsAnimated());
    }
    public void StartCreatePublicVoxel()
    {
        StartCoroutine(NodeToVoxelAnimated());
    }
    public void StartCreatePrivateVoxel()
    {
        StartCoroutine(PrivateNodeToVoxelAnimated());
    }

    private IEnumerator CreatePathAnimated()
    {
        Queue<Voxel> targetPool = new Queue<Voxel>(_targets);

        Dijkstra<Voxel, Edge<Voxel>> dijkstra = new EasyGraph.Dijkstra<Voxel, Edge<Voxel>>(_voxelGrid.VoxelGraph);
        _publicPath.AddRange(dijkstra.GetShortestPath(targetPool.Dequeue(), targetPool.Dequeue()));

        while (targetPool.Count > 0)
        {
            //Get the distance from the next point to all the points in path
            //take the shortest distance
            //store the shortest path into path
            Voxel nextVoxel = targetPool.Dequeue();
            SetNextShortestPath(nextVoxel, _publicPath, dijkstra);
        }
        Debug.Log("public " + _publicPath.Count);
        foreach (var voxel in _publicPath)
        {
            voxel.SetAsPublicVoxel();
            yield return new WaitForSeconds(0.1f);
        }
    }
    private IEnumerator GeneratePrivatePathsAnimated()
    {
        Dijkstra<Voxel, Edge<Voxel>> dijkstra = new EasyGraph.Dijkstra<Voxel, Edge<Voxel>>(_voxelGrid.VoxelGraph);

        int createdPaths = 0;
        while (createdPaths < _targetPrivateAmount)
        {
            // get a random voxel
            var origin = GetRandomBoundaryVoxel();
            foreach (var voxel in _privateNode)
            {
                Debug.Log("private node " + _privateNode.Count);
            }
            dijkstra.DijkstraCalculateWeights(origin);

            // try to connect to the closest point in the public path of this layer
            // if none available, use the closest point
            Voxel target;
            var targetsOnLayer = _publicPath.Where(v => v.Index.y == origin.Index.y).ToList();
            if (targetsOnLayer.Count > 0) target = targetsOnLayer.MinBy(v => dijkstra.VertexWeight(v));
            else target = _publicPath.MinBy(v => dijkstra.VertexWeight(v));

            var path = dijkstra.GetShortestPath(origin, target);
            foreach (var voxel in path)
            {
                if (!_publicPath.Contains(voxel))
                {
                    voxel.SetAsPrivateVoxel();
                    if (!_privatePath.Contains(voxel)) _privatePath.Add(voxel);
                    Debug.Log("private path " + _privatePath.Count);
                }
            }
            createdPaths++;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator NodeToVoxelAnimated()
    {
        foreach (var voxel in _publicPath)
        {           
            voxel._voxelGO.transform.GetChild(0).gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
    }
    private IEnumerator PrivateNodeToVoxelAnimated()
    {
        foreach (var voxel in _privatePath)
        {
            voxel._voxelGO.transform.GetChild(1).gameObject.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion

    #region Private Method

    private void ResetEnvironment()
    {
        
    }

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
        foreach (var voxel in _publicPath)
        {
            //voxel.Status = VoxelType.Public;
            voxel.SetAsPublicVoxel();
            //voxel._voxelGO.transform.GetChild(0).gameObject.SetActive(true);         
        }
    }
    private void ReturnNodeToVoxel()
    {
        foreach (var voxel in _publicPath)
        {
            voxel._voxelGO.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    private void PrivateNodeToVoxel()
    {
        foreach (var voxel in _privatePath)
        {
            voxel.SetAsPrivateVoxel();
            //voxel._voxelGO.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
    private void ReturnPrivateNodeToVoxel()
    {
        foreach (var voxel in _privatePath)
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

    //private void Voxels()
    //{
    //    foreach (var voxel in _voxelGrid.Voxels)
    //    {
    //        if (voxel.Status == VoxelType.Empty)
    //        {

    //        }
    //    }
    //}


    #endregion
}


