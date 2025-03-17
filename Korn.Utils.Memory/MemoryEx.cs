using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace Korn.Utils.Memory
{
    public unsafe static class MemoryEx
    {
        [DllImport("kernel32")] static extern void RtlZeroMemory(IntPtr address, IntPtr size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Zero(IntPtr address, int size) => RtlZeroMemory(address, (IntPtr)size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Zero(void* pointer, int size) => RtlZeroMemory((IntPtr)pointer, (IntPtr)size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static IntPtr Allocate(int count) => Marshal.AllocCoTaskMem(count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static IntPtr AllocateZero(int count)
        {
            var pointer = Marshal.AllocCoTaskMem(count);
            Zero(pointer, count);
            return pointer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc(int count) => (void*)AllocateZero(count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Alloc<T>() => (T*)AllocateZero(sizeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Alloc<T>(int count) => (T*)AllocateZero(sizeof(T) * count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* FastAlloc(int count) => (void*)Allocate(count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* FastAlloc<T>() => (T*)Allocate(sizeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* FastAlloc<T>(int count) => (T*)Allocate(sizeof(T) * count);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* pointer) => Free((IntPtr)pointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(IntPtr pointer)
        {
            if (pointer != IntPtr.Zero)
                Marshal.FreeCoTaskMem(pointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(IntPtr to, IntPtr from, int byteLength) => Buffer.MemoryCopy((byte*)from, (byte*)to, byteLength, byteLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(IntPtr to, T[] from)
        {
            int length = sizeof(T) * from.Length;
            fixed (void* fromPtr = from)
                Buffer.MemoryCopy(fromPtr, (byte*)to, length, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T, T2>(T[] to, T2[] from, int byteLength)
        {
            fixed (void* fromPtr = from)
            fixed (void* toPtr = to)
                Buffer.MemoryCopy(fromPtr, toPtr, byteLength, byteLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy<T>(T[] to, IntPtr from)
        {
            var byteLength = sizeof(T) * to.Length;
            fixed (void* toPtr = to)
                Buffer.MemoryCopy((byte*)from, toPtr, byteLength, byteLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Read(IntPtr ptr, int len)
        {
            var bytes = new byte[len];
            Copy(bytes, ptr);   
            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Read<T>(IntPtr ptr, int len)
        {
            var bytes = new T[len];
            Copy(bytes, ptr);
            return bytes;
        }
    }
}
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type