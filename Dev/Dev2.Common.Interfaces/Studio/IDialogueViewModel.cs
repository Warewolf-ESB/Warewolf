
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

// ReSharper disable once CheckNamespace
namespace Dev2.Common.Interfaces.Studio
{
    // Sashen Naidoo - 29-08-2012 - PBI 5037
    public interface IDialogueViewModel : IDisposable
    {

        String Title { get; }
        String DescriptionTitleText { get; }
        ImageSource ImageSource { get; }
        String DescriptionText { get; }
        String Hyperlink { get; }
        string HyperlinkText { get; }
        Visibility HyperlinkVisibility { get; }
        ICommand OkCommand { get; }
        //event ClosedOperationEventHandler OnOkClick;
        void SetupDialogue(string title, string description, string imageSourceuri, string descriptionTitleText, string hyperlink = null, string linkText = null);

    }
}
