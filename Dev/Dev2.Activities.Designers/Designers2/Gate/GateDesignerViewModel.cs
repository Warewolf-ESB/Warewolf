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
using System.Activities.Presentation.Model;

namespace Dev2.Activities.Designers2.Gate
{
    public class GateDesignerViewModel : ActivityDesignerViewModel
    {
        public GateDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            ImagePath = "pack://application:,,,/Warewolf Studio;component/Images/gate-open.png";
            GateFailure = "Retry [Gate]";
        }

        public string ImagePath { get; set; }
        public string GateFailure { get; set; }

        public override void UpdateHelpDescriptor(string helpText)
        {
            
        }
    }
}
