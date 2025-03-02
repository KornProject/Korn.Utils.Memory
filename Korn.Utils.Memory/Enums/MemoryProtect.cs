using System;

namespace Korn.Utils.Memory
{
    [Flags]
    public enum MemoryProtect
    {
        ZeroAccess = 0,
        NoAccess = 1,
        ReadOnly = 2,
        ReadWrite = 4,
        WriteCopy = 8,
        Execute = 16,
        ExecuteRead = 32,
        ExecuteReadWrite = 64,
        ExecuteWriteCopy = 128,
        Guard = 256,
        ReadWriteGuard = 260,
        NoCache = 512
    }

    public static class MemoryProtectExtensions
    {
        public static bool IsWritable(this MemoryProtect self) => 
            self == MemoryProtect.WriteCopy ||
            self == MemoryProtect.ReadWrite ||
            self == MemoryProtect.ExecuteWriteCopy || 
            self == MemoryProtect.ExecuteReadWrite ||
            self == MemoryProtect.ReadWriteGuard; 
    }
}