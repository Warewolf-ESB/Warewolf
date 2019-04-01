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

using System.Text;
using System.Xml;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Sql;
using Unlimited.Framework.Converters.Graph;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
	public class SqliteDatabaseBroker : AbstractDatabaseBroker<SqliteServer>
	{
		protected override string NormalizeXmlPayload(string payload)
		{
			var result = new StringBuilder();

			var xDoc = new XmlDocument();
			xDoc.LoadXml(payload);
			var nl = xDoc.SelectNodes("//NewDataSet/Table/*[starts-with(local-name(),'XML_')]");
			var foundXMLFrags = 0;

			if (nl != null)
			{
				foreach (XmlNode n in nl)
				{
					var tmp = n.InnerXml;
					result = result.Append(tmp);
					foundXMLFrags++;
				}
			}

			var res = result.ToString();

			if (foundXMLFrags >= 1)
			{
				res = "<FromXMLPayloads>" + res + "</FromXMLPayloads>";
			}
			else
			{
				if (foundXMLFrags == 0)
				{
					res = payload;
				}
			}

			return base.NormalizeXmlPayload(res);
		}

		public override IOutputDescription TestSqliteService(SqliteDBService dbService)
		{
			VerifyArgument.IsNotNull("SqliteDBService", dbService);
			VerifyArgument.IsNotNull("SqliteDBService.Source", dbService.Source);

			IOutputDescription result;
			using (var server = CreateSqliteDbServer(dbService.Source as SqliteDBSource))
			{
				server.Connect(((SqliteDBSource)dbService.Source).ConnectionString);
				server.BeginTransaction();
				try
				{
					//
					// Execute command and normalize XML
					//
					var command = server.CreateCommand();
					command.CommandText = dbService.Method.QueryString;
					var dataTable = server.FetchDataTable(command);

					//
					// Map shape of XML
					//

					result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
					var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
					result.DataSourceShapes.Add(dataSourceShape);

					var dataBrowser = DataBrowserFactory.CreateDataBrowser();
					dataSourceShape.Paths.AddRange(dataBrowser.Map(dataTable));
				}
				finally
				{
					server.RollbackTransaction();
				}
			}

			return result;
		}
	}
}
