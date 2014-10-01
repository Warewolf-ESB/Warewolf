
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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

            var svrFrag = @"<Component Id=""cmp{0}"" Directory=""dir1931D9C5012B41449415D8D0C9710DEC"" Guid=""{1}"">
                <File Id=""fil{2}"" KeyPath=""yes"" Source=""..\ProductBuild\Server\{3}"" />
            </Component>";

            var studioFrag = @"<Component Id=""cmp{0}"" Directory=""dir1931D9C5012B41449415D8D0C9710DEC"" Guid=""{{1}}"">
                <File Id=""fil{2}"" KeyPath=""yes"" Source=""..\ProductBuild\Studio\{3}"" />
            </Component>";

            List<string> svrList = new List<string>();
            List<string> stdList = new List<string>();

            foreach (var d in dirs)
            {

                var file = Path.GetFileName(d);

                var id = "{" + Guid.NewGuid().ToString().ToUpper() + "}";
                var id2 = "{" + Guid.NewGuid().ToString().ToUpper() + "}";

                var svr = string.Format(svrFrag, Guid.NewGuid().ToString().ToUpper(), id, Guid.NewGuid().ToString().ToUpper(), file);
                var std = string.Format(studioFrag, Guid.NewGuid().ToString().ToUpper(), id2, Guid.NewGuid().ToString().ToUpper(), file);

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
