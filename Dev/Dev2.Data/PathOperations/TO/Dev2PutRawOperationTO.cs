/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;

namespace Dev2.PathOperations
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a TO for RawPut operations
    /// </summary>
    public class Dev2PutRawOperationTO : IDev2PutRawOperationTO
    {

        public Dev2PutRawOperationTO(WriteType writeType, string contents, bool fileContentsAsBase64 = false)
        {
            WriteType = writeType;
            FileContents = contents;
            FileContentsAsBase64 = fileContentsAsBase64;
        }

        public WriteType WriteType { get; set; }

        public string FileContents
        {
            get;
            set;
        }

        public bool FileContentsAsBase64 
        { 
            get; 
            set; 
        }
    }
}
