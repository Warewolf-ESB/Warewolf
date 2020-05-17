#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.SystemTemplates.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dev2.Data.Decisions
{
  public class DecisionExpression
  {
    public Dev2Decision Expression { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public OperandType LogicalOperandType { get; set; }
    public DecisionExpression Chain { get; set; }
  }
}