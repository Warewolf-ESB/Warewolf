using System;
using System.Globalization;
using System.Threading;

namespace Test.Localisation
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-gb");
            Console.WriteLine(Warewolf.Studio.Resources.Languages.Core.ServerNotConnected);
            Console.ReadLine();
        }
    }
}
