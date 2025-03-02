using Korn.Utils.Memory;
using System;
using System.Runtime.InteropServices;

static unsafe class Interop
{
    const string kernel = "kernel32";

    [DllImport(kernel)] public static extern
        int GetLastError();

    [DllImport(kernel)] public static extern
       bool VirtualQuery(IntPtr address, MemoryBaseInfo* buffer, int length);

    [DllImport(kernel)] public static extern
        bool VirtualProtect(IntPtr address, long size, MemoryProtect newProtect, MemoryProtect* oldProtect);

    [DllImport(kernel)] public static extern
        IntPtr VirtualAlloc(IntPtr address, long size, MemoryState allocationType, MemoryProtect protect);

    [DllImport(kernel)] public static extern
        bool VirtualFree(IntPtr address, long size, MemoryFreeType freeType);
        
    public static bool VirtualQuery(IntPtr baseAddress, MemoryBaseInfo* mbi) => VirtualQuery(baseAddress, mbi, sizeof(MemoryBaseInfo));

    public static MemoryBaseInfo VirtualQuery(IntPtr baseAddress)
    {
        MemoryBaseInfo mbi;
        if (!VirtualQuery(baseAddress, &mbi, sizeof(MemoryBaseInfo)))
            return default;
        return mbi;
    }

    public static bool VirtualProtect(IntPtr address, long size, MemoryProtect newProtect)
    {
        MemoryProtect oldProtection;
        return VirtualProtect(address, size, newProtect, &oldProtection);
    }
}