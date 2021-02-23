/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Text;
using Warewolf.Options;

namespace Dev2.Common.Interfaces
{
    public interface IFormDataCondition
    {
        enFormDataTableType TableType { get; set; }

        IEnumerable<IFormDataParameters> Eval(string left, Func<string, string, string, IEnumerable<string[]>> getArgumentsFunc, bool hasError);
        void SetOptions(IFormDataOptionConditionExpression option);

        void RenderDescription(StringBuilder sb);
    }
}
