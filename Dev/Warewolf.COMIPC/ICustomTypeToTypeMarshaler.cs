using System;
using System.Runtime.InteropServices;

namespace WarewolfCOMIPC
{
    public interface ICustomTypeToTypeMarshaler : ICustomMarshaler
    {
        object MarshalNativeToManaged(IntPtr pNativeData)
        {
            Console.WriteLine("An attempt was made to marshal native object to managed.");
            return null;
        }

        IntPtr MarshalManagedToNative(object ManagedObj)
        {
            Console.WriteLine("An attempt was made to marshal managed object to native.");
            return new IntPtr();
        }

        void CleanUpNativeData(IntPtr pNativeData)
        {
            Console.WriteLine("An attempt was made to cleanup native data.");
        }

        void CleanUpManagedData(object ManagedObj)
        {
            Console.WriteLine("An attempt was made to cleanup manged data.");
        }

        int GetNativeDataSize()
        {
            Console.WriteLine("An attempt was made to get native data size.");
            return 0;
        }
    }
}