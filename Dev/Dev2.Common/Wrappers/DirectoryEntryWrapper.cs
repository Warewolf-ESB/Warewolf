/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using System.DirectoryServices;

namespace Dev2.Common.Wrappers
{
    public class DirectoryEntryWrapper : IDirectoryEntry
    {
        private readonly IDirectoryEntryFactory _nativeFactory;
        private readonly DirectoryEntry _wrapped;

        internal DirectoryEntryWrapper(DirectoryEntry wrapped, IDirectoryEntryFactory directoryEntryFactory)
        {
            _wrapped = wrapped;
            _nativeFactory = directoryEntryFactory;
        }

        public IDirectoryEntries Children
        {
            get { return _nativeFactory.Create(_wrapped.Children); }
        }

        public string SchemaClassName
        {
            get { return _wrapped.SchemaClassName; }
        }

        public string Name
        {
            get { return _wrapped.SchemaClassName; }
        }

        public DirectoryEntry Instance
        {
            get { return _wrapped; }
        }
    }
}