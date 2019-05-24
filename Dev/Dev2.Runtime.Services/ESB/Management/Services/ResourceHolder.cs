#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dev2.Common.Interfaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class ResourceHolder : IResourceHolder
    {
        readonly string _path;

        public ResourceHolder(string path)
        {
            _path = path;
        }

        public List<FileInfo> GetFilesList()
        {
            var list = Directory.GetDirectories(_path, "*", SearchOption.TopDirectoryOnly).Where(a => a.Contains("Resources")).ToList();
            var fileInfos = list.Select(fileName => new FileInfo(fileName)).ToList();
            return fileInfos;
        }
    }
}