
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
    /// Purpose : To provide overwrite value to CRUD path operations
    /// </summary>
    public class Dev2CRUDOperationTO : IPathOverwrite
    {

        public Dev2CRUDOperationTO(bool overwrite,bool doRecursiveCopy = true)
        {
            Overwrite = overwrite;
            DoRecursiveCopy = doRecursiveCopy;
        }

        public bool DoRecursiveCopy { get; set; }

        public bool Overwrite
        {
            get;
            set;
        }
    }
}
