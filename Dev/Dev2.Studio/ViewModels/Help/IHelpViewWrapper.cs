
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows;
using System.Windows.Controls;
using Dev2.CustomControls;
using Dev2.Studio.Views.Help;

namespace Dev2.ViewModels.Help
{
    public interface IHelpViewWrapper
    {
        HelpView HelpView { get; }
        WebBrowser WebBrowser { get; }
        CircularProgressBar CircularProgressBar { get; }
        Visibility WebBrowserVisibility { get; set; }
        Visibility CircularProgressBarVisibility { get; set; }
        void Navigate(string uri);
    }
}
