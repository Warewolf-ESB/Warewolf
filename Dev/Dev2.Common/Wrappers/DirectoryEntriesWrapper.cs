/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections;
using System.DirectoryServices;
using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.Common.Wrappers
{
    public class DirectoryEntriesWrapper : IDirectoryEntries
    {
        private readonly DirectoryEntries _directoryEntries;

        internal DirectoryEntriesWrapper(DirectoryEntries directory)
        {
            _directoryEntries = directory;
        }

        public IEnumerator GetEnumerator()
        {
            return _directoryEntries.GetEnumerator();
        }

        public DirectoryEntries Instance
        {
            get { return _directoryEntries; }
        }
    }
}