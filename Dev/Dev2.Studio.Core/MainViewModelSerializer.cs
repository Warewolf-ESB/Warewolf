using System;
using System.IO;
using System.Transactions;
using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels;
using Newtonsoft.Json;

namespace Dev2.Studio.Core
{
    public interface IMainViewModelSerializer
    {
        MainViewModel Fetch();
        bool Save();
        string VmPath { get; set; }
    }
    

    public class MainViewModelSerializer : IMainViewModelSerializer
    {

        private readonly IFile _fileWraper;
        private readonly IMainViewModel _mainViewModel;

        public MainViewModelSerializer(IMainViewModel mainViewModel)
            : this(new FileWrapper())
        {
            _mainViewModel = mainViewModel;
            VmPath = Path.Combine(EnvironmentVariables.WorkspacePath, "Cache", GlobalConstants.ServerWorkspaceID + ".xml");
        }

        public MainViewModelSerializer(IFile fileWraper, IMainViewModel mainViewModel = null)
        {
            _fileWraper = fileWraper;
            _mainViewModel = mainViewModel;
        }

        #region Implementation of IMainViewModelSerializer

        public MainViewModel Fetch()
        {

            try
            {
                var vm = _fileWraper.ReadAllText(VmPath);
                var mainViewModel = JsonConvert.DeserializeObject<MainViewModel>(vm);
                return mainViewModel;
            }
            catch(Exception e)
            {
                Dev2Logger.Error("Failed to DeserializeObject MainViewModel", e);
                return null;
            }
        }

        public bool Save()
        {

            using (var ts = new TransactionScope())
            {
                try
                {
                    var serializeObject = JsonConvert.SerializeObject(_mainViewModel);
                    _fileWraper.WriteAllText(VmPath, serializeObject);
                    ts.Complete();
                    return true;
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error("Failed to save a serialized MainViewModel", ex);
                    ts.Dispose();
                }

            }
            return false;
        }

        public string VmPath { get; set; }

        #endregion
    }
}
