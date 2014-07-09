using System;
using System.Data;
using System.Linq;
using System.Text;

namespace Tu.Extensions
{
    public static class DataTableUtils
    {
        public static string ToCsv(this DataTable dt, string separator, string filter, string[] columnsToIgnore)
        {
            if(dt == null)
            {
                throw new ArgumentNullException("dt");
            }

            var result = new StringBuilder();


            if(!string.IsNullOrEmpty(filter))
            {
                var rows = dt.Select(string.Format("NOT ({0})", filter));
                foreach(var row in rows)
                {
                    dt.Rows.Remove(row);
                }
            }

            if(columnsToIgnore != null && columnsToIgnore.Length > 0)
            {
                foreach(var columnName in columnsToIgnore)
                {
                    dt.Columns.Remove(columnName);
                }
            }

            var headers = from c in dt.Columns.Cast<DataColumn>()
                          select c.ColumnName;

            result.AppendLine(string.Join(separator, headers));

            foreach(DataRow row in dt.Rows)
            {
                result.AppendLine(string.Join("|", row.ItemArray));
            }
            return result.ToString();
        }

        public static DataTable ToDataTable(this string csv, string separator)
        {
            var dt = new DataTable();
            var lines = csv.ToLines();

            for(var i = 0; i < lines.Count; i++)
            {
                var items = lines[i].ToFields(separator);
                if(i == 0)
                {
                    foreach(var item in items)
                    {
                        dt.Columns.Add(item, typeof(string));
                    }
                }
                else
                {
                    var dr = dt.NewRow();
                    for(var j = 0; j < items.Count; j++)
                    {
                        if(j < dt.Columns.Count)
                        {
                            var value = items[j];
                            if(!string.IsNullOrWhiteSpace(value))
                            {
                                dr[j] = value;
                            }
                        }
                    }
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }
    }
}
