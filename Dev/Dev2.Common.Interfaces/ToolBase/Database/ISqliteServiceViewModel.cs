#pragma warning disable
﻿using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces.ToolBase.Database
{
	public interface ISqliteServiceViewModel
	{
		IOutputsToolRegion OutputsRegion { get; set; }

		bool GenerateOutputsVisible { get; set; }

		ISqliteService ToModel();

		void ErrorMessage(Exception exception, bool hasError);

		void SetDisplayName(string displayName);
	}
}
