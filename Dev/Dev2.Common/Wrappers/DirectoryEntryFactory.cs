/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using Dev2.Common.Interfaces.Wrappers;

namespace Dev2.Common.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class DirectoryEntryFactory : IDirectoryEntryFactory
    {
        public IDirectoryEntry Create(string path)
        {
            return new DirectoryEntryWrapper(new DirectoryEntry(path), this);
        }

        public IDirectoryEntries Create(DirectoryEntries directoryEntries)
        {
            return new DirectoryEntriesWrapper(directoryEntries);
        }
    }
}