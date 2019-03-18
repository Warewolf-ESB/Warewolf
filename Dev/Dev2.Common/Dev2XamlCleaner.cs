#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Text;
using System.Text.RegularExpressions;

namespace Dev2.Common
{
    public class Dev2XamlCleaner
    {
        const string replacePrefix = "assembly=";

        public static readonly string[] badNamespaces =
        {
            /*
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:Unlimited.Framework;assembly=Dev2.Studio.Core""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:System;assembly=System.AddIn""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:Unlimited.Framework;assembly=Unlimited.Applications.BusinessDesignStudio""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:System;assembly=System.Reactive""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:Unlimited.Framework;assembly=Warewolf Studio""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Warewolf Studio""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:System;assembly=System.ComponentModel.Composition""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:Dev2.Studio.Core.Activities;assembly=Dev2.Studio.Core.Activities""",
                                                            */
            @"xmlns:[A-Za-z]+=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core""",
            @"<Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" />",
            @"<AssemblyReference>Dev2.Core</AssemblyReference>"
        };

        public static readonly string[,] replaceNamespaces =
        {
            {"Unlimited.Applications.BusinessDesignStudio.Activities", "Dev2.Activities"},
            {"Unlimited.Framework", "Dev2.Core"},
            {"Dev2.DataList.Contract", "Dev2.Data"},
            {"Unlimited.Appliations.BusinessDesignStudio", "Warewolf Studio"}
        };

        /// <summary>
        ///     Cleans the service def.
        /// </summary>
        /// <param name="def">The def.</param>
        /// <returns></returns>
        public StringBuilder CleanServiceDef(StringBuilder def)
        {
            var result = StripNaughtyNamespaces(def);

            result = ReplaceChangedNamespaces(result);

            return result;
        }


        /// <summary>
        ///     Replaces the changed namespaces.
        /// </summary>
        /// <param name="def">The def.</param>
        /// <returns></returns>
        StringBuilder ReplaceChangedNamespaces(StringBuilder def)
        {
            var result = def;
            for (int i = 0; i < replaceNamespaces.Length / 2; i++)
            {
                result = result.Replace(replacePrefix + replaceNamespaces[i, 0],
                    replacePrefix + replaceNamespaces[i, 1]);
            }

            return result;
        }

        /// <summary>
        ///     Strips the naughty namespaces.
        /// </summary>
        /// <param name="def">The def.</param>
        /// <returns></returns>
        public StringBuilder StripNaughtyNamespaces(StringBuilder def)
        {
            var result = def;
            foreach (string ns in badNamespaces)
            {
                // Have to make it a string for Regex ;(
                var defStr = def.ToString();
                var m = Regex.Match(defStr, ns);
                if (m.Success)
                {
                    // we have a hit ;)
                    // search backward for the start xmlns: ...
                    for (int i = 0; i < m.Groups.Count; i++)
                    {
                        var val = m.Groups[i].Value;
                        result = result.Replace(val, string.Empty);
                    }
                }
            }

            return result;
        }
    }
}