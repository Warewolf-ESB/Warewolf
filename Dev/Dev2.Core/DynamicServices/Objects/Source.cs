#region Change Log
//  Author:         Sameer Chunilall
//  Date:           2010-01-24
//  Log No:         9299
//  Description:    The Source type represents all possible data providers within the Dynamic Service Engine Programming Model
//                  A data provider is responsible for serving xml data to all data consumers.
//                  Data consumers are applications that leverage the Dynamic Service Engine
#endregion

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Unlimited.Framework;

namespace Dev2.DynamicServices {
    #region Using Directives
    using System;


    #endregion

    #region Source Class - Represents a data source
    /// <summary>
    /// Represents a Data Provider in the Dynamic Service Engine Programming Model
    /// </summary>
    public class Source : DynamicServiceObjectBase {
        private WebServiceInvoker _invoker;
        private Uri _webServiceIri;

        #region Constructors

        public Source() : base(enDynamicServiceObjectType.Source){
            Type = enSourceType.Unknown;
            ID = Guid.Empty;
        }

        #endregion

        public Guid ID { get; set; }

        /// <summary>
        /// The type of the data source
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enSourceType Type { get; set; }
        /// <summary>
        /// The connection string to the data source: This applies only to SqlDatabase and MySqlDatabase type data sources
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// The Uri to the web service: REST type web services are not supported by this type
        /// </summary>
        
        public Uri WebServiceUri {
            get {
                return _webServiceIri;
            }
            set {
                _webServiceIri = value;


            }
        }

        public WebServiceInvoker Invoker {
            get {
                return _invoker;
            }
        }
        
        /// <summary>
        /// Name of the assembly that will be used as a plug-in source: This applies only to Plugin type data sources
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// The physical path to the assembly that will be used as a plug-in source: This applies only to Plugin type data sources
        /// </summary>
        public string AssemblyLocation { get; set; }

        /// <summary>
        /// Validates the Source to ensure integrity before the source is added to the dynamic service engine
        /// </summary>
        /// <returns>Boolean</returns>
        public override bool Compile() {
            base.Compile();

            switch (this.Type) {
                case enSourceType.SqlDatabase:
                if (string.IsNullOrEmpty(this.ConnectionString)) {
                    WriteCompileError(Resources.CompilerError_MissingConnectionString);
                }
                break;

                case enSourceType.WebService:
                if (WebServiceUri == null) {
                    WriteCompileError(Resources.CompilerError_MissingUri);
                }
                else {

                    if (!Uri.IsWellFormedUriString(WebServiceUri.ToString(), UriKind.RelativeOrAbsolute)) {

                        WriteCompileError(Resources.CompilerError_InvalidUri);
                    }
                    else {
                        try {
                            _invoker = new WebServiceInvoker(WebServiceUri);
                        }
                        catch (Exception ex) {
                            string data = string.Format("<{0}>{1}\r\nDetail:\r\n{2}</{0}>", "CompilerError", "Unable to generate Web Service Proxy",new UnlimitedObject(ex).XmlString);
                            WriteCompileError(data);
                        }
                    }
                }

                break;

                case enSourceType.Plugin:
                if (string.IsNullOrEmpty(this.AssemblyName)) {
                    WriteCompileError(Resources.CompilerError_MissingAssemblyName);
                }

                if (string.IsNullOrEmpty(this.AssemblyLocation)) {
                    WriteCompileError(Resources.CompilerError_MissingAssemblyLocation);
                }
                break;

                case enSourceType.Unknown:
                WriteCompileError(Resources.CompilerError_InvalidSourceType);
                break;

                default:
                break;

            }

            return this.IsCompiled;
            
        }

    }
    #endregion
}
