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
