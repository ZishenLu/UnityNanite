using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class MeshletRWUtil
{
    public static void SaveMeshletDatas(MeshletData[] datas, string filePath)
    {
        using (var fs = new FileStream(filePath, FileMode.Create))
        {
            using (var bw = new BinaryWriter(fs))
            {
                FileUtil.WriteArray(bw, datas);
            }
        }
    }

    public static MeshletData[] LoadMeshletDatas(string filePath)
    {
        using (var fs = new FileStream(filePath, FileMode.Open))
        {
            using (var br = new BinaryReader(fs))
            {
                int size = Marshal.SizeOf(typeof(MeshletData));
                uint count = (uint)(fs.Length / size);
                MeshletData[] datas = FileUtil.ReadArray<MeshletData>(br, count);
                return datas;
            }
        }
    }
}
