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
        //ClusterMgr.Inst.ReadDatas();
        //Vector3[] vertices = ClusterMgr.Inst.ReadVerticesDatas();

        //Debug.Log("Load: "+vertices.Length + " mesh: " + _mesh.vertices.Length);

        //Debug.Log("Count: " + ClusterMgr.Inst.Count);
    }
}
