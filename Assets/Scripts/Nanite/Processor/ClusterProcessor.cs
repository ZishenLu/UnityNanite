using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClusterProcessor
{
    private ClusterGroupProcessor _groupProcessor;
    public ClusterData clusterData;
    private uint[] _indices;
    private int _lod;

    public ClusterProcessor(ClusterGroupProcessor groupProcessor, uint[] indices, int lod)
    {
        _groupProcessor = groupProcessor;
        clusterData = new ClusterData();
        _indices = indices;
        _lod = lod;
    }

    public MeshletGroup[] BuildClusterGroup()
    {
        var adjacencyData = AdjacencyBuilder.CreateAdjacencyData(clusterData);
        var meshletGroup = MeshletGroupBuilder.BuildMeshletsGroup((int)clusterData.meshletCount, adjacencyData.XAdjacency.ToArray(), 
            adjacencyData.EdgeAdjacency.ToArray(), adjacencyData.EdgeWeights.ToArray());
        return meshletGroup;
    }

    public ClusterData[] BuildSimplifyMeshes(MeshletGroup[] meshletGroups, ClusterData data, List<MeshletData> meshletDatas, int offset)
    {
        var len = meshletGroups.Length;
        ClusterData[] clusterDatas = new ClusterData[len];
        for (int i = 0; i < len; i++)
        {
            var meshletGroup = meshletGroups[i].meshlets;
            var indices = meshletGroups[i].GetGroupIndices(clusterData);
            var simplifyIndices = MeshletGroupBuilder.SimplifyGroup(indices, _lod, _groupProcessor.vertices, out float error);
            var targetVertices = simplifyIndices.GetVertices(_groupProcessor.vertices);
                
            float childError = 0f;
            float scale = NaniteUtil.SimplifyScale(targetVertices, (uint)targetVertices.Length / 3, sizeof(float) * 3);
            float spaceError = error * scale;
            for (int j = 0; j < meshletGroup.Count; j++)
            {
                var meshletIndex = meshletGroup[j];
                var meshletData = meshletDatas[meshletIndex + offset];
                childError = Mathf.Max(childError, meshletData.error);
            }
            spaceError += childError;
            
            clusterDatas[i] = ClusterDataBuilder.CreateClusterData(_groupProcessor.vertices, simplifyIndices, spaceError);
            var firstMeshlet = clusterDatas[i].meshlets[0];
            var firstMeshletIndices = firstMeshlet.GetMeshletIndices(clusterDatas[i]);
            var sphere = firstMeshletIndices.GetBoundSphere(_groupProcessor.vertices);

            for (int j = 1; j < clusterDatas[i].meshletCount; j++)
            {
                var tmpMeshlet = clusterDatas[i].meshlets[j];
                var tmpIndices = tmpMeshlet.GetMeshletIndices(clusterDatas[i]);
                var tmpSphere = tmpIndices.GetBoundSphere(_groupProcessor.vertices);
                sphere = sphere.MergeSpheres(tmpSphere);
            }

            for (int j = 0; j < meshletGroup.Count; j++)
            {
                var meshletIndex = meshletGroup[j];
                var meshletData = meshletDatas[meshletIndex + offset];
                meshletData.parentError = spaceError;
                meshletData.parentSphere = sphere;
                meshletDatas[meshletIndex + offset] = meshletData; // struct 需要重新赋值
            }
        }
        return clusterDatas;
    }
}
