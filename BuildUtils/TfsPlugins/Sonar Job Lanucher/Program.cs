using System.Net;

namespace Sonar_Job_Lanucher
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 1)
            {
                WebRequest wr = WebRequest.Create(args[0]);
                wr.Timeout = int.MaxValue;
                wr.GetResponse();
            }
        }
    }
}
