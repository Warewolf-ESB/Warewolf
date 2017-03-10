using System;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Runtime.WebServer.TransferObjects;

namespace Dev2.Runtime.WebServer
{
    internal class ExecutionDto
    {
        public WebRequestTO WebRequestTO { get; set; }
        public string ServiceName { get; set; }
        public string PayLoad { get; set; }
        public IDSFDataObject DataObject { get; set; }
        public EsbExecuteRequest Request { get; set; }
        public Guid DataListIdGuid { get; set; }
        public Guid WorkspaceID { get; set; }
        public IResource Resource { get; set; }
        public DataListFormat DataListFormat { get; set; }
        public Dev2JsonSerializer Serializer { get; set; }
        public ErrorResultTO ErrorResultTO { get; set; }
    }
}
