
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Webs.Callbacks;

// ReSharper disable once CheckNamespace
namespace Dev2.Webs
{
    public static class WebHelper
    {
        internal static string CleanModelData(Dev2DecisionCallbackHandler callBackHandler)
        {
            // Remove naughty chars...
            string tmp = callBackHandler.ModelData;
            // remove the silly Choose... from the string
            tmp = Dev2DecisionStack.RemoveDummyOptionsFromModel(tmp.ToStringBuilder());
            // remove [[]], &, !
            tmp = Dev2DecisionStack.RemoveNaughtyCharsFromModel(tmp);
            return tmp;
        }
    }
}
