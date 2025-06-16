using System.Drawing;

namespace Korn.Utils
{
    public unsafe static class MemoryAllocator
    {
        public static MemoryBaseInfo AllocateNear(Address address, long size)
        {
            size = (size & 0xFFFFF000) + ((size & 0xFFF) > 0 ? 0x1000 : 0); // int anyway

            var mbi = FindFreeNear(address, size);
            if (!mbi.IsValid)
                return default;

            return Allocate(mbi.BaseAddress, size);
        }            

        public static MemoryBaseInfo Allocate(Address address, long size)
        {
            address = Kernel32.VirtualAlloc(address, size, MemoryState.Commit | MemoryState.Reserve, MemoryProtect.ExecuteReadWrite);
            return Query(address);
        }

        public static MemoryBaseInfo Allocate(long size) => Allocate(null, size);

        public static MemoryBaseInfo Query(Address address) => Kernel32.VirtualQuery(address);

        public static void Query(Address address, MemoryBaseInfo* mbi) => Kernel32.VirtualQuery(address, mbi);

        public static MemoryBaseInfo FindFreeNear(Address address, long size)
        {
            var top = QueryTopFirstFree(address, size);
            var bot = QueryBotFirstFree(address, size);

            var topDistance = (long)top.BaseAddress - address.Signed;
            var botDistance = address.Signed - (long)bot.BaseAddress;

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

        // for MemoryFreeType.Decommit
        /*
        public static void Free(Address address, long size) => Kernel32.VirtualFree(address, size, MemoryFreeType.Release);
        public static void Free(MemoryBaseInfo* mbi) => Free(mbi->BaseAddress, mbi->RegionSize);
        public static void Free(Address address)
        {
            var mbi = Query(address);
            Free(&mbi);
        }
        */

        // https://learn.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-virtualfree: for MemoryFreeType.Release size should be zero
        public static void Free(MemoryBaseInfo* mbi) => Free(mbi->BaseAddress);
        public static void Free(Address address) => Kernel32.VirtualFree(address, 0, MemoryFreeType.Release);

        public static MemoryBaseInfo QueryTopFirstFree(Address address, long size)
        {
            var mbi = Query(address);
            while (address.Unsigned < 0x7FFFFFFFFFFF)
            {
                if (mbi.State == MemoryState.Free && (long)mbi.RegionSize >= size)
                    return mbi;

                address = mbi.BaseAddress + mbi.RegionSize;
                Query(address, &mbi);
            }

            return default;
        }

        public static MemoryBaseInfo QueryBotFirstFree(Address address, long size)
        {
            var mbi = Query(address);
            while (address.Unsigned > 0x10000)
            {
                if (mbi.State == MemoryState.Free && (long)mbi.RegionSize >= size)
                    return mbi;

                address = mbi.BaseAddress - 1;
                Query(address, &mbi);
            }

            return default;
        }

        public static MemoryBaseInfo QueryNextTop(MemoryBaseInfo* mbi) => Query(mbi->BaseAddress + mbi->RegionSize);
        public static MemoryBaseInfo QueryNextBot(MemoryBaseInfo* mbi) => Query(mbi->BaseAddress - 1);
    }
}