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
    public class LicenseData : ILicenseData
    {
        public string SubscriptionStatus{get;set;}
        public string CustomerId { get; set; }
        public string PlanId { get; set; }
        public string SubscriptionId {get;set;}
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerEmail { get; set; }
        public long CardNumber { get; set; }
        public int? CardCvv { get; set; }
        public int CardExpiryYear { get; set; }
        public int CardExpiryMonth { get; set; }
        public bool IsLicensed { get; set; }
        public SubscriptionStatus? Status { get; set; }
        public DateTime? TrialEnd { get; set; }
        public bool EndOfTerm { get; set; }
    }
}