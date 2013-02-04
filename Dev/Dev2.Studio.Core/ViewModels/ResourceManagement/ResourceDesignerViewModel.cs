using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.ViewModels.Base;
using System;
using System.Text;

namespace Dev2.Studio.Core.ViewModels {
    public class ResourceDesignerViewModel : SimpleBaseViewModel, IDisposable, IResourceDesignerViewModel 
    {
        #region Class Members

        private IEnvironmentModel _resourceEnvironment = null;
        private IResourceModel _resourceModel;

        #endregion Class Members

        #region Ctor

        public ResourceDesignerViewModel(IResourceModel model, IEnvironmentModel environment) {
            _resourceModel = model;
            _resourceEnvironment = environment;
        }

        #endregion

        #region Properties

        public IEnvironmentModel ResourceEnvironment {
            get{
                return _resourceEnvironment;
            }
        }

        public string ServiceDefinition {
            get {
                if (string.IsNullOrEmpty(_resourceModel.ServiceDefinition)) {
                    _resourceModel.ServiceDefinition = DefaultDefinition();
                }

                return _resourceModel.ServiceDefinition;
            }
            set {
                _resourceModel.ServiceDefinition = value;
                base.OnPropertyChanged("ServiceDefinition");
            }

        }

        public IResourceModel ResourceModel {
            
            get { return _resourceModel; }
            set { _resourceModel = value; }
        }

        #endregion

        #region Methods

        public void BindToModel()
        {
            ResourceModel.ServiceDefinition = ServiceDefinition;
        }

        public void UpdateServiceDefinition() {
            base.OnPropertyChanged("ServiceDefinition");
        }

        private string DefaultDefinition() {

            var sb = new StringBuilder();

            switch (_resourceModel.ResourceType) {
                case ResourceType.Service:
                sb.Append(string.Format("<Service Name=\"{0}\">", 
                _resourceModel.ResourceName));
                sb.Append("\r\n\t\t");
                sb.Append("<Actions>");
                sb.Append("\r\n\t\t\t");
                sb.Append("<Action Name=\"\" Type=\"\" SourceName=\"\" SourceMethod=\"\">");
                sb.Append("\r\n\t\t\t\t");
                sb.Append("<Input Name=\"\" Source=\"\">");
                sb.Append("\r\n\t\t\t\t\t");
                sb.Append("<Validator Type=\"Required\" />");
                sb.Append("\r\n\t\t\t\t");
                sb.Append("</Input>");
                sb.Append("\r\n\t\t\t");
                sb.Append("</Action>");
                sb.Append("\r\n\t\t");
                sb.Append("</Actions>");
                sb.Append("\r\n\t");
                sb.Append("</Service>");
                
                break;

                case ResourceType.Source:
                sb.Append(string.Format("<Source Name=\"{0}\" Type=\"\" ConnectionString=\"\" AssemblyName=\"\" AssemblyLocation=\"\" Uri=\"\" /> ", _resourceModel.ResourceName));
                break;

                default:
                throw new ArgumentException("Unexpected Resource Type");

            }

            return sb.ToString();
        }

        void IDisposable.Dispose() {

        }

        #endregion Methods
    }
}
