using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshletGroupBuilder
{
    public static MeshletGroup[] BuildMeshletsGroup(int meshletsCount, int[] xadjacency, int[] edgeAdjacency, int[] edgeWeights)
    {
        int count = meshletsCount;
        int[] options = new int[NaniteConst.METIS_NOPTIONS];
        int ncon = 1;
        int nparts = count / 32;

        var groups = new MeshletGroup[nparts];
        for (int i = 0; i < nparts; i++)
        {
            groups[i].meshlets = new List<int>();
        }
        NaniteUtil.SetDefaultOptions(options);
        options[NaniteConst.METIS_OPTION_OBJTYPE] = NaniteConst.METIS_OBJTYPE_CUT;
        options[NaniteConst.METIS_OPTION_CCORDER] = 1;
        int[] partition = new int[count];

        int edgeCut;
        int result = NaniteUtil.PartGraphKway(count, ncon, xadjacency,
            edgeAdjacency, edgeWeights, nparts, options,
            out edgeCut, partition);

        Debug.Assert(result == 1, "Graph partitioning failed!");

        for (int i = 0; i < count; i++)
        {
            int partIndex = partition[i];
            groups[partIndex].meshlets.Add(i);
        }

        return groups;
    }

    public static uint[] SimplifyGroup(uint[] indices, int lod, float[] vertices, out float error)
    {
        float tLod = (float)(lod + 1f) / 25;
        uint targetIndexCount = (uint)(indices.Length * 0.5f);
        float targetError = 0.9f * tLod + 0.01f * (1 - tLod);

        uint[] output = new uint[indices.Length];
        uint outputCount = NaniteUtil.MeshSimplify(
            output,
            indices,
            (uint)indices.Length,
            vertices,
            (uint)vertices.Length / 3,
            sizeof(float) * 3,
            targetIndexCount,
            targetError,
            NaniteConst.SIMPLIFY_LOCK_BORDER,
            out error
        );

        uint[] result = new uint[outputCount];
        Array.Copy(output, result, outputCount);
        return result;
    }
}