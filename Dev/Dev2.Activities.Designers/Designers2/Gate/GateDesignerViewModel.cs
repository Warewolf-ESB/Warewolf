/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Designers2.Core;
using Dev2.Common.Gates;
using Dev2.Common.Interfaces.Enums;
using Dev2.Studio.Interfaces;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Activities.Designers2.Gate
{
    public class GateDesignerViewModel : ActivityDesignerViewModel
    {
        private GateFailureOptions _gateFailureOption;
        private GateRetryStrategies _retryStrategy;

        public GateDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
        }

        public GateFailureOptions GateFailure
        {
            get => _gateFailureOption;
            set
            {
                _gateFailureOption = value;
            }
        }
        public IEnumerable<string> GateFailureOptions => GateOptionsHelper<GateFailureOptions>.GetDescriptionsAsList(typeof(GateFailureOptions)).ToList();
        public IEnumerable<string> GateRetryStrategies => GateOptionsHelper<GateRetryStrategies>.GetDescriptionsAsList(typeof(GateRetryStrategies)).ToList();
        public string SelectedGateFailure
        {
            get => GateOptionsHelper<GateFailureOptions>.GetEnumDescription(GateFailure.ToString());
            set
            {
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(GateFailure.ToString()))
                {
                    return;
                }

                var gateFailure = GateFailureOptions.Single(p => p.ToString().Contains(value));
                var enumFromDescription = GateOptionsHelper<GateFailureOptions>.GetEnumFromDescription(gateFailure);
                GateFailure = enumFromDescription;
            }
        }
        public GateRetryStrategies GateRetryStrategy
        {
            get => _retryStrategy;
            set
            {
                _retryStrategy = value;
            }
        }
        public string SelectedRetryStrategy
        {
            get => GateOptionsHelper<GateRetryStrategies>.GetEnumDescription(GateRetryStrategy.ToString());
            set
            {
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(GateRetryStrategy.ToString()))
                {
                    return;
                }

                var retryStrategy = GateRetryStrategies.Single(p => p.ToString().Contains(value));
                var enumFromDescription = GateOptionsHelper<GateRetryStrategies>.GetEnumFromDescription(retryStrategy);
                GateRetryStrategy = enumFromDescription;
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
