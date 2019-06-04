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
using System.Data;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Dev2.Activities.SqlBulkInsert
{
    public class MySqlBulkCopyWrapper : ISqlBulkCopy
    {
        #region Implementation of IDisposable

        bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        readonly MySqlBulkLoader _sbc;
        public MySqlBulkCopyWrapper(MySqlBulkLoader copyTool)
        {
            _sbc = copyTool;           
        }

        public bool WriteToServer(DataTable dt)
        {
            if(_sbc == null)
            {
                throw new ArgumentException("_sbc");
            }
            var filename = Path.GetTempFileName();
            try
            {         
                _sbc.Connection.Open();
           
                 WriteDataTable(dt, File.CreateText(filename));
                 _sbc.LineTerminator = Environment.NewLine;
                _sbc.FileName = filename;
                _sbc.Local = true;
                var res =_sbc.Load();         
                return res>0;
            }
            finally 
            {

               _sbc.Connection.Close();
                File.Delete(filename);
            }
        }

        public static void WriteDataTable(DataTable sourceTable, StreamWriter writer)
        {


            foreach (DataRow row in sourceTable.Rows)
            {
                var items = row.ItemArray.Select(o => QuoteValue(o.ToString())).ToArray();
                writer.WriteLine(String.Join(",", items));
            }

            writer.Flush();
            writer.Close();
        }

        static string QuoteValue(string value) => value;
    }
}