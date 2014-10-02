
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class WebServicesMock : WebServices
    {
        public WebServicesMock()
        {
        }

        public WebServicesMock(IResourceCatalog resourceCatalog, WebExecuteString webExecute)
            : base(resourceCatalog, webExecute)
        {
        }

        public new Service DeserializeService(string args)
        {
            return base.DeserializeService(args);
        }

        public new Service DeserializeService(XElement xml, ResourceType resourceType)
        {
            return base.DeserializeService(xml, resourceType);
        }

        public bool FetchRecordsetAddFields { get; set; }
        public int FetchRecordsetHitCount { get; set; }
        public override RecordsetList FetchRecordset(WebService service, bool addFields)
        {
            FetchRecordsetHitCount++;
            FetchRecordsetAddFields = addFields;
            return base.FetchRecordset(service, addFields);
        }
    }
}
