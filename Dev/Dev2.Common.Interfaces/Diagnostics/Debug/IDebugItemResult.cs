/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Xml.Serialization;

namespace Dev2.Common.Interfaces.Diagnostics.Debug
{
    public interface IDebugItemResult : IXmlSerializable
    {
        DebugItemResultType Type { get; set; }
        string Label { get; set; }
        string Operator { get; set; }
        string Variable { get; set; }
        string Value { get; set; }
        string GroupName { get; set; }
        int GroupIndex { get; set; }
        string MoreLink { get; set; }
        bool HasError { get; set; }
        bool TestStepHasError { get; set; }
        bool MockSelected { get; set; }

        string GetMoreLinkItem();
    }
}