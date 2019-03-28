#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Common.Interfaces.DB
{
	public interface ISqliteService
	{
		string Name { get; set; }
		string Path { get; set; }
		ISqliteDBSource Source { get; set; }
		IList<IServiceInput> Inputs { get; set; }
		IList<IServiceOutputMapping> OutputMappings { get; set; }
		Guid Id { get; set; }
	}
}
