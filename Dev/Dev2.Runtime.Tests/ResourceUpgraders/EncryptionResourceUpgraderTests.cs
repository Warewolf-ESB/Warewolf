/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Text.RegularExpressions;
using Dev2.Runtime.ResourceUpgrades;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Security.Encryption;

namespace Dev2.Tests.Runtime.ResourceUpgraders
{
    [TestClass]
    public class EncryptionResourceUpgraderTests
    {
        readonly string _connectionString;
        readonly string _beforeContainingSource;
        readonly string _beforeWithoutSource;

        public EncryptionResourceUpgraderTests()
        {
            _connectionString = @"Data Source=RSAKLFSVRGENDEV,1433;Initial Catalog=Dev2TestingDB;User ID=testuser;Password=test007;";
            _beforeContainingSource = @"<first><second><third><Source ID=""ebba47dc-e5d4-4303-a203-09e2e9761d16"" Version=""1.0"" Name=""testingDBSrc"" ResourceType=""DbSource"" IsValid=""false"" ServerType=""SqlDatabase"" Type=""SqlDatabase"" ConnectionString=""" + _connectionString + @""" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" ServerVersion=""0.4.2.2""></third></second></first>";
            _beforeWithoutSource = @"<first><second><third><xSource ID=""ebba47dc-e5d4-4303-a203-09e2e9761d16"" Version=""1.0"" Name=""testingDBSrc"" ResourceType=""DbSource"" IsValid=""false"" ServerType=""SqlDatabase"" Type=""SqlDatabase"" ConnectionString=""" + _connectionString + @""" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" ServerVersion=""0.4.2.2""></third></second></first>";
        }


        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("EncryptionResourceUpgrader_Upgrade")]
        // ReSharper disable InconsistentNaming
        public void EncryptionResourceUpgrader_Upgrade_HasMatchin_ExpectReplace()
        {
            _matchAndReplaceWhereAppropriate(_beforeContainingSource, _beforeWithoutSource, _connectionString);
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
            string output = upgrader.EncryptSourceConnectionStrings(_beforeContainingSource);
            output.Should().NotBeNullOrEmpty();
            output.Should().NotBe(_beforeContainingSource);
            output.Should().NotContain(_connectionString);
            Match m = cs.Match(output);
            m.Success.Should().BeTrue();
            m.Groups.Count.Should().BeGreaterOrEqualTo(1);
            m.Groups[1].Success.Should().BeTrue();
            string x = m.Groups[1].Value;
            DpapiWrapper.Decrypt(x).Should().Be(_connectionString);
        }

        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("EncryptionResourceUpgrader_Upgrade")]
        // ReSharper disable InconsistentNaming
        public void EncryptionResourceUpgrader_TwiceUpgrade_DoesNotEncrypt()
        {
            //------------Setup for test--------------------------
            var upgrader = new EncryptionResourceUpgrader();
            Regex cs = new Regex(@"ConnectionString=""([^""]+)""");

            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            string output = upgrader.EncryptSourceConnectionStrings(_beforeContainingSource);
            output.Should().NotBeNullOrEmpty();
            output.Should().NotBe(_beforeContainingSource);
            output.Should().NotContain(_connectionString);
            string output2 = upgrader.EncryptSourceConnectionStrings(output);
            output.Should().NotBeNullOrEmpty();
            output2.Should().Be(output);
        }
    }
}
