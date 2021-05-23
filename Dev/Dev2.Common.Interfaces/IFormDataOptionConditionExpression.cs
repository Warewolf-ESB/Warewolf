﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



using Warewolf;
using Warewolf.Options;

namespace Dev2.Common.Interfaces
{
    public interface IFormDataOptionConditionExpression
    {
        enFormDataTableType TableType { get; set; }
        string FileBase64 { get; set; }
        string FileName { get; set; }
        string Value { get; set; }
        INamedInt SelectedTableType { get; set; }
    }
}
