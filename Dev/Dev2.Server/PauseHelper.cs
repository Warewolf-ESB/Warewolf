using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2
{
    public interface IPauseHelper
    {
        void Pause();
    }

    public class PauseHelper : IPauseHelper
    {
        public void Pause()
        {
            Console.ReadLine();
        }
    }
}
