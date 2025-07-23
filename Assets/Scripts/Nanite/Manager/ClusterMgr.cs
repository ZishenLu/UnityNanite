using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ClusterMgr
{
    private static ClusterMgr _inst;

    public static ClusterMgr Inst
    {
        get
        {
            if(_inst == null)
            {
                _inst = new ClusterMgr();
            }
            return _inst;
        }
    }

    public uint maxVertices = 64;

    public uint maxIndices = 128;

    [SerializeField]
    private string guid = "546fb816-ee79-4cad-b860-6c4dd53154f0";// "29908f5c-041d-4dd1-b967-5d42174b7b49";

    public void BuildMeshlets(Mesh mesh)
    {
        ClusterGroupProcessor clusterGroup = new ClusterGroupProcessor(mesh);
        clusterGroup.GenerateClusterHierarchy();
        clusterGroup.SaveToFlie();
        guid = clusterGroup.guid.ToString();
        AssetDatabase.Refresh();
    }

    public uint Count = 128;

    public MeshletData[] ReadDatas()
    {
        var str = $"Assets/Resources/Cluster/{guid}/cluster.bin";
        var arr = MeshletRWUtil.LoadMeshletDatas(str);
        //var target = new MeshletData[323];
        //Array.Copy(arr, 600, target, 0, 323);
        //return target;
        return arr;
    }

    public uint[] ReadIndices()
    {
        var str = $"Assets/Resources/Cluster/{guid}/indices.bin";
        return FileUtil.ReadFromFile<uint>(str);
    }

    public Vector3[] ReadVerticesDatas()
    {
        var str = $"Assets/Resources/Cluster/{guid}/vertices.bin";
        using (FileStream fs = new FileStream(str, FileMode.Open))
        {
            using (BinaryReader br = new BinaryReader(fs))
            {
                uint count = (uint)fs.Length / 3 / sizeof(float);
                Vector3[] datas = FileUtil.ReadArray<Vector3>(br, count);
                return datas;
            }
        }
    }
}
