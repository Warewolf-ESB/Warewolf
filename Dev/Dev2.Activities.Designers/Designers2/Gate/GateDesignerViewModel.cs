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
using Dev2.Studio.Interfaces;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using Warewolf.Data.Options.Enums;

namespace Dev2.Activities.Designers2.Gate
{
    public class GateDesignerViewModel : ActivityDesignerViewModel
    {
        private GateFailureAction _gateFailureOption;
        private RetryAlgorithm _retryStrategy;

        public GateDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
        }

        public GateFailureAction GateFailure
        {
            get => _gateFailureOption;
            set
            {
                _gateFailureOption = value;
            }
        }
        public IEnumerable<string> GateFailureOptions => GateOptionsHelper<GateFailureAction>.GetDescriptionsAsList(typeof(GateFailureAction)).ToList();
        public IEnumerable<string> GateRetryStrategies => GateOptionsHelper<RetryAlgorithm>.GetDescriptionsAsList(typeof(RetryAlgorithm)).ToList();
        public string SelectedGateFailure
        {
            get => GateOptionsHelper<GateFailureAction>.GetEnumDescription(GateFailure.ToString());
            set
            {
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(GateFailure.ToString()))
                {
                    return;
                }

                var gateFailure = GateFailureOptions.Single(p => p.ToString().Contains(value));
                var enumFromDescription = GateOptionsHelper<GateFailureAction>.GetEnumFromDescription(gateFailure);
                GateFailure = enumFromDescription;
            }
        }
        public RetryAlgorithm GateRetryStrategy
        {
            get => _retryStrategy;
            set
            {
                _retryStrategy = value;
            }
        }
        public string SelectedRetryStrategy
        {
            get => GateOptionsHelper<RetryAlgorithm>.GetEnumDescription(GateRetryStrategy.ToString());
            set
            {
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(GateRetryStrategy.ToString()))
                {
                    return;
                }

                var retryStrategy = GateRetryStrategies.Single(p => p.ToString().Contains(value));
                var enumFromDescription = GateOptionsHelper<RetryAlgorithm>.GetEnumFromDescription(retryStrategy);
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
