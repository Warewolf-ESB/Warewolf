namespace PluginTesterDependency.Echo
{
    public class ExternalPing
    {

        public string Pong(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                return "Pong [ " + text + " ]";    
            }

            return "Pong [ Null or Empty String ]";
        }
    }
}
