using Dev2.Common.Interfaces.DB;

namespace Warewolf.Studio.Views
{
    public class HelpTextValue:IDbHelpText
    {
        public HelpTextValue(string helpText)
        {
            HelpText = helpText;
        }

        #region Implementation of IDbHelpText

        public string HelpText { get; private set; }

        #endregion
    }
}