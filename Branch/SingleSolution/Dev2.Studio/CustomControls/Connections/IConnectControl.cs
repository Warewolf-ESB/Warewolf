using System.Collections.Generic;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.UI
{
    public interface IConnectControl : IHandle<UpdateActiveEnvironmentMessage>
    {
        ICommand ServerChangedCommand { get; set; }

        ICommand EnvironmentChangedCommand { get; set; }

       // IEnvironmentModel SelectedServer { get; set; }

        string LabelText { get; set; }

       // IList<IEnvironmentModel> Servers { get; }

    }
}