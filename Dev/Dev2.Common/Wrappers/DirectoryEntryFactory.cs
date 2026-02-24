/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Wrappers;
using System.Runtime.InteropServices;
using Dev2.Common;
using System.Diagnostics.CodeAnalysis;
#if !NOTNANOSERVER
using System.DirectoryServices;
#endif

namespace Dev2.Common.Wrappers
{
    public class DirectoryEntryFactory : IDirectoryEntryFactory
	{
		public IDirectoryEntry Create(string path)
        {
            // Avoid using System.DirectoryServices on non-Windows or Nano Server where
            // the native Active Directory COM libraries (eg activeds.dll) are not present.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || GlobalConstants.IsNanoServer())
            {
                return new NullDirectoryEntry();
            }

            return new Dev2DirectoryEntry(path);
        }
        [ExcludeFromCodeCoverage]
        public IDirectoryEntry Create<T>(T member)
        {
#if !NOTNANOSERVER
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || GlobalConstants.IsNanoServer())
            {
                return new NullDirectoryEntry();
            }

            return new Dev2DirectoryEntry(new DirectoryEntry(member));
#else
            return new NullDirectoryEntry();
#endif
        }

        // Minimal null-object implementations to avoid touching DirectoryEntry on unsupported platforms
#if !NOTNANOSERVER
        class NullDirectoryEntries : IDirectoryEntries
        {
            public SchemaNameCollection SchemaFilter => null;
            public DirectoryEntries Instance => null;
            public System.Collections.IEnumerator GetEnumerator()
            {
                yield break;
            }
        }

        class NullDirectoryEntry : IDirectoryEntry
        {
            public IDirectoryEntries Children => new NullDirectoryEntries();
            public string SchemaClassName => string.Empty;
            public string Name => string.Empty;
            public DirectoryEntry Instance => null;
            public void Dispose() { }
            public object Invoke(string methodName, params object[] args) => null;
        }
#else
        class NullDirectoryEntries : IDirectoryEntries
        {
            public SchemaNameCollection SchemaFilter => null;
            public object Instance => null;
            public System.Collections.IEnumerator GetEnumerator()
            {
                yield break;
            }
        }

        class NullDirectoryEntry : IDirectoryEntry
        {
            public IDirectoryEntries Children => new NullDirectoryEntries();
            public string SchemaClassName => string.Empty;
            public string Name => string.Empty;
            public object Instance => null;
            public void Dispose() { }
            public object Invoke(string methodName, params object[] args) => null;
        }
#endif
	}
}
