using System;

namespace Korn.Utils
{
    public static unsafe class ExternalMemoryAllocator
    {
        public static IntPtr Allocate(IntPtr processHandle, Address address, long size) => Kernel32.VirtualAllocEx(processHandle, address, size, MemoryState.Commit | MemoryState.Reserve, MemoryProtect.ExecuteReadWrite);
    }
}