﻿using System;
using System.Runtime.InteropServices;

namespace WarewolfCOMIPC
{
    public class CustomTypeToTypeMarshaler : ICustomMarshaler
    {
        public static ICustomMarshaler GetInstance(string pstrCookie) => new CustomTypeToTypeMarshaler();

        public object MarshalNativeToManaged(IntPtr pNativeData) =>
#if NETFRAMEWORK
            !(pNativeData == (IntPtr) 0L)
                ? (object) Marshal.GetTypeForITypeInfo(pNativeData) :
#endif
                throw new ArgumentNullException(nameof (pNativeData));

        public IntPtr MarshalManagedToNative(object ManagedObj) =>
#if NETFRAMEWORK
            ManagedObj != null
                ? Marshal.GetITypeInfoForType(ManagedObj as Type) :
#endif
                throw new ArgumentNullException(nameof(ManagedObj));

        public void CleanUpNativeData(IntPtr pNativeData) => Console.WriteLine("An attempt was made to cleanup native data.");

        public void CleanUpManagedData(object ManagedObj) => Console.WriteLine("An attempt was made to cleanup manged data.");

        public int GetNativeDataSize() => -1;
    }
}