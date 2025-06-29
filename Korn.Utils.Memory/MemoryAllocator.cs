using Korn.Modules.WinApi;
using Korn.Modules.WinApi.Kernel;
using System;

namespace Korn.Utils
{
    public unsafe static class MemoryAllocator
    {
        static IntPtr VirtualAlloc(IntPtr address, long size) => Kernel32.VirtualAlloc(address, size, MemoryState.Commit | MemoryState.Reserve, MemoryProtect.ExecuteReadWrite);

        public static MemoryBaseInfo AllocateNear(IntPtr nearAddress, long size)
        {
            MemoryBaseInfo mbi;

            var topBarrierAddress = Math.Min((long)nearAddress + 0x7FFFFFF0 - size, 0x7FFFFFFFFFFF);
            var botBarrierAddress = Math.Max((long)nearAddress - 0x7FFFFFF0, 0x10000);

            var address = nearAddress;
            while ((long)address < topBarrierAddress)
            {
                Query(address, &mbi);
                if (mbi.State == MemoryState.Free && (long)mbi.RegionSize >= size)
                {
                    var allocatedAddress = VirtualAlloc(address, size);
                    if (allocatedAddress != default)
                        return Query(allocatedAddress);
                    else address += 0x1000;
                }
                else address = (IntPtr)((long)mbi.BaseAddress + mbi.RegionSize);
            }

            address = nearAddress;
            while ((long)address < topBarrierAddress)
            {
                Query(address, &mbi);
                if (mbi.State == MemoryState.Free && (long)mbi.RegionSize >= size)
                {
                    var allocatedAddress = VirtualAlloc(address, size);
                    if (allocatedAddress != default)
                        return Query(allocatedAddress);
                    else address -= 0x1000;
                }
                else address = mbi.AllocationBase - 1;
            }

            return default;
        }

        public static MemoryBaseInfo Allocate(IntPtr address, long size)
        {
            address = VirtualAlloc(address, size);
            if (address == IntPtr.Zero)
                return default;

            return Query(address);
        }

        public static MemoryBaseInfo Allocate(long size) => Allocate(default, size);

        public static MemoryBaseInfo Query(IntPtr address) => Kernel32.VirtualQuery(address);

        public static void Query(IntPtr address, MemoryBaseInfo* mbi) => Kernel32.VirtualQuery(address, mbi);

        // https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-virtualfree: for MemoryFreeType.Release size should be zero
        public static void Free(MemoryBaseInfo* mbi) => Free(mbi->BaseAddress);
        public static void Free(IntPtr address) => Kernel32.VirtualFree(address, 0, MemoryFreeType.Release);

        public static MemoryBaseInfo QueryNextTop(MemoryBaseInfo* mbi) => Query((IntPtr)((long)mbi->BaseAddress + mbi->RegionSize));

        public static MemoryBaseInfo QueryNextBot(MemoryBaseInfo* mbi) => Query(mbi->BaseAddress - 1);
    }
}