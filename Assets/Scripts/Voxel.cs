using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EasyGraph;

//public enum VoxelState { Dead = 0, Alive = 1, Available = 2 }
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
    public bool IsPath;
    public bool IsPrivatePath;
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
                _voxelGO.transform.GetChild(0).gameObject.SetActive(false);
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
    private float _state;
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
        _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Basic");
        _voxelGO.GetComponent<VoxelTrigger>().ConnectedVoxel = this;
    }

    /// <summary>
    /// Generic constructor, alllows the use of inheritance
    /// </summary>
    public Voxel() { }

    #endregion

    #region Public methods
    public void SetAsPath()
    {
        if (!IsTarget)
        {
            _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Path");
            _voxelGO.tag = "PathVoxel";
            IsPath = true;
        }
    }
    public void SetAsPrivatePath()
    {
        if (!IsTarget && !IsPath)
        {
            _voxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Private");
            _voxelGO.tag = "PrivatePathVoxel";
            IsPrivatePath = true;
        }
    }

    public void GetNeighbours()
    {
        Voxel[] neighbours = new Voxel[6];
        // Add check for indices out of bounds
        // if index in grid, add voxel on index
        //CheckBounds(Vector3Int index, VoxelGrid grid)
        if (Util.CheckBounds(Index + Vector3Int.up, _voxelGrid))
        {
            neighbours[0] = _voxelGrid.Voxels[Index.x, Index.y + 1, Index.z];
        }
        // if index not in grid, add null;
        else
        {
            neighbours[0] = null;
        }
        if (Util.CheckBounds(Index + Vector3Int.down, _voxelGrid))
        {
            neighbours[1] = _voxelGrid.Voxels[Index.x, Index.y - 1, Index.z];
        }
        else
        {
            neighbours[1] = null;
        }
        if (Util.CheckBounds(Index + Vector3Int.left, _voxelGrid))
        {
            neighbours[2] = _voxelGrid.Voxels[Index.x - 1, Index.y, Index.z];
        }
        else
        {
            neighbours[2] = null;
        }
        if (Util.CheckBounds(Index + Vector3Int.right, _voxelGrid))
        {
            neighbours[3] = _voxelGrid.Voxels[Index.x + 1, Index.y, Index.z];
        }
        else
        {
            neighbours[3] = null;
        }
        //Get the neighours of this voxel
        //return neighbours;
    }

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


    /// <summary>
    /// Activates the visibility of this voxel
    /// </summary>
    public void ActivateVoxel(bool state)
    {
        IsActive = state;
        _voxelGO.GetComponent<MeshRenderer>().enabled = state;
        _voxelGO.GetComponent<BoxCollider>().enabled = state;
    }

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