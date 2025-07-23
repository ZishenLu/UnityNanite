using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

public class ClusterGroupProcessor
{
    public static int maxLod = 25;
    private Guid _guid;
    private Mesh _mesh;
    private float[] _vertices;
    private uint[] _indices;
    private List<ClusterData> _clusterDatas;
    private List<uint> _targetIndices;
    private List<MeshletData> _meshletDatas;
    private List<MeshletGroupData> _groupDatas;

    public uint[] indices => _indices;
    public float[] vertices => _vertices;
    public Guid guid => _guid;

    public ClusterGroupProcessor(Mesh mesh)
    {
        _mesh = mesh;
        _vertices = new float[_mesh.vertices.Length * 3];
        _indices = new uint[_mesh.triangles.Length];
        _guid = Guid.NewGuid();
        BufferUtil.Vector3ToFloat(_mesh.vertices, _vertices);
        BufferUtil.IntToUInt(_mesh.triangles, _indices);
        _clusterDatas = new List<ClusterData>();
        _targetIndices = new List<uint>();
        _meshletDatas = new List<MeshletData>();
        _groupDatas = new List<MeshletGroupData>();
    }

    public void GenerateClusterHierarchy()
    {
        var originIndices = _indices;
        var clusterData = ClusterDataBuilder.CreateClusterData(_vertices, originIndices, 0);

        for (int i = 0; i < maxLod; i++)
        {
            var cluster = new ClusterProcessor(this, originIndices, i);
            cluster.clusterData = clusterData;
            var group = cluster.BuildClusterGroup();
            var meshletDatas = clusterData.CreateMeshletDatas((uint)i, _vertices, (uint)_targetIndices.Count);
            var indices = clusterData.GetIndices();
            var meshletOffset = _meshletDatas.Count;
            _meshletDatas.AddRange(meshletDatas);
            _targetIndices.AddRange(indices);
            if (group.Length <= 2) break;
            var clusterDatas = cluster.BuildSimplifyMeshes(group, clusterData, _meshletDatas, meshletOffset);
            clusterData = clusterDatas.MergeClusterDatas();
            //clusterData = clusterDatas[1];
        }
    }

    public void SaveToFlie()
    {
        if(!Directory.Exists($"Assets/Resources/Cluster/{guid}"))
        {
            Directory.CreateDirectory($"Assets/Resources/Cluster/{guid}");
        }
        //for(int i = 0; i < _meshletDatas.Count; i++)
        //{
        //    var meshletData = _meshletDatas[i];
        //    Debug.Log($"Meshlet {i} - error: {meshletData.error}, parentError: {meshletData.parentError}, sphere: {meshletData.sphere}, parentSphere: {meshletData.parentSphere}");
        //}
        FileUtil.WriteToFile($"Assets/Resources/Cluster/{guid}/vertices.bin", _vertices);
        FileUtil.WriteToFile($"Assets/Resources/Cluster/{guid}/indices.bin", _targetIndices.ToArray());
        FileUtil.WriteToFile($"Assets/Resources/Cluster/{guid}/cluster.bin", _meshletDatas.ToArray());
    }
}