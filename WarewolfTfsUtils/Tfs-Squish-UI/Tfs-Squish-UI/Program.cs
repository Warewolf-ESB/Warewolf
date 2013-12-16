using System;

namespace Tfs.Squish
{
   
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 2)
            {
                Console.WriteLine("Usage : tfs-squish [serverURI] [filePath]");
                foreach (var s in args)
                {
                    Console.WriteLine("Arg-> " + s);
                }

            }else if (args.Length == 2)
            {
                var serverURI = args[0];
                var fileName = args[1];
                TfsAnnotate tfsAnn = new TfsAnnotate(serverURI);

                tfsAnn.FetchAnnotateInfo(fileName, false);    
            }
        }
    }
}
