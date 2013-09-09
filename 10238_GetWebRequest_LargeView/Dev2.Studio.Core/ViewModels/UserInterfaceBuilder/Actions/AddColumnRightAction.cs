using System;
using System.Linq;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;
using System.Reactive.Linq;
using System.Diagnostics;

namespace Dev2.Studio.Core.Actions {
    public class AddColumnRightAction : AbstractAction {

        readonly ILayoutObjectViewModel _layoutObject;
        readonly ILayoutGridViewModel _layoutViewModel;
        int _newColumnNumber = -1;

        public AddColumnRightAction(ILayoutObjectViewModel layoutObject) {
            _layoutObject = layoutObject;
            _layoutViewModel = layoutObject.LayoutObjectGrid;
        }

        protected override void ExecuteCore() {
            _newColumnNumber = _layoutObject.GridColumn + 1;

            //Push only the current column and columns after it right 1 column
            _layoutViewModel.LayoutObjects
                .Where(c => c.GridColumn > _layoutObject.GridColumn)
                .ToList()
                .ForEach(c => c.GridColumn += 1);

            //Get the columns we need to add to the new row
            var rows = _layoutViewModel.LayoutObjects.Max(c => c.GridRow) + 1;

            //Add the new row
            Observable.Range(0, rows)
                .Subscribe<int>(c => { _layoutObject.AddLayoutObject(c, _layoutObject.GridColumn + 1); });

            Debug.Assert(_layoutViewModel.LayoutObjects.Count(c => c.GridColumn < 0) == 0);

            //Let the grid know that we have a new row
            _layoutViewModel.Columns += 1;

            var t = from c in _layoutViewModel.LayoutObjects
                    group c by new { c.GridRow, c.GridColumn } into grp
                    where grp.Count() > 1
                    select new { grp.Key };


            Debug.Assert(!t.Any());
        }

        protected override void UnExecuteCore() {
            _layoutViewModel.RemoveColumn(_newColumnNumber);
        }
    }
}
