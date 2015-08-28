
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide the Zip methods its arguments
    /// </summary>
    public class Dev2ZipOperationTO : IZip {

        public Dev2ZipOperationTO(string ratio, string passwd, string name,bool overwrite) {
            CompressionRatio = ratio;
            ArchivePassword = passwd;
            ArchiveName = name;
            Overwrite = overwrite;
        }

        public string CompressionRatio {
            get;
            set;
        }

        public string ArchivePassword {
            get;
            set;
        }

        public string ArchiveName {
            get;
            set;
        }

        public bool Overwrite { get; set; }
    }
}
