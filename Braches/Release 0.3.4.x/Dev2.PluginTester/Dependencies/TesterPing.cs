using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginTesterDependency.Echo;

namespace Dev2.Dependencies
{
    public class TesterPing
    {

        public string Ping(string text)
        {
            return new ExternalPing().Pong(text);
        }
    }
}
