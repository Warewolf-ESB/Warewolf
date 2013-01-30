using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Unlimited.Framework.Converters.Graph
{
    public class DataBrowserFactory
    {
        #region Methods

        public static IDataBrowser CreateDataBrowser()
        {
            return new DataBrowser();
        }

        #endregion Methods
    }
}
