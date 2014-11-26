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

namespace Dev2
{
    public interface IFrameworkFileIO
    {
        Stream Get(Uri path, string userName = "", string password = "");
        void Put(Stream data, Uri path, bool overwrite = false, string userName = "", string password = "");
        void Delete(Uri path, string userName = "", string password = "");
        IList<Uri> List(Uri path, string userName = "", string password = "");
        void CreateDirectory(Uri path, string userName = "", string password = "");

        void Copy(Uri sourcePath, Uri destinationPath, bool overWrite = false, string userName = "",
            string password = "");

        void Move(Uri sourcePath, Uri destinationPath, bool overWrite = false, string userName = "",
            string password = "");
    }
}