/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;
using System.IO;
using System.IO.Compression;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Common.Tests.Wrappers
{
    [TestClass]
    public class IonicZipFileWrapperTests
    {
        string _workDir;

        [TestInitialize]
        public void Init()
        {
            _workDir = Path.Combine(Path.GetTempPath(), "IonicZipFileWrapperTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_workDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_workDir))
            {
                Directory.Delete(_workDir, true);
            }
        }

        // DotNetZip.Original's ZipEntry.Extract(string baseDirectory, ...) unconditionally rewrites the
        // computed output path with `.Replace("/", "\\")`, which breaks extraction on non-Windows: the
        // resulting path has no recognised directory separators there, so Directory.GetDirectoryName
        // returns "" and Directory.CreateDirectory("") throws. ZipEntryWrapper.Extract works around this
        // on non-Windows by building the target path itself. These tests exercise that wrapper through
        // its public surface (IonicZipFileWrapperFactory + IZipEntry), the same way CommonDataUtils.
        // ExtractFile uses it, so on Linux CI they exercise the workaround and on Windows they exercise
        // the untouched original code path.
        [TestMethod]
        public void ZipEntryWrapper_Extract_NestedEntry_CreatesIntermediateDirectoriesAndFile()
        {
            var zipPath = CreateZip(("sub/dir/file.txt", "hello from nested file"));
            var destDir = Path.Combine(_workDir, "dest");

            using (var zip = new IonicZipFileWrapperFactory().Read(zipPath))
            {
                foreach (var entry in zip)
                {
                    entry.Extract(destDir, FileOverwrite.Yes);
                }
            }

            var extractedFile = Path.Combine(destDir, "sub", "dir", "file.txt");
            Assert.IsTrue(File.Exists(extractedFile), "Expected extracted file at " + extractedFile);
            Assert.AreEqual("hello from nested file", File.ReadAllText(extractedFile));
        }

        [TestMethod]
        public void ZipEntryWrapper_Extract_RootLevelEntry_CreatesFileDirectlyUnderDestination()
        {
            var zipPath = CreateZip(("file.txt", "hello from root file"));
            var destDir = Path.Combine(_workDir, "dest");

            using (var zip = new IonicZipFileWrapperFactory().Read(zipPath))
            {
                foreach (var entry in zip)
                {
                    entry.Extract(destDir, FileOverwrite.Yes);
                }
            }

            var extractedFile = Path.Combine(destDir, "file.txt");
            Assert.IsTrue(File.Exists(extractedFile), "Expected extracted file at " + extractedFile);
            Assert.AreEqual("hello from root file", File.ReadAllText(extractedFile));
        }

        [TestMethod]
        public void ZipEntryWrapper_Extract_OverwriteNo_LeavesExistingFileUntouched()
        {
            var zipPath = CreateZip(("file.txt", "new content"));
            var destDir = Path.Combine(_workDir, "dest");
            Directory.CreateDirectory(destDir);
            var existingFile = Path.Combine(destDir, "file.txt");
            File.WriteAllText(existingFile, "original content");

            using (var zip = new IonicZipFileWrapperFactory().Read(zipPath))
            {
                foreach (var entry in zip)
                {
                    entry.Extract(destDir, FileOverwrite.No);
                }
            }

            Assert.AreEqual("original content", File.ReadAllText(existingFile));
        }

        [TestMethod]
        public void ZipEntryWrapper_Extract_OverwriteYes_ReplacesExistingFile()
        {
            var zipPath = CreateZip(("file.txt", "new content"));
            var destDir = Path.Combine(_workDir, "dest");
            Directory.CreateDirectory(destDir);
            var existingFile = Path.Combine(destDir, "file.txt");
            File.WriteAllText(existingFile, "original content");

            using (var zip = new IonicZipFileWrapperFactory().Read(zipPath))
            {
                foreach (var entry in zip)
                {
                    entry.Extract(destDir, FileOverwrite.Yes);
                }
            }

            Assert.AreEqual("new content", File.ReadAllText(existingFile));
        }

        string CreateZip(params (string entryName, string content)[] entries)
        {
            var zipPath = Path.Combine(_workDir, Guid.NewGuid().ToString("N") + ".zip");
            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var (entryName, content) in entries)
                {
                    var entry = archive.CreateEntry(entryName);
                    using var writer = new StreamWriter(entry.Open());
                    writer.Write(content);
                }
            }
            return zipPath;
        }
    }
}
