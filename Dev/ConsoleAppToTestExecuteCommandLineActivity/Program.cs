using System;

namespace ConsoleAppToTestExecuteCommandLineActivity
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                return;
            }
            var switchArg = args[0];
            switch(switchArg)
            {
                case "user":
                    Console.Read();
                    return;
                case "output":
                    Console.WriteLine("This is output from the user");
                    return;
                case "differentoutput":
                    Console.WriteLine("This is a different output from the user");
                    return;
                case "error":
                    Console.WriteLine("This is error");
                    Console.Error.Write("The console errored.");
                    return;
            }
        }
    }
}
