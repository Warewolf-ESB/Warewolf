using System;
using System.IO;
using System.Transactions;
using Dev2.Common;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Dev2.Interfaces;
using Warewolf.Security.Encryption;

namespace Dev2.Studio.Core
{
    public interface IMainViewModelSerializer
    {
        IMainViewModel Fetch();
        bool Save();
        string VmPath { get; set; }
    }

    public class MainViewModelSerializer : IMainViewModelSerializer
    {

        private readonly ISerializer _serializer;
        private readonly IFile _fileWraper;
        private readonly IMainViewModel _mainViewModel;

        public MainViewModelSerializer(IMainViewModel mainViewModel)
            : this(new Dev2JsonSerializer(), new FileWrapper())
        {
            _mainViewModel = mainViewModel;
            VmPath = Path.Combine(EnvironmentVariables.WorkspacePath, "Cache", GlobalConstants.ServerWorkspaceID + ".xml");
        }

        public MainViewModelSerializer(ISerializer serializer, IFile fileWraper, IMainViewModel mainViewModel = null)
        {
            _serializer = serializer;
            _fileWraper = fileWraper;
            _mainViewModel = mainViewModel;
        }

        #region Implementation of IMainViewModelSerializer

        public IMainViewModel Fetch()
        {
            var vm = _fileWraper.ReadAllText(VmPath);
            var encrypt = DpapiWrapper.Decrypt(vm);
            var mainViewModel = _serializer.Deserialize<IMainViewModel>(encrypt);
            return mainViewModel;
        }

        public bool Save()
        {

            using (var ts = new TransactionScope())
            {
                try
                {
                    var serialize = _serializer.Serialize(_mainViewModel);
                    var encrypt = DpapiWrapper.Encrypt(serialize);
                    _fileWraper.WriteAllText(VmPath, encrypt);
                    ts.Complete();
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
