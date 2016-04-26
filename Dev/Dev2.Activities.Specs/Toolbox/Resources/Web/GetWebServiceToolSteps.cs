//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Dev2.Activities.Designers2.Core;
//using Dev2.Activities.Designers2.Web_Service_Get;
//using Dev2.Common.Interfaces;
//using Dev2.Common.Interfaces.ServerProxyLayer;
//using Dev2.Common.Interfaces.WebService;
//using TechTalk.SpecFlow;
//using Dev2.Common.Interfaces.Core;
//using Dev2.Common.Interfaces.DB;
//using Dev2.Communication;
//using Dev2.Runtime.ServiceModel.Data;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Dev2.Studio.Core.Activities.Utils;
//using Warewolf.Core;

//namespace Dev2.Activities.Specs
//{
//    public class WebModel :IWebServiceModel
//    {

//        public ICollection<Common.Interfaces.ServerProxyLayer.IWebServiceSource> RetrieveSources()
//        {

//            return new List<IWebServiceSource> { new WebServiceSourceDefinition() { Name = "WebHeloo", AuthenticationType = AuthenticationType.Windows }, new WebServiceSourceDefinition() { Name = "Dev2CountriesWebService", DefaultQuery = "?extension=[[extension]]&prefix=[[prefix]]", HostName = "http://rsaklfsvrtfsbld/integrationTestSite/GetCountries.ashx " } };
//        }

//        public void CreateNewSource()
//        {
//            throw new NotImplementedException();
//        }

//        public void EditSource(Common.Interfaces.ServerProxyLayer.IWebServiceSource selectedSource)
//        {
//            EditSourceCalled = true;
//        }

//        public string TestService(Common.Interfaces.WebServices.IWebService inputValues)
//        {
//            if (inputValues.Source.Name == "WebHeloo")
//            {
//                var webService = new WebService();
//                webService.RequestResponse = "{\"rec\" : [{\"a\":\"1\",\"b\":\"a\"}]}";
//                webService.Recordsets = new RecordsetList();
//                webService.Recordsets.Add(new Recordset() { Fields = new List<RecordsetField>() { new RecordsetField() { Name = "a", RecordsetAlias = "[[rec().a]]" }, new RecordsetField() { Name = "b", RecordsetAlias = "[[rec().b]]" } } });
//                return new Dev2JsonSerializer().Serialize(webService);
//            }
//            return "bob";
//        }

//        public void SaveService(Common.Interfaces.WebServices.IWebService toModel)
//        {
//            throw new NotImplementedException();
//        }

//        public Common.Interfaces.IStudioUpdateManager UpdateRepository
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public Common.Interfaces.IQueryManager QueryProxy
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public System.Collections.ObjectModel.ObservableCollection<Common.Interfaces.ServerProxyLayer.IWebServiceSource> Sources
//        {
//            get { throw new NotImplementedException(); }
//        }
//        public bool EditSourceCalled { get; set; }

//        public string HandlePasteResponse(string current)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    [Binding]
//    public class GetWebServiceToolSteps
//    {
//        [Given(@"I drag Web Get Request Connector Tool onto the design surface")]
//        public void GivenIDragWebGetRequestConnectorToolOntoTheDesignSurface()
//        {
//            var activity = new DsfWebGetActivity();
//            var modelItem = ModelItemUtils.CreateModelItem(activity);
//            var webmodel = new WebModel();
//            WebServiceGetViewModel web = new WebServiceGetViewModel(modelItem,webmodel);
//            ScenarioContext.Current.Add("viewModel", web);
//            ScenarioContext.Current.Add("model", webmodel);
//        }

//        [When(@"I Select ""(.*)"" as web Source")]
//        public void WhenISelectAsWebSource(string p0)
//        {
//            var vm = GetViewModel();
//            vm.SourceRegion.SelectedSource = vm.SourceRegion.Sources.First(a => a.Name == p0);
//            Assert.IsTrue(vm.InputArea.IsEnabled);
//        }

//        [When(@"I Select WebHeloo as Source")]
//        public void WhenISelectWebHelooAsSource()
//        {
//            var vm = GetViewModel();
//            vm.SourceRegion.SelectedSource = vm.SourceRegion.Sources.First(a=>a.Name=="WebHeloo");
//            Assert.IsTrue(vm.InputArea.IsEnabled);
//        }

//        [When(@"Request header is enabled")]
//        public void WhenRequestHeaderIsEnabled()
//        {
//            var vm = GetViewModel();
//            Assert.IsTrue(vm.InputArea.IsEnabled);
//        }

//        [When(@"Request Url is enabled")]
//        public void WhenRequestUrlIsEnabled()
//        {
//            var vm = GetViewModel();

//            Assert.IsTrue(vm.InputArea.IsEnabled);
//        }

//        [When(@"Generate Outputs is enabled")]
//        public void WhenGenerateOutputsIsEnabled()
//        {
//            var vm = GetViewModel();
//            Assert.IsTrue(vm.TestInputCommand.CanExecute());
//        }

//        [When(@"Outputs are")]
//        public void WhenOutputsAre(Table table)
//        {
//            var outputs = GetViewModel().OutputsRegion.Outputs;
//            foreach(var row in table.Rows)
//            {
//                Assert.IsTrue(outputs.Any(a=>a.MappedFrom==row[0] && a.MappedTo==row[1]));

//            }
//        }

//        [When(@"Recordset is ""(.*)""")]
//        public void WhenRecordsetIs(string p0)
//        {
//            if(p0=="")
//            {
//                Assert.IsTrue(String.IsNullOrEmpty(GetViewModel().OutputsRegion.RecordsetName));
//            }
//            else
//            Assert.AreEqual(GetViewModel().OutputsRegion.RecordsetName,p0);
//        }

//        [When(@"there are ""(.*)"" validation errors of ""(.*)""")]
//        public void WhenThereAreValidationErrorsOf(string p0, string p1)
//        {
//            if(p0.ToLower()=="no")
//                Assert.IsTrue(GetViewModel().Errors == null || GetViewModel().Errors.Count == 0);
//        }

//        [When(@"I Select Dev(.*)CountriesWebService as Source")]
//        public void WhenISelectDevCountriesWebServiceAsSource(int p0)
//        {

//        }

//        [When(@"I click Generate Outputs")]
//        public async void WhenIClickGenerateOutputs()
//        {
//            await GetViewModel().TestInputCommand.Execute();
//        }

//        [When(@"Test Request Variables is Successful")]
//        public void WhenTestRequestVariablesIsSuccessful()
//        {
//            GetViewModel().ManageServiceInputViewModel.TestCommand.Execute(null);
//        }

//        [When(@"I click Done")]
//        public void WhenIClickDone()
//        {
//            GetViewModel().ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping>() { new ServiceOutputMapping("CountryID", "CountryID", ""), new ServiceOutputMapping("Description", "Description", "") };

//            var manageWebServiceInputViewModel = GetViewModel().ManageServiceInputViewModel as ManageWebServiceInputViewModel;
//            if(manageWebServiceInputViewModel != null)
//            {
//                manageWebServiceInputViewModel.ExecuteOk();
//            }
//            else
//            {
//                Assert.Fail("Ballet dancing stress ball");
//            }
//        }

//        [When(@"New is Enabled")]
//        public void WhenNewIsEnabled()
//        {
//            Assert.IsTrue(GetViewModel().SourceRegion.NewSourceCommand.CanExecute(null));
//        }

//        [When(@"Edit is Enabled")]
//        public void WhenEditIsEnabled()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [When(@"I click Edit")]
//        public void WhenIClickEdit()
//        {
//            GetViewModel().SourceRegion.EditSourceCommand.Execute(null);
//        }

//        [When(@"Test Inputs is Successful")]
//        public void WhenTestInputsIsSuccessful()
//        {

//        }

//        [When(@"I change Source from Dev(.*)CountriesWebService  to Google Address Lookup")]
//        public void WhenIChangeSourceFromDevCountriesWebServiceToGoogleAddressLookup(int p0)
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [When(@"I Select TestingReturnText as Source")]
//        public void WhenISelectTestingReturnTextAsSource()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [When(@"Test is Successful")]
//        public void WhenTestIsSuccessful()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [When(@"Outputs is Enabled")]
//        public void WhenOutputsIsEnabled()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [When(@"I click ""(.*)""")]
//        public void WhenIClick(string p0)
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [When(@"I click Cancel")]
//        public void WhenIClickCancel()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [When(@"I change Source from WebHeloo  to Google Address Lookup")]
//        public void WhenIChangeSourceFromWebHelooToGoogleAddressLookup()
//        {
//            ScenarioContext.Current.Pending();
//        }
//        [When(@"I change Source from ""(.*)""  to ""(.*)""")]
//        public void WhenIChangeSourceFromTo(string p0, string p1)
//        {
//            var vm = GetViewModel();
//            vm.SourceRegion.SelectedSource = vm.SourceRegion.Sources.First(a => a.Name == p1);
//            Assert.IsTrue(vm.InputArea.IsEnabled);
//        }


//        [Then(@"Source is Enabled")]
//        public void ThenSourceIsEnabled()
//        {
//           var vm =  GetViewModel();

//           Assert.IsTrue(vm.SourceRegion.Sources.Count > 0);
//        }

//        private static WebServiceGetViewModel GetViewModel()
//        {
//            return ScenarioContext.Current["viewModel"] as WebServiceGetViewModel;
//        }

//        private static WebModel GetModel()
//        {
//            return ScenarioContext.Current["model"] as WebModel;
//        }


//        [Then(@"New is Enabled")]
//        public void ThenNewIsEnabled()
//        {
//            var vm = GetViewModel();

//            Assert.IsTrue(vm.SourceRegion.NewSourceCommand.CanExecute(null));
//        }

//        [Then(@"Edit is Enabled")]
//        public void ThenEditIsEnabled()
//        {
//            var vm = GetViewModel();

//            Assert.IsTrue(vm.SourceRegion.NewSourceCommand.CanExecute(null));
//        }

//        [Then(@"Edit is Disabled")]
//        public void ThenEditIsDisabled()
//        {
//            var vm = GetViewModel();

//            Assert.IsFalse(vm.SourceRegion.EditSourceCommand.CanExecute(null));
//        }


//        [Then(@"Header is Enabled")]
//        public void ThenHeaderIsEnabled()
//        {
//            var vm = GetViewModel();

//            Assert.IsTrue(vm.InputArea.IsEnabled);
//        }

//        [Then(@"Header is added as")]
//        public void ThenHeaderIsAddedAs(Table table)
//        {
//            foreach(var header in table.Rows)
//            {
//                GetViewModel().InputArea.Headers.Add(new NameValue( header[0],header[1]));
//            }
//        }


//        [Then(@"Header appears as")]
//        public void ThenHeaderAppearsAs(Table table)
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"Url is Enabled")]
//        public void ThenUrlIsEnabled()
//        {
//            var vm = GetViewModel();

//            Assert.IsTrue(vm.InputArea.IsEnabled);
//        }

//        [Then(@"Query is Enabled")]
//        public void ThenQueryIsEnabled()
//        {
//            var vm = GetViewModel();

//            Assert.IsTrue(vm.InputArea.IsEnabled);
//        }

//        [Then(@"Generate Outputs is Enabled")]
//        public void ThenGenerateOutputsIsEnabled()
//        {
//            var vm = GetViewModel();

//            Assert.IsTrue(vm.TestInputCommand.CanExecute());
//        }

//        [Then(@"the Generate Outputs window is opened")]
//        public void ThenTheGenerateOutputsWindowIsOpened()
//        {
//            var vm = GetViewModel();

//            Assert.IsTrue(vm.GenerateOutputsVisible);
//        }

//        [Then(@"Variables to test appear as")]
//        public void ThenVariablesToTestAppearAs(Table table)
//        {
//           var vm = GetViewModel();
//            foreach(var row in table.Rows)
//            {
//                Assert.IsTrue(vm.ManageServiceInputViewModel.InputArea.Inputs.Any(a=>a.Name==row[0]));
//                vm.ManageServiceInputViewModel.InputArea.Inputs.First(a => a.Name == row[0]).Value=row[1];
//            }
//        }

//        [Then(@"the response is loaded")]
//        public void ThenTheResponseIsLoaded()
//        {
//            GetViewModel().ManageServiceInputViewModel.OutputArea.Outputs = new List<IServiceOutputMapping>() { new ServiceOutputMapping("CountryID", "CountryID",""), new ServiceOutputMapping("Description","Description","") };

//            Assert.IsFalse(GetViewModel().ManageServiceInputViewModel.PasteResponseVisible);
//        }

//        [Then(@"Mapping is Enabled")]
//        public void ThenMappingIsEnabled()
//        {

//        }

//        [Then(@"output mappings are")]
//        public void ThenOutputMappingsAre(Table table)
//        {

//        }

//        [Then(@"""(.*)"" is ""(.*)""")]
//        public void ThenIs(string p0, string p1)
//        {

//        }

//        [Then(@"the Dev(.*)CountriesWebService Source tab is opened")]
//        public void ThenTheDevCountriesWebServiceSourceTabIsOpened(int p0)
//        {
//            Assert.IsTrue(GetModel().EditSourceCalled);
//        }

//        [Then(@"Url is Visible")]
//        public void ThenUrlIsVisible()
//        {
//            Assert.IsTrue(GetViewModel().InputArea.IsEnabled);
//        }

//        [Then(@"Query is Visible")]
//        public void ThenQueryIsVisible()
//        {
//            Assert.IsTrue(GetViewModel().InputArea.IsEnabled);
//        }

//        [Then(@"Web Inputs is Enabled")]
//        public void ThenWebInputsIsEnabled()
//        {
//            Assert.IsTrue(GetViewModel().ManageServiceInputViewModel.IsEnabled);
//        }

//        [Then(@"Web Test is Enabled")]
//        public void ThenWebTestIsEnabled()
//        {
//            Assert.IsTrue(GetViewModel().ManageServiceInputViewModel.IsEnabled);
//        }

//        [Then(@"Web Paste is Enabled")]
//        public void ThenWebPasteIsEnabled()
//        {
//            Assert.IsTrue(GetViewModel().ManageServiceInputViewModel.IsEnabled);
//        }


//        [Then(@"Query String equals \?extension=\[\[extension]]&prefix=\[\[prefix]]")]
//        public void ThenQueryStringEqualsExtensionExtensionPrefixPrefix()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"Query String equals ""(.*)""")]
//        public void ThenQueryStringEquals(string p0)
//        {
//            Assert.AreEqual(p0,GetViewModel().InputArea.QueryString);
//        }

//        [Then(@"Url as ""(.*)""")]
//        public void ThenUrlAs(string p0)
//        {
//            Assert.AreEqual(p0.Trim(), GetViewModel().InputArea.RequestUrl.Trim());
//        }


//        [Then(@"Url as http://rsaklfsvrtfsbld/integrationTestSite/GetCountries\.ashx")]
//        public void ThenUrlAsHttpRsaklfsvrtfsbldIntegrationTestSiteGetCountries_Ashx()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"I edit the Header as")]
//        public void ThenIEditTheHeaderAs(Table table)
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"Test is Enabled")]
//        public void ThenTestIsEnabled()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"Paste is Enabled")]
//        public void ThenPasteIsEnabled()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"I Paste into Response")]
//        public void ThenIPasteIntoResponse()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"service input mappings are")]
//        public void ThenServiceInputMappingsAre(Table table)
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"Mappings is Disabled")]
//        public void ThenMappingsIsDisabled()
//        {
//            Assert.IsFalse(GetViewModel().OutputsRegion.IsEnabled);
//        }

//        [Then(@"I click Generate Outputs")]
//        public void ThenIClickGenerateOutputs()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"Generate Outputs window is Enabled")]
//        public void ThenGenerateOutputsWindowIsEnabled()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"Variables is Enabled")]
//        public void ThenVariablesIsEnabled()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"I click Test")]
//        public void ThenIClickTest()
//        {
//            ScenarioContext.Current.Pending();
//        }
//        [Then(@"Web Outputs appear as")]
//        public void ThenWebOutputsAppearAs(Table table)
//        {
//            var output = table.Rows[0][0];
//            Assert.AreEqual(output,GetViewModel().ManageServiceInputViewModel.TestResults);
//        }


//        [Then(@"I click Done")]
//        public void ThenIClickDone()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"Recordset Name equals rec")]
//        public void ThenRecordsetNameEqualsRec()
//        {

//        }

//        [Then(@"web Recordset Name equals ""(.*)""")]
//        public void ThenWebRecordsetNameEquals(string p0)
//        {
//            Assert.AreEqual(GetViewModel().OutputsRegion.RecordsetName, p0);
//        }


//        [Then(@"Mapping is Disabled")]
//        public void ThenMappingIsDisabled()
//        {
//            ScenarioContext.Current.Pending();
//        }

//        [Then(@"I click Cancel")]
//        public void ThenIClickCancel()
//        {
//            // ReSharper disable once PossibleNullReferenceException
//            (GetViewModel().ManageServiceInputViewModel as ManageWebServiceInputViewModel).ExecuteClose();
//        }

//        [Then(@"I change Source from Google Address Lookup  to WebHeloo")]
//        public void ThenIChangeSourceFromGoogleAddressLookupToWebHeloo()
//        {

//        }
//    }
//}
