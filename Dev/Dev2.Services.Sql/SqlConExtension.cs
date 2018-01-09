using System;
using System.Data;
using Dev2.Common;

namespace Dev2.Services.Sql
{
    public static class SqlConExtension
    {
        public static void TryOpen(this ISqlConnection connection)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
            }
        }

        
    }
}