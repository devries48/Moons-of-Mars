using UnityEngine;

// This class contains some helper functions to make life a little easier working with compute shaders
// (Very work-in-progress!)

public static class ComputeHelper
{
    public static int GetStride<T>()
    {
        return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
    }

    public static ComputeBuffer CreateStructuredBuffer<T>(T[] data)
    {
        var buffer = new ComputeBuffer(data.Length, GetStride<T>());
        buffer.SetData(data);
        return buffer;
    }

    /// Releases supplied buffer/s if not null
    public static void Release(params ComputeBuffer[] buffers)
    {
        for (int i = 0; i < buffers.Length; i++)
        {
            if (buffers[i] != null)
            {
                buffers[i].Release();
            }
        }
    }

    // ------ Instancing Helpers

    // Create args buffer for instanced indirect rendering
    public static ComputeBuffer CreateArgsBuffer(Mesh mesh, int numInstances)
    {
        const int subMeshIndex = 0;
        uint[] args = new uint[5];
        args[0] = (uint)mesh.GetIndexCount(subMeshIndex);
        args[1] = (uint)numInstances;
        args[2] = (uint)mesh.GetIndexStart(subMeshIndex);
        args[3] = (uint)mesh.GetBaseVertex(subMeshIndex);
        args[4] = 0; // offset

        ComputeBuffer argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);
        return argsBuffer;
    }

}
