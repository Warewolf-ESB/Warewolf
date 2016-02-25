using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Designers2.Web_Service_Get;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using TechTalk.SpecFlow;
using Dev2.Common.Interfaces.Core;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Specs
{
    public class WebModel :IWebServiceModel
    {

        public System.Collections.Generic.ICollection<Common.Interfaces.ServerProxyLayer.IWebServiceSource> RetrieveSources()
        {
            
            return new List<IWebServiceSource> { new WebServiceSourceDefinition() { Name = "Heloo", AuthenticationType = AuthenticationType.Windows } };
        }

        public void CreateNewSource()
        {
            throw new NotImplementedException();
        }

        public void EditSource(Common.Interfaces.ServerProxyLayer.IWebServiceSource selectedSource)
        {
            throw new NotImplementedException();
        }

        public string TestService(Common.Interfaces.WebServices.IWebService inputValues)
        {
            throw new NotImplementedException();
        }

        public void SaveService(Common.Interfaces.WebServices.IWebService toModel)
        {
            throw new NotImplementedException();
        }

        public Common.Interfaces.IStudioUpdateManager UpdateRepository
        {
            get { throw new NotImplementedException(); }
        }

        public Common.Interfaces.IQueryManager QueryProxy
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.ObjectModel.ObservableCollection<Common.Interfaces.ServerProxyLayer.IWebServiceSource> Sources
        {
            get { throw new NotImplementedException(); }
        }

        public string HandlePasteResponse(string current)
        {
            throw new NotImplementedException();
        }
    }

    [Binding]
    public class GetWebServiceToolSteps
    {
        [Given(@"I drag Web Get Request Connector Tool onto the design surface")]
        public void GivenIDragWebGetRequestConnectorToolOntoTheDesignSurface()
        {
            var activity = new DsfWebGetActivity();
            var modelItem = ModelItemUtils.CreateModelItem(activity);
            var webmodel = new WebModel();
            WebServiceGetViewModel web = new WebServiceGetViewModel(modelItem,webmodel);
            ScenarioContext.Current.Add("viewModel", web);
            ScenarioContext.Current.Add("model", mockDbServiceModel);
        }
        
        [When(@"I Select WebHeloo as Source")]
        public void WhenISelectWebHelooAsSource()
        {
            var vm = GetViewModel();
            vm.SourceRegion.SelectedSource = vm.SourceRegion.Sources.First(a=>a.Name=="WebHeloo");
            Assert.IsTrue(vm.InputArea.IsVisible);
        }
        
        [When(@"Request header is enabled")]
        public void WhenRequestHeaderIsEnabled()
        {
            var vm = GetViewModel();
            Assert.IsTrue(vm.InputArea.IsVisible);
        }
        
        [When(@"Request Url is enabled")]
        public void WhenRequestUrlIsEnabled()
        {
            var vm = GetViewModel();
    
            Assert.IsTrue(vm.InputArea.IsVisible);
        }
        
        [When(@"Generate Outputs is enabled")]
        public void WhenGenerateOutputsIsEnabled()
        {
            var vm = GetViewModel();
            Assert.IsTrue(vm.GenerateOutputsVisible);
        }
        
        [When(@"Outputs are")]
        public void WhenOutputsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"Recordset is ""(.*)""")]
        public void WhenRecordsetIs(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"there are ""(.*)"" validation errors of ""(.*)""")]
        public void WhenThereAreValidationErrorsOf(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I Select Dev(.*)CountriesWebService as Source")]
        public void WhenISelectDevCountriesWebServiceAsSource(int p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I click Generate Outputs")]
        public void WhenIClickGenerateOutputs()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"Test Request Variables is Successful")]
        public void WhenTestRequestVariablesIsSuccessful()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I click Done")]
        public void WhenIClickDone()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"New is Enabled")]
        public void WhenNewIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"Edit is Enabled")]
        public void WhenEditIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I click Edit")]
        public void WhenIClickEdit()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"Test Inputs is Successful")]
        public void WhenTestInputsIsSuccessful()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I change Source from Dev(.*)CountriesWebService  to Google Address Lookup")]
        public void WhenIChangeSourceFromDevCountriesWebServiceToGoogleAddressLookup(int p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I Select TestingReturnText as Source")]
        public void WhenISelectTestingReturnTextAsSource()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"Test is Successful")]
        public void WhenTestIsSuccessful()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"Outputs is Enabled")]
        public void WhenOutputsIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I click ""(.*)""")]
        public void WhenIClick(string p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I click Cancel")]
        public void WhenIClickCancel()
        {
            ScenarioContext.Current.Pending();
        }
        
        [When(@"I change Source from WebHeloo  to Google Address Lookup")]
        public void WhenIChangeSourceFromWebHelooToGoogleAddressLookup()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Source is Enabled")]
        public void ThenSourceIsEnabled()
        {
           var vm =  GetViewModel();

           Assert.IsTrue(vm.SourceRegion.Sources.Count > 0);
        }

        private static WebServiceGetViewModel GetViewModel()
        {
           return; ScenarioContext.Current["ViewModel"] as WebServiceGetViewModel;
        }

        [Then(@"New is Enabled")]
        public void ThenNewIsEnabled()
        {
            var vm = GetViewModel();

            Assert.IsTrue(vm.SourceRegion.NewSourceCommand.CanExecute());
        }
        
        [Then(@"Edit is Enabled")]
        public void ThenEditIsEnabled()
        {
            var vm = GetViewModel();

            Assert.IsTrue(vm.SourceRegion.NewSourceCommand.CanExecute());
        }
        
        [Then(@"Header is Enabled")]
        public void ThenHeaderIsEnabled()
        {
            var vm = GetViewModel();

            Assert.IsTrue(vm.InputArea.IsVisible);
        }
        
        [Then(@"Header appears as")]
        public void ThenHeaderAppearsAs(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Url is Enabled")]
        public void ThenUrlIsEnabled()
        {
            var vm = GetViewModel();

            Assert.IsTrue(vm.InputArea.IsVisible);
        }
        
        [Then(@"Query is Enabled")]
        public void ThenQueryIsEnabled()
        {
            var vm = GetViewModel();

            Assert.IsTrue(vm.InputArea.IsVisible);
        }
        
        [Then(@"Generate Outputs is Enabled")]
        public void ThenGenerateOutputsIsEnabled()
        {
            var vm = GetViewModel();

            Assert.IsTrue(vm.OutputsRegion.IsVisible);
        }
        
        [Then(@"the Generate Outputs window is opened")]
        public void ThenTheGenerateOutputsWindowIsOpened()
        {
            var vm = GetViewModel();

            Assert.IsTrue(vm.GenerateOutputsVisible.IsVisible);
        }
        
        [Then(@"Variables to test appear as")]
        public void ThenVariablesToTestAppearAs(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the response is loaded")]
        public void ThenTheResponseIsLoaded()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Mapping is Enabled")]
        public void ThenMappingIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"output mappings are")]
        public void ThenOutputMappingsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"""(.*)"" is ""(.*)""")]
        public void ThenIs(string p0, string p1)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"the Dev(.*)CountriesWebService Source tab is opened")]
        public void ThenTheDevCountriesWebServiceSourceTabIsOpened(int p0)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Url is Visible")]
        public void ThenUrlIsVisible()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Query is Visible")]
        public void ThenQueryIsVisible()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Query String equals \?extension=\[\[extension]]&prefix=\[\[prefix]]")]
        public void ThenQueryStringEqualsExtensionExtensionPrefixPrefix()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Url as http://rsaklfsvrtfsbld/integrationTestSite/GetCountries\.ashx")]
        public void ThenUrlAsHttpRsaklfsvrtfsbldIntegrationTestSiteGetCountries_Ashx()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I edit the Header as")]
        public void ThenIEditTheHeaderAs(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Test is Enabled")]
        public void ThenTestIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Paste is Enabled")]
        public void ThenPasteIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I Paste into Response")]
        public void ThenIPasteIntoResponse()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"service input mappings are")]
        public void ThenServiceInputMappingsAre(Table table)
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Mappings is Disabled")]
        public void ThenMappingsIsDisabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I click Generate Outputs")]
        public void ThenIClickGenerateOutputs()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Generate Outputs window is Enabled")]
        public void ThenGenerateOutputsWindowIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Variables is Enabled")]
        public void ThenVariablesIsEnabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I click Test")]
        public void ThenIClickTest()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I click Done")]
        public void ThenIClickDone()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Recordset Name equals rec")]
        public void ThenRecordsetNameEqualsRec()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"Mapping is Disabled")]
        public void ThenMappingIsDisabled()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I click Cancel")]
        public void ThenIClickCancel()
        {
            ScenarioContext.Current.Pending();
        }
        
        [Then(@"I change Source from Google Address Lookup  to WebHeloo")]
        public void ThenIChangeSourceFromGoogleAddressLookupToWebHeloo()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
