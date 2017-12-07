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

namespace Dev2.Studio.Core.ViewModels
{
    public class ResourceDesignerViewModel : SimpleBaseViewModel, IDisposable, IDesignerViewModel
    {
        readonly IServer _server;

        public ResourceDesignerViewModel(IContextualResourceModel model, IServer server)
        {
            ResourceModel = model;
            _server = server;
        }

        public IServer Server => _server;

        public StringBuilder ServiceDefinition
        {
            get
            {
                if (ResourceModel.WorkflowXaml == null || ResourceModel.WorkflowXaml.Length == 0)
                {
                    ResourceModel.WorkflowXaml = DefaultDefinition();
                }

                return ResourceModel.WorkflowXaml;
            }
            set
            {
                ResourceModel.WorkflowXaml = value;
                NotifyOfPropertyChange(() => ServiceDefinition);
                if (ResourceModel != null)
                {
                    ResourceModel.WorkflowXaml = ServiceDefinition;
                }
            }

        }

        public IContextualResourceModel ResourceModel { get; set; }

        StringBuilder DefaultDefinition()
        {
            var sb = new StringBuilder();
            switch (ResourceModel.ResourceType)
            {
                case ResourceType.Service:
                    sb.Append($"<Service Name=\"{ResourceModel.ResourceName}\">");
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
                    sb.Append($"<Source Name=\"{ResourceModel.ResourceName}\" Type=\"\" ConnectionString=\"\" AssemblyName=\"\" AssemblyLocation=\"\" Uri=\"\" /> ");
                    break;
                default:
                    break;
            }

            return sb;
        }

        void IDisposable.Dispose()
        {

        }
    }
}
