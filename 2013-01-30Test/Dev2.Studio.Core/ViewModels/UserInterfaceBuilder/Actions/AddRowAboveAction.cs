using System;
using System.Linq;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;
using System.Reactive.Linq;
using System.Diagnostics;

namespace Dev2.Studio.Core.Actions {
    public class AddRowAboveAction : AbstractAction {
        readonly ILayoutObjectViewModel _layoutObject;
        readonly ILayoutGridViewModel _layoutViewModel;
        int _newRowNumber = -1;

        public AddRowAboveAction(ILayoutObjectViewModel layoutObject) {
            _layoutObject = layoutObject;
            _layoutViewModel = layoutObject.LayoutObjectGrid;
        }

        protected override void ExecuteCore() {
            var rowAbove = _layoutObject.GridRow - 1;

            _newRowNumber = rowAbove < 0 ? 0 : rowAbove + 1;

            if (rowAbove < 0) {
                //Push every row down 1 row
                _layoutViewModel.LayoutObjects
                    .ToList()
                    .ForEach(c => c.GridRow += 1);
            }
            else {
                //Push only the current row and rows after it down 1 row
                _layoutViewModel.LayoutObjects
                    .Where(c => c.GridRow >= _layoutObject.GridRow)
                    .ToList()
                    .ForEach(c => c.GridRow += 1);
            }

            //Get the columns we need to add to the new row
            var cols = _layoutViewModel.LayoutObjects.Max(c => c.GridColumn) + 1;

            //Add the new row
            Observable.Range(0, cols)
                .Subscribe<int>(c => { _layoutObject.AddLayoutObject(_newRowNumber, c); });


            Debug.Assert(_layoutViewModel.LayoutObjects.Count(c => c.GridRow < 0) == 0);
            //Debug.Assert(_layoutViewModel.LayoutObjects.GroupBy(c=> c.GridColumn))

            var t = from c in _layoutViewModel.LayoutObjects
                    group c by new { c.GridRow, c.GridColumn } into grp
                    where grp.Count() > 1
                    select new { grp.Key };


            Debug.Assert(t.Count() == 0);

            //Let the grid know that we have a new row
            _layoutViewModel.Rows += 1;
        }

        protected override void UnExecuteCore() {
            _layoutViewModel.RemoveRow(_newRowNumber);
        }
    }
}
