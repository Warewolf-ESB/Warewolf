using System.Text;

namespace Dev2.Util
{
    /// <summary>
    /// Used to clean the XAML def upon changes
    /// </summary>
    public class Dev2XamlCleaner
    {

        public static readonly string[] badNamespaces = {
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:Unlimited.Framework;assembly=Dev2.Studio.Core""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:System;assembly=System.AddIn""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:Unlimited.Framework;assembly=Unlimited.Applications.BusinessDesignStudio""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:System;assembly=System.Reactive""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:Unlimited.Framework;assembly=Warewolf Studio""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Warewolf Studio""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:System;assembly=System.ComponentModel.Composition""",
                                                            @"xmlns:[A-Za-z0-9]+=""clr-namespace:Dev2.Studio.Core.Activities;assembly=Dev2.Studio.Core.Activities"""
        };

        const string replacePrefix = "assembly=";

        public static readonly string[,] replaceNamespaces = { 
                                                                { "Unlimited.Applications.BusinessDesignStudio.Activities","Dev2.Activities" },
                                                                { "Unlimited.Framework","Dev2.Core" },
                                                                { "Dev2.DataList.Contract","Dev2.Data" },
                                                                { "Unlimited.Appliations.BusinessDesignStudio","Warewolf Studio" }
                                                             };

        /// <summary>
        /// Cleans the service def.
        /// </summary>
        /// <param name="def">The def.</param>
        /// <returns></returns>
        public StringBuilder CleanServiceDef(StringBuilder def)
        {
            //var result = StripNaughtyNamespaces(def);

            var result = def;

            result = ReplaceChangedNamespaces(result);

            return result;
        }


        /// <summary>
        /// Replaces the changed namespaces.
        /// </summary>
        /// <param name="def">The def.</param>
        /// <returns></returns>
        private StringBuilder ReplaceChangedNamespaces(StringBuilder def)
        {
            var result = def;
            for(int i = 0; i < (replaceNamespaces.Length / 2); i++)
            {
                result = result.Replace((replacePrefix + replaceNamespaces[i, 0]), (replacePrefix + replaceNamespaces[i, 1]));
            }

            return result;
        }
    }
}
