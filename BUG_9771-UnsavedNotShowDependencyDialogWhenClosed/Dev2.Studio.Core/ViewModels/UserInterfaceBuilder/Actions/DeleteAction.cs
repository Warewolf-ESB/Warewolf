using System.ComponentModel.Composition;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Actions {
    public class DeleteAction : AbstractAction {

        readonly ILayoutObjectViewModel _deletedCell;
        readonly ILayoutObjectViewModel _copyOfDeletedCell;
        bool _updateModelItem = false;

        public DeleteAction(ILayoutObjectViewModel deletedCell, bool updateModelItem) {
            _deletedCell = deletedCell;
            _updateModelItem = updateModelItem;
            _copyOfDeletedCell = LayoutObjectViewModelFactory.CreateLayoutObject(deletedCell.LayoutObjectGrid);
            _copyOfDeletedCell.CopyFrom(_deletedCell, true);
        }

        protected override void ExecuteCore() {
            _deletedCell.ClearCellContent(_updateModelItem);
        }

        protected override void UnExecuteCore() {
            _deletedCell.CopyFrom(_copyOfDeletedCell, true);
            _deletedCell.LayoutObjectGrid.UpdateModelItem();
        }
    }
}
