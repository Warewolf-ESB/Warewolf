
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Models;
using Dev2.Studio.Core.Helpers;
using Dev2.Studio.Core.Models;
using Dev2.Studio.ViewModels.Deploy;

namespace Dev2.Factory
{
    public static class DeployViewModelFactory
    {
        public static DeployViewModel GetDeployViewModel(object input)
        {
            DeployViewModel deployViewModel = null;

            if(input != null)
            {
                TypeSwitch.Do(input,
                              TypeSwitch.Case<ExplorerItemModel>(
                                  x => deployViewModel = new DeployViewModel(x.ResourceId, x.EnvironmentId)),
                              TypeSwitch.Case<ResourceModel>(
                                  x => deployViewModel = new DeployViewModel(x.ID, x.Environment.ID)),
                              TypeSwitch.Default(() => deployViewModel = new DeployViewModel()));
            }
            else
            {
                deployViewModel = new DeployViewModel();
            }

            return deployViewModel;
        }
    }
}
