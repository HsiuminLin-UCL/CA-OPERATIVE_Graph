using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EasyGraph;
using System.Linq;
using Random = UnityEngine.Random;

public enum VoxelType { Empty, Public, Private, Semi }
public class Voxel : IEquatable<Voxel>
{
    #region Public Fields

    public Vector3Int Index;
    public List<Face> Faces = new List<Face>(6);
    public Vector3 Center => (Index + _voxelGrid.Origin) * _size;
    public bool IsActive;
    public bool IsOccupied;
    public bool IsOrigin;
    //public bool IsBoundary;
    public GameObject _voxelGO;
    public bool IsPublicPath;
    public bool IsPrivateNode;
    public bool IsPrivatePath;
    public bool IsSemi;
    
    
    public bool IsTarget 
    {
        get
        {
            return _isTarget;
        }
        set
        {
            _isTarget = value;

            if (!value)
            {
                _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Basic");
                _voxelGO.tag = "Node";
                //_voxelGO.transform.GetChild(0).gameObject.SetActive(false);
                
            }
            else
            {
                _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Target");
                _voxelGO.tag = "TargetNode";
                return;
            }
        }
    }
    #endregion

    #region Private Fields

    private bool _isTarget;
    
    private VoxelType _voxelType = VoxelType.Empty;

    #endregion

    #region Protected fields

    protected VoxelGrid _voxelGrid;
    protected float _size;

    #endregion

    #region Graph Edge
    List<Edge<GameObject>> _edges;
    UndirecteGraph<GameObject, Edge<GameObject>> _undirectedGraph;
    List<GameObject> _edgeLines;
    Dijkstra<GameObject, Edge<GameObject>> _dijkstra;
    #endregion

    #region Contructors

    /// <summary>
    /// Creates a regular voxel on a voxel grid
    /// </summary>
    /// <param name="index">The index of the Voxel</param>
    /// <param name="voxelgrid">The <see cref="VoxelGrid"/> this <see cref="Voxel"/> is attached to</param>
    /// <param name="voxelGameObject">The <see cref="GameObject"/> used on the Voxel</param>
    public Voxel(Vector3Int index, VoxelGrid voxelGrid, GameObject voxelGameObject)
    {
        Index = index;
        _voxelGrid = voxelGrid;
        _size = _voxelGrid.VoxelSize;
        _voxelGO = GameObject.Instantiate(voxelGameObject, (_voxelGrid.Origin + Index) * _size, Quaternion.identity);
        _voxelGO.transform.position = (_voxelGrid.Origin + Index) * _size;
        _voxelGO.transform.localScale *= _voxelGrid.VoxelSize;
        _voxelGO.name = $"Voxel_{Index.x}_{Index.y}_{Index.z}";
        //_voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Basic");
        _voxelGO.GetComponent<VoxelTrigger>().ConnectedVoxel = this;

        Status = VoxelType.Empty;
    }

    #endregion

    #region Public methods
    

    public VoxelType Status
    {
        get
        {
            return _voxelType;
        }
        set
        {
            _voxelGO.SetActive(value == VoxelType.Empty);
            _voxelType = value;
        }

    }

    public void SetAsPublicVoxel()
    {
        if (Status != VoxelType.Public)
        {
            //_voxelGO.GetComponent<VoxelTrigger>().ConnectedVoxel.Status = VoxelType.Public;
            _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Public");
            _voxelGO.transform.GetChild(0).gameObject.SetActive(true);
        }
    }
    public void SetAsPrivateVoxel()
    {
        if (Status != VoxelType.Private)
        {
            //_voxelGO.GetComponent<VoxelTrigger>().ConnectedVoxel.Status = VoxelType.Private;
            _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Private");
            _voxelGO.transform.GetChild(1).gameObject.SetActive(true);
        }
    }
    public void SetAsSemiVoxel()
    {
        if (Status != VoxelType.Semi)
        {
            //_voxelGO.GetComponent<VoxelTrigger>().ConnectedVoxel.Status = VoxelType.Semi;
            _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Semi");
            _voxelGO.transform.GetChild(2).gameObject.SetActive(true);
        }
    }

    public void SetAsPublicPath()
    {
        if (!IsTarget)
        {
            _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Public");
            _voxelGO.tag = "PublicPath";
            IsPublicPath = true;
        }
    }
    public void SetAsPrivatePath()
    {
        if (!IsTarget && !IsPublicPath)
        {
            _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Private");
            _voxelGO.tag = "PrivatePathVoxel";
            IsPrivatePath = true;
        }
    }


    //public void SetAsPrivateVoxel()
    //{
    //    if (!IsTarget && !IsPath)
    //    {
    //        _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Private");
    //        _voxelGO.tag = "PrivateVoxel";
    //        IsPrivateVoxel = true;
    //    }
    //}



    //public void SetAsSemi()
    //{
    //    if (!IsTarget && !IsPath)
    //    {
    //        _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Semi");
    //        _voxelGO.tag = "SemiVoxel";
    //        IsSemi = true;
    //    }
    //}


    /// <summary>
    /// Get the neighbouring voxels at each face, if it exists
    /// </summary>
    /// <returns>All neighbour voxels</returns>
    public IEnumerable<Voxel> GetFaceNeighbours()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;
        var s = _voxelGrid.GridSize;

        if (x != 0) yield return _voxelGrid.Voxels[x - 1, y, z];
        if (x != s.x - 1) yield return _voxelGrid.Voxels[x + 1, y, z];

        if (y != 0) yield return _voxelGrid.Voxels[x, y - 1, z];
        if (y != s.y - 1) yield return _voxelGrid.Voxels[x, y + 1, z];

        if (z != 0) yield return _voxelGrid.Voxels[x, y, z - 1];
        if (z != s.z - 1) yield return _voxelGrid.Voxels[x, y, z + 1];
    }

    public IEnumerable<Voxel> GetFaceNeighboursInLayer()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;
        var s = _voxelGrid.GridSize;

        if (x != 0) yield return _voxelGrid.Voxels[x - 1, y, z];
        if (x != s.x - 1) yield return _voxelGrid.Voxels[x + 1, y, z];

        if (z != 0) yield return _voxelGrid.Voxels[x, y, z - 1];
        if (z != s.z - 1) yield return _voxelGrid.Voxels[x, y, z + 1];
    }

    
    public Voxel[] GetFaceNeighboursArray()
    {
        Voxel[] result = new Voxel[6];

        int x = Index.x;
        int y = Index.y;
        int z = Index.z;
        var s = _voxelGrid.GridSize;

        if (x != s.x - 1) result[0] = _voxelGrid.Voxels[x + 1, y, z];
        else result[0] = null;

        if (x != 0) result[1] = _voxelGrid.Voxels[x - 1, y, z];
        else result[1] = null;

        if (y != s.y - 1) result[2] = _voxelGrid.Voxels[x, y + 1, z];
        else result[2] = null;

        if (y != 0) result[3] = _voxelGrid.Voxels[x, y - 1, z];
        else result[3] = null;

        if (z != s.z - 1) result[4] = _voxelGrid.Voxels[x, y, z + 1];
        else result[4] = null;

        if (z != 0) result[5] = _voxelGrid.Voxels[x, y, z - 1];
        else result[5] = null;

        return result;
    }

    public int[] GetRandomWalk(int time)
    {
        int[] voxels = new int[time];

        int x = Index.x;
        int y = Index.y;
        int z = Index.z;

        for (int i = 0; i < time; i++)
        {
            int rnd = Random.Range(0, 6);
            voxels[i] = step(rnd);

        }

        //Step by choice x,y,z
        int step(int rnd)
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
        return voxels;
    }


    public IEnumerable<Voxel> GetRandomWalkers()
    {
        int x = Index.x;
        int y = Index.y;
        int z = Index.z;
        //var s = _voxelGrid.GridSize;
        int rnd = Random.Range(0, 6);

        int choice = rnd;
        if (choice == 0) yield return _voxelGrid.Voxels[x ++, y, z];
        if (choice == 1) yield return _voxelGrid.Voxels[x --, y, z];
        if (choice == 2) yield return _voxelGrid.Voxels[x, y++, z];
        if (choice == 3) yield return _voxelGrid.Voxels[x, y--, z];
        if (choice == 4) yield return _voxelGrid.Voxels[x, y, z++];
        if (choice == 5) yield return _voxelGrid.Voxels[x, y, z--];

    }


    /// <summary>
    /// Activates the visibility of this voxel
    /// </summary>
    public void ActivateVoxel(bool state)
    {
        IsActive = state;
        _voxelGO.GetComponent<MeshRenderer>().enabled = state;
        _voxelGO.GetComponent<BoxCollider>().enabled = state;
    }

    
    public bool IsFacade => GetFaceNeighboursInLayer().Count() < 4;

    //public bool IsWalker => GetRandomWalk(0).Count() < 5;
    #endregion

    #region Equality checks
    /// <summary>
    /// Checks if two Voxels are equal based on their Index
    /// </summary>
    /// <param name="other">The <see cref="Voxel"/> to compare with</param>
    /// <returns>True if the Voxels are equal</returns>
    public bool Equals(Voxel other)
    {
        return (other != null) && (Index == other.Index);
    }

    /// <summary>
    /// Get the HashCode of this <see cref="Voxel"/> based on its Index
    /// </summary>
    /// <returns>The HashCode as an Int</returns>
    public override int GetHashCode()
    {
        return Index.GetHashCode();
    }

    #endregion
}