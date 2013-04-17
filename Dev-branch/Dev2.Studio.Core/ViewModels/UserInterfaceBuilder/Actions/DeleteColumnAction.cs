using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Actions {
    public class DeleteColumnAction : AbstractAction {
        readonly ILayoutGridViewModel _grid;
        readonly ILayoutObjectViewModel _layoutObject;
        readonly List<ILayoutObjectViewModel> _removedList;
        int columnToDelete = -1;


        public DeleteColumnAction(ILayoutObjectViewModel layoutObject) {
            _layoutObject = layoutObject;
            columnToDelete = layoutObject.GridColumn;
            _grid = _layoutObject.LayoutObjectGrid;
            _removedList = new List<ILayoutObjectViewModel>();
        }

        protected override void ExecuteCore() {
            if (_grid.LayoutObjects.Max(c => c.GridColumn) > 0) {
                _grid.Columns -= 1;
                _grid.LayoutObjects
                    .Where(col => col.GridColumn == columnToDelete)
                    .ToList()
                    .ForEach(col => {
                        col.IsSelected = false;
                        _removedList.Add(col);    
                        _grid.LayoutObjects.Remove(col); 
                    
                    });

                _grid.LayoutObjects
                    .Where(col => col.GridColumn > columnToDelete)
                    .ToList()
                    .ForEach(col => col.GridColumn -= 1);


                _grid.UpdateLayout();
                _grid.SetDefaultSelected();

                _grid.UpdateModelItem();
            }
        }

        protected override void UnExecuteCore() {
            _grid.Columns += 1;

            _grid.LayoutObjects
                .Where(row => row.GridColumn >= columnToDelete)
                .ToList()
                .ForEach(row => row.GridColumn += 1);

            _removedList.ForEach(c => {
                _grid.LayoutObjects.Add(c);

            });

            _removedList.Clear();

            _grid.UpdateLayout();

            _grid.UpdateModelItem();
            _grid.SetDefaultSelected();
        }
    }
}
