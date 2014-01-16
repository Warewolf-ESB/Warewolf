
using System;

namespace ExtractCurrentTfsIteration
{
    class Program
    {
        static int Main(string[] args)
        {

            int preFix = 0;

            if(args.Length == 1)
            {
                Int32.TryParse(args[0], out preFix);
            }

            return new TfsTeamConfigurationExtractor().ExtractMajorMinorReleaseValue(preFix);
        }
    }
}
