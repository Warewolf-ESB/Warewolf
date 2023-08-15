#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using System;
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

            result = StripViewState(result);

            return result;
        }

        StringBuilder StripViewState(StringBuilder def)
        {
            var startViewStateTag = "<sap:WorkflowViewStateService.ViewState>";
			var endViewStateTag = "</sap:WorkflowViewStateService.ViewState>";
			var findViewStateStart = IndexOf(def, startViewStateTag);
			int findViewStateEnd = IndexOf(def, endViewStateTag);
			while (findViewStateStart >= 0 && findViewStateEnd >= 0)
            {
				def.Remove(findViewStateStart, findViewStateEnd - findViewStateStart + endViewStateTag.Length);
				findViewStateStart = IndexOf(def, startViewStateTag);
				findViewStateEnd = IndexOf(def, endViewStateTag);
			}
            return def;
        }

		int IndexOf(StringBuilder sb, string value)
		{
			int index;
			int length = value.Length;
			int maxSearchLength = (sb.Length - length) + 1;

			for (int i = 0; i < maxSearchLength; ++i)
			{
				if (sb[i] == value[0])
				{
					index = 1;
					while ((index < length) && (sb[i + index] == value[index]))
						++index;

					if (index == length)
						return i;
				}
			}

			return -1;
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