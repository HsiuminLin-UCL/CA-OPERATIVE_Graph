using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using EasyGraph;

// Sources: https://github.com/daversd/RC4_M1_C3.git

//public enum VoxelState { Dead = 0, Alive = 1, Available = 2}
public class Voxel : IEquatable<Voxel>
{
    #region Public Fields
    public Vector3Int Index;
    public Vector3 Center => (Index + _voxelGrid.Origin) * _size;
    public bool IsActive;
    public bool IsOrigin;
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
                VoxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Basic");
                VoxelGO.tag = "Node";
                VoxelGO.transform.GetChild(0).gameObject.SetActive(false);
                
            }
            else
            {
                VoxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Target");
                VoxelGO.tag = "TargetNode";
                return;
            }
        }
    }

    public GameObject VoxelGO;
    public bool IsPath;

    public bool IsRandomPath;

    #endregion

    private float _state;

    #region Protected Fiedls
    //Create properties of the voxel
    
    protected VoxelGrid _voxelGrid;
    protected float _size;
    private bool _isTarget;
    #endregion

    List<Edge<GameObject>> _edges;
    UndirecteGraph<GameObject, Edge<GameObject>> _undirectedGraph;
    List<GameObject> _edgeLines;
    Dijkstra<GameObject, Edge<GameObject>> _dijkstra;

    #region Contructors
    public Voxel(Vector3Int index, VoxelGrid voxelgrid, GameObject voxelGameObject)
    {
        Index = index;
        _voxelGrid = voxelgrid;
        _size = _voxelGrid.VoxelSize;
        VoxelGO = GameObject.Instantiate(voxelGameObject, (_voxelGrid.Origin + Index) * _size, Quaternion.identity);
        VoxelGO.transform.localScale *= _voxelGrid.VoxelSize;
        VoxelGO.name = $"Node_{Index.x}_{Index.y}_{Index.z}";
        VoxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Basic");
        VoxelGO.GetComponent<VoxelTrigger>().ConnectedVoxel = this;
    }

    #endregion

    #region Public Methods


    public void SetAsPath()
    {
        if (!IsTarget)
        {
            VoxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Path");
            VoxelGO.tag = "PathVoxel";
            IsPath = true;

        }
    }

    public void SetAsRandomPath()
    {
        if (!IsTarget && !IsPath)
        {
            VoxelGO.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/RandomPath");
            VoxelGO.tag = "RandomPathVoxel";
            IsRandomPath = true;

        }
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