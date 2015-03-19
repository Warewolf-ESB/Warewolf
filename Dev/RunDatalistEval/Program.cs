

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using WarewolfParserInterop;

namespace RunDatalistEval
{
    class Program
    {
        static void Main(string[] args)
        {
            var env = new Warewolf.Storage.ExecutionEnvironment();
            while (true)
            {
                for (int i = 0; i < 20000; i++)
                {

                    env.MultiAssign(new List<IAssignValue>
                    {
                        new AssignValue("[[a]]","aa"),
                        new AssignValue("[[rec().a]]","25"),
                        new AssignValue("[[rec().a]]","the quick brown fox"),

                    });
                }
                Console.WriteLine("finished");
                Console.ReadLine();
            }
        }
    }
}
