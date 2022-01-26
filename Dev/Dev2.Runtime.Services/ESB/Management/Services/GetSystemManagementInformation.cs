/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Runtime.Services.Interfaces;
using System;
using System.Runtime.InteropServices;

namespace Dev2.Runtime.Services.ESB.Management.Services
{
    public class GetSystemManagementInformation : IGetSystemManagementInformation
    {
        private ManagementObject _managementObject;

        public GetSystemManagementInformation(ManagementObject managementObject)
        {
            _managementObject = managementObject;
        }

        public int GetNumberOfCores()
        {
            var coreCount = 0;

            if (_managementObject.OSPlatform == OSPlatform.Windows)
            {
                try
                {
                    using (_managementObject.ManagementObjectSearcher)
                    {
                        using (_managementObject.ManagementObjectCollection = _managementObject.ManagementObjectSearcher.Get())
                            foreach (var item in _managementObject.ManagementObjectCollection)
                            {
                                coreCount += int.Parse(item["NumberOfCores"].ToString());
                            }
                    }
                }
                catch (Exception err)
                {
                    Dev2Logger.Error(err.Message, GlobalConstants.WarewolfError);
                }
            }

            return coreCount;
        }
    }
}
