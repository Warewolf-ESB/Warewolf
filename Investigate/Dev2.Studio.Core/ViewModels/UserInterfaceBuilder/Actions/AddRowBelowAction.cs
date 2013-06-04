using System;
using System.Linq;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;
using System.Reactive.Linq;
using System.Diagnostics;

namespace Dev2.Studio.Core.Actions {
    public class AddRowBelowAction : AbstractAction {
        readonly ILayoutObjectViewModel _layoutObject;
        readonly ILayoutGridViewModel _layoutViewModel;
        int _newRowNumber = -1;

        public AddRowBelowAction(ILayoutObjectViewModel layoutObject) {
            _layoutObject = layoutObject;
            _layoutViewModel = layoutObject.LayoutObjectGrid;
        }

        protected override void ExecuteCore() {
            _newRowNumber = _layoutObject.GridRow + 1;


            //Push only the current row and rows after it down 1 row
            _layoutViewModel.LayoutObjects
                .Where(c => c.GridRow > _layoutObject.GridRow)
                .ToList()
                .ForEach(c => c.GridRow += 1);

            //Get the columns we need to add to the new row
            var cols = _layoutViewModel.LayoutObjects.Max(c => c.GridColumn) + 1;

            //Add the new row
            Observable.Range(0, cols)
                .Subscribe<int>(c => { _layoutObject.AddLayoutObject(_layoutObject.GridRow + 1, c); });

            Debug.Assert(_layoutViewModel.LayoutObjects.Where(c => c.GridRow < 0).Count() == 0, "Row cannot be less than zero");

            //Let the grid know that we have a new row
            _layoutViewModel.Rows += 1;

            var t = from c in _layoutViewModel.LayoutObjects
                    group c by new { c.GridRow, c.GridColumn } into grp
                    where grp.Count() > 1
                    select new { grp.Key };


            Debug.Assert(t.Count() == 0, "Cannot have multiple layout objects with the same grid row and column value ");


        }

        protected override void UnExecuteCore() {
            _layoutViewModel.RemoveRow(_newRowNumber);
        }
    }
}
