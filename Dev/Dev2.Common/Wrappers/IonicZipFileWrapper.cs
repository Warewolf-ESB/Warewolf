/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Common.Interfaces.Wrappers;
using Ionic.Zip;

namespace Dev2.Common.Wrappers
{
    public class IonicZipFileWrapper : IIonicZipFileWrapper
    {
        private readonly ZipFile _ionicZip;

        public IonicZipFileWrapper()
        {
        }
        public IonicZipFileWrapper(ZipFile ionicZip)
        {
            this._ionicZip = ionicZip;
        }

        public string Password
        {
            get;
            set;
        }

        public void Dispose()
        {
            if (_ionicZip != null)
            {
                _ionicZip.Dispose();
            }
        }

        public IEnumerator<IZipEntry> GetEnumerator()
        {
            foreach (var entry in _ionicZip)
            {
                yield return new ZipEntryWrapper(entry);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class IonicZipFileWrapperFactory : IIonicZipFileWrapperFactory
    {
        public IIonicZipFileWrapper Read(string tempFile) => new IonicZipFileWrapper(ZipFile.Read(tempFile));
        public IIonicZipFileWrapper Read(Stream stream) => new IonicZipFileWrapper(ZipFile.Read(stream));
    }

    [ExcludeFromCodeCoverage]
    class ZipEntryWrapper : IZipEntry
    {
        readonly ZipEntry _zipEntry;
        public ZipEntryWrapper(ZipEntry zipEntry)
        {
            _zipEntry = zipEntry;
        }

        public void Extract(string extractFromPath, FileOverwrite overwrite)
        {
            if (overwrite == FileOverwrite.Yes)
            {
                _zipEntry.Extract(extractFromPath, ExtractExistingFileAction.OverwriteSilently);
            }
            else
            {
                if (overwrite == FileOverwrite.No)
                {
                    _zipEntry.Extract(extractFromPath, ExtractExistingFileAction.DoNotOverwrite);
                }
            }
        }
    }
}
