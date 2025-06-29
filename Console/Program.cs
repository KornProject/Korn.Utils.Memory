using Korn.Modules.WinApi.Kernel;
using Korn.Utils;

TestMemoryNearAllocationAlgothim();

void TestMemoryNearAllocationAlgothim()
{
    var nearTo = 0x7FFB28BC0000;

    MemoryBaseInfo mbi;
    do
    {
        mbi = MemoryAllocator.AllocateNear((IntPtr)nearTo, 0x1000);
        Console.WriteLine($"Allocation: {mbi.BaseAddress:X}, distance: {(long)mbi.BaseAddress - nearTo}");
    }
    while (mbi.BaseAddress != default);

    _ = 3;
}