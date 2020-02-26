/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;
using Warewolf.Options;

namespace Warewolf.Data.Options
{
    public class RabbitMqPublishOptions
    {
        public RabbitMqPublishOptions()
        {
        }

        [DataValue("Correlation")]
        [MultiDataProvider(typeof(Auto), typeof(Manual))]
        [OptionUX(nameof(OptionRadioButtons))]
        [HelpText(nameof(Studio.Resources.Languages.HelpText.RabbitMQHelpAutoCorrelationID))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.RabbitMQToolTipAutoCorrelationID))]
        [Orientation(Orientation.Horizontal)]
        public AutoCorrelation AutoCorrelation { get; set; } = new Auto();
    }

    public class AutoCorrelation
    {
        public CorrelationAction Correlation { get; protected set; } = CorrelationAction.Auto;
    }

    public class Manual : AutoCorrelation
    {
        [HelpText(nameof(Studio.Resources.Languages.HelpText.RabbitMQHelpCorrelationID))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.RabbitMQToolTipCorrelationID))]
        public string CorrelationID { get; set; }
        [Description("Manual CorrelationID") ]
        public Manual()
        {
            Correlation = CorrelationAction.Manual;
        }
    }

    public class Auto : AutoCorrelation
    {
        [HelpText(nameof(Studio.Resources.Languages.HelpText.RabbitMQHelpAutoCorrelationID))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.RabbitMQToolTipAutoCorrelationID))]
        [Orientation(Orientation.Horizontal)]
        public AutoCorrelationAction AutoCorrelation { get; set; } = AutoCorrelationAction.ExecutionID;

        [Description("Auto CorrelationID") ]
        public Auto()
        {
            Correlation = CorrelationAction.Auto;
        }
    }
    public enum AutoCorrelationAction
    {
        ExecutionID = 0,
        
        CustomTransactionID = 1,
    }
    public enum CorrelationAction
    {
        [Description("Auto CorrelationID") ]
        Auto = 0,
        [Description("Manual CorrelationID") ]
        Manual = 1,
    }
}