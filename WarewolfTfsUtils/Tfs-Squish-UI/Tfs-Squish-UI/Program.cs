using System;
using Tfs.Squish;

namespace Tfs_Squish_UI
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 3)
            {
                Console.WriteLine("Usage : tfs-squish [serverURI] [worksapceNamePerTFS] [filePath]");

            }else if (args.Length == 3)
            {
                var serverURI = args[0];
                var workspace = args[1];
                var fileName = args[2];

                TfsAnnotate tfsAnn = new TfsAnnotate(serverURI, workspace);

                tfsAnn.MyInvoke(fileName, false);    
            }
        }
    }
}
