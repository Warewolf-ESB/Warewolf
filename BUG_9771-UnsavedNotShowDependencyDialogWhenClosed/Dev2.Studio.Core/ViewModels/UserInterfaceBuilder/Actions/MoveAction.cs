using System.ComponentModel.Composition;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Actions {
    public class MoveAction : AbstractAction {

        private readonly ILayoutObjectViewModel _source;
        private readonly ILayoutObjectViewModel _target;
        private readonly ILayoutObjectViewModel _copyTarget;

        public MoveAction(ILayoutObjectViewModel source, ILayoutObjectViewModel target) {
            _source = source;
            _target = target;
            if (_target.HasContent) {
                _copyTarget = LayoutObjectViewModelFactory.CreateLayoutObject(_target.LayoutObjectGrid);
                _copyTarget.CopyFrom(_target);
            }
        }

        protected override void ExecuteCore() {
            //Copy all service data from source to target;
            _target.CopyFrom(_source);
            _target.LayoutObjectGrid.UpdateModelItem();
            //Unselect the source item
            _source.IsSelected = false;
            //clear the source item and update the webpage activity definition at the server
            _source.ClearCellContent(true);
            //Set the drop target as the selected cell
            _target.IsSelected = true;
        }

        protected override void UnExecuteCore() {
            //Copy all service data from source to target;
            _source.CopyFrom(_target);
            //Unselect the source item
            _target.IsSelected = false;
            //clear the source item and update the webpage activity definition at the server
            if (_copyTarget != null) {
                _target.CopyFrom(_copyTarget);
                _target.LayoutObjectGrid.UpdateModelItem();
            }
            else {
                _target.ClearCellContent(true);                
            }
            //Set the drop target as the selected cell
            _source.IsSelected = true;
        }
    }
}
