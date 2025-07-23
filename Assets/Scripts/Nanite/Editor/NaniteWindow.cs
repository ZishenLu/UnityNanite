using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

public class NaniteWindow : EditorWindow
{
    [MenuItem("Tools/Nanite")]
    static void NaniteWin()
    {
        NaniteWindow win = GetWindow<NaniteWindow>();
        win.titleContent = new GUIContent("Nanite");
        win.Show();
    }

    Mesh _mesh;

    private void OnGUI()
    {
        _mesh = EditorGUILayout.ObjectField("NaniteMesh", _mesh, typeof(Mesh), true) as Mesh;

        if (GUILayout.Button("Generate Meshlet"))
        {
            GenerateMeshlet();
        }
    }

    private void GenerateMeshlet()
    {
        ClusterMgr.Inst.BuildMeshlets(_mesh);
        //var datas = ClusterMgr.Inst.ReadDatas();
        //for (int i = 0; i < datas.Length; i++)
        //{
        //    var data = datas[i];
        //    Debug.Log($" {i}: error: {data.error}, parentError: {data.parentError}, sphere: {data.sphere}, parentSphere: {data.parentSphere}");
        //}


        //Debug.Log("Count: " + ClusterMgr.Inst.Count);
    }

    private float GetFloat(Vector3 center, Vector3 point, float radius)
    {
        return GetPower2(center - point) - radius * radius;
    }

    private float GetPower2(Vector3 center)
    {
        return center.x * center.x + center.y * center.y + center.z * center.z;
    }
}
