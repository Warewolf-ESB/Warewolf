using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.UI
{
    public interface IConnectControl: IDisposable
    {
        ICommand ServerChangedCommand { get; set; }

        ICommand EnvironmentChangedCommand { get; set; }

        IServer SelectedServer { get; set; }

        string LabelText { get; set; }

        IList<IServer> Servers { get; }

        void Handle(UpdateActiveEnvironmentMessage message);
    }
}