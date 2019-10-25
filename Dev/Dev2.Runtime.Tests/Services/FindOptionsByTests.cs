/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Text;
using Dev2.Workspaces;
using Warewolf.Service;
using Dev2.Common.Common;
using Dev2.Common.Serializers;
using Warewolf.Options;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FindOptionsByTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FindOptionsBy))]
        public void FindOptionsBy_Given_OptionsServiceGateResume_IsNull_ExpectedEmptySerializeToBuilder()
        {
            //----------------------Arrange----------------------

            var valuesDic = new Dictionary<string, StringBuilder>();

            var sut = new FindOptionsBy();
            //----------------------Act--------------------------
            var result = sut.Execute(valuesDic, new Mock<IWorkspace>().Object);
            //----------------------Assert-----------------------
            var actual = result.Substring(1, result.Length - 2);

            Assert.AreEqual(expected: string.Empty, actual: actual);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FindOptionsBy))]
        public void FindOptionsBy_Given_OptionsServiceGateResume_IsNotNull_ExpectedEmptySerializeToBuilder()
        {
            //----------------------Arrange----------------------
            var valuesDic = new Dictionary<string, StringBuilder>
            {
                { OptionsService.ParameterName, OptionsService.GateResume.ToStringBuilder() }
            };

            var sut = new FindOptionsBy();
            //----------------------Act--------------------------
            var result = sut.Execute(valuesDic, new Mock<IWorkspace>().Object);
            //----------------------Assert-----------------------
            var actual = result.Substring(1, result.Length - 2);
            var expected = "{\"$id\":\"1\",\"$type\":\"Warewolf.Options.OptionCombobox, Warewolf.Data\",\"Name\":\"GateFailureAction\",\"Options\":{\"$type\":\"System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Collections.Generic.IEnumerable`1[[Warewolf.Options.IOption, Warewolf.Interfaces]], mscorlib]], mscorlib\",\"Retry\":[{\"$id\":\"2\",\"$type\":\"Warewolf.Options.OptionEnum, Warewolf.Data\",\"Name\":\"Resume\",\"Value\":1,\"Default\":null},{\"$id\":\"3\",\"$type\":\"Warewolf.Options.OptionAutocomplete, Warewolf.Data\",\"Name\":\"ResumeEndpoint\",\"Value\":\"Not Yet Set\",\"Default\":\"\",\"Suggestions\":null},{\"$id\":\"4\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"Count\",\"Value\":2,\"Default\":0},{\"$id\":\"5\",\"$type\":\"Warewolf.Options.OptionCombobox, Warewolf.Data\",\"Name\":\"Strategy\",\"Options\":{\"$type\":\"System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Collections.Generic.IEnumerable`1[[Warewolf.Options.IOption, Warewolf.Interfaces]], mscorlib]], mscorlib\",\"NoBackoffStrategy\":[{\"$id\":\"6\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"TimeOut\",\"Value\":60000,\"Default\":0}],\"ConstantBackoffStrategy\":[{\"$id\":\"7\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"TimeOut\",\"Value\":60000,\"Default\":0}],\"LinearBackoffStrategy\":[{\"$id\":\"8\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"TimeOut\",\"Value\":60000,\"Default\":0}],\"FibonacciBackoffStrategy\":[{\"$id\":\"9\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"TimeOut\",\"Value\":60000,\"Default\":0}],\"QuadraticBackoffStrategy\":[{\"$id\":\"10\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"TimeOut\",\"Value\":60000,\"Default\":0}]},\"Value\":\"NoBackoff\"}]},\"Value\":\"Retry\"}";

            Assert.AreEqual(expected: expected, actual: actual);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(FindOptionsBy))]
        public void FindOptionsBy_Given_OptionsServiceGateResume_ExpectDeserializeSuccess()
        {
            //----------------------Arrange----------------------
            var values = new Dictionary<string, StringBuilder>
            {
                { OptionsService.ParameterName, OptionsService.GateResume.ToStringBuilder() }
            };

            var sut = new FindOptionsBy();
            //----------------------Act--------------------------
            var jsonResponse = sut.Execute(values, new Mock<IWorkspace>().Object);
            //----------------------Assert-----------------------
            var serializer = new Dev2JsonSerializer();
            var result = serializer.Deserialize<IOption[]>(jsonResponse);
            Assert.IsNotNull(result, "failed to deserialize previously serialized options");
            Assert.AreEqual("GateFailureAction", result[0].Name);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FindOptionsBy))]
        public void FindOptionsBy_If_CreateServiceEntry_IsCalled_ExpectedSuccess()
        {
            //----------------------Arrange----------------------
            var sut = new FindOptionsBy();

            //----------------------Act--------------------------
            var result = sut.CreateServiceEntry();
            //----------------------Assert-----------------------
            Assert.IsNotNull(result.DataListSpecification);
            Assert.AreEqual(typeof(DynamicServices.DynamicService), result.GetType());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(FindOptionsBy))]
        public void FindOptionsBy_If_HandlesTypes_IsCalled_ExpectedSuccess()
        {
            //----------------------Arrange----------------------
            var sut = new FindOptionsBy();
            //----------------------Act--------------------------
            var result = sut.HandlesType();
            //----------------------Assert-----------------------
            Assert.AreEqual(nameof(FindOptionsBy), result);
        }
    }
}