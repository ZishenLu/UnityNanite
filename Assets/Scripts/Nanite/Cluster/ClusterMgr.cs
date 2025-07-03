using System.IO;
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

    public string path
    {
        get
        {
            return Path.Combine(Application.dataPath, "Resources/Cluster/datas.bin");
        }
    }

    public string verticesPath
    {
        get
        {
            return Path.Combine(Application.dataPath, "Resources/Cluster/datas.vertices.bin");
        }
    }

    public void BuildMeshlets(Mesh mesh)
    {
        //Cluster cluster = new Cluster(mesh);

        //cluster.Build();
        //cluster.SaveToFile(path);
        //cluster.SaveVerticesToFile(verticesPath);
    }

    public uint Count;

    public uint[] ReadDatas()
    {
        using(FileStream fs = new FileStream(path, FileMode.Open))
        {
            using(BinaryReader br = new BinaryReader(fs))
            {
                Count = br.ReadUInt32();
                uint count = br.ReadUInt32();
                uint[] datas = FileUtil.ReadArray<uint>(br, count);
                return datas;
            }
        }
    }

    public Vector3[] ReadVerticesDatas()
    {
        using (FileStream fs = new FileStream(verticesPath, FileMode.Open))
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
