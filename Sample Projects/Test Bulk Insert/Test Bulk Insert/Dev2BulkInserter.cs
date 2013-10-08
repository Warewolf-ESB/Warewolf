using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace Test_Bulk_Insert
{
    public class Dev2BulkInserter
    {

        public void TestBulkInsert()
        {
            string conStr = "server=RSAKLFSVRGENDEV;database=DemoDB;integrated security=true;";
            
            using (SqlConnection con = new SqlConnection(conStr))
            {
                SqlBulkCopy sbc = new SqlBulkCopy(con, SqlBulkCopyOptions.CheckConstraints, null);
                sbc.DestinationTableName = "People";
                sbc.BatchSize = 10;

                con.Open();

                try
                {

                    DataTable tmpTbl = new DataTable("People");
                    tmpTbl.Columns.Add("ID");
                    tmpTbl.Columns.Add("Name");
                    
                    for (int i = 0; i < 1000; i++)
                    {
                        var myRow = tmpTbl.NewRow();
                        myRow["ID"] = i;
                        if (i%5 == 1)
                        {
                            myRow["Name"] = Guid.NewGuid() + " " + Guid.NewGuid() + " " + Guid.NewGuid() + " " +
                                            Guid.NewGuid() + " " + Guid.NewGuid() + " " + Guid.NewGuid() + " " +
                                            Guid.NewGuid() + " " + Guid.NewGuid() + " " + Guid.NewGuid() + " END ";
                        }
                        else
                        {
                            myRow["Name"] = Guid.NewGuid();    
                        }
 
                    }

                    sbc.SqlRowsCopied += (sender, args) =>
                        {
                            var rowsMoved = args.RowsCopied;

                            rowsMoved += 0;
                        };

                    sbc.WriteToServer(tmpTbl);
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }

            }
        }
    }
}
