/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Warewolf.Options;

namespace Warewolf.Data.Decisions.Operations
{
    public class IsNotXml : IDecisionOperation
    {
        public Enum HandlesType() => enDecisionType.IsNotXML;

        public bool Invoke(string[] cols)
        {
            var data = DataListUtilBase.AdjustForEncodingIssues(cols[0]);
            var isXml = DataListUtilBase.IsXml(data, out bool isFragment);
            return !(isXml || isFragment);
        }
    }
}
