using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;
using Dev2.Studio.Core.Interfaces;
using System.ComponentModel.Composition;
using Dev2.Studio.Core.Factories;

namespace Dev2.Studio.Core.Actions {
    public class ClearAllAction : AbstractAction{

        readonly ILayoutGridViewModel _layoutGrid;
        private List<ILayoutObjectViewModel> _clearedCells;

        public ClearAllAction(ILayoutGridViewModel layoutGrid) {
            _layoutGrid = layoutGrid;
            
        }

        protected override void ExecuteCore() {
            _clearedCells = new List<ILayoutObjectViewModel>();
            _layoutGrid.LayoutObjects.ToList().ForEach(c => {
                var copy = LayoutObjectViewModelFactory.CreateLayoutObject(_layoutGrid);
                copy.CopyFrom(c, true);
                _clearedCells.Add(copy);
                c.ClearCellContent(true);
            });            
        }

        protected override void UnExecuteCore() {
            _layoutGrid.LayoutObjects.Clear();
            _clearedCells.ForEach(c => {
                                      if (_layoutGrid != null) {
                                          _layoutGrid.LayoutObjects.Add(c);
                                      }
                                  });
            _layoutGrid.UpdateModelItem();
        }
    }
}
