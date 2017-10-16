/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Interfaces;

namespace Dev2.PathOperations
{
    public class Dev2CRUDOperationTO : IDev2CRUDOperationTO
    {
        public Dev2CRUDOperationTO(bool overwrite)
            : this(overwrite, true)
        {
        }

        public Dev2CRUDOperationTO(bool overwrite, bool doRecursiveCopy)
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
