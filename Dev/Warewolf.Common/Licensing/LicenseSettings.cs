// 
// /*
// *  Warewolf - Once bitten, there's no going back
// *  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
// *  Licensed under GNU Affero General Public License 3.0 or later.
// *  Some rights reserved.
// *  Visit our website for more information <http://warewolf.io/>
// *  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
// *  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
// */

namespace Warewolf.Licensing
{
    public static class LicenseSettings
    {
        public static string CustomerId { get; set; }
        public static string SubscriptionId { get; set; }
        public static string PlanId { get; set; }
        public static bool IsLicensed { get; set; }
        public static string NotRegistered = "Not Registered";
        public static string SiteName { get; set; }
        public static string ApiKey { get; set; }

    }
}