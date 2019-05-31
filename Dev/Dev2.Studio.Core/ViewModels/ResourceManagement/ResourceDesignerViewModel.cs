#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
        IContextualResourceModel _contexttualResourceModel;

        public ResourceDesignerViewModel(IContextualResourceModel model, IServer server)
        {
            _contexttualResourceModel = model;
            _server = server;
        }

        public IServer Server => _server;

        public StringBuilder ServiceDefinition
        {
            get
            {
                if (_contexttualResourceModel.WorkflowXaml == null || _contexttualResourceModel.WorkflowXaml.Length == 0)
                {
                    _contexttualResourceModel.WorkflowXaml = DefaultDefinition();
                }
                return _contexttualResourceModel.WorkflowXaml;
            }
            set
            {
                _contexttualResourceModel.WorkflowXaml = value;
                NotifyOfPropertyChange(() => ServiceDefinition);
                if (ResourceModel != null)
                {
                    ResourceModel.WorkflowXaml = ServiceDefinition;
                }
            }
        }

        public IContextualResourceModel ResourceModel
        {
            get => _contexttualResourceModel;
            set { _contexttualResourceModel = value; }
        }

        StringBuilder DefaultDefinition()
        {
            var sb = new StringBuilder();

            if (_contexttualResourceModel.ResourceType == ResourceType.Service)
            {
                sb.Append($"<Service Name=\"{_contexttualResourceModel.ResourceName}\">");
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
            }
            if (_contexttualResourceModel.ResourceType == ResourceType.Source)
            {
                sb.Append($"<Source Name=\"{_contexttualResourceModel.ResourceName}\" Type=\"\" ConnectionString=\"\" AssemblyName=\"\" AssemblyLocation=\"\" Uri=\"\" /> ");
            }

            return sb;
        }

        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
