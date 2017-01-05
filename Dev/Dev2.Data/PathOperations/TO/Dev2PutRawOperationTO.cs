/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.PathOperations
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a TO for RawPut operations
    /// </summary>
    public class Dev2PutRawOperationTO
    {

        public Dev2PutRawOperationTO(WriteType writeType, string contents)
        {
            WriteType = writeType;
            FileContents = contents;
        }

        public WriteType WriteType { get; set; }

        public string FileContents
        {
            get;
            set;
        }


    }
}
