using System;
using TestPackBuilder;

namespace TestPackBuilderExe
{
    public class Program
    {
        public static void Main(string[] args)
        {

            if(args.Length == 3)
            {
                var testScanner = new TestScanner();
                int total;
                Int32.TryParse(args[1], out total);
                testScanner.GenerateTestPacks(args[0], total, args[2]);
            }

        }
    }
}
