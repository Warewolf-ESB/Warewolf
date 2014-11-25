
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows.Input;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Interfaces {
    public interface ILayoutObjectViewModel : IPropertyEditorWizard {
        /// <summary>
        /// The Grid that this cell is bound to
        /// </summary>
        ILayoutGridViewModel LayoutObjectGrid { get; }

        /// <summary>
        /// The Name of this cell
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The xml configuration data for this cell. This data is sent to the webpart at runtime for binding purposes.
        /// </summary>
        string XmlConfiguration { get; set; }        
        string DisplayName { get; set; }
        string WebpartServiceDisplayName { get; set; }
        string WebpartServiceName { get; set; }
        string IconPath { get; set; }
        int GridColumn { get; set; }
        int GridRow { get; set; }
        int GridColumnSpan { get; set; }
        int GridRowSpan { get; set; }
        double LeftBorderThickness { get; set; }
        double TopBorderThickness { get; set; }
        double RightBorderThickness { get; set; }
        double BottomBorderThickness { get; set; }
        bool HasRowBelow { get; }
        bool HasRowAbove { get; }
        bool HasColumnLeft { get; }
        bool HasColumnRight { get; }
        bool IsSelected { get; set; }
        ILayoutObjectViewModel CellRight { get; }
        ILayoutObjectViewModel CellLeft { get; }
        ILayoutObjectViewModel CellAbove { get; }
        ILayoutObjectViewModel CellBelow { get; }
        bool HasContent { get; }
        string PreviousXmlConfig { get; set; }
        string PreviousWebpartServiceName { get; set; }
        string PreviousIconPath { get; set; }
        ICommand ClearAllCommand { get; }
        ICommand EditCommand { get; }
        ICommand CopyCommand { get; }
        ICommand PasteCommand { get; }
        ICommand CutCommand { get; }
        ICommand DeleteCommand { get; }
        ICommand AddRowAboveCommand { get; }
        ICommand AddRowBelowCommand { get; }
        ICommand AddColumnRightCommand { get; }
        ICommand AddColumnLeftCommand { get; }
        ICommand DeleteRowCommand { get; }
        ICommand DeleteColumnCommand { get; }
        ICommand DeleteCellCommand { get; }
        void AddLayoutObject(int row, int col);
        void CopyFrom(ILayoutObjectViewModel cell,bool includeCoOrdinates=false);
        void ClearCellContent(bool updateModelItem);
        void ClearPreviousContents();
        void SetGrid(ILayoutGridViewModel grid);
        //void Dev2Set(string value);
        void Delete(bool updateModelItem);
        void ClearAll();

    }
}
