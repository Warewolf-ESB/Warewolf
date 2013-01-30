using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Dev2.Data.Util
{
    public static class GCWriter
    {

        /// <summary>
        /// Writes the data.
        /// </summary>
        /// <param name="data">The data.</param>
        public static void WriteData(string data)
        {
            File.AppendAllText(@"c:\foo\log.log", "\n\r"+data);
        }
    }
}
