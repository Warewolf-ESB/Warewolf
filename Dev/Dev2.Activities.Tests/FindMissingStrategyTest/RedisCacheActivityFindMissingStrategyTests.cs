/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.RedisCache;
using Dev2.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.FindMissingStrategyTest
{
    [TestClass]
    public class RedisCacheActivityFindMissingStrategyTests
    {
        [TestMethod]
        [Timeout(60000)]
        [TestCategory("RedisCache")]
        [Owner("Devaji Chotaliya")]
        public void RedisCacheActivityFindMissingStrategy_GetActivityFields_CheckRedisCacheActivityWithDsfActivity_ExpectedAllFindMissingFieldsToBeReturned()
        {
            //--------------Arrange------------------------------
            var dsfActivity = new DsfActivity
            {
                InputMapping = @"<Inputs><Input Name=""reg"" Source=""NUD2347"" DefaultValue=""NUD2347""><Validator Type=""Required"" /></Input><Input Name=""asdfsad"" Source=""registration223"" DefaultValue=""w3rt24324""><Validator Type=""Required"" /></Input><Input Name=""number"" Source=""[[number]]"" /></Inputs>",
                OutputMapping = @"<Outputs><Output Name=""vehicleVin"" MapsTo=""VIN"" Value="""" /><Output Name=""vehicleColor"" MapsTo=""VehicleColor"" Value="""" /><Output Name=""speed"" MapsTo=""speed"" Value="""" Recordset=""Fines"" /><Output Name=""date"" MapsTo=""date"" Value=""Fines.Date"" Recordset=""Fines"" /><Output Name=""location"" MapsTo=""location"" Value="""" Recordset=""Fines"" /></Outputs>"
            };
            var activity = new RedisCacheActivity
            {
                DisplayName="Redis Cache",
                ActivityFunc = {Handler=dsfActivity }
            };
            var findMissingStrategyFactory = new Dev2FindMissingStrategyFactory();
            var strategy = findMissingStrategyFactory.CreateFindMissingStrategy(enFindMissingType.RedisCache);

            //--------------Act----------------------------------
            var actual = strategy.GetActivityFields(activity);

            //--------------Assert-------------------------------
            var expected = new List<string> { "NUD2347", "registration223", "[[number]]", "Fines.Date", "5" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [TestCategory("RedisCache")]
        [Owner("Devaji Chotaliya")]
        public void RedisCacheActivityFindMissingStrategy_GetActivityFields_CheckRedisCacheActivityWithDsfMultiAssignActivity_ExpectedAllFindMissingFieldsToBeReturned()
        {
            //--------------Arrange------------------------------
            var multiAssignActivity = new DsfMultiAssignActivity { FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[AssignRight1]]", "[[AssignLeft1]]", 1), new ActivityDTO("[[AssignRight2]]", "[[AssignLeft2]]", 2) } };
            var activity = new RedisCacheActivity
            {
                DisplayName = "Redis Cache",
                ActivityFunc = { Handler = multiAssignActivity }
            };
            var findMissingStrategyFactory = new Dev2FindMissingStrategyFactory();
            var strategy = findMissingStrategyFactory.CreateFindMissingStrategy(enFindMissingType.RedisCache);

            //--------------Act----------------------------------
            var actual = strategy.GetActivityFields(activity);

            //--------------Assert-------------------------------
            var expected = new List<string> { "[[AssignRight1]]", "[[AssignLeft1]]", "[[AssignRight2]]", "[[AssignLeft2]]", "5" };
            CollectionAssert.AreEqual(expected, actual);
        }

    }
}
