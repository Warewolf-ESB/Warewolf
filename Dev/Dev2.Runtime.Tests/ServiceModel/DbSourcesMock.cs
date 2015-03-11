
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Tests.Runtime.ServiceModel
{
    public class DbSourcesMock : DbSources
    {
        public int DatabaseValidationHitCount { get; set; }

        public override DatabaseValidationResult DoDatabaseValidation(DbSource dbSourceDetails)
        {
            //PBI 8720
            DatabaseValidationHitCount++;
            return new DatabaseValidationResult { IsValid = true };
        }
    }
}
