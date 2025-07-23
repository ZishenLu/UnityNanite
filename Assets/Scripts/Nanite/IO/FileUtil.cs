using System;
using System.IO;
using System.Runtime.InteropServices;

public static class FileUtil
{
    public static void WriteToFile<T>(string path, T[] data) where T : struct
    {
        int structSize = Marshal.SizeOf<T>();
        byte[] bytes = new byte[data.Length * structSize];

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            Marshal.Copy(handle.AddrOfPinnedObject(), bytes, 0, bytes.Length);
            File.WriteAllBytes(path, bytes);
        }
        finally
        {
            handle.Free();
        }
    }

    public static T[] ReadFromFile<T>(string path) where T : struct
    {
        byte[] bytes = File.ReadAllBytes(path);
        int structSize = Marshal.SizeOf<T>();
        int itemCount = bytes.Length / structSize;

        T[] result = new T[itemCount];
        GCHandle handle = GCHandle.Alloc(result, GCHandleType.Pinned);
        try
        {
            Marshal.Copy(bytes, 0, handle.AddrOfPinnedObject(), bytes.Length);
            return result;
        }
        finally
        {
            handle.Free();
        }
    }

    public static T[] ReadArray<T>(BinaryReader reader, uint count) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        byte[] bytes = reader.ReadBytes((int)(count * (uint)size));
        T[] array = new T[count];
        GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        try
        {
            Marshal.Copy(bytes, 0, handle.AddrOfPinnedObject(), bytes.Length);
        }
        finally
        {
            handle.Free();
        }
        return array;
    }

    public static void WriteArray<T>(BinaryWriter writer, T[] array) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        int count = array.Length;
        GCHandle handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        byte[] bytes = new byte[count * size];
        try
        {
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, bytes, 0, count * size);
            writer.Write(bytes);
        }
        finally
        {
            handle.Free();
        }
    }
}
