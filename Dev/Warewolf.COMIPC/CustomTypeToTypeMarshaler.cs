using System;
using System.Runtime.InteropServices;

namespace WarewolfCOMIPC
{
    public class CustomTypeToTypeMarshaler : ICustomMarshaler
    {
        public static ICustomMarshaler GetInstance(string pstrCookie) => new CustomTypeToTypeMarshaler();

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            Console.WriteLine("An attempt was made to marshal native object to managed.");
            return null;
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            Console.WriteLine("An attempt was made to marshal managed object to native.");
            return new IntPtr();
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            Console.WriteLine("An attempt was made to cleanup native data.");
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            Console.WriteLine("An attempt was made to cleanup manged data.");
        }

        public int GetNativeDataSize()
        {
            Console.WriteLine("An attempt was made to get native data size.");
            return 0;
        }
    }
}