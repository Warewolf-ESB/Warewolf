#pragma warning disable
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
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Resources;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Warewolf.Security.Encryption;

namespace Dev2.Runtime.ServiceModel.Data
{
	public class SqliteDBSource : Resource, IResourceSource, ISqliteDB, IAuditingSource
	{
		#region CTOR

		public SqliteDBSource()
		{
			ResourceType = "DbSource";
			ServerType = enSourceType.SQLiteDatabase;
		}

		#endregion

		#region Properties

		[JsonConverter(typeof(StringEnumConverter))]
		public enSourceType ServerType { get; set; }

		public string DatabaseName { get; set; }
		public string QueryString { get; set; }
		public enSourceType Type { get; set; }
		public Guid Id { get; set; }
		#endregion

		#region ConnectionString

		public string ConnectionString => ":memory:";

	

		public bool Equals(ISqliteDBSource other)
		{
			throw new NotImplementedException();
		}

		#endregion


	}
}
