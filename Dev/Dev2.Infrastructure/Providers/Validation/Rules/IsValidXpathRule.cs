using Dev2.Providers.Errors;
using System;
using System.Xml;
using System.Xml.XPath;

namespace Dev2.Providers.Validation.Rules
{
    public class IsValidXpathRule : Rule<string>
    {
        public IsValidXpathRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "is not a valid expression";
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            bool isValid;

            XmlDocument doc = new XmlDocument();
            XPathNavigator nav = doc.CreateNavigator();
            try
            {
                nav.Compile(value);
                isValid = true;
            }
            catch(Exception)
            {
                isValid = false;
            }
            return !isValid ? CreatError() : null;
        }
    }
}