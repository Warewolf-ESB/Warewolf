
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;

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

            if(dl != null)
            {
                result = dl.ToString(SaveOptions.DisableFormatting);
            }

            return result;
        }

        /// <summary>
        /// Extracts the data list.
        /// </summary>
        /// <param name="serviceDef">The service def.</param>
        /// <returns></returns>
        public static string ExtractDataList(StringBuilder serviceDef)
        {
            string result = string.Empty;

            var xe = serviceDef.ToXElement();

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

            if(tmpB != null)
            {
                tmpA = tmpB.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionRootTag);
            }


            if(tmpA != null)
            {
                var dl = tmpA.Elements().FirstOrDefault(c => c.Name == GlobalConstants.OutputRootTag);

                if(dl != null)
                {
                    result = dl.ToString();
                }
            }

            return result;
        }

        public static string ExtractOutputMapping(StringBuilder serviceDef)
        {
            string result = string.Empty;

            var xe = serviceDef.ToXElement();

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

            if(tmpB != null)
            {
                tmpA = tmpB.Elements().FirstOrDefault(c => c.Name == GlobalConstants.ActionRootTag);
            }


            if(tmpA != null)
            {
                var dl = tmpA.Elements().FirstOrDefault(c => c.Name == GlobalConstants.InputRootTag);

                if(dl != null)
                {
                    result = dl.ToString();
                }
            }

            return result;
        }

        public static string ExtractInputMapping(StringBuilder serviceDef)
        {
            string result = string.Empty;

            var xe = serviceDef.ToXElement();

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


        public static bool MappingValuesChanged(IList<IDev2Definition> oldMappings, IList<IDev2Definition> newMappings)
        {
            return MappingChanged(oldMappings, newMappings, (oldMapping, newMapping) => oldMapping.Value == newMapping.Value);
        }

        public static bool MappingNamesChanged(IList<IDev2Definition> oldMappings, IList<IDev2Definition> newMappings)
        {
            return MappingChanged(oldMappings, newMappings, (oldMapping, newMapping) => oldMapping.Name == newMapping.Name);
        }


        static bool MappingChanged(ICollection<IDev2Definition> oldMappings, ICollection<IDev2Definition> newMappings, Func<IDev2Definition, IDev2Definition, bool> equals)
        {
            if(oldMappings == null || newMappings == null || oldMappings.Count != newMappings.Count)
            {
                return true;
            }

            return newMappings.Select(newMapping => oldMappings.FirstOrDefault(old => @equals(old, newMapping))).Any(oldMapping => oldMapping == null);
        }

    }

}
