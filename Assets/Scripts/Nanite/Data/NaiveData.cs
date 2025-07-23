using UnityEngine;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Meshlet
{
    public uint vertex_offset;
    public uint triangle_offset;
    public uint vertex_count;
    public uint triangle_count;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MeshoptBounds
{
    public Vector3 center;
    public float radius; 
    public Vector3 cone_apex;
    public Vector3 cone_axis;
    public float cone_cutoff;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public sbyte[] cone_axis_s8; // 3字节 (累计47)

    public sbyte cone_cutoff_s8; // 1字节 (累计48)
};