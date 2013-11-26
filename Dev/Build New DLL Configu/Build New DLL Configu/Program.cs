using System;
using System.Collections.Generic;
using System.IO;

namespace Build_New_DLL_Configu
{
    /// <summary>
    /// Used to bootstrap creating new dll dependencies 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // location of dlls
            var dir = @"F:\foo\NewDLLs";

            var dirs = Directory.GetFiles(dir);

            var svrFrag = @"<Component Id=""cmp{0}"" Directory=""dir1931D9C5012B41449415D8D0C9710DEC"" Guid=""{{1}}"">
                <File Id=""fil{2}"" KeyPath=""yes"" Source=""..\ProductBuild\Server\{3}"" />
            </Component>";

            var studioFrag = @"<Component Id=""cmp{0}"" Directory=""dir1931D9C5012B41449415D8D0C9710DEC"" Guid=""{{1}}"">
                <File Id=""fil{2}"" KeyPath=""yes"" Source=""..\ProductBuild\Studio\{3}"" />
            </Component>";

            List<string> svrList = new List<string>();
            List<string> stdList = new List<string>();

            foreach (var d in dirs)
            {
                var svr = string.Format(svrFrag, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), d);
                var std = string.Format(studioFrag, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), d);

                svrList.Add(svr);
                stdList.Add(std);
            }


            foreach (var tmp in svrList)
            {
                File.AppendAllText(@"F:\foo\server.txt",tmp);
            }

            foreach(var tmp in stdList)
            {
                File.AppendAllText(@"F:\foo\studio.txt", tmp);
            }

        }
    }
}
