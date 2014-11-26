
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Interfaces;
using Dev2.UndoFramework;

// ReSharper disable once CheckNamespace
// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Interfaces
// ReSharper restore CheckNamespace
{
    public interface ILayoutGridViewModel
    {

        ICommand UndoCommand { get; }
        ICommand RedoCommand { get; }
        ICommand MoveUpCommand { get; }
        ICommand MoveDownCommand { get; }
        ICommand MoveRightCommand { get; }
        ICommand MoveLeftCommand { get; }
        ICommand CopyCommand { get; }
        ICommand CutCommand { get; }
        ICommand PasteCommand { get; }
        ICommand OpenWebsiteCommand { get; }
        ActionManager UndoFramework { get; set; }
        bool CanPaste { get; }
        bool CanCopyOrCut { get; }
        bool IsAnyCellSelected { get; }
        ObservableCollection<IResourceModel> Websites { get; }
        IResourceModel SelectedWebsite { get; set; }
        string MetaTags { get; set; }
        string FormEncodingType { get; set; }
        IWebActivity ActivityModelItem { get; }
        //IEnvironmentModel ResourceEnvironment { get; }
        int Rows { get; set; }
        int Columns { get; set; }
        IContextualResourceModel ResourceModel { get; }
        string XmlConfiguration { get; }
        ILayoutObjectViewModel ActiveCell { get; }
        ILayoutObjectViewModel CopiedCell { get; set; }
        ILayoutObjectViewModel CutCell { get; set; }
        ObservableCollection<ILayoutObjectViewModel> LayoutObjects { get; }
        void UpdateModelItem();
        void Navigate();
        void SetDefaultSelected();
        void UpdateLayout();
        void RemoveColumn(int col);
        void RemoveRow(int row);
        void SetActiveCell(ILayoutObjectViewModel cell);
        void AddNewUiElement(ILayoutObjectViewModel targetCell, string webPartServiceName, string iconPath);
        void Move(ILayoutObjectViewModel sourceCell, ILayoutObjectViewModel targetCell);
        void BindXmlConfigurationToGrid();
        void Cut();
    }
}
