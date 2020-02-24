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
using System.Linq;

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
            var expected = "{\"$id\":\"1\",\"$type\":\"Warewolf.Options.OptionRadioButtons, Warewolf.Data\",\"Name\":\"GateOpts\",\"HelpText\":\"Select whether the Gate Tool will continue on error or end the workflow\",\"Tooltip\":\"Select whether the Gate Tool will continue on error or end the workflow\",\"Options\":{\"$type\":\"System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Collections.Generic.IEnumerable`1[[Warewolf.Options.IOption, Warewolf.Interfaces]], mscorlib]], mscorlib\",\"Continue\":[{\"$id\":\"2\",\"$type\":\"Warewolf.Options.OptionCombobox, Warewolf.Data\",\"Name\":\"Strategy\",\"HelpText\":\"Retry strategy that will be actioned <br/><br/><b>NoBackoff: </b>On Error Retry Immediately<br/><br/><b>LinearBackoff:</b> Delay increases along with every attempt on Linear curve\",\"Tooltip\":\"Select the retry strategy that will be actioned\",\"Options\":{\"$type\":\"System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.Collections.Generic.IEnumerable`1[[Warewolf.Options.IOption, Warewolf.Interfaces]], mscorlib]], mscorlib\",\"NoBackoff\":[{\"$id\":\"3\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"MaxRetries\",\"HelpText\":\"Set maximum retries. The tool will keep retrying until the maximum retry count is hit\",\"Tooltip\":\"Set the maximum retry count. The tool will keep retrying until the maximum retry count is hit\",\"Value\":3,\"Default\":0}],\"ConstantBackoff\":[{\"$id\":\"4\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"Increment\",\"HelpText\":\"Set the delay increment between every retry attempt\",\"Tooltip\":\"Set the delay increment between every retry attempt\",\"Value\":100,\"Default\":0},{\"$id\":\"5\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"MaxRetries\",\"HelpText\":\"Set maximum retries. The tool will keep retrying until the maximum retry count is hit\",\"Tooltip\":\"Set the maximum retry count. The tool will keep retrying until the maximum retry count is hit\",\"Value\":2,\"Default\":0}],\"LinearBackoff\":[{\"$id\":\"6\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"Increment\",\"HelpText\":\"Set the delay increment between every retry attempt\",\"Tooltip\":\"Set the delay increment between every retry attempt\",\"Value\":50,\"Default\":0},{\"$id\":\"7\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"MaxRetries\",\"HelpText\":\"Set maximum retries. The tool will keep retrying until the maximum retry count is hit\",\"Tooltip\":\"Set the maximum retry count. The tool will keep retrying until the maximum retry count is hit\",\"Value\":2,\"Default\":0}],\"FibonacciBackoff\":[{\"$id\":\"8\",\"$type\":\"Warewolf.Options.OptionInt, Warewolf.Data\",\"Name\":\"MaxRetries\",\"HelpText\":\"Set maximum retries. The tool will keep retrying until the maximum retry count is hit\",\"Tooltip\":\"Set the maximum retry count. The tool will keep retrying until the maximum retry count is hit\",\"Value\":2,\"Default\":0}]},\"OptionNames\":[\"NoBackoff\",\"ConstantBackoff\",\"LinearBackoff\",\"FibonacciBackoff\"],\"SelectedOptions\":[{\"$ref\":\"3\"}],\"Value\":\"NoBackoff\"}],\"EndWorkflow\":[]},\"OptionNames\":[{\"$id\":\"9\",\"$type\":\"Warewolf.Options.OptionRadioButtons+OptionName, Warewolf.Data\",\"Name\":\"Continue\",\"IsChecked\":true},{\"$id\":\"10\",\"$type\":\"Warewolf.Options.OptionRadioButtons+OptionName, Warewolf.Data\",\"Name\":\"EndWorkflow\",\"IsChecked\":false}],\"SelectedOptions\":[{\"$ref\":\"2\"}],\"Value\":\"Continue\",\"Orientation\":1}";


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
            Assert.AreEqual("GateOpts", result[0].Name);
            var optionRadioButtons = (OptionRadioButtons)result[0];
            Assert.AreEqual("Continue", optionRadioButtons.Value);
            var selectedOptions = optionRadioButtons.SelectedOptions.ToArray();
            Assert.AreEqual(1, 1);
            Assert.AreEqual("Strategy", selectedOptions[0].Name);
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