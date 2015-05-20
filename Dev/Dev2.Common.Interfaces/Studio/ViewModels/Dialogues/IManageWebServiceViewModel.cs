using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Data.ServiceModel;

namespace Dev2.Common.Interfaces.Studio.ViewModels.Dialogues
{
    public interface IManageWebServiceViewModel
    {

        // ReSharper disable UnusedParameter.Global
        /// <summary>
        /// currently selected web resquest method post get
        /// </summary>
        WebRequestMethod SelectedWebRequestMethod { get; set; }

        /// <summary>
        /// the collections of supported web request methods
        /// </summary>
        ICollection<WebRequestMethod> WebRequestMethods { get; set; }

        /// <summary>
        /// Command to create a new web source 
        /// </summary>
        ICommand NewWebSourceCommand { get; set; }
        /// <summary>
        /// Available Sources
        /// </summary>
        ICollection<IWebServiceSource> Sources { get;}
        
        
        /// <summary>
        /// Currently Selected Source
        /// </summary>
        IWebServiceSource SelectedSource { get; set; }


        /// <summary>
        /// The underlying Web service
        /// </summary>
        IWebService WebService { get; set; }
       
        /// <summary>
        /// Label for selecteing a header
        /// </summary>
        string SelectSourceHeader { get; set; }
        /// <summary>
        /// Request headers
        /// </summary>
        ICollection<INameValue> Headers { get; set; }
        /// <summary>
        /// Select the headers
        /// </summary>
        string SelectHeadersHeader { get; set; }

        /// <summary>
        /// The Web service query string
        /// </summary>
        string RequestUrlQuery { get; set; }

        /// <summary>
        /// The URL as per the Source
        /// </summary>
        string SourceUrl { get; set; }

        /// <summary>
        ///  The form Header
        /// </summary>
        string RequestUrlHeader { get; set; }

        /// <summary>
        /// The Request Body
        /// </summary>
        string RequestBody { get; set; }

        /// <summary>
        /// Request Body Header
        /// </summary>
        string RequestBodyHeader { get; set; }

        /// <summary>
        /// the warewolf variables defined in the body,headers and query string
        /// </summary>
        ICollection<INameValue> Variables { get; set; }

        /// <summary>
        /// the response from the web service
        /// </summary>
        string Response { get; set; }

        /// <summary>
        /// the command to paste a command into the response
        /// </summary>
        ICommand PasteResponseCommand { get; set; }


        /// <summary>
        /// Test a web Service
        /// </summary>
        ICommand TestCommand { get; set; }

        /// <summary>
        /// Text for the Test button
        /// </summary>
        string TestCommandButtonText { get; set; }

        /// <summary>
        /// Text for the Save button
        /// </summary>
        string SaveCommandText { get; set; }

        /// <summary>
        /// command to save
        /// </summary>
        ICommand SaveCommand { get; set; }

        /// <summary>
        /// Cancel Command
        /// </summary>
        ICommand CancelCommand { get; set; }

        /// <summary>
        /// Has the Source changed
        /// </summary>
        bool HasChanged { get; set; }
        // ReSharper restore UnusedParameter.Global


        /// <summary>
        /// List OfInputs
        /// </summary>
        ICollection<IWebserviceOutputs> Outputs {get;set;}

        /// <summary>
        /// List Of Outputs
        /// </summary>
        ICollection<IWebserviceInputs> Inputs { get; set; }

        /// <summary>
        /// Input region ColumnHeader
        /// </summary>
        string InputsHeader { get; set; }

        /// <summary>
        ///  input column Header
        /// </summary>
        string InputNameHeader { get; set; }

        /// <summary>
        /// Default value column Header
        /// </summary>
        string DefaultValueHeader { get; set; }

        /// <summary>
        /// IsRequiredFieldColumnHeader
        /// </summary>
        string RequiredFieldHeader { get; set; }

        /// <summary>
        /// EmptyIsNullColumnHeader
        /// </summary>
        string EmptyIsNullHeader { get; set; }

        /// <summary>
        /// Outputs RegionHeader
        /// </summary>
        string OutputsHeader { get; set; }

        /// <summary>
        /// Output Column BName Header
        /// </summary>
        string OutputName { get; set; }

        /// <summary>
        /// Output Column alias Header
        /// </summary>
        string OutputAliasHeader { get; set; }
        string ResourceName { get; set; }
    }

    public interface IWebService
    {
        string Name { get; set; }
        string Path { get; set; }
        IWebServiceSource Source { get; set; }
        IList<IWebserviceInputs> Inputs { get; set; }
        IList<IWebserviceOutputs> OutputMappings { get; set; }
        string QueryString { get; set; }
        Guid Id { get; set; }
    }

    public interface IWebserviceInputs
    {
    }

    public interface IWebserviceOutputs
    {
    }
}