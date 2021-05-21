/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;

namespace Warewolf.Enums
{
    public enum SubscriptionStatus
    {
        [Description("future")]
        Future,
        [Description("in_trial")]
        InTrial,
        [Description("active")]
        Active,
        [Description("non_renewing")]
        NonRenewing,
        [Description("paused")]
        Paused,
        [Description("cancelled")]
        Cancelled
    }
}