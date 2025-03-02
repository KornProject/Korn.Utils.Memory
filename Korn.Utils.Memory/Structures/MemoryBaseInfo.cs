using System;

namespace Korn.Utils.Memory
{
    public unsafe struct MemoryBaseInfo
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public long RegionSize;
        public MemoryState State;
        public MemoryProtect Protect;
        public MemoryType Type;

        public bool IsValid => BaseAddress != IntPtr.Zero;

        public bool SetProtection(MemoryProtect protection)
        {
            var result = Interop.VirtualProtect(BaseAddress, RegionSize, protection);
            if (result)
                Protect = protection;

            return result;
        }
    }
}