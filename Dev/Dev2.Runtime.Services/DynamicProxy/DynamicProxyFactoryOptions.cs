using System.Text;



namespace Dev2.Runtime.DynamicProxy
{
    public delegate string ProxyCodeModifier(string proxyCode);

    public class DynamicProxyFactoryOptions
    {
        public enum LanguageOptions { CS, VB }
        public enum FormatModeOptions { Auto, XmlSerializer, DataContractSerializer }

        private LanguageOptions lang;
        private FormatModeOptions mode;
        private ProxyCodeModifier codeModifier;

        public DynamicProxyFactoryOptions()
        {
            lang = LanguageOptions.CS;
            mode = FormatModeOptions.Auto;
            codeModifier = null;
        }

        public LanguageOptions Language
        {
            get
            {
                return lang;
            }

            set
            {
                lang = value;
            }
        }

        public FormatModeOptions FormatMode
        {
            get
            {
                return mode;
            }

            set
            {
                mode = value;
            }
        }

        // The code modifier allows the user of the dynamic proxy factory to modify 
        // the generated proxy code before it is compiled and used. This is useful in 
        // situations where the generated proxy has to be modified manually for interop 
        // reason.
        public ProxyCodeModifier CodeModifier
        {
            get
            {
                return codeModifier;
            }

            set
            {
                codeModifier = value;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DynamicProxyFactoryOptions[");
            sb.Append("Language=" + Language);
            sb.Append(",FormatMode=" + FormatMode);
            sb.Append(",CodeModifier=" + CodeModifier);
            sb.Append("]");

            return sb.ToString();
        }
    }
}
