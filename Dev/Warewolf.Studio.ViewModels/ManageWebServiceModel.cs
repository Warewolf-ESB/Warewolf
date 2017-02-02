/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;

namespace Warewolf.Studio.ViewModels
{
    public class ManageWebServiceModel : IWebServiceModel
    {
        public IStudioUpdateManager UpdateRepository { get; private set; }
        public IQueryManager QueryProxy { get; private set; }
        public ObservableCollection<IWebServiceSource> Sources
        {
            get
            {
                if (_sources == null)
                {
                    _sources = new ObservableCollection<IWebServiceSource>(QueryProxy.FetchWebServiceSources());
                }
                return _sources;
            }
        }

        readonly IShellViewModel _shell;
        ObservableCollection<IWebServiceSource> _sources;

        public ManageWebServiceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, IShellViewModel shell, IServer server)
        {
            UpdateRepository = updateRepository;
            QueryProxy = queryProxy;
            _shell = shell;
            shell.SetActiveServer(server);
            
        }

        public ManageWebServiceModel()
        {
        }

        #region Implementation of IWebServiceModel

        
        public ICollection<IWebServiceSource> RetrieveSources()
        {
            return new List<IWebServiceSource>(QueryProxy.FetchWebServiceSources());
        }

        public void CreateNewSource()
        {
            _shell.NewWebSource(string.Empty);
        }

        public void EditSource(IWebServiceSource selectedSource)
        {
            _shell.EditResource(selectedSource);
        }

        public string TestService(IWebService inputValues)
        {
            if (UpdateRepository != null)
            {
                return UpdateRepository.TestWebService(inputValues);
            }
            return "Error";
        }

        public void SaveService(IWebService toModel)
        {
            UpdateRepository.Save(toModel);
        }

        #endregion
    }
}
