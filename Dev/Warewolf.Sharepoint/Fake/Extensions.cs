#pragma warning disable

using Microsoft.SharePoint.Client;
using System;
using System.IO;

namespace Warewolf.Sharepoint
{
    public static class Extensions
    {
        public static void SaveBinaryDirect(ClientContext ctx, string site, Stream f, bool b)
        {
            throw new NotImplementedException();

        }

        public static FileInfo OpenBinaryDirect(ClientContext ctx, string fileRef)
        {
            throw new NotImplementedException();
        }
    }
}
