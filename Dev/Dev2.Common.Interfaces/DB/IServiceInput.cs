/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Warewolf.Data;

namespace Dev2.Common.Interfaces.DB
{
    public interface IServiceInput : IServiceInputBase
    {
        bool RequiredField { get; set; }
        bool EmptyIsNull { get; set; }
        string TypeName { get; set; }
        enIntellisensePartType IntellisenseFilter { get; set; }
        bool IsObject { get; set; }
        string Dev2ReturnType { get; set; }
        string ShortTypeName { get; set; }
        string FullName { get; }
        string ActionName { get; set; }
    }
}
