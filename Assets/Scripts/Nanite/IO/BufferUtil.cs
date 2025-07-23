using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class BufferUtil
{
    public static void Vector3ToFloat(Vector3[] vectors, float[] result)
    {
        GCHandle handle = GCHandle.Alloc(vectors, GCHandleType.Pinned);
        try
        {
            IntPtr pointer = handle.AddrOfPinnedObject();
            Marshal.Copy(pointer, result, 0, result.Length);
        }
        finally
        {
            handle.Free();
        }
    }

    public static void IntToUInt(int[] vectors, uint[] result)
    {
        GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
        try
        {
            IntPtr pointer = handle.AddrOfPinnedObject();
            Marshal.Copy(vectors, 0, pointer, result.Length);
        }
        finally
        {
            handle.Free();
        }
    }
}
