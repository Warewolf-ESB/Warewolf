/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Common;
using Dev2.Data.SystemTemplates.Models;

// ReSharper disable once CheckNamespace
namespace Dev2.Webs
{
    public static class FlowNodeHelper
    {
        internal static string CleanModelData(string callBackHandler)
        {
            // Remove naughty chars...
            string tmp = callBackHandler;
            // remove the silly Choose... from the string
            tmp = Dev2DecisionStack.RemoveDummyOptionsFromModel(tmp.ToStringBuilder());
            // remove [[]], &, !
            tmp = Dev2DecisionStack.RemoveNaughtyCharsFromModel(tmp);
            return tmp;
        }
    }
}
