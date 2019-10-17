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
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Activities.Designers2.Gate
{
    public class GateDesignerViewModel : ActivityDesignerViewModel
    {
        string _selectedGateFailure;
        private GateFailureOptions _gateFailureOption;
        public GateDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            ImagePath = "pack://application:,,,/Warewolf Studio;component/Images/gate-open.png";
        }

        public string ImagePath { get; set; }
        public GateFailureOptions GateFailure
        {
            get => _gateFailureOption;
            set
            {
                _gateFailureOption = value;
            }
        }
        public IEnumerable<string> GateFailureOptions => GateFailureOptionsHelper<GateFailureOptions>.GetDiscriptionsAsList(typeof(GateFailureOptions)).ToList();
        public string SelectedGateFailure
        {
            get => GateFailureOptionsHelper<GateFailureOptions>.GetEnumDescription(GateFailure.ToString());
            set
            {
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(GateFailure.ToString()))
                {
                    return;
                }

                var gateFailure = GateFailureOptions.Single(p => p.ToString().Contains(value));
                _selectedGateFailure = gateFailure;

                var enumFromDescription = GateFailureOptionsHelper<GateFailureOptions>.GetEnumFromDescription(gateFailure);
                GateFailure = enumFromDescription;
            }
        }
        public override void UpdateHelpDescriptor(string helpText)
        {

        }
    }
}
