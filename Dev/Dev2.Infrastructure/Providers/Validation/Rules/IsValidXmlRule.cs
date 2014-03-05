using Dev2.Providers.Errors;
using System;
using System.Xml;

namespace Dev2.Providers.Validation.Rules
{
    public class IsValidXmlRule : Rule<string>
    {
        public IsValidXmlRule(Func<string> getValue)
            : base(getValue)
        {
            ErrorText = "is not a valid expression";
        }

        public override IActionableErrorInfo Check()
        {
            var value = GetValue();
            bool isValid;

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(value);
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