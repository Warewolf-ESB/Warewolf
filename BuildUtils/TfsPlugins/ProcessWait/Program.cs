using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ProcessWait
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                
            }
        }

        static int Wait(string pid)
        {

            int id = Int32.Parse(pid);

            var proc = Process.GetProcessById(id);

            while(proc.cou)
            

            if (proc != null)
            {
                
            }


            
        }
    }
}
