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
using System.Text;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Warewolf.Resource.Errors;


namespace Dev2.Studio.Core.ViewModels
{
    public class ResourceDesignerViewModel : SimpleBaseViewModel, IDisposable, IDesignerViewModel
    {
        #region Class Members

        private readonly IServer _server;
        private IContextualResourceModel _contexttualResourceModel;

        #endregion Class Members

        #region Ctor

        public ResourceDesignerViewModel(IContextualResourceModel model, IServer server)
        {
            _contexttualResourceModel = model;
            _server = server;
        }

        #endregion

        #region Properties

        public IServer Server => _server;

        public StringBuilder ServiceDefinition
        {
            get
            {
                if(_contexttualResourceModel.WorkflowXaml == null || _contexttualResourceModel.WorkflowXaml.Length == 0)
                {
                    _contexttualResourceModel.WorkflowXaml = DefaultDefinition();
                }

                return _contexttualResourceModel.WorkflowXaml;
            }
            set
            {
                _contexttualResourceModel.WorkflowXaml = value;
                NotifyOfPropertyChange(() => ServiceDefinition);
                if(ResourceModel != null) ResourceModel.WorkflowXaml = ServiceDefinition;
            }

        }

        public IContextualResourceModel ResourceModel
        {

            get { return _contexttualResourceModel; }
            set { _contexttualResourceModel = value; }
        }

        #endregion

        #region Methods

        private StringBuilder DefaultDefinition()
        {

            var sb = new StringBuilder();

            switch(_contexttualResourceModel.ResourceType)
            {
                case ResourceType.Service:
                    sb.Append(string.Format("<Service Name=\"{0}\">",
                    _contexttualResourceModel.ResourceName));
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
                    sb.Append(string.Format("<Source Name=\"{0}\" Type=\"\" ConnectionString=\"\" AssemblyName=\"\" AssemblyLocation=\"\" Uri=\"\" /> ", _contexttualResourceModel.ResourceName));
                    break;

                default:
                    throw new ArgumentException(ErrorResource.UnexpectedResourceType);

            }

            return sb;
        }

        void IDisposable.Dispose()
        {

        }

        #endregion Methods
    }
}
