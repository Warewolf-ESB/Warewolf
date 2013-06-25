using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO
{
    internal static class __Error
    {
        internal static void EndOfFile()
        {
            throw new EndOfStreamException(WeaveUtility.GetResourceString("IO.EOF_ReadBeyondEOF"));
        }

        internal static void FileNotOpen()
        {
            throw new ObjectDisposedException(null, WeaveUtility.GetResourceString("ObjectDisposed_FileClosed"));
        }
    }

}
