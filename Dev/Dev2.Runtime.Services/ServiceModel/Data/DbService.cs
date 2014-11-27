
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class DbService : Service
    {
        public Recordset Recordset { get; set; }

        #region CTOR

        public DbService()
        {
            ResourceType = Common.Interfaces.Data.ResourceType.DbService;
            Source = new DbSource();
            Recordset = new Recordset();
        }

        public DbService(XElement xml)
            : base(xml)
        {
            ResourceType = Common.Interfaces.Data.ResourceType.DbService;
            var action = xml.Descendants("Action").FirstOrDefault();
            if(action == null)
            {
                return;
            }

            Source = CreateSource<WebSource>(action);
            Method = CreateInputsMethod(action);

            var recordSets = CreateOutputsRecordsetList(action);
            Recordset = recordSets.FirstOrDefault() ?? new Recordset { Name = action.AttributeSafe("Name") };

            if(String.IsNullOrEmpty(Recordset.Name))
            {
                Recordset.Name = Method.Name;
            }
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var actionName = Recordset == null || Recordset.Name == null ? string.Empty : Recordset.Name;
            var result = CreateXml(enActionType.InvokeStoredProc, actionName, Source, new RecordsetList { Recordset });
            return result;
        }

        #endregion

        #region Create

        public static DbService Create()
        {
            var result = new DbService
            {
                ResourceID = Guid.Empty,
                Source = { ResourceID = Guid.Empty },
            };
            return result;
        }

        #endregion


    }
}
