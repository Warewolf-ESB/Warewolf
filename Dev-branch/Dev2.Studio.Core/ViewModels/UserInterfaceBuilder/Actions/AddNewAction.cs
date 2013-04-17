using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;

namespace Dev2.Studio.Core.Actions {
    public class AddNewAction : AbstractAction {

        readonly ILayoutObjectViewModel _targetCell;
        private readonly string _webPartServiceName;
        private readonly string _iconPath;
        private string _currentConfiguration;

        public AddNewAction(ILayoutObjectViewModel targetCell, string webPartServiceName, string iconPath) {
            _targetCell = targetCell;
            _webPartServiceName = webPartServiceName;
            _iconPath = iconPath;
        }

        protected override void ExecuteCore() {
            _targetCell.WebpartServiceName = _webPartServiceName;
            _targetCell.IconPath = _iconPath;
            if (!string.IsNullOrEmpty(_currentConfiguration)) {
                if (!_currentConfiguration.ToUpper().Contains("EMPTY")) {
                    _targetCell.XmlConfiguration = _currentConfiguration;
                }
            }
            else {
                _targetCell.WebpartServiceDisplayName = _webPartServiceName;
            }
            if (_targetCell.WebpartServiceName.Equals("File")) {
                _targetCell.LayoutObjectGrid.FormEncodingType = "multipart/form-data";
            }
            _targetCell.LayoutObjectGrid.UpdateModelItem();
                        
        }

        protected override void UnExecuteCore() {
            if (!string.IsNullOrEmpty(_targetCell.PreviousWebpartServiceName)) {
                _targetCell.WebpartServiceName = _targetCell.PreviousWebpartServiceName;
                _targetCell.IconPath = _targetCell.PreviousIconPath;
                _targetCell.XmlConfiguration = _targetCell.PreviousXmlConfig;
                _targetCell.ClearPreviousContents();
                _targetCell.LayoutObjectGrid.UpdateModelItem();
            }
            else {
                if (!string.IsNullOrEmpty(_targetCell.XmlConfiguration)) {
                    _currentConfiguration = _targetCell.XmlConfiguration;
                }
                _targetCell.ClearCellContent(true);
            }
        }
    }
}
