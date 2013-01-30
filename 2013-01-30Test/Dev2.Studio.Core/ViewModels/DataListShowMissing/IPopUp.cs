using System;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows;
namespace Dev2.Studio.Core.ViewModels{
    public interface IPopUp {
        string Header { get; set; }
        string Description { get; set; }
        string Question { get; set; }
        MessageBoxImage ImageType { get; set; }
        MessageBoxButton Buttons { get; set; }
        MessageBoxResult Show();
    }
}
