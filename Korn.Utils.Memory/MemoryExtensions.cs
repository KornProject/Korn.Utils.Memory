using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Korn.Utils.Memory
{
    public unsafe static class MemoryExtensions
    {
        public static void Copy(IntPtr to, IntPtr from, int byteLength) => Buffer.MemoryCopy((byte*)from, (byte*)to, byteLength, byteLength);

        public static void Copy<T>(IntPtr to, T[] from) where T : unmanaged
        {
            int length = sizeof(T) * from.Length;
            fixed (void* fromPtr = from)
                Buffer.MemoryCopy(fromPtr, (byte*)to, length, length);
        }

        public static void Copy<T, T2>(T[] to, T2[] from, int byteLength) where T : unmanaged where T2 : unmanaged
        {
            fixed (void* fromPtr = from)
            fixed (void* toPtr = to)
                Buffer.MemoryCopy(fromPtr, toPtr, byteLength, byteLength);
        }

        public static void Copy<T>(T[] to, IntPtr from) where T : unmanaged
        {
            var byteLength = sizeof(T) * to.Length;
            fixed (void* toPtr = to)
                Buffer.MemoryCopy((byte*)from, toPtr, byteLength, byteLength);
        }

        public static void Zero(IntPtr poitner, int length)
        {
            var longLength = length / sizeof(long);
            var longPointer = (long*)poitner;
            for (var i = 0; i < longLength; i++)
                *longPointer++ = 0;

            var byteLength = length % sizeof(byte);
            var bytePointer = (byte*)longPointer;
            for (var i = 0; i < byteLength; i++)
                *bytePointer++ = 0;
        }

        public static byte[] Read(IntPtr ptr, int len)
        {
            var bytes = new byte[len];
            Copy(bytes, ptr);   
            return bytes;
        }

        public static T[] Read<T>(IntPtr ptr, int len) where T : unmanaged
        {
            var bytes = new T[len];
            Copy(bytes, ptr);
            return bytes;
        }
    }
}