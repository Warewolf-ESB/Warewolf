using System;
using Dev2.Runtime;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class ServicesMock : Dev2.Runtime.ServiceModel.Services
    {
        public int FetchRecordsetHitCount { get; set; }
        public bool FetchRecordsetAddFields { get; set; }

        public override Recordset FetchRecordset(DbService service, bool addFields)
        {
            FetchRecordsetHitCount++;
            FetchRecordsetAddFields = addFields;
            return service.Recordset;
        }

        public override RecordsetList FetchRecordset(PluginService service, bool addFields)
        {
            FetchRecordsetHitCount++;
            FetchRecordsetAddFields = addFields;
            service.Recordsets = new RecordsetList { service.Recordset };
            return service.Recordsets;
        }

        public override RecordsetList FetchRecordset(WebService service, bool addFields)
        {
            FetchRecordsetHitCount++;
            FetchRecordsetAddFields = addFields;
            //service.Recordsets = new RecordsetList();// { service.Recordset };
            return service.Recordsets;
        }

        public override ServiceMethodList FetchMethods(DbSource source)
        {
            var result = new ServiceMethodList();
            var random = new Random();
            for(var i = 0; i < 50; i++)
            {
                var method = new ServiceMethod { Name = string.Format("dbo.Pr_GetCake_{0:00}", i) };
                for(var j = 0; j < 10; j++)
                {
                    var varLength = j % 4 == 0 ? 30 : 15;
                    method.Parameters.Add(new MethodParameter { Name = random.GenerateString(varLength, "@") });
                }
                method.SourceCode = "ALTER procedure " + method.Name + "\n(\n\t@CakeName varchar(50)\n)\nas\n\nselect * from Country \nwhere [Description] like @Prefix + '%'\norder by Description asc";

                result.Add(method);
            }

            return result;
        }
    }
}
