using System;
using System.Runtime.InteropServices;








namespace GACManagerApi.Fusion
{
    [StructLayout(LayoutKind.Sequential)]
    public class FUSION_INSTALL_REFERENCE
    {
        public FUSION_INSTALL_REFERENCE(Guid guid, String id, String data)
        {
            cbSize = 2 * IntPtr.Size + 16 + (id.Length + data.Length) * 2;
            flags = 0;
            // quiet compiler warning 
            if (flags == 0)
            {
            }
            guidScheme = guid;
            identifier = id;
            description = data;
        }

        public Guid GuidScheme
        {
            get { return guidScheme; }
        }

        public String Identifier
        {
            get { return identifier; }
        }

        public String Description
        {
            get { return description; }
        }

        private int cbSize;
        private int flags;
        private Guid guidScheme;
        [MarshalAs(UnmanagedType.LPWStr)]
        private String identifier;
        [MarshalAs(UnmanagedType.LPWStr)]
        private String description;
    }
    
}
