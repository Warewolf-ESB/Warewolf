
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Dev2.Common.Interfaces.Utils;

namespace Dev2.Crypto.HashGenerator
{
    class Program
    {
        static void Main(string[] args)
        {

            string filename = args[0];
            if (string.IsNullOrEmpty(filename))
            {
                Console.WriteLine("filename param missing");
                return;
            }
            if (File.Exists(filename))
            {
                CryptoProvider crypt = new CryptoProvider(new SHA256CryptoServiceProvider());
                FileInfo f = new FileInfo(filename);
                var directory = f.Directory;
                
                if(directory != null)
                {
                    var fileName = f.FullName + ".hash";
                    if(args.Length == 2)
                    {

                        fileName =  args[1];
                        Console.WriteLine("output file name provided" +args[1]);
                    }
                    else
                    {
                        Console.WriteLine("output file name generated as" + fileName);
                    }
                    var sb = new StringBuilder();   
                    var hash = crypt.ComputeHash(File.Open(filename, FileMode.Open));
                    foreach (var b in hash)
                    {
                        sb.Append(b);
                    }
                   
                    File.WriteAllText(fileName,sb.ToString());
                    
                }
                
            }
            else
            {
                Console.WriteLine("File does not exist");
            }
        }
    }
}
