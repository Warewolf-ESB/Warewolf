//import { LogEntry } from './logentry.model';

//describe('LogEntry', () => {

//  it('should create an instance of LogEntry', () => {
//    expect(new LogEntry()).toBeTruthy();
//  });

//  it('should accept values', () => {
//    let log = new LogEntry();
//    log = {
//      WorkflowID: "15481eaf-0004-49f0-b70b-afadb76cf29d",
//      WorkflowName: "Login",
//      ExecutionID: "b154b8d6-e08c-4f24-a75f-0e67ad0b78cb",
//      AuditType: "LogPreExecuteState",
//      PreviousActivity: "SQL Server Database",
//      PreviousActivityType: "Dev2.Activities.DsfSqlServerDatabaseActivity",
//      PreviousActivityID: "00000000-0000-0000-0000-000000000000",
//      NextActivity: "SQL Server Database",
//      NextActivityType: "Dev2.Activities.DsfSqlServerDatabaseActivity",
//      NextActivityID: "00000000-0000-0000-0000-000000000000",
//      ServerID: "51a58300-7e9d-4927-a57b-e5d700b11b55",
//      ParentID: "00000000-0000-0000-0000-000000000000",
//      ClientID: "",
//      ExecutingUser: "System.Security.Principal.WindowsPrincipal",
//      ExecutionOrigin: 2,
//      ExecutionOriginDescription: "",
//      ExecutionToken: '{"$id":"1","$type":"Dev2.Runtime.Execution.ExecutionToken, Dev2.Runtime.Services","IsUserCanceled":false}',
//      AdditionalDetail: '{"$id":"1","$type":"<>f__AnonymousType0`5[[Dev2.Runtime.ServiceModel.Data.DbSource, Dev2.Runtime.Services],[System.String, mscorlib],[System.String, mscorlib],[System.Int32, mscorlib],[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]], Dev2.Services.Execution","Source":{"$id":"2","$type":"Dev2.Runtime.ServiceModel.Data.DbSource, Dev2.Runtime.Services","ServerType":"SqlDatabase","Server":"mssqlcryptotrader.database.windows.net","DatabaseName":"EllidexDB","Port":0,"ConnectionTimeout":30,"AuthenticationType":"User","UserID":"cryptotrader","Password":"T3st!234","DataList":"","ConnectionString":"Data Source=mssqlcryptotrader.database.windows.net;Initial Catalog=EllidexDB;User ID=cryptotrader;Password=T3st!234;;Connection Timeout=30","IsSource":true,"IsService":false,"IsFolder":false,"IsReservedService":false,"IsServer":false,"IsResourceVersion":false,"Version":null,"ResourceID":"0a6dfc8e-64ef-4f89-a9dd-27d7daa7180a","ResourceType":"SqlDatabase","ResourceName":"EllidexDB","IsValid":false,"Errors":[],"ReloadActions":false,"UserPermissions":0,"VersionInfo":null},"ProcedureName":"dbo.Warewolf_Login","SqlQuery":null,"ConnectionTimeout":0,"CommandTimeout":null}',
//      IsSubExecution: 0,
//      IsRemoteWorkflow: 0,
//      Environment: '{"Environment":{"scalars":{"val":"Hello"},"record_sets":{"recset1":{"WarewolfPositionColumn":[1,2,3],"field1":["Bob","Jane","Jill"],"field2":[2,3,1999],"field3":["C","G","Z"],"field4":[21.2,26.4,60]}},"json_objects":{}},"Errors":[],"AllErrors":[]}',
//      AuditDate: "2018/10/12 02:48:27 PM",
//    }
//    expect(log.WorkflowID).toEqual("15481eaf-0004-49f0-b70b-afadb76cf29d");
//    expect(log.WorkflowName).toEqual("Login");
//    expect(log.ExecutionID).toEqual('b154b8d6-e08c-4f24-a75f-0e67ad0b78cb');
//    expect(log.AuditType).toEqual("LogPreExecuteState");
//    expect(log.PreviousActivity).toEqual("SQL Server Database");
//    expect(log.PreviousActivityType).toEqual('Dev2.Activities.DsfSqlServerDatabaseActivity');
//    expect(log.PreviousActivityID).toEqual("00000000-0000-0000-0000-000000000000");
//    expect(log.NextActivity).toEqual("SQL Server Database");
//    expect(log.NextActivityID).toEqual('00000000-0000-0000-0000-000000000000');
//    expect(log.NextActivityType).toEqual("Dev2.Activities.DsfSqlServerDatabaseActivity");
//    expect(log.ServerID).toEqual("51a58300-7e9d-4927-a57b-e5d700b11b55");
//    expect(log.ParentID).toEqual('00000000-0000-0000-0000-000000000000');
//    expect(log.ClientID).toEqual("");
//    expect(log.ExecutingUser).toEqual("System.Security.Principal.WindowsPrincipal");
//    expect(log.ExecutionOrigin).toEqual(2);
//    expect(log.IsRemoteWorkflow).toEqual(0);
//    expect(log.Environment).toEqual('{"Environment":{"scalars":{"val":"Hello"},"record_sets":{"recset1":{"WarewolfPositionColumn":[1,2,3],"field1":["Bob","Jane","Jill"],"field2":[2,3,1999],"field3":["C","G","Z"],"field4":[21.2,26.4,60]}},"json_objects":{}},"Errors":[],"AllErrors":[]}');
//    expect(log.AuditDate).toEqual('2018/10/12 02:48:27 PM');
//    expect(log.ExecutionOriginDescription).toEqual('');
//    expect(log.ExecutionToken).toEqual('{"$id":"1","$type":"Dev2.Runtime.Execution.ExecutionToken, Dev2.Runtime.Services","IsUserCanceled":false}');
//    expect(log.AdditionalDetail).toEqual('{"$id":"1","$type":"<>f__AnonymousType0`5[[Dev2.Runtime.ServiceModel.Data.DbSource, Dev2.Runtime.Services],[System.String, mscorlib],[System.String, mscorlib],[System.Int32, mscorlib],[System.Nullable`1[[System.Int32, mscorlib]], mscorlib]], Dev2.Services.Execution","Source":{"$id":"2","$type":"Dev2.Runtime.ServiceModel.Data.DbSource, Dev2.Runtime.Services","ServerType":"SqlDatabase","Server":"mssqlcryptotrader.database.windows.net","DatabaseName":"EllidexDB","Port":0,"ConnectionTimeout":30,"AuthenticationType":"User","UserID":"cryptotrader","Password":"T3st!234","DataList":"","ConnectionString":"Data Source=mssqlcryptotrader.database.windows.net;Initial Catalog=EllidexDB;User ID=cryptotrader;Password=T3st!234;;Connection Timeout=30","IsSource":true,"IsService":false,"IsFolder":false,"IsReservedService":false,"IsServer":false,"IsResourceVersion":false,"Version":null,"ResourceID":"0a6dfc8e-64ef-4f89-a9dd-27d7daa7180a","ResourceType":"SqlDatabase","ResourceName":"EllidexDB","IsValid":false,"Errors":[],"ReloadActions":false,"UserPermissions":0,"VersionInfo":null},"ProcedureName":"dbo.Warewolf_Login","SqlQuery":null,"ConnectionTimeout":0,"CommandTimeout":null}');
//    expect(log.IsSubExecution).toEqual(0);
//  });
//})
