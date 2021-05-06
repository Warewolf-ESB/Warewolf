/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Warewolf.Enums;

namespace Warewolf.Licensing
{
    public interface ILicenseData
    {
        string CustomerId { get; set; }
        string PlanId { get; set; }
        string CustomerFirstName { get; set; }
        string CustomerLastName { get; set; }
        string CustomerEmail { get; set; }
        long CardNumber { get; set; }
        int? CardCvv { get; set; }
        int CardExpiryYear { get; set; }
        int CardExpiryMonth { get; set; }
        bool IsLicensed { get; set; }
        SubscriptionStatus? Status { get; set; }
        DateTime? TrialEnd { get; set; }
        bool EndOfTerm { get; set; }
    }
}