using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct MeshletEdge
{
    public uint first;
    public uint second;
    public MeshletEdge(uint aIndex, uint bIndex)
    {
        if(aIndex < bIndex)
        {
            first = aIndex; second = bIndex;
        }
        else
        {
            first = bIndex; second = aIndex;
        }
    }

    public override bool Equals(object obj)
    {
        if(obj is MeshletEdge edge)
        {
            return edge.first == first && edge.second == second;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(first, second);
    }
}

public struct MeshletGroup
{
    public List<int> meshlets;
}

public class Cluster
{
    private const int METIS_NOPTIONS = 40;
    private const int METIS_OBJTYPE_CUT = 0;
    private const int METIS_OPTION_OBJTYPE = 1;
    private const int METIS_OPTION_CCORDER = 13;
    private Dictionary<MeshletEdge, List<uint>> _edge2Meshlets;
    private Dictionary<uint, List<MeshletEdge>> _meshlet2Edges;

    public Cluster(float[] vertices, uint[] indices)
    {
        _originIndices = indices;
        _originVertices = vertices;
        _edge2Meshlets = new Dictionary<MeshletEdge, List<uint>>();
        _meshlet2Edges = new Dictionary<uint, List<MeshletEdge>>();
        _xadjacency = new List<int>();
        _edgeAdjacency = new List<int>();
        _edgeWeights = new List<int>();
    }

    private uint maxVertices
    {
        get
        {
            return ClusterMgr.Inst.maxVertices;
        }
    }

    private uint maxIndices
    {
        get
        {
            return ClusterMgr.Inst.maxIndices;
        }
    }

    private float[] _originVertices;
    private uint[] _originIndices;
    private Meshlet[] _meshlets;
    private uint _maxTriCount;
    private uint _meshletsCount;
    private uint[] _meshletVertices;
    private byte[] _meshletIndices;
    private List<int> _xadjacency;
    private List<int> _edgeAdjacency;
    private List<int> _edgeWeights;
    private MeshletGroup[] _group;
    

    public void Build()
    {
        var maxLets = NaniteUtil.BuildMeshletsBound((uint)_originIndices.Length, maxVertices, maxIndices);

        _meshlets = new Meshlet[maxLets];
        _meshletVertices = new uint[maxLets * maxVertices];
        _meshletIndices = new byte[maxLets * maxIndices * 3];

        //uint[] triangles = new uint[_mesh.triangles.Length];
        //BufferUtil.IntToUInt(_mesh.triangles, triangles);

        //float[] vertices = new float[_mesh.vertices.Length * 3];
        //BufferUtil.Vector3ToFloat(_mesh.vertices, vertices);

        _meshletsCount = NaniteUtil.BuildMeshlets(_meshlets, _meshletVertices, _meshletIndices, _originIndices, 
            (uint)_originIndices.Length, _originVertices, (uint)_originVertices.Length / 3, sizeof(float) * 3, maxVertices, maxIndices, 0.0f);

        _maxTriCount = 0;
        foreach(var item in _meshlets)
        {
            if(_maxTriCount < item.triangle_count)
            {
                _maxTriCount = item.triangle_count;
            }
        }

        BuildMeshletsEdge();
        BuildMeshXAdj();
        BuildMeshletsGroup();
    }

    public void BuildMeshletsEdge()
    {
        Meshlet[] meshlets = _meshlets;
        for (uint meshletIndex = 0; meshletIndex < _meshletsCount; meshletIndex++)
        {
            for (uint triangleIndex = 0; triangleIndex < meshlets[meshletIndex].triangle_count; triangleIndex++)
            {
                for (uint index = 0; index < 3; index++)
                {
                    uint aIndex = GetIndex(meshletIndex, triangleIndex * 3 + index);
                    uint bIndex = GetIndex(meshletIndex, triangleIndex * 3 + (index + 1) % 3);
                    MeshletEdge meshletEdge = new MeshletEdge(aIndex, bIndex);

                    AddMeshlet2Edges(meshletIndex, triangleIndex, index, meshletEdge);
                    AddEdge2Meshlets(meshletIndex, triangleIndex, index, meshletEdge);
                }
            }
        }
        RemoveEdge2Meshlets();
        RemoveMeshlets2Edge();
    }

    private void RemoveEdge2Meshlets()
    {
        List<MeshletEdge> keys = new List<MeshletEdge>(_edge2Meshlets.Keys);
        for (int i = keys.Count - 1; i >= 0; i--)
        {
            var key = keys[i];
            if (_edge2Meshlets[key].Count < 2)
            {
                _edge2Meshlets.Remove(key);
            }
        }
    }

    private void RemoveMeshlets2Edge()
    {
        List<uint> keys = new List<uint>(_meshlet2Edges.Keys);
        for (int i = keys.Count - 1; i >= 0; i--)
        {
            var key = keys[i];
            var list = _meshlet2Edges[key];
            for (int j = list.Count - 1; j >= 0; j--)
            {
                if (list.Contains(list[j]))
                {
                    list.Remove(list[j]);
                }
            }
        }
    }

    public void BuildMeshXAdj()
    {
        Meshlet[] meshlets = _meshlets;
        for (uint meshletIndex = 0; meshletIndex < _meshletsCount; meshletIndex++)
        {
            int startIndex = _edgeAdjacency.Count;
            _xadjacency.Add(startIndex);
            foreach (var edge in _meshlet2Edges[meshletIndex])
            {
                if (!_edge2Meshlets.ContainsKey(edge)) continue;
                foreach (var meshlet in _edge2Meshlets[edge])
                {
                    if(meshlet != meshletIndex)
                    {
                        var curRange = _xadjacency.GetRange(startIndex, _xadjacency.Count - startIndex);
                        var connectionMeshletIndex = curRange.FindIndex(x => x == meshlet);
                        if(connectionMeshletIndex != -1)
                        {
                            _edgeWeights[connectionMeshletIndex + startIndex]++;
                        }
                        else
                        {
                            _edgeAdjacency.Add((int)meshlet);
                            _edgeWeights.Add(1);
                        }
                    }
                }
            }
        }
        _xadjacency.Add(_edgeAdjacency.Count);
    }

    public void BuildMeshletsGroup()
    {
        int count = (int)_meshletsCount;
        int[] options = new int[METIS_NOPTIONS];
        int ncon = 1;
        int nparts = count / 16;

        _group = new MeshletGroup[nparts];
        for (int i = 0; i < nparts; i++)
        {
            _group[i].meshlets = new List<int>();
        }
        NaniteUtil.SetDefaultOptions(options);
        options[METIS_OPTION_OBJTYPE] = METIS_OBJTYPE_CUT;
        options[METIS_OPTION_CCORDER] = 1;
        int[] partition = new int[count];

        int edgeCut = 1;
        int result = NaniteUtil.PartGraphKway(count, ncon, _xadjacency.ToArray(),
            _edgeAdjacency.ToArray(), _edgeWeights.ToArray(), nparts, options,
            edgeCut, partition);

        for (int i = 0; i < count; i++)
        {
            int partIndex = partition[i];
            _group[partIndex].meshlets.Add(i);
        }
    }

    public void BuildSimplifyMesh()
    {
        foreach (var group in _group)
        {
            for (int i = 0; i < group.meshlets.Count; i++)
            {
                
            }
        }
    }

    private void AddMeshlet2Edges(uint meshletIndex, uint triangleIndex, uint index, MeshletEdge meshletEdge)
    {
        if (_edge2Meshlets.ContainsKey(meshletEdge))
        {
            var list = _edge2Meshlets[meshletEdge];
            if (!list.Contains(meshletIndex))
            {
                list.Add(meshletIndex);
            }
        }
        else
        {
            _edge2Meshlets.Add(meshletEdge, new List<uint> { meshletIndex });
        }
    }

    private void AddEdge2Meshlets(uint meshletIndex, uint triangleIndex, uint index, MeshletEdge meshletEdge)
    {
        if (_meshlet2Edges.ContainsKey(meshletIndex))
        {
            var list = _meshlet2Edges[meshletIndex];
            if (!list.Contains(meshletEdge))
            {
                list.Add(meshletEdge);
            }
        }
        else
        {
            _meshlet2Edges.Add(meshletIndex, new List<MeshletEdge> { meshletEdge });
        }
    }

    public uint GetIndex(uint instanceIndex, uint index)
    {
        uint indexCount = _meshlets[instanceIndex].triangle_count * 3;
        if(index >= indexCount)
        {
            index = indexCount - 1;
        }

        uint triOffset = _meshlets[instanceIndex].triangle_offset;
        uint verOffset = _meshlets[instanceIndex].vertex_offset;

        uint localIndex = _meshletIndices[triOffset + index];
        uint triIndex = _meshletVertices[localIndex + verOffset];
        return triIndex;
    }

    public void SaveVerticesToFile(string file)
    {
        using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
        {
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                FileUtil.WriteArray(bw, _originVertices);
            }
        }
    }

    public void SaveToFile(string file)
    {
        uint[] datas = new uint[_maxTriCount * _meshletsCount * 3];
        for (uint i = 0; i < _meshletsCount; i++)
        {
            for (uint j = 0; j < _maxTriCount; j++)
            {
                for (uint k = 0; k < 3; k++)
                {
                    datas[i * _maxTriCount * 3 + j * 3 + k] = GetIndex(i, j * 3 + k);
                }
            }
        }

        using (FileStream fs = new FileStream(file, FileMode.OpenOrCreate))
        {
            using (BinaryWriter bw = new BinaryWriter(fs)) 
            { 
                bw.Write(_maxTriCount);
                bw.Write(_maxTriCount * _meshletsCount * 3);
                FileUtil.WriteArray(bw, datas);
            }
        }
    }
}
