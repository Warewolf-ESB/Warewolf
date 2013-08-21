using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;

namespace Dev2.Diagnostics
{
    public class DoNothingListener : TraceListener
    {
        public override void Fail(string msg, string detailedMsg)
        {
            StudioLogger.LogMessage(msg + detailedMsg);
            // log the message (don't display a MessageBox)
        }

        public override void Write(string message)
        {

        }

        public override void WriteLine(string message)
        {
        }
    }
}
