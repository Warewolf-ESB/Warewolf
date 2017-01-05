/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.DynamicServices;
using Dev2.TO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.TOTests
{

    [TestClass]
    public class JsonMappingEvaluatedTests
    {
        [TestMethod]
        [Owner("Kerneels Roos")]
        [TestCategory("JsonMappingEvaluated_Constructor")]
        public void JsonMappingEvaluated_Constructor_SetsProperties()
        {
            //------------Setup for test--------------------------
            var dataObject = new DsfDataObject(xmldata: string.Empty, dataListId: Guid.NewGuid());
            dataObject.Environment.Assign("[[a]]", "10",0);
            dataObject.Environment.Assign("[[as]]", "hellow world", 0);
            dataObject.Environment.Assign("[[af]]", "9.9", 0);
            dataObject.Environment.Assign("[[b]]", "20", 0);
            dataObject.Environment.Assign("[[rec(1).a]]", "50", 0);
            dataObject.Environment.Assign("[[rec(1).b]]", "500", 0);
            dataObject.Environment.Assign("[[rec(2).a]]", "60", 0);
            dataObject.Environment.Assign("[[rec(2).b]]", "600", 0);
            //------------Execute Test---------------------------

            // scalar evaluating to atom
            string sn = "[[a]]";
            const string sns = "[[as]]";
            const string snf = "[[af]]";
            string dn = "a";
            const string dns = "as";
            const string dnf = "af";
            var scalarsSn = new[] { "[[x().z]]", "[[x]]", sn, sns, snf };
            var scalarsDn = new[] { "z", "x", dn, dns, dnf };
            var scalarsV = new[] { new object[] { null }, (object)null, 10, "hellow world", 9.9 };
            for (int i = 0; i < scalarsSn.Length; i++)
            {
                var jsonMappingEvaluatedLocal = new JsonMappingEvaluated(
                    dataObject.Environment,
                    scalarsSn[i]);
                //------------Assert Results-------------------------
                jsonMappingEvaluatedLocal.Should().NotBeNull();
                jsonMappingEvaluatedLocal.Simple.Should().NotBeNull();
                jsonMappingEvaluatedLocal.Simple.SourceName.Should().Be(scalarsSn[i]);
                jsonMappingEvaluatedLocal.Simple.DestinationName.Should().Be(scalarsDn[i]);
                if (i != 0)
                {
                    //((object[])((CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult)jsonMappingEvaluatedLocal.EvalResult).Item.GetValue(0))[0].Should().Be(((object[])((CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult)dataObject.Environment.EvalForJson(scalarsSn[i])).Item.GetValue(0))[0]);
                    jsonMappingEvaluatedLocal.EvalResult.Should().Be(dataObject.Environment.EvalForJson(scalarsSn[i]));
                    jsonMappingEvaluatedLocal.EvalResultAsObject.Should().Be(scalarsV[i]);
                }
                jsonMappingEvaluatedLocal.Count.Should().Be(1);
            }


            // recordset
            sn = "[[rec().a]]"; dn = "a";
            var jsonMappingEvaluated = new JsonMappingEvaluated(dataObject.Environment, sn);
            //------------Assert Results-------------------------
            jsonMappingEvaluated.Should().NotBeNull();
            jsonMappingEvaluated.Simple.Should().NotBeNull();
            jsonMappingEvaluated.Simple.SourceName.Should().Be(sn);
            jsonMappingEvaluated.Simple.DestinationName.Should().Be(dn);
            ((CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult)jsonMappingEvaluated.EvalResult).Item.GetValue(0).Should().Be(
                ((CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult)dataObject.Environment.Eval(sn,0)).Item.GetValue(0));
            jsonMappingEvaluated.Count.Should().Be(1);

            // recordset name
            sn = "[[rec()]]"; dn = "rec";
            jsonMappingEvaluated = new JsonMappingEvaluated(dataObject.Environment, sn);
            //------------Assert Results-------------------------
            jsonMappingEvaluated.Should().NotBeNull();
            jsonMappingEvaluated.Simple.Should().NotBeNull();
            jsonMappingEvaluated.Simple.SourceName.Should().Be(sn);
            jsonMappingEvaluated.Simple.DestinationName.Should().Be(dn);
            //jsonMappingEvaluated.EvalResult.Should().Be(dataObject.Environment.Eval(sn));
            //((CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult)jsonMappingEvaluated.EvalResult).Item.Data["a"][0].Should().Be(
            //  ((CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult)dataObject.Environment.Eval(sn)).Item.Data["a"][0]);
            jsonMappingEvaluated.Count.Should().Be(1);
        }
    }
}
