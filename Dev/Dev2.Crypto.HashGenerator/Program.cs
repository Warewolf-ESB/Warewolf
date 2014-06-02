using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Dev2.Common.Utils;

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

                        fileName = directory.FullName+"\\"+ args[1];
                        Console.WriteLine("output file name provided");
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
