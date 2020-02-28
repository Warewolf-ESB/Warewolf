/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Serializers;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using System.Collections.Generic;
using System.Text;
using Warewolf.Data.Options;
using Warewolf.Options;
using Warewolf.Service;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FindOptionsBy : DefaultEsbManagementEndpoint
    {

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var result = new List<IOption>();

            if (values.TryGetValue(OptionsService.ParameterName, out StringBuilder optionsRequestedSB))
            {
                var optionsRequested = optionsRequestedSB.ToString();

                if (optionsRequested == OptionsService.GateResume)
                {
                    var failureOptions = OptionConvertor.Convert(new GateOptions());

                    result.AddRange(failureOptions);
                }
            }

            var serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Type ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => nameof(FindOptionsBy);

    }
}
