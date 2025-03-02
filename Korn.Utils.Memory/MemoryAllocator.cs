using System;

namespace Korn.Utils.Memory
{
    public unsafe static class MemoryAllocator
    {
        public static MemoryBaseInfo AllocateNear(IntPtr address, long size)
        {
            size = (size & 0xFFFFF000) + ((size & 0xFFF) > 0 ? 0x1000 : 0); // int anyway

            var mbi = FindFreeNear(address, size);
            if (!mbi.IsValid)
                return default;

            return Allocate(mbi.BaseAddress, size);
        }            

        public static MemoryBaseInfo Allocate(IntPtr address, long size)
        {
            address = Interop.VirtualAlloc(address, size, MemoryState.Commit | MemoryState.Reserve, MemoryProtect.ExecuteReadWrite);
            return Query(address);
        }

        public static MemoryBaseInfo Allocate(long size) => Allocate(IntPtr.Zero, size);

        public static MemoryBaseInfo Query(IntPtr address) => Interop.VirtualQuery(address);

        public static void Query(IntPtr address, MemoryBaseInfo* mbi) => Interop.VirtualQuery(address, mbi);

        public static MemoryBaseInfo FindFreeNear(IntPtr address, long size)
        {
            var top = QueryTopFirstFree(address, size);
            var bot = QueryBotFirstFree(address, size);

            var topDistance = (long)top.BaseAddress - (long)address;
            var botDistance = (long)address - (long)bot.BaseAddress;

            if (!top.IsValid || !bot.IsValid)
            {
                if (top.IsValid && topDistance < 0xFFFFFFFF)
                    return top;

                if (bot.IsValid && botDistance < 0xFFFFFFFF)
                    return bot;

                return default;
            }

            if (botDistance < topDistance && botDistance < 0xFFFFFFFF)
                return bot;

            if (topDistance < botDistance && topDistance < 0xFFFFFFFF)
                return top;

            return default;
        }

        public static void Free(IntPtr address, long size) => Interop.VirtualFree(address, size, MemoryFreeType.Release);
        public static void Free(MemoryBaseInfo* mbi) => Free(mbi->BaseAddress, mbi->RegionSize);
        public static void Free(IntPtr address)
        {
            var mbi = Query(address);
            Free(&mbi);
        }

        public static MemoryBaseInfo QueryTopFirstFree(IntPtr address, long size)
        {
            MemoryBaseInfo mbi = Query(address);
            while ((ulong)address < 0x7FFFFFFFFFFF)
            {
                if (mbi.State == MemoryState.Free && (long)mbi.RegionSize >= size)
                    return mbi;

                address = (IntPtr)((long)mbi.BaseAddress + mbi.RegionSize);
                Query(address, &mbi);
            }

            return default;
        }

        public static MemoryBaseInfo QueryBotFirstFree(IntPtr address, long size)
        {
            MemoryBaseInfo mbi = Query(address);
            while ((ulong)address > 0x10000)
            {
                if (mbi.State == MemoryState.Free && (long)mbi.RegionSize >= size)
                    return mbi;

                address = (IntPtr)((long)mbi.BaseAddress - 1);
                Query(address, &mbi);
            }

            return default;
        }

        public static MemoryBaseInfo QueryNextTop(MemoryBaseInfo* mbi) => Query((IntPtr)((long)mbi->BaseAddress + mbi->RegionSize));
        public static MemoryBaseInfo QueryNextBot(MemoryBaseInfo* mbi) => Query(mbi->BaseAddress - 1);
    }
}