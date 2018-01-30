using System;
using System.Windows.Input;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;

namespace Dev2.Common.Interfaces
{
    public interface IManageExchangeSourceViewModel
    {
        string AutoDiscoverUrl { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        int Timeout { get; set; }
        string TestLabel { get; }
        string TestMessage { get; }
        bool TestFailed { get; set; }
        bool Testing { get; }
        bool TestPassed { get; set; }
        string EmailTo { get; set; }
        string AutoDiscoverLabel { get; }
        string UserNameLabel { get; }
        string PasswordLabel { get; }
        string TimeoutLabel { get; }
        string EmailFromLabel { get; }
        string EmailToLabel { get; }
        ICommand OkCommand { get; set; }
        ICommand SendCommand { get; set; }
    }

    public interface IManageExchangeSourceModel
    {
        string TestConnection(IExchangeSource resource);
        void Save(IExchangeSource toDbSource);
        string ServerName { get; }
        IExchangeSource FetchSource(Guid exchangeSourceResourceID);
    }
}

