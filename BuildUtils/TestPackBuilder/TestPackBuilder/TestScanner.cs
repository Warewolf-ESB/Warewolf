using System.IO;
using System.Text;

namespace TestPackBuilder
{
    /// <summary>
    /// Description of MyClass.
    /// </summary>
    public class TestScanner
    {
        private const string _signatureStart = "public void";
        private const string _signatureEnd = "(";

        public string ScanDirectory(string directoryToScan, string fileExtentionToScanFor, string testAnnotationSearchString)
        {

            if(directoryToScan == null || !Directory.Exists(directoryToScan))
            {
                return "Error : Directory Does Not Exist";
            }

            if(string.IsNullOrEmpty(fileExtentionToScanFor))
            {
                return "Error : No File Extention To Scan For";
            }

            var toScanFor = BuildSearchExtention(fileExtentionToScanFor);
            var files = Directory.GetFiles(directoryToScan, toScanFor);
            var result = new StringBuilder("<Methods>");

            foreach(var file in files)
            {

                var contents = File.ReadAllText(file);
                // we have a match folks ;)
                result.Append(ProcessFile(contents, testAnnotationSearchString));

            }

            result.Append("</Methods>");

            return result.ToString();
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

        private string ProcessFile(string contents, string testAnnotationSearchString)
        {
            var idx = -1;
            var result = new StringBuilder();
            var nameEnd = 0;

            while((idx = contents.IndexOf(testAnnotationSearchString, nameEnd)) >= 0)
            {
                var nameStart = contents.IndexOf(_signatureStart, idx);
                if(nameStart > 0)
                {
                    nameStart += _signatureStart.Length;
                    nameEnd = contents.IndexOf(_signatureEnd, nameStart);

                    // we got one ;)
                    if(nameEnd > nameStart)
                    {
                        var len = (nameEnd - nameStart);
                        var testName = contents.Substring(nameStart, len);

                        result.Append(BuildFragment(testName.Trim()));
                    }
                    else
                    {
                        // time to end it ;)
                        nameEnd = contents.Length - 1;
                    }
                }
            }

            return result.ToString();
        }

        private string BuildFragment(string val)
        {
            StringBuilder result = new StringBuilder();

            result.Append("<TestMethod>");
            result.Append(val);
            result.Append("</TestMethod>");

            return result.ToString();
        }
    }
}
