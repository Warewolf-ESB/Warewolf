
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
    /// Purpose : To provide a TO for Put operations
    /// </summary>
    public class Dev2PutOperationTO : IFileWrite, IPathOverwrite {

        internal Dev2PutOperationTO(bool append, string contents, bool overwrite) {
            Append = append;
            FileContents = contents;
            Overwrite = overwrite;
        }

        public bool Append {
            get;
            set;
        }

        public string FileContents {
            get;
            set;
        }

        public bool Overwrite {
            get;
            set;
        }
    }
}
