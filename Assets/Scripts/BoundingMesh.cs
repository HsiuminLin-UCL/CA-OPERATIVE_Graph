using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoundingMesh
{
    #region Private fields
    private static IEnumerable<Collider> _colliders;
    #endregion

    #region Public fields
    public static BoundingMesh Instance { get; } = new BoundingMesh();
    public static Bounds MeshBounds;

    #endregion

    #region constructor
    public BoundingMesh()
    {
        GameObject[] boundingMeshes = GameObject.FindGameObjectsWithTag("BoundingMesh");
        _colliders = boundingMeshes.Select(g => g.GetComponent<Collider>());

        Bounds meshBounds = new Bounds();

        foreach (var collider in _colliders)
        {
            meshBounds.Encapsulate(collider.bounds);
        }
        MeshBounds = meshBounds;
    }
    #endregion

    #region public functions
    /// <summary>
    /// Get the origin of the grid
    /// </summary>
    /// <param name="voxelOffset">amount of voxels to be added around the bounding meshes</param>
    /// <param name="voxelSize">The size of the voxels</param>
    /// <returns></returns>
    public static Vector3 GetOrigin(int voxelOffset, float voxelSize) =>
        MeshBounds.min - Vector3.one * voxelSize;

    public static Vector3Int GetGridDimensions(int voxelOffset, float voxelSize) =>
        Vector3Int.RoundToInt(MeshBounds.size / voxelSize) + Vector3Int.one*voxelOffset*2;


    /// <summary>
    /// Check if a voxel is inside the bounding meshes, using the Voxel centre
    /// </summary>
    /// <param name="voxel">voxel to check</param>
    /// <returns></returns>
    public static bool IsInsideCentre(Voxel voxel)
    {
        Physics.queriesHitBackfaces = true;

        var bounding = voxel.Center;
        var sortedHits = new Dictionary<Collider, int>();
        foreach (var collider in _colliders)
            sortedHits.Add(collider, 0);

        while (Physics.Raycast(new Ray(bounding, Vector3.forward), out RaycastHit hit))
        {
            var collider = hit.collider;

            if (sortedHits.ContainsKey(collider))
                sortedHits[collider]++;

            bounding = hit.point + Vector3.forward * 0.00001f;
        }

        bool isInside = sortedHits.Any(kv => kv.Value % 2 != 0);
        return isInside;
    }

    //public static bool IsInsideCentreLine(VoxelGrid line)
    //{
    //    Physics.queriesHitBackfaces = true;

    //    var bounding = line.Center;
    //    var sortedHits = new Dictionary<Collider, int>();
    //    foreach (var collider in _colliders)
    //        sortedHits.Add(collider, 0);

    //    while (Physics.Raycast(new Ray(bounding, Vector3.forward), out RaycastHit hit))
    //    {
    //        var collider = hit.collider;

    //        if (sortedHits.ContainsKey(collider))
    //            sortedHits[collider]++;

    //        bounding = hit.point + Vector3.forward * 0.00001f;
    //    }

    //    bool isInside = sortedHits.Any(kv => kv.Value % 2 != 0);
    //    return isInside;
    //}


    #endregion
}
