/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Warewolf.Options;

namespace Warewolf.Data.Options
{
    public class RabbitMqPublishOptions
    {
        public RabbitMqPublishOptions()
        {
        }

        [OptionUX(nameof(OptionAutocomplete))]
        [HelpText(nameof(Studio.Resources.Languages.HelpText.RabbitMQHelpCorrelationID))]
        [Tooltip(nameof(Studio.Resources.Languages.Tooltips.RabbitMQToolTipCorrelationID))]
        [Orientation(Orientation.Horizontal)]
        public string CorrelationID { get; set; }
    }
}