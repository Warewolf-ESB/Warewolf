using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestPackBuilder
{
    public class TestPackTO : IComparable<TestPackTO>
    {
        public string TestDLLName { get; set; }
        public string TestName { get; set; }

        #region Implementation of IComparable

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj"/> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj"/>. Greater than zero This instance follows <paramref name="obj"/> in the sort order. 
        /// </returns>
        /// <param name="obj">An object to compare with this instance. </param><exception cref="T:System.ArgumentException"><paramref name="obj"/> is not the same type as this instance. </exception>
        public int CompareTo(object obj)
        {
            return 0;
        }

        #endregion

        public int CompareTo(TestPackTO other)
        {
            return String.Compare(TestName, other.TestName, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class TestScanner
    {
        private const string _signatureStart = "public void";
        private const string _signatureEnd = "(";


        public string GenerateTestPacks(string dirToScan, int totalTestPacks, string destDir)
        {

            CreateTestPackDirectory(destDir);
            var files = CreateTestPackFiles(totalTestPacks, destDir);


            // get test
            var tests = RecursivelyScanDirectory(dirToScan, ".cs", "[TestMethod]");
            tests.Sort();
            var testPackStrings = new List<string>(totalTestPacks);
            var testPackDLLS = new List<string>(totalTestPacks);
            for(int i = 0; i < totalTestPacks; i++)
            {
                testPackDLLS.Add(string.Empty);
            }

            // build string
            var pos = 0;
            foreach(var test in tests)
            {
                if(!string.IsNullOrEmpty(test.TestName))
                {
                    if(pos >= testPackStrings.Count)
                    {
                        testPackStrings.Add(string.Empty);
                    }

                    testPackStrings[pos] += test.TestName + Environment.NewLine;
                    testPackDLLS[pos] += test.TestDLLName + ",";
                    pos++;
                    if(pos >= totalTestPacks)
                    {
                        pos = 0;
                    }
                }
            }

            for(int i = 0; i < totalTestPacks; i++)
            {
                foreach(var source in testPackDLLS.Select(c => c).Distinct())
                {
                    File.AppendAllText(files[i] + ".dlls", source);
                }
            }


            // dump to file
            for(int i = 0; i < totalTestPacks; i++)
            {
                File.AppendAllText(files[i], testPackStrings[i]);
            }

            return destDir;
        }


        public List<TestPackTO> RecursivelyScanDirectory(string directoryToScan, string fileExtentionToScanFor, string testAnnotationSearchString)
        {
            return InternalDirectoryScan(directoryToScan, fileExtentionToScanFor, testAnnotationSearchString, true);
        }

        public List<TestPackTO> ScanDirectory(string directoryToScan, string fileExtentionToScanFor, string testAnnotationSearchString)
        {
            return InternalDirectoryScan(directoryToScan, fileExtentionToScanFor, testAnnotationSearchString, false);
        }

        private void CreateTestPackDirectory(string destDir)
        {
            try
            {
                Directory.CreateDirectory(destDir);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            { }
        }

        private static List<string> CreateTestPackFiles(int totalTestPacks, string destDir)
        {
            // create test pack files
            var files = new List<string>();
            for(int i = 0; i < totalTestPacks; i++)
            {
                var thefile = Path.Combine(destDir, i + ".testpack");
                File.Create(thefile).Close();

                files.Add(thefile);
            }

            return files;
        }

        private List<TestPackTO> InternalDirectoryScan(string directoryToScan, string fileExtentionToScanFor, string testAnnotationSearchString, bool recusiveScan)
        {

            if(directoryToScan == null || !Directory.Exists(directoryToScan))
            {
                return new List<TestPackTO> { new TestPackTO { TestDLLName = "ERROR", TestName = "Error : Directory Does Not Exist" } };
            }

            if(string.IsNullOrEmpty(fileExtentionToScanFor))
            {
                return new List<TestPackTO> { new TestPackTO { TestDLLName = "ERROR", TestName = "Error : No File Extension To Scan For" } };
            }

            var toScanFor = BuildSearchExtention(fileExtentionToScanFor);
            var result = new List<TestPackTO>();

            if(!recusiveScan)
            {
                result.AddRange(ScanDirectoryForFiles(directoryToScan, toScanFor, testAnnotationSearchString, "NA"));
            }
            else
            {
                // Build up the root bits
                //result.AddRange(ScanDirectoryForFiles(directoryToScan, toScanFor, testAnnotationSearchString));
                List<string> directories = Directory.GetDirectories(directoryToScan).ToList();

                // TODO : Here we need to save the Dirs with Test to the side ;)

                var dir = directories.FirstOrDefault();
                if(dir != null)
                {
                    directories.RemoveAt(0);
                }

                // Now process to ID test dlls ;)
                List<string> magicalTestDirs = new List<string>();
                foreach(var theDir in directories)
                {
                    if(theDir.EndsWith(".Tests") || theDir.EndsWith(".Specs") || theDir.EndsWith(".Test"))
                    {
                        magicalTestDirs.Add(theDir);
                    }
                }

                while(dir != null)
                {
                    var testDLL = ExtractTestDLLName(magicalTestDirs, dir);

                    if(testDLL != null)
                    {

                        result.AddRange(ScanDirectoryForFiles(dir, toScanFor, testAnnotationSearchString, testDLL));
                        // now scan this directory for more directories ;)
                        var subDirectories = Directory.GetDirectories(dir).ToList();

                        directories.AddRange(subDirectories);
                    }

                    // get new dir ;)
                    dir = directories.FirstOrDefault();
                    if(dir != null)
                    {
                        directories.RemoveAt(0);
                    }

                }
            }

            return result;
        }

        private static readonly IDictionary<string, string> _dirToDll = new Dictionary<string, string>();
        private string ExtractTestDLLName(List<string> testDirectories, string path)
        {
            int pos = 0;
            string result = null;
            while(pos < testDirectories.Count && string.IsNullOrEmpty(result))
            {
                var dir = testDirectories[pos];
                if(path.IndexOf(dir, StringComparison.Ordinal) >= 0)
                {
                    if(_dirToDll.ContainsKey(dir))
                    {
                        result = _dirToDll[dir];
                    }
                    else
                    {
                        result = Path.GetFileName(dir) + ".dll";
                        _dirToDll[dir] = result;
                    }
                }
                pos++;
            }

            return result;
        }

        private IEnumerable<TestPackTO> ScanDirectoryForFiles(string directoryToScan, string toScanFor, string testAnnotationSearchString, string dirDLL)
        {
            var files = Directory.GetFiles(directoryToScan, toScanFor);
            var result = new List<TestPackTO>();

            foreach(var file in files)
            {
                var contents = File.ReadAllText(file);
                // we have a match folks ;)
                result.AddRange(ProcessFile(contents, testAnnotationSearchString, dirDLL));
            }

            return result;
        }

        private string BuildSearchExtention(string fileExtentionToScanFor)
        {
            var toScanFor = "*";

            if(fileExtentionToScanFor.StartsWith("*"))
            {
                toScanFor = fileExtentionToScanFor;
            }
            else if(!fileExtentionToScanFor.StartsWith("."))
            {
                toScanFor += "." + fileExtentionToScanFor;
            }
            else
            {
                toScanFor += fileExtentionToScanFor;
            }

            return toScanFor;
        }

        private IEnumerable<TestPackTO> ProcessFile(string contents, string testAnnotationSearchString, string dirDLL)
        {
            int idx;
            var result = new List<TestPackTO>();
            var nameEnd = 0;

            while((idx = contents.IndexOf(testAnnotationSearchString, nameEnd, StringComparison.Ordinal)) >= 0)
            {
                var nameStart = contents.IndexOf(_signatureStart, idx, StringComparison.Ordinal);
                if(nameStart > 0)
                {
                    nameStart += _signatureStart.Length;
                    nameEnd = contents.IndexOf(_signatureEnd, nameStart, StringComparison.Ordinal);

                    // we got one ;)
                    if(nameEnd > nameStart)
                    {
                        var len = (nameEnd - nameStart);
                        var testName = contents.Substring(nameStart, len).Trim();

                        if(!string.IsNullOrEmpty(testName))
                        {
                            result.Add(new TestPackTO { TestDLLName = dirDLL, TestName = testName });
                        }
                    }
                    else
                    {
                        // time to end it ;)
                        nameEnd = contents.Length - 1;
                    }
                }
            }

            return result;
        }
    }

}
