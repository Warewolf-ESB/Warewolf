using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.MEF;
using Dev2.Integration.Tests.Enums;

namespace Dev2.Integration.Tests.Test_Requiring_Implementation {
    [TestClass]
    public class TestsRequireImplementation {

        #region locals

        private string WebserverURI = ServerSettings.WebserverURI;
                string WebserverUrl = ServerSettings.WebserverURI;
        string tempXmlString = string.Empty;
        DataListValueInjector dataListValueInjector = new DataListValueInjector();
        string DropDownList = "Drop Down List";

        #endregion locals

        // This class is a placeholder for the Test cases that require implementation on it's respective area
        // AREA : BPM

        #region AREA : BPM

        #region TextBox

        // WEBPART : TextBox Miscallaneous Tests
//        [TestMethod()]
//        public void TextBox_InputSpecialCharactersInDisplayText_Expected_CurrentlyInconclusive() {
//            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_DisplayText, @"~!@#$%^&*()_+{}|<>?:""';[]\/.,");
//            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextBox, tempXmlString);
//            string expected = @"
//<span class="""">
// <input type=""text"" name=""Name"" id=""Name"" value="""" maxlength=""""   class=""""  readonly=""true"" disabled=""disabled""/> 
//</span>
//
//
//
//
//<input type=""hidden"" id=""NameErrMsg"" value="""" />";

//            string ResponseData = TestHelper.PostDataToWebserver(PostData);
//            string actual = TestHelper.ReturnFragment(ResponseData);


//            Assert.Inconclusive(expected, actual);
//        }

        #endregion TextBox

        #region Drop-Down List 

        // WEBPART : Drop-Down List Miscallaneous Tests
        
        //[TestMethod()]
        //public void DropDownList_InputSpecialCharactersInDisplayText_Expected_CurrentlyInconclusive() {
        //    string PostData = String.Format("{0}{1}?{2}", WebserverURI, DropDownList, tempXmlString);
        //    string expected = @"<![CDATA[<span class=""internalError"">Webpart Render Error : No Name Assigned</span>]]>";

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    Assert.Inconclusive(expected, actual);
        //}

        #endregion Drop-Down List 

        #region Text Area

        // WEBPART : Text Area Miscallaneous Tests 

//        [TestMethod()]
//        public void TextArea_InputSpecialCharactersInDisplayText_Expected_CurrentlyInconclusive() {
//            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_DisplayText, @"~!@#$%^&*()_+<>?:""{}|\][';/.,");
//            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.TextArea, tempXmlString);
//            string expected = @"<span class="""">
// <textarea rows=""10"" cols=""25"" name=""name"" id=""name""    class=""""  wrap='off' readonly=""true"" disabled=""disabled""/></textarea> 
//</span>
//
//
//<input type=""hidden"" id=""nameErrMsg"" value="""" />";

//            string ResponseData = TestHelper.PostDataToWebserver(PostData);
//            string actual = TestHelper.ReturnFragment(ResponseData);

//            Assert.Inconclusive(expected, actual);
//        }

        #endregion Text Area
       
        #region Password

        // WEBPART : Password Miscallaneous Tests

        //[TestMethod()]
        //public void Password_InputSpecialCharactersInDisplayText_Expected_CurrentlyInconclusive() {
        //    tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.Password_DataList, TestResource.Xpath_DisplayText, @"~!@#$%^&*()_+<>:{}.,;'""");
        //    string PostData = String.Format("{0}{1}?{2}", WebserverUrl, WebpartType.Password, tempXmlString);
        //    string expected = @"<span class=""""><input type=""password"" name=""MyPassword"" id=""MyPassword""  class="""" /></span><input type=""hidden"" id=""Dev2MinChars"" value="""" /><input type=""hidden"" id=""Dev2MaxChars"" value="""" /><input type=""hidden"" id=""MyPasswordErrMsg"" value="""" />".Replace(" ", "");

        //    string ResponseData = TestHelper.PostDataToWebserver(PostData);
        //    string actual = TestHelper.ReturnFragment(ResponseData);

        //    Assert.Inconclusive(expected, actual);
        //}

        #endregion Password

        #region Date-Picker

        // WEBPART : Date-Picker Miscallaneous Tests

//        [TestMethod]
//        public void DatePicker_DispayTextSpecialCharacters_Expected_CurrentlyInconclusive() {
//            tempXmlString = dataListValueInjector.InjectDataListValue(tempXmlString, TestResource.Xpath_DisplayText, @"~!@#$%^&*()_+<>?:""""{}|\][';/.,");
//            string PostData = String.Format("{0}{1}?{2}", WebserverURI, DatePicker, tempXmlString);
//            string expected = @"<span>
//
//<input type=""text"" name=""PickDate"" id=""PickDate"" value=""dd/mm/yyyy""   class=""""  readonly=""true"" disabled=""disabled""/>
//
//<script>
//	// make region date picker
//	$(""#PickDate"").datepicker({ dateFormat: 'dd/mm/yy'});
//	// clear formating when user clicks
//	$(""#PickDate"").click(function(){ 
//		this.value = """";
//	});
//	// do validation for the date field here
//	$(""#PickDate"").mouseup(function(){
//		
//		
//		var pDate = validateDate(this.value, ""dd/mm/yyyy"");
//		if(pDate == false){
//			alert(""Invalid date entered. Please use [ dd/mm/yyyy ] format if manually capturing! "");
//			this.value="""";
//		}
//	});
//	$(document).ready(function(){
//		$(""#PickDate"").val('dd/mm/yyyy');
//	});
//
//</script>
//
//
//
//</span>";

//            string ResponseData = TestHelper.PostDataToWebserver(PostData);
//            string actual = TestHelper.ReturnFragment(ResponseData);

//            Assert.Inconclusive("Inconclusive");

//        }

        #endregion Date-Picker

        #region Button

        // Button Miscallaneous Tests

//        [TestMethod()]
//        public void Button_DispayTextSpecialCharacters_Test()
//        {
//            tempXmlString = dataListValueInjector.InjectDataListValue(TestResource.Button_DataList, TestResource.Xpath_DisplayText, @"~!@#$%^&*()_+<>?"":{},./;'[]\|");
//            string PostData = String.Format("{0}{1}?{2}", WebserverURI, WebpartType.Button, tempXmlString);
//            string expected = @"<button type=""Submit"" name=""ButtonClicked"" id=""ButtonClicked"" value=""~!@#$%^&amp;amp;amp;*()_+=-`|}{[]\"";:'?><,./""   class="""" >~!@#$%^&amp;amp;amp;*()_+=-`|}{[]\"";:'?><,./</button>
//
//<script>
//  $(""#ButtonClicked"").click(function(){
//      
//   });
//</script>";

//            string ResponseData = TestHelper.PostDataToWebserver(PostData);
//            string actual = TestHelper.ReturnFragment(ResponseData);

//            Assert.Inconclusive("Inconclusive");
//        }

        #endregion Button

        #region New Service Wizard 

        // New Service Wizard ServiceOutputDescription Tests

        //[TestMethod]
        //public void ServiceInputOutputDescription_SetValueOfTopLabelTest() {
        //    string PostData = String.Format("{0}{1}", WebserverUrl, "ServiceInputOutputDescription?Dev2ServiceDetailsName=Get Countries&Dev2ServiceDetailsWorkType=Database&Dev2ServiceSetupSourceMethod=sp_GetCountries");
        //    string typeOfWork = @"<label style=""font-weight:bold"">Get Countries - Database - sp_GetCountries</label>";

        //    string responseData = TestHelper.PostDataToWebserver(PostData);
        //    //Assert.IsTrue(responseData.Contains(typeOfWork));
        //    Assert.Inconclusive("WTF, missing work?!");
        //}
        //[TestMethod]
        //public void ServiceInputOutputDescription_SetValueOfTopLabelTest_NoPostData_Expected_BlankDescriptionLabel() {
        //    string PostData = String.Format("{0}{1}", WebserverUrl, "ServiceInputOutputDescription");
        //    string typeOfWork = @"<label style=""font-weight:bold""> -  - </label>";

        //    string responseData = TestHelper.PostDataToWebserver(PostData);
        //    Assert.IsTrue(responseData.Contains(typeOfWork));
        //}

        //[TestMethod]
        //public void ServiceInputOutputDescription_DisplayInputParametersTest_Expected_CurrentlyInconclusive() {
        //    string PostData = String.Format("{0}{1}", WebserverUrl, "ServiceInputOutputDescription?Dev2ServiceDetailsName=Get Countries&Dev2ServiceDetailsWorkType=Database&Dev2ServiceSetupSourceMethod=sp_GetCountries&Dev2ServiceSetupInputs=<Prefix/>&Dev2ServiceSetupOutputs=<GetCities><CountryID/><Description/></GetCities>");
        //    string typeOfWork = @"$('#Dev2ServiceSetupInputs').val('<Dev2ServiceSetupInputs><Prefix /></Dev2ServiceSetupInputs>');";

        //    string responseData = TestHelper.PostDataToWebserver(PostData);
        //    //Assert.IsTrue(responseData.Contains(typeOfWork));

        //    Assert.Inconclusive("WTF, missing work?!");
        //}

        //[TestMethod]
        //public void ServiceInputOutputDescription_DisplayInputParametersTest_NoInputParameterData_Expected_CurrentlyInconclusive() {
        //    string PostData = String.Format("{0}{1}", WebserverUrl, "ServiceInputOutputDescription?Dev2ServiceDetailsName=Get Countries&Dev2ServiceDetailsWorkType=Database&Dev2ServiceSetupSourceMethod=sp_GetCountries");
        //    string typeOfWork = @"$('#Dev2ServiceSetupInputs').val('');";

        //    string responseData = TestHelper.PostDataToWebserver(PostData);
        //    //Assert.IsTrue(responseData.Contains(typeOfWork));

        //    Assert.Inconclusive("WTF, missing work?!");
        //}

        //[TestMethod]
        //public void ServiceInputOutputDescription_DisplayOutputParametersTest_Expected_CurrentlyInconclusive() {
        //    string PostData = String.Format("{0}{1}", WebserverUrl, "ServiceInputOutputDescription?Dev2ServiceDetailsName=Get Countries&Dev2ServiceDetailsWorkType=Database&Dev2ServiceSetupSourceMethod=sp_GetCountries&Dev2ServiceSetupInputs=<Prefix/>&Dev2ServiceSetupOutputs=<GetCities><CountryID/><Description/></GetCities>");
        //    string typeOfWork = @"$('#Dev2ServiceSetupOutputs').val('<Dev2ServiceSetupOutputs><GetCities><CountryID /><Description /></GetCities></Dev2ServiceSetupOutputs>');";

        //    string responseData = TestHelper.PostDataToWebserver(PostData);
        //    //Assert.IsTrue(responseData.Contains(typeOfWork));

        //    Assert.Inconclusive("WTF, missing work?!");
        //}
        //[TestMethod]
        //public void ServiceInputOutputDescription_DisplayOutputParametersTest_NoOutputParameterData_Expected_CurrentlyInconclusive() {
        //    string PostData = String.Format("{0}{1}", WebserverUrl, "ServiceInputOutputDescription");
        //    string typeOfWork = @"$('#Dev2ServiceSetupOutputs').val('');";

        //    string responseData = TestHelper.PostDataToWebserver(PostData);
        //    //Assert.IsTrue(responseData.Contains(typeOfWork));

        //    Assert.Inconclusive("WTF, missing work?!");
        //}

        //[TestMethod]
        //public void ServiceInputOutputDescription_ForceResultsToRecordsetTest_Expected_CurrentlyInconclusive() {
        //    string PostData = String.Format("{0}{1}", WebserverUrl, "ServiceInputOutputDescription?Dev2ServiceDetailsName=Get Countries&Dev2ServiceDetailsWorkType=Database&Dev2ServiceSetupSourceMethod=sp_GetCountries&Dev2ServiceSetupInputs=<Prefix/>&Dev2ServiceSetupOutputs=<GetCities><CountryID/><Description/></GetCities>");
        //    string typeOfWork = @"<tr><td><input type=""checkbox"" id=""Dev2ServiceInputOutputDescriptionForceRecordset"">Force results to Recordset?</input></td></tr>";

        //    string responseData = TestHelper.PostDataToWebserver(PostData);
        //    //Assert.IsTrue(responseData.Contains(typeOfWork));
        //    Assert.Inconclusive("WTF, missing work?!");
        //}

                #endregion New Service Wizard

        #endregion AREA : BPM

    }

}