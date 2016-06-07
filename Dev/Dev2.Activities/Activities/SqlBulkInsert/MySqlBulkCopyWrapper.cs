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

        private readonly MySqlBulkLoader _sbc;
        public MySqlBulkCopyWrapper(MySqlBulkLoader copyTool)
        {
            _sbc = copyTool;
           
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if(_sbc != null)
            {
             //   _sbc.();
            }
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
                string[] items = row.ItemArray.Select(o => QuoteValue(o.ToString())).ToArray();
                writer.WriteLine(String.Join(",", items));
            }

            writer.Flush();
            writer.Close();
        }

        private static string QuoteValue(string value)
        {
            return value;
        }

        #endregion
    }
}