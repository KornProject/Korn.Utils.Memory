using System;
using System.Text;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
namespace Korn.Utils
{
    public unsafe struct ExternalMemory
    {
        public ExternalMemory(IntPtr processHandle) => ProcessHandle = processHandle;

        IntPtr ProcessHandle;

        public void Write(Address address, byte[] buffer) => Write(ProcessHandle, address, buffer);
        public void Write(Address address, byte* buffer, int size) => Write(ProcessHandle, address, buffer, size);
        public void Write<T>(Address address, T value) where T : unmanaged => Write(ProcessHandle, address, value);
        public void Write<T>(Address address, T* pointer) where T : unmanaged => Write(ProcessHandle, address, pointer);
        public byte[] Read(Address address, int length) => Read(ProcessHandle, address, length);
        public void Read(Address source, void* destination, int length) => Read(ProcessHandle, source, destination, length);
        public T Read<T>(Address address) where T : unmanaged => Read<T>(ProcessHandle, address);

        public string ReadUTF8(Address address) => ReadUTF8(address, 32);

        public string ReadUTF8(Address address, int partSize)
        {
            const int MaxLength = short.MaxValue;

            var offset = 0;
            var length = partSize;
            var buffer = new byte[length];
            while (length < MaxLength)
            {
                fixed (byte* pointer_ = buffer)
                {
                    var pointer = pointer_;
                    pointer += offset;
                    Read(address.Signed + offset, pointer, partSize);

                    #region Proclyato
                    var value = ((ulong*)pointer)[0];
                    if ((value & 0xFF) == 0) break;
                    if ((value & 0xFF00) == 0) { offset += 1; break; }
                    if ((value & 0xFF0000) == 0) { offset += 2; break; }
                    if ((value & 0xFF000000) == 0) { offset += 3; break; }
                    if ((value & 0xFF00000000) == 0) { offset += 4; break; }
                    if ((value & 0xFF0000000000) == 0) { offset += 5; break; }
                    if ((value & 0xFF000000000000) == 0) { offset += 6; break; }
                    if ((value & 0xFF00000000000000) == 0) { offset += 7; break; }

                    value = ((ulong*)pointer)[1];
                    if ((value & 0xFF) == 0) { offset += 8; break; }
                    if ((value & 0xFF00) == 0) { offset += 9; break; }
                    if ((value & 0xFF0000) == 0) { offset += 10; break; }
                    if ((value & 0xFF000000) == 0) { offset += 11; break; }
                    if ((value & 0xFF00000000) == 0) { offset += 12; break; }
                    if ((value & 0xFF0000000000) == 0) { offset += 13; break; }
                    if ((value & 0xFF000000000000) == 0) { offset += 14; break; }
                    if ((value & 0xFF00000000000000) == 0) { offset += 15; break; }

                    value = ((ulong*)pointer)[2];
                    if ((value & 0xFF) == 0) { offset += 16; break; }
                    if ((value & 0xFF00) == 0) { offset += 17; break; }
                    if ((value & 0xFF0000) == 0) { offset += 18; break; }
                    if ((value & 0xFF000000) == 0) { offset += 19; break; }
                    if ((value & 0xFF00000000) == 0) { offset += 20; break; }
                    if ((value & 0xFF0000000000) == 0) { offset += 21; break; }
                    if ((value & 0xFF000000000000) == 0) { offset += 22; break; }
                    if ((value & 0xFF00000000000000) == 0) { offset += 23; break; }

                    value = ((ulong*)pointer)[3];
                    if ((value & 0xFF) == 0) { offset += 24; break; }
                    if ((value & 0xFF00) == 0) { offset += 25; break; }
                    if ((value & 0xFF0000) == 0) { offset += 26; break; }
                    if ((value & 0xFF000000) == 0) { offset += 27; break; }
                    if ((value & 0xFF00000000) == 0) { offset += 28; break; }
                    if ((value & 0xFF0000000000) == 0) { offset += 29; break; }
                    if ((value & 0xFF000000000000) == 0) { offset += 30; break; }
                    if ((value & 0xFF00000000000000) == 0) { offset += 31; break; }
                    #endregion

                    offset = length;
                    length += partSize;
                    Array.Resize(ref buffer, length);
                }                
            }

            var result = Encoding.UTF8.GetString(buffer, 0, offset);
            return result;
        }

        public static void Write(IntPtr process, Address address, byte[] buffer) => Kernel32.WriteProcessMemory(process, address, buffer);
        public static void Write(IntPtr process, Address address, byte* buffer, int size) => Kernel32.WriteProcessMemory(process, address, buffer, size);
        public static void Write<T>(IntPtr process, Address address, T value) where T : unmanaged => Kernel32.WriteProcessMemory(process, address, value);
        public static void Write<T>(IntPtr process, Address address, T* pointer) where T : unmanaged => Kernel32.WriteProcessMemory(process, address, pointer);
        public static byte[] Read(IntPtr process, Address address, int length) => Kernel32.ReadProcessMemory(process, address, length);
        public static void Read(IntPtr process, Address source, void* destination, int length) => Kernel32.ReadProcessMemory(process, source, destination, length);
        public static T Read<T>(IntPtr process, Address address) where T : unmanaged => Kernel32.ReadProcessMemory<T>(process, address);
    }
}