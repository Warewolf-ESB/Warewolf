/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Globalization;
using System.Windows.Data;
using Dev2;
using Dev2.Studio.Interfaces;

namespace Warewolf.UI
{
    public class WorkflowNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Data.WorkflowWithInputs workflowWithInputs && workflowWithInputs.Value != Guid.Empty)
            {
                var shellViewModel = CustomContainer.Get<IShellViewModel>();
                var resource = shellViewModel?.GetResource(workflowWithInputs.Value.ToString());
                if (resource is null)
                {
                    return Binding.DoNothing;
                }
                workflowWithInputs.Name = resource.ResourceName;
                workflowWithInputs.Inputs = shellViewModel?.GetInputsFromWorkflow(workflowWithInputs.Value);

                return resource.ResourceName;
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
