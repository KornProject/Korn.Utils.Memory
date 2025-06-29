using Korn.Modules.WinApi;
using System;
using System.Runtime.InteropServices;
using System.Text;

#if NET8_0
using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
#endif

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace Korn.Utils
{
    public unsafe static class Memory
    {
        static Memory()
        {
#if NET8_0
            MemoryProvider = Avx2.IsSupported ? new AVX2MemoryProvied() : new DefaultMemoryProvider();
#else
            MemoryProvider = new DefaultMemoryProvider();
#endif
        }

        abstract class AbstractMemoryProvider
        {
            public void Copy(void* source, void* destination, long length) => Copy((byte*)source, (byte*)destination, length);
            public abstract void Copy(byte* source, byte* destination, long length);
            public void Zero(void* destination, long length) => Zero((byte*)destination, length);
            public abstract void Zero(byte* destination, long length);
        }

        class DefaultMemoryProvider : AbstractMemoryProvider
        {
            public override unsafe void Copy(byte* source, byte* destination, long length) => Buffer.MemoryCopy(source, destination, length, length);
            public override void Zero(byte* destination, long length) => Kernel32.RtlZeroMemory((IntPtr)destination, length);
        }

        static AbstractMemoryProvider MemoryProvider;
#if NET8_0
        class AVX2MemoryProvied : AbstractMemoryProvider
        {
            const int BlockSize = 32;

            public override unsafe void Copy(byte* source, byte* destination, long length)
            {
                long index = 0;
                var lastBlockIndex = length - BlockSize;
                for (; index <= lastBlockIndex; index += BlockSize)
                {
                    var vector = Avx.LoadVector256(source + index);
                    Avx.Store(destination + index, vector);
                }

                for (; index < length; index++)
                    destination[index] = source[index];
            }

            public override void Zero(byte* destination, long length)
            {
                var vector = new Vector256<byte>();

                long index = 0;
                var lastBlockIndex = length - BlockSize;
                for (; index <= lastBlockIndex; index += BlockSize)
                    Avx.Store(destination + index, vector);

                for (; index < length; index++)
                    destination[index] = 0;
            }
        }
#endif

        public static void Zero(void* pointer, int size) => MemoryProvider.Zero(pointer, size);

        public static void Copy(void* source, void* destination, long byteLength) => MemoryProvider.Copy(
            source, 
            destination, 
            byteLength
        );

        public static void Copy<T>(T[] sourceArray, void* destination)
        {
            fixed (T* source = sourceArray)
                MemoryProvider.Copy(
                    source,
                    destination,
                    ((long*)source)[-1] * sizeof(T)
                );
        }

        public static void Copy<T>(T[] sourceArray, T[] destinationArray, long length)
        {
            fixed (T* source = sourceArray, destination = destinationArray)
                MemoryProvider.Copy(
                    source,
                    destination,
                    length * sizeof(T)
                );
        }

        public static void Copy<T>(T[] sourceArray, T[] destinationArray)
        {
            fixed (T* source = sourceArray, destination = destinationArray)
                MemoryProvider.Copy(
                    source,
                    destination,
                    ((long*)source)[-1] * sizeof(T)
                );
        }

        public static void Copy<T>(void* source, T[] destinationArray, long byteLength)
        {
            fixed (T* destination = destinationArray)
                MemoryProvider.Copy(
                    source,
                    destination,
                    byteLength
                );
        }

        static byte* Allocate(int count) => (byte*)Marshal.AllocCoTaskMem(count);

        static byte* ZeroAllocate(int count)
        {
            var pointer = Allocate(count);
            Zero(pointer, count);
            return pointer;
        }

        public static void* ZeroAlloc(int count) => ZeroAllocate(count);

        public static T* ZeroAlloc<T>() => (T*)ZeroAllocate(sizeof(T));

        public static T* ZeroAlloc<T>(int count) => (T*)ZeroAllocate(sizeof(T) * count);

        public static void* Alloc(int count) => Allocate(count);

        public static T* Alloc<T>() => (T*)Allocate(sizeof(T));

        public static T* Alloc<T>(int count) => (T*)Allocate(sizeof(T) * count);

        public static void Free(void* pointer) => Free((IntPtr)pointer);

        public static void Free(IntPtr address)
        {
            if (address != default)
                Marshal.FreeCoTaskMem(address);
        }
        
        public static byte[] Read(void* ptr, int length)
        {
            var byteArray = new byte[length];
            fixed (byte* bytes = byteArray)
                Copy(ptr, bytes, length);   
            return byteArray;
        }

        public static T[] Read<T>(void* ptr, int length)
        {
            var byteArray = new T[length];
            fixed (T* bytes = byteArray)
                Copy(ptr, bytes, length);
            return byteArray;
        }

        public static string ReadUTF8(void* adress)
        {
            const int MaxLength = short.MaxValue;

            var pointer = (byte*)adress;
            var length = 0;
            while (length < MaxLength && pointer[length] != 0x00)
                length++;

            var result = Encoding.UTF8.GetString(pointer, length);
            return result;
        }
    }
}
