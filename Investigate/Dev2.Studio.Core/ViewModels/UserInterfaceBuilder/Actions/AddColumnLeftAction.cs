using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;
using System.Reactive.Linq;
using System.Diagnostics;

namespace Dev2.Studio.Core.Actions {
    public class AddColumnLeftAction : AbstractAction {
        readonly ILayoutObjectViewModel _layoutObject;
        readonly ILayoutGridViewModel _layoutViewModel;
        readonly List<Tuple<int, int>> _removalList;
        int _newColumnNumber = -1;

        public AddColumnLeftAction(ILayoutObjectViewModel layoutObject) {
            _layoutObject = layoutObject;
            _layoutViewModel = layoutObject.LayoutObjectGrid;
            _removalList = new List<Tuple<int, int>>();
        }

        protected override void ExecuteCore() {
            var columnLeft = _layoutObject.GridColumn - 1;

            _newColumnNumber = columnLeft < 0 ? 0 : columnLeft + 1;


            if (columnLeft < 0) {
                //Push every column right 1 column
                _layoutViewModel.LayoutObjects
                    .ToList()
                    .ForEach(c => c.GridColumn += 1);
            }
            else {
                //Push only the current column and columns after it right 1 column
                if (_layoutViewModel.LayoutObjects != null)
                    _layoutViewModel.LayoutObjects
                        .Where(c => c.GridColumn >= _layoutObject.GridColumn)
                        .ToList()
                        .ForEach(c => c.GridColumn += 1);
            }

            //Get the columns we need to add to the new column
            if (_layoutViewModel.LayoutObjects != null)
            {
                var rows = _layoutViewModel.LayoutObjects.Max(c => c.GridRow) + 1;

                //Add the new column
                Observable.Range(0, rows)
                    .Subscribe<int>(c => {
                                             _layoutObject.AddLayoutObject(c, _newColumnNumber);
                                             _removalList.Add(new Tuple<int,int>(c, _newColumnNumber));
                    });
            }

            Debug.Assert(_layoutViewModel.LayoutObjects != null && _layoutViewModel.LayoutObjects.Count(c => c.GridColumn < 0) == 0);
            //Let the grid know that we have a new column
            _layoutViewModel.Columns += 1;

            var t = from c in _layoutViewModel.LayoutObjects
                    group c by new { c.GridRow, c.GridColumn } into grp
                    where grp.Count() > 1
                    select new { grp.Key };


            Debug.Assert(!t.Any());
        }

        protected override void UnExecuteCore() {
            _layoutObject.LayoutObjectGrid.RemoveColumn(_newColumnNumber);
        }
    }
}
