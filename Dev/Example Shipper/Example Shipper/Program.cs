
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.IO;
using System.Text;

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

                    // set the server ID
                    if (!data.Contains("ServerID=\"51A58300-7E9D-4927-A57B-E5D700B11B55\""))
                    {
                        StringBuilder buildNewServerID = new StringBuilder();
                        buildNewServerID.Append(data.Substring(0, data.IndexOf("ServerID=\"") + "ServerID=\"".Length));
                        buildNewServerID.Append("51A58300-7E9D-4927-A57B-E5D700B11B55");
                        int restOfDefinitionStartIndex = data.IndexOf("ServerID=\"") + "ServerID=\"".Length + Guid.Empty.ToString().Length;
                        buildNewServerID.Append(data.Substring(restOfDefinitionStartIndex, data.Length - restOfDefinitionStartIndex));
                        data = buildNewServerID.ToString();
                    }

                    if (data.Contains("<Signature"))
                    {
                        // remove the signature ;)
                        var buildWithoutSignature = new StringBuilder();
                        buildWithoutSignature.Append(data.Substring(0, data.IndexOf("<Signature", StringComparison.Ordinal)));

                        if (data.Contains("</Source>"))
                        {
                            buildWithoutSignature.Append("</Source>");
                        }
                        else if (data.Contains("</Service>"))
                        {
                            buildWithoutSignature.Append("</Service>");
                        }

                        data = buildWithoutSignature.ToString();
                    }

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
