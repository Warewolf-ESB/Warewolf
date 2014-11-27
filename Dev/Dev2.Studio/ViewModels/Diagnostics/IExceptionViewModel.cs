
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Studio.Feedback;
using Dev2.Studio.Model;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.Diagnostics
{
    public interface IExceptionViewModel
    {
        string OutputText { get; set; }
        string OutputPath { get; set; }
        string ServerLogTempPath { get; set; }
        string StudioLogTempPath { get; set; }
        IWindowManager WindowNavigation { get; set; }
        IFeedbackInvoker FeedbackInvoker { get; set; }
        BindableCollection<ExceptionUiModel> Exception { get; set; }
        string StackTrace { get; set; }
        void Show();
        void SendReport();
        bool Critical { get; set; }
    }
}
