using System;
using System.IO;

namespace Example_Shipper
{
    /// <summary>
    /// Used to prep example workflows for shipping ;)
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                var inputDir = args[0];
                var outputDir = args[1];

                var dirs = Directory.GetFiles(inputDir);

                foreach (var dir in dirs)
                {
                    var theFile = Path.GetFileName(dir);
                    var data = File.ReadAllText(dir);

                    // set type to Unknown ;)
                    data = data.Replace("ResourceType=\"WorkflowService\"", "ResourceType=\"Unknown\"");

                    // remove the signature ;)
                    var idx = data.IndexOf("</Action>", StringComparison.Ordinal);

                    data = data.Substring(0, (idx + 9));

                    data += "</Service>";

                    if (theFile != null)
                    {
                        var outputFile = Path.Combine(outputDir, theFile);
                        File.WriteAllText(outputFile, data);
                    }
                    else
                    {
                        Console.WriteLine("Could not process path [ " + dir + " ]");
                    }
                }
            }
            else
            {
                Console.WriteLine("ERROR : You need to specify input and output directories");
            }
        }
    }
}
