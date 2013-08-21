using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.DataList.Contract;

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
                result = dl.ToString(SaveOptions.DisableFormatting);
            }

            return result;
        }

        public static string ExtractOutputMapping(string serviceDef)
        {
            string result = string.Empty;
            XElement xe = XElement.Parse(serviceDef);

            // could have service as its root ;(
            var tmpB = xe.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionsRootTag);

            var tmpA = xe.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionRootTag);

            if (tmpB != null)
            {
                tmpA = tmpB.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionRootTag);
            }


            if (tmpA != null)
            {
                var dl = tmpA.Elements().FirstOrDefault(c => c.Name == GlobalConstants.OutputRootTag);

                if (dl != null)
                {
                    result = dl.ToString();
                }
            }

            return result;
        }

        public static string ExtractInputMapping(string serviceDef)
        {
            string result = string.Empty;
            XElement xe = XElement.Parse(serviceDef);

            // could have service as its root ;(
            var tmpB = xe.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionsRootTag);

            var tmpA = xe.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionRootTag);

            if (tmpB != null)
            {
                tmpA = tmpB.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionRootTag);
            }


            if (tmpA != null)
            {
                var dl = tmpA.Elements().FirstOrDefault(c => c.Name == GlobalConstants.InputRootTag);

                if (dl != null)
                {
                    result = dl.ToString();
                }
            }

            return result;
        }

        
        public static bool MappingsChanged(IList<IDev2Definition> oldMappings, IList<IDev2Definition> newMappings)
        {
            var mappingChanged = false;
            newMappings.ToList().ForEach(definition =>
            {
                if(oldMappings.ToList().Find(dev2Definition => dev2Definition.Value == definition.Value) == null)
                {
                    mappingChanged = true;
                }
            });
            return mappingChanged;
        }
    }
    
}
