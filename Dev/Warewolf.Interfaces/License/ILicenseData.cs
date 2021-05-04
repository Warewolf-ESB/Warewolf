/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
namespace Warewolf.License
{
    public class LicenseData : ILicenseData
    {
        public string CustomerId { get; set; }
        public string Customer { get; set; }
        public string PlanId { get; set; }
        public bool IsValid { get; set; }
    }

    public interface ILicenseData
    {
        string CustomerId { get; set; }
        string Customer { get; set; }
        string PlanId { get; set; }
        bool IsValid { get; set; }
    }
}