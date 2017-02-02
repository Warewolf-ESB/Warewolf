/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Hosting
{
    public class ManagementServiceResource : Resource
    {
        public DynamicService Service { get; private set; }

        public ManagementServiceResource(DynamicService service)
        {
            if(service == null)
            {
                throw new ArgumentNullException("service");
            }
            Service = service;
            ResourceID = service.ID == Guid.Empty ? Guid.NewGuid() : service.ID;
            ResourceName = service.Name;
            ResourceType = "ReservedService";
            DataList = service.DataListSpecification;
        }

        #region Overrides of Resource

        public override bool IsSource => false;
        public override bool IsService => false;

        public override bool IsFolder => false;

        public override bool IsReservedService => true;

        public override bool IsServer => false;
        public override bool IsResourceVersion => false;

        #endregion
    }
}
