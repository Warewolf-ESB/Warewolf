/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;

namespace Dev2.Runtime.ESB.Management
{
    class EsbManagementServiceEntry
    {
        public static DynamicService CreateESBManagementServiceEntry(string HandleType, string DataListSpecification)
        {
            var findServices = new DynamicService
            {
                Name = HandleType,
                DataListSpecification = new StringBuilder(DataListSpecification)
            };
            using (ServiceAction serviceAction = new ServiceAction
            {
                Name = HandleType,
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandleType
            })
            {
                findServices.Actions.Add(serviceAction);
                return findServices;
            }
        }
    }
}
