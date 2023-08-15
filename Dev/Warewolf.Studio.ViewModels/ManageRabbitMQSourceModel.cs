using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.RabbitMQ;
using System;
using System.Collections.Generic;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Interfaces;
using System.Text;

namespace Warewolf.Studio.ViewModels
{
    public class ManageRabbitMQSourceModel : IRabbitMQSourceModel
    {
        readonly IStudioUpdateManager _updateManager;
        readonly IQueryManager _queryManager;
        readonly IShellViewModel _shellViewModel;
        const string passwordAttr = "Password=";

        public ManageRabbitMQSourceModel(IStudioUpdateManager updateManager, IQueryManager queryManager, IShellViewModel shellViewModel)
        {
            _updateManager = updateManager;
            _queryManager = queryManager;
            _shellViewModel = shellViewModel;
        }

        #region Implementation of IRabbitMQSourceModel

        public ICollection<IRabbitMQServiceSourceDefinition> RetrieveSources() => new List<IRabbitMQServiceSourceDefinition>(_queryManager.FetchRabbitMQServiceSources());

        public void CreateNewSource() => _shellViewModel.NewRabbitMQSource(string.Empty);

        public void EditSource(IRabbitMQServiceSourceDefinition source) => _shellViewModel.EditResource(source);

        public string TestSource(IRabbitMQServiceSourceDefinition source) => _updateManager.TestConnection(source);

        public void SaveSource(IRabbitMQServiceSourceDefinition source) => _updateManager.Save(source);
        
        public IRabbitMQServiceSourceDefinition FetchSource(Guid resourceID)
        {
            var xaml = _queryManager.FetchResourceXaml(resourceID);
            xaml = EscapeValue(xaml);
            var source = new RabbitMQSource(xaml.ToXElement());
            var def = new RabbitMQServiceSourceDefinition(source);
            return def;
        }


        public static StringBuilder EscapeValue(StringBuilder xml)
        {
            try
            {                
                if (xml.Contains(passwordAttr))
                {
                    var firstIndexPOS = xml.ToString().IndexOf(passwordAttr) + passwordAttr.Length;
                    var secondIndexPOS = xml.ToString().IndexOf("VirtualHost=") - 1;
                    var indexLength = secondIndexPOS - firstIndexPOS;
                    var tmpStr = xml.ToString().Substring(firstIndexPOS, indexLength);
                    tmpStr = tmpStr.Replace("&", "&amp;").Replace("'", "&apos;").Replace("\"", "&quot;").Replace(">", "&gt;").Replace("<", "&lt;");
                    xml = xml.Remove(firstIndexPOS, indexLength);
                    xml = xml.Insert(firstIndexPOS, tmpStr);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return xml;
        }
        #endregion Implementation of IRabbitMQSourceModel
    }
}