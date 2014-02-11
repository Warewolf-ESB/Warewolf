
using System;

namespace ExtractCurrentTfsIteration
{
    class Program
    {
        static void Main(string[] args)
        {

            int preFix = 0;

            if(args.Length == 1)
            {
                Int32.TryParse(args[0], out preFix);
            }

            var result =  new TfsTeamConfigurationExtractor().ExtractMajorMinorReleaseValue(preFix);

            Console.Write(result);
        }
    }
}
