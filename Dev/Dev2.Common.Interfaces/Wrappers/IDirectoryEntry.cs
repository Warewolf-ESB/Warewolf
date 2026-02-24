/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Microsoft.Win32;
using System;
using System.Collections;
using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace Dev2.Common.Interfaces.Wrappers
{
    public interface IDirectoryEntry : IWrappedObject<DirectoryEntry>, IDisposable
    {
        IDirectoryEntries Children { get; }
        string SchemaClassName { get; }
        string Name { get; }

        object Invoke(string methodName, params object[] args);

    }

    class NullDirectoryEntries : IDirectoryEntries
    {
        public SchemaNameCollection SchemaFilter => null;
        public DirectoryEntries Instance => null;
        public System.Collections.IEnumerator GetEnumerator()
        {
            yield break;
        }
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
            try
            {
                // Avoid attempting to load DirectoryEntry on unsupported platforms
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || IsNanoServer())
                {
                    _directoryEntry = null;
                }
                else
                {
                    _directoryEntry = new DirectoryEntry(path);
                }
            }
            catch
            {
                _directoryEntry = null;
            }
		}

		public static bool IsNanoServer()
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return false;

			try
			{
				using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
				if (key == null) return false;

				var productName = (key.GetValue("ProductName") as string) ?? string.Empty;
				if (productName.IndexOf("Nano", StringComparison.OrdinalIgnoreCase) >= 0)
					return true;

				var installationType = (key.GetValue("InstallationType") as string) ?? string.Empty;
				if (installationType.IndexOf("Nano", StringComparison.OrdinalIgnoreCase) >= 0)
					return true;
			}
			catch
			{
				// Access denied or other problem - treat as not Nano (or handle as appropriate)
			}

			return false;
		}
		public IDirectoryEntries Children => Instance == null ? new NullDirectoryEntries() : new Dev2DirectoryEntries(Instance.Children);

        public string SchemaClassName => Instance == null ? string.Empty : Instance.SchemaClassName;

        public string Name => Instance == null ? string.Empty : Instance.Name;

        public DirectoryEntry Instance => _directoryEntry;

        public void Dispose()
        {
            Instance?.Dispose();
        }

        public object Invoke(string methodName, params object[] args)
        {
            return Instance == null ? null : Instance.Invoke(methodName, args);
        }
    }
}