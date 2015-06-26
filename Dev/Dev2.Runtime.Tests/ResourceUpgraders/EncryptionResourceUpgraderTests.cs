
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Xml.Linq;
using System;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Runtime.ResourceUpgrades;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Dev2.Warewolf.Security.Encryption;

namespace Dev2.Tests.Runtime.ResourceUpgraders
{
    [TestClass]
    public class EncryptionResourceUpgraderTests
    {

        string connectionString,
            beforeContainingSource,
 beforeWithoutSource;

        public EncryptionResourceUpgraderTests()
        {
            connectionString = @"Data Source=RSAKLFSVRGENDEV,1433;Initial Catalog=Dev2TestingDB;User ID=testuser;Password=test007;";
            beforeContainingSource = @"<first><second><third><Source ID=""ebba47dc-e5d4-4303-a203-09e2e9761d16"" Version=""1.0"" Name=""testingDBSrc"" ResourceType=""DbSource"" IsValid=""false"" ServerType=""SqlDatabase"" Type=""SqlDatabase"" ConnectionString=""" + connectionString + @""" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" ServerVersion=""0.4.2.2""></third></second></first>";
            beforeWithoutSource = @"<first><second><third><xSource ID=""ebba47dc-e5d4-4303-a203-09e2e9761d16"" Version=""1.0"" Name=""testingDBSrc"" ResourceType=""DbSource"" IsValid=""false"" ServerType=""SqlDatabase"" Type=""SqlDatabase"" ConnectionString=""" + connectionString + @""" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" ServerVersion=""0.4.2.2""></third></second></first>";
        }


        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("EncryptionResourceUpgrader_Upgrade")]
        // ReSharper disable InconsistentNaming
        public void EncryptionResourceUpgrader_Upgrade_HasMatchin_ExpectReplace()
        {
            _matchAndReplaceWhereAppropriate(beforeContainingSource, beforeWithoutSource, connectionString);
        }

        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("EncryptionResourceUpgrader_Upgrade")]
        public void EncryptionResourceUpgrader_Upgrade_HasMatchin_DsfFileWrite()
        {
            _matchAndReplaceWhereAppropriate(
                @"&gt;&lt;uaba:DsfFileWrite Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" Append=""False"" AppendBottom=""False"" AppendTop=""False"" DatabindRecursive=""False"" DisplayName=""Write"" FileContents=""TestData"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,120"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""C:\Temp\PathOperationsTestFolder\NewFolder\NewFolderFirstInnerFolder\CreatedTestFile.txt"" Overwrite=""True"" Password=""ThePassword"" Result=""[[WriteFileRes]]"" SimulationMode=""OnDemand"" UniqueID=""72a21106-f694-4d1c-b07b-c227b0e828f3"" Username=""""&gt;",
            @"&gt;&lt;uaba:NonDsfFileWrite Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnErrorVariable=""{x:Null}"" OnErrorWorkflow=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" AmbientDataList=""[AmbientDataList]"" Append=""False"" AppendBottom=""False"" AppendTop=""False"" DatabindRecursive=""False"" DisplayName=""Write"" FileContents=""TestData"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""200,120"" InstructionList=""[InstructionList]"" IsEndedOnError=""False"" IsNotCertVerifiable=""False"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" OutputPath=""C:\Temp\PathOperationsTestFolder\NewFolder\NewFolderFirstInnerFolder\CreatedTestFile.txt"" Overwrite=""True"" Password=""ThePassword"" Result=""[[WriteFileRes]]"" SimulationMode=""OnDemand"" UniqueID=""72a21106-f694-4d1c-b07b-c227b0e828f3"" Username=""""&gt;",
            "ThePassword");
        }

        void _matchAndReplaceWhereAppropriate(string matchingString, string nonMatchingString, string pieceToReplace)
        {
            //------------Setup for test--------------------------
            var upgrader = new EncryptionResourceUpgrader();

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            string output = upgrader.EncryptPasswordsAndConnectionStrings(matchingString);
            output.Should().NotBeNullOrEmpty();
            output.Should().NotBe(matchingString);
            output.Should().NotContain(pieceToReplace);

            output = upgrader.EncryptPasswordsAndConnectionStrings(nonMatchingString);
            output.Should().NotBeNullOrEmpty();
            output.Should().Be(nonMatchingString);
            output.Should().Contain(pieceToReplace);
        }



        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("EncryptionResourceUpgrader_Upgrade")]
        // ReSharper disable InconsistentNaming
        public void EncryptionResourceUpgrader_Upgrade_CanDecrypt()
        {
            //------------Setup for test--------------------------
            var upgrader = new EncryptionResourceUpgrader();
            Regex cs = new Regex(@"ConnectionString=""([^""]+)""");

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            string output = upgrader.EncryptSourceConnectionStrings(beforeContainingSource);
            output.Should().NotBeNullOrEmpty();
            output.Should().NotBe(beforeContainingSource);
            output.Should().NotContain(connectionString);
            Match m = cs.Match(output);
            m.Success.Should().BeTrue();
            m.Groups.Count.Should().BeGreaterOrEqualTo(1);
            m.Groups[1].Success.Should().BeTrue();
            string x = m.Groups[1].Value;
            DpapiWrapper.Decrypt(x).Should().Be(connectionString);
        }

        /*
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BaseResourceUpgrader_Upgrade")]
        public void BaseResourceUpgrader_Upgrade_HasMatchin_ExpectReplaceAlt()
        {
            //------------Setup for test--------------------------
            var baseResourceUpgrader = new BaseResourceUpgrader();

            //------------Execute Test---------------------------
            Assert.AreEqual("<a>clr-namespace:Dev2.Common.Interfaces.Core.Convertors.Case;assembly=Dev2.Common.Interfaces</a>", baseResourceUpgrader.UpgradeFunc(XElement.Parse("<a>clr-namespace:Dev2.Interfaces;assembly=Dev2.Core</a>")).ToString());
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("BaseResourceUpgrader_Upgrade")]
        public void BaseResourceUpgrader_Upgrade_NoMatch_NoReplace()
        {
            //------------Setup for test--------------------------
            var baseResourceUpgrader = new BaseResourceUpgrader();

            //------------Execute Test---------------------------
            Assert.AreEqual("<a>bob</a>", baseResourceUpgrader.UpgradeFunc(XElement.Parse("<a>bob</a>")).ToString());
            //------------Assert Results-------------------------
        }
         * */
        // ReSharper restore InconsistentNaming
    }
}
