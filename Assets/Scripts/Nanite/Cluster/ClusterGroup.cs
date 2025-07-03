using System.Collections.Generic;
using UnityEngine;

public struct Sphere
{
    public Vector3 center;
    public float radius;
}

public struct ClusterData
{
    public uint[] indices;
    public uint lod;
    public uint triangleCount;
    public Sphere sphere;
    public Sphere parentSphere;
    public float error;
    public float parentError;
}

public class ClusterGroup
{
    private Mesh _mesh;
    private float[] _vertices;
    private List<ClusterData> _clusterDatas;
    private List<Cluster> _clusters;
    private int _maxLod = 25;

    public ClusterGroup(Mesh mesh)
    {
        _mesh = mesh;
        BufferUtil.Vector3ToFloat(_mesh.vertices, _vertices);
        _clusterDatas = new List<ClusterData>();
        _clusters = new List<Cluster>();
    }

    public void GenerateClusterHierarchy()
    {
        for (int i = 0; i < _maxLod; i++)
        {
            
        }
    }
}
