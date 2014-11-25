
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Net;
using System.Xml.Linq;
using Microsoft.SqlServer.Server;

namespace Warewolf.Sql
{
    // PBI 8600

    /// <author>Trevor.Williams-Ros</author>
    /// <date>2013/02/21</date>
    public class Workflows
    {
        #region RunWorkflowForXml

        [SqlFunction]
        public static SqlXml RunWorkflowForXml(SqlString serverUri, SqlString rootName)
        {
            var xml = new Workflows().RunWorkflowForXmlImpl(
                serverUri.IsNull ? null : serverUri.Value,
                rootName.IsNull ? null : rootName.Value
                );
            return new SqlXml(xml.CreateReader());
        }

        #endregion

        #region RunWorkflowForSql

        [SqlProcedure]
        public static void RunWorkflowForSql(SqlString serverUri, SqlString recordsetName)
        {
            new Workflows().RunWorkflowForSqlImpl(new SqlCtx(),
                serverUri.IsNull ? null : serverUri.Value,
                recordsetName.IsNull ? null : recordsetName.Value
                );
        }

        #endregion

        #region RunWorkflowForXmlImpl

        public XElement RunWorkflowForXmlImpl(string serverUri, string rootName = null)
        {
            var xml = RunWorkflow(serverUri);
            if(!string.IsNullOrEmpty(rootName))
            {
                xml.Name = rootName;
            }
            return xml;
        }

        #endregion

        #region RunWorkflowForSqlImpl

        public DataTable RunWorkflowForSqlImpl(ISqlCtx sqlCtx, string serverUri, string recordsetName)
        {
            // if recordsetName is specified then select elements with matching name and convert children to DataTable rows
            // else iterate through XML and process each element as follows:
            // - if element has no children then add to first DataTable row
            // - else create DataTable for element and convert children to DataTable rows
            //   - when finished iterating, add DataTable's to first DataTable

            if(sqlCtx == null)
            {
                throw new ArgumentNullException("sqlCtx");
            }

            if(string.IsNullOrEmpty(serverUri))
            {
                return new DataTable();
            }

            var xml = RunWorkflowForXmlImpl(serverUri);
            var dt = new DataTable(recordsetName);

            if(!string.IsNullOrEmpty(recordsetName))
            {
                SqlDataRecord dataRecord = null;

                var hasRecords = false;
                foreach(var node in xml.Elements(recordsetName))
                {
                    hasRecords = true;
                    var record = AddRecord(dt, node);
                    if(dataRecord == null)
                    {
                        dataRecord = sqlCtx.SendStart(dt);
                    }
                    sqlCtx.SendRow(dataRecord, record.ItemArray);
                }
                if(hasRecords)
                {
                    sqlCtx.SendEnd();
                }
            }
            else
            {
                var values = new List<object>();
                var recordsets = new Dictionary<string, DataTable>();

                #region Parse values and recordsets

                foreach(var node in xml.Elements())
                {
                    var nodeName = node.Name.LocalName;
                    if(node.HasElements)
                    {
                        DataTable rs;
                        if(!recordsets.TryGetValue(nodeName, out rs))
                        {
                            rs = new DataTable(nodeName);
                            recordsets.Add(nodeName, rs);
                        }
                        AddRecord(rs, node);
                    }
                    else
                    {
                        var fieldName = nodeName;
                        AddColumn(dt, fieldName);
                        values.Add(node.Value);
                    }
                }

                #endregion

                dt.Rows.Add(values.ToArray());

                #region Now add rows and columns for each recordset

                foreach(var rsName in recordsets.Keys)
                {
                    var startCol = dt.Columns.Count;
                    var rs = recordsets[rsName];
                    foreach(DataColumn column in rs.Columns)
                    {
                        AddColumn(dt, rsName + column.ColumnName);
                    }
                    var endCol = dt.Columns.Count;

                    foreach(DataRow rsRow in rs.Rows)
                    {
                        var row = dt.NewRow();
                        for(int i = startCol, j = 0; i < endCol; i++, j++)
                        {
                            row[i] = rsRow.ItemArray[j];
                        }
                        dt.Rows.Add(row);
                    }
                }

                #endregion

                if(dt.Rows.Count > 0)
                {
                    var dataRecord = sqlCtx.SendStart(dt);
                    foreach(DataRow record in dt.Rows)
                    {
                        sqlCtx.SendRow(dataRecord, record.ItemArray);
                    }
                    sqlCtx.SendEnd();
                }
            }

            return dt;
        }

        #endregion

        #region AddColumn

        static void AddColumn(DataTable dataTable, string columnName)
        {
            var column = dataTable.Columns[columnName];
            if(column == null)
            {
                dataTable.Columns.Add(columnName, typeof(string));
            }
        }

        #endregion

        #region AddRecord

        static DataRow AddRecord(DataTable rs, XContainer rsXml)
        {
            var values = new List<object>();
            foreach(var fieldNode in rsXml.Elements())
            {
                AddColumn(rs, fieldNode.Name.LocalName);
                values.Add(fieldNode.Value);
            }
            return rs.Rows.Add(values.ToArray());
        }

        #endregion

        #region RunWorkflow

        /// <summary>
        /// Runs the workflow at the given request URI.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <returns>The XML payload of the workflow.</returns>
        public virtual XElement RunWorkflow(string requestUri)
        {
            if(string.IsNullOrEmpty(requestUri))
            {
                return new XElement("DataList");
            }

            var webClient = new WebClient { Credentials = CredentialCache.DefaultCredentials };
            var result = webClient.DownloadString(requestUri);
            return XElement.Parse(result);
        }

        #endregion
    }
}
