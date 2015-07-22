

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common.Interfaces;
using WarewolfParserInterop;

namespace RunDatalistEval
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var env = new Warewolf.Storage.ExecutionEnvironment();
            while (true)
            {
                for (int i = 0; i < 20000; i++)
                {

                    env.MultiAssign(new List<IAssignValue>
                    {
                        new AssignValue("[[a]]","aa"),
                        new AssignValue("[[rec().a]]","25"),
                        new AssignValue("[[rec().b]]","the quick brown fox"),

                    });
                }
                var x = st.ElapsedMilliseconds; 
                Console.WriteLine("finished");
                Console.ReadLine();
            }
        }
    }
}
