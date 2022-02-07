/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Runtime.Services.Interfaces;
using System.Management;
using System.Runtime.InteropServices;

namespace Dev2.Runtime.Services.ESB.Management.Services
{
    public class ManagementObjectSearcherFactory : IManagementObjectSearcherFactory
    {
        public ManagementObjectSearcher New(OSPlatform osPlatform, ObjectQuery objectQuery)
        {
            if(osPlatform == OSPlatform.Windows)
            {
                return new ManagementObjectSearcher(objectQuery);
            }
            else
            {
                return new ManagementObjectSearcher();
            }
        }
    }
}
