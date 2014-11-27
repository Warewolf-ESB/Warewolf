
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;

namespace Dev2.Runtime.ESB.Management.Sources
{
    /// <summary>
    /// Root source for management services
    /// Replace : GetDefaultSources() in DynamicServicesHost
    /// </summary>
    public class ManagementServiceSource 
    {
        public Source CreateSourceEntry()
        {
            Source s = new Source();
            s.Name = HandlesType();
            s.Type = enSourceType.ManagementDynamicService;

            return s;
        }

        public string HandlesType()
        {
            return "ManagementDynamicService";
        }
    }
}
