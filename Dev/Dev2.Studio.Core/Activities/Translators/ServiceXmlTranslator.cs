
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Activities;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Activities.Interfaces;
using Dev2.Studio.Core.Activities.TO;
using Dev2.Studio.Core.Activities.Utils;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Activities.Translators
{
    public class ServiceXmlTranslator : IServiceXmlTranslator
    {
        public ActivityViewModelTO GetActivityViewModelTO(ModelItem modelItem)
        {
            ActivityViewModelTO result = new ActivityViewModelTO();

            var argument = ModelItemUtils.GetProperty("HelpLink", modelItem) as InArgument;
            if(argument != null)
            {
                result.HasHelpPage = !string.IsNullOrWhiteSpace(argument.Expression.ToString());
            }

            var inArgument1 = ModelItemUtils.GetProperty("ActionName", modelItem) as InArgument;
            if(inArgument1 != null)
            {
                result.Action = inArgument1.Expression.ToString();
            }

            var simulationMode = ModelItemUtils.GetProperty("SimulationMode", modelItem);
            if(simulationMode != null)
            {
                result.Simulation = simulationMode.ToString();
            }

            var inArgument = ModelItemUtils.GetProperty("FriendlySourceName", modelItem) as InArgument;
            if(inArgument != null)
            {

                result.SourceName = inArgument.Expression.ToString();
            }

            var inArgument2 = ModelItemUtils.GetProperty("Type", modelItem) as InArgument;
            if(inArgument2 != null)
            {
                result.Type = inArgument2.Expression.ToString();
            }

            //result.HasWizard = wizardEngine.HasWizard(modelItem, resource);
            return result;
        }
    }
}
