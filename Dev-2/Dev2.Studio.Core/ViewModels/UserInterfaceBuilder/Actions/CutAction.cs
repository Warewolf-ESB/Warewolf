using System.ComponentModel.Composition;
using System.Linq;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Actions {
    public class CutAction : AbstractAction {
        readonly ILayoutObjectViewModel _layoutObject;
        private ILayoutObjectViewModel _activeCell;

        public CutAction(ILayoutObjectViewModel layoutObject) {
            _layoutObject = layoutObject;
        }


        protected override void ExecuteCore() {
            _layoutObject.LayoutObjectGrid.CopiedCell =
               LayoutObjectViewModelFactory.CreateLayoutObject(_layoutObject.LayoutObjectGrid);
            _layoutObject.LayoutObjectGrid.CopiedCell.CopyFrom(_layoutObject.LayoutObjectGrid.ActiveCell, true);

            _activeCell = LayoutObjectViewModelFactory.CreateLayoutObject(_layoutObject.LayoutObjectGrid);
            _activeCell.CopyFrom(_layoutObject.LayoutObjectGrid.ActiveCell, true);


            _layoutObject.LayoutObjectGrid.ActiveCell.ClearCellContent(true);
        }

        protected override void UnExecuteCore() {
            if (_layoutObject.LayoutObjectGrid.LayoutObjects != null) {
                var copied = _layoutObject.LayoutObjectGrid.LayoutObjects.FirstOrDefault(c => c.GridRow == _activeCell.GridRow && c.GridColumn == _activeCell.GridColumn);
                if (copied != null) {
                    int idx = _layoutObject.LayoutObjectGrid.LayoutObjects.IndexOf(copied);
                    _layoutObject.LayoutObjectGrid.LayoutObjects.RemoveAt(idx);
                    _layoutObject.LayoutObjectGrid.LayoutObjects.Insert(idx, _activeCell);
                    _layoutObject.LayoutObjectGrid.UpdateModelItem();
                }
            }
        }
    }
}
