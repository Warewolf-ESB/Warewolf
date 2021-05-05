/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.DirectoryServices;

namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IDirectoryEntry : IWrappedObject<DirectoryEntry>, IDisposable
    {
        IDirectoryEntries Children { get; }
        string SchemaClassName { get; }
        string Name { get; }

        object Invoke(string methodName, params object[] args);

    }

    public interface IDirectoryEntries : IEnumerable, IWrappedObject<DirectoryEntries>
    {
        SchemaNameCollection SchemaFilter { get; }
    }
    public class Dev2DirectoryEntries : IDirectoryEntries
    {
        readonly DirectoryEntries _entries;
        public Dev2DirectoryEntries(DirectoryEntries entries)
        {
            _entries = entries;
        }
        public DirectoryEntries Instance => _entries;

        public SchemaNameCollection SchemaFilter => Instance.SchemaFilter;

        public IEnumerator GetEnumerator()
        {

            foreach (var item in Instance)
            {
                yield return new Dev2DirectoryEntry(item as DirectoryEntry);
            }
        }
    }

    public class Dev2DirectoryEntry : IDirectoryEntry
    {
        readonly DirectoryEntry _directoryEntry;
        public Dev2DirectoryEntry(DirectoryEntry directoryEntry)
        {
            _directoryEntry = directoryEntry;
        }
        public Dev2DirectoryEntry(string path)
        {
            _directoryEntry = new DirectoryEntry(path);
        }
        public IDirectoryEntries Children => new Dev2DirectoryEntries(Instance.Children);

        public string SchemaClassName => Instance.SchemaClassName;

        public string Name => Instance.Name;

        public DirectoryEntry Instance => _directoryEntry;

        public void Dispose()
        {
            Instance.Dispose();
        }

        public object Invoke(string methodName, params object[] args)
        {
            return Instance.Invoke(methodName, args);
        }
    }
}