/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Studio.Core.Models;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;


namespace Dev2.Studio.Core.Factories
{
    public static class ServiceDebugInfoModelFactory
    {
        public static IServiceDebugInfoModel CreateServiceDebugInfoModel(IContextualResourceModel resourceModel, string serviceInputData, DebugMode debugSetting)
        {
            IServiceDebugInfoModel serviceDebugInfoModel = new ServiceDebugInfoModel();
            serviceDebugInfoModel.ResourceModel = resourceModel;
            serviceDebugInfoModel.DebugModeSetting = debugSetting;
            serviceDebugInfoModel.ServiceInputData = serviceInputData;
            serviceDebugInfoModel.RememberInputs = true;
            return serviceDebugInfoModel;
        }
    }
}
