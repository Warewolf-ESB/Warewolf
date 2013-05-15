using System.ComponentModel.Composition;
using System.Linq;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Actions {
    public class PasteAction : AbstractAction{

        private readonly ILayoutGridViewModel _layoutGrid;
        private ILayoutObjectViewModel _copiedCell;
        private ILayoutObjectViewModel _snapshotActiveCell;
        private ILayoutObjectViewModel _activeCell;
        private readonly bool _isCopy = false;

        public PasteAction(ILayoutGridViewModel layoutGrid, bool isCopy) {
            _layoutGrid = layoutGrid;
            _isCopy = isCopy;
            _activeCell = LayoutObjectViewModelFactory.CreateLayoutObject(_layoutGrid);
            _copiedCell = LayoutObjectViewModelFactory.CreateLayoutObject(_layoutGrid);
            _copiedCell.CopyFrom(_layoutGrid.CopiedCell, true);
            _snapshotActiveCell = LayoutObjectViewModelFactory.CreateLayoutObject(_layoutGrid);
            _snapshotActiveCell.CopyFrom(_layoutGrid.ActiveCell, true);
        }

        protected override void ExecuteCore() {
            if ((_layoutGrid.CopiedCell != null) && _layoutGrid.CopiedCell.HasContent) {
                int activeCellGridRow = _snapshotActiveCell.GridRow;
                int activeCellGridCol = _snapshotActiveCell.GridColumn;

                _snapshotActiveCell.CopyFrom(_copiedCell);

                var active = _layoutGrid.LayoutObjects.FirstOrDefault(c => c.GridRow == activeCellGridRow && c.GridColumn == activeCellGridCol);
                if (active != null) {
                    _activeCell.CopyFrom(active, true);
                    active.CopyFrom(_copiedCell);
                }
                _snapshotActiveCell.LayoutObjectGrid.UpdateModelItem();
            }            
        }

        protected override void UnExecuteCore() {
            var active = _layoutGrid.LayoutObjects.FirstOrDefault(c => c.GridRow == _snapshotActiveCell.GridRow && c.GridColumn == _snapshotActiveCell.GridColumn);
            if (active != null) {
                active.CopyFrom(_activeCell);
            }
            _snapshotActiveCell.LayoutObjectGrid.UpdateModelItem();
        }
    }
}
