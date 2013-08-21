using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Actions {
    public class DeleteRowAction : AbstractAction {
        readonly ILayoutGridViewModel _grid;
        readonly ILayoutObjectViewModel _layoutObject;
        readonly List<ILayoutObjectViewModel> _removedList;
        readonly int _rowToDelete = -1;

        public DeleteRowAction(ILayoutObjectViewModel layoutObject) {
            _layoutObject = layoutObject;
            _rowToDelete = _layoutObject.GridRow;
            _grid = layoutObject.LayoutObjectGrid;
            _removedList = new List<ILayoutObjectViewModel>();
        }

        protected override void ExecuteCore() {
            if (_grid.LayoutObjects.Max(c => c.GridRow) > 0) {

                _grid.Rows -= 1;

                _grid.LayoutObjects
                    .Where(row => row.GridRow == _rowToDelete)
                    .ToList()
                    .ForEach(row => {
                        row.IsSelected = false;
                        _removedList.Add(row);
                        _grid.LayoutObjects.Remove(row);
                        
                    });

                _grid.LayoutObjects
                    .Where(row => row.GridRow > _rowToDelete)
                    .ToList()
                    .ForEach(row => row.GridRow -= 1);

                

                _grid.SetDefaultSelected();

                _grid.UpdateModelItem();
            }
        }

        protected override void UnExecuteCore() {
            _grid.Rows += 1;

            _grid.LayoutObjects
                .Where(row => row.GridRow >= _rowToDelete)
                .ToList()
                .ForEach(row => row.GridRow += 1);

            _removedList.ForEach(c => _grid.LayoutObjects.Add(c));            

            _removedList.Clear();
            _grid.UpdateModelItem();

            
            _grid.SetDefaultSelected();

        }
    }
}
