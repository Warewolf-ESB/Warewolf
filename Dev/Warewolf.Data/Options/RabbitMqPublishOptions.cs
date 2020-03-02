/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
    public class RabbitMqPublishOptions : IOptionConvertParameter
    {
        public RabbitMqPublishOptions()
        {
        }

        [DataValue("Correlation")]
        [MultiDataProvider( typeof(ExecutionID),typeof(CustomTransactionID), typeof(Manual))]
        [OptionUX(nameof(OptionCombobox))]
        [HelpText(nameof(Studio.Resources.Languages.HelpText.RabbitMQHelpAutoCorrelationID))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.RabbitMQToolTipAutoCorrelationID))]
        [Orientation(Orientation.Horizontal)]
        public AutoCorrelation AutoCorrelation { get; set; } 

        public void Notify()
        {
            OnChange?.Invoke();
        }

        public delegate void NotifyHandler();

        public event NotifyHandler OnChange;
    }

    public class AutoCorrelation
    {
        public CorrelationAction Correlation { get; protected set; } = CorrelationAction.ExecutionID;
    }

    public class ExecutionID : AutoCorrelation
    {
        public ExecutionID()
        {
            Correlation = CorrelationAction.ExecutionID;
        }
    }

    public class CustomTransactionID : AutoCorrelation
    {
        public CustomTransactionID()
        {
            Correlation = CorrelationAction.CustomTransactionID;
        }
    }

    public class Manual : AutoCorrelation
    {
        [HelpText(nameof(Studio.Resources.Languages.HelpText.RabbitMQHelpCorrelationID))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.RabbitMQToolTipCorrelationID))]
        public string CorrelationID { get; set; }

        [Description("Manual CorrelationID")]
        public Manual()
        {
            Correlation = CorrelationAction.ManualID;
        }
    }

    public enum CorrelationAction
    {
        ExecutionID = 0,
        CustomTransactionID = 1,
        ManualID = 2
    }
}