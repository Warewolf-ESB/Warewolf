using System.Linq;
using System.Xml.Linq;
using Dev2.Common;

namespace Dev2.Data.ServiceModel.Helper
{
    /// <summary>
    /// Helper methods to extract a datalist from service method
    /// </summary>
    public static class ServiceUtils
    {

        /// <summary>
        /// Extracts the data list.
        /// </summary>
        /// <param name="serviceDef">The service def.</param>
        /// <returns></returns>
        public static string ExtractDataList(string serviceDef)
        {
            string result = string.Empty;
            XElement xe = XElement.Parse(serviceDef);

            var dl = xe.Elements().FirstOrDefault(c => c.Name == GlobalConstants.DataListRootTag);

            if (dl != null)
            {
                result = dl.ToString(SaveOptions.None);
            }

            return result;
        }
    }
}
