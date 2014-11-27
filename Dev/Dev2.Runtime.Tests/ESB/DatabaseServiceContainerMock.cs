
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Linq;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.Execution;
using Dev2.Workspaces;

namespace Dev2.Tests.Runtime.ESB
{
    public class DatabaseServiceContainerMock : DatabaseServiceContainer
    {
        public DatabaseServiceContainerMock(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : base(sa, dataObj, theWorkspace, esbChannel)
        {
        }

        public int GetXmlDataFromSqlServiceActionHitCount { get; private set; }
        public string DatabaseRespsonseXml { get; set; }

        // override
        protected string GetXmlDataFromSqlServiceAction(ServiceAction serviceAction, DataList.Contract.IDev2IteratorCollection iteratorCollection, System.Collections.Generic.IList<DataList.Contract.IDev2DataListEvaluateIterator> itrs, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            
            GetXmlDataFromSqlServiceActionHitCount++;
            if (string.IsNullOrEmpty(DatabaseRespsonseXml))
            {
               // return base.GetXmlDataFromSqlServiceAction(serviceAction, iteratorCollection, itrs, out errors);
            }

            errors = new ErrorResultTO();

            // Need to mimick processing inputs otherwise we end up in any infinite loop!
            if (serviceAction.ServiceActionInputs.Any())
            {
                int pos = 0;
                foreach (var itr in itrs)
                {
                    var val = iteratorCollection.FetchNextRow(itr);
                    var sai = serviceAction.ServiceActionInputs[pos];
                }
            }
            return DatabaseRespsonseXml;
        }
    }
}
