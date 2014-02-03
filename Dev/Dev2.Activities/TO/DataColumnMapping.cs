using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Util;

namespace Dev2.TO
{
    public class DataColumnMapping : ValidatedObject, IDev2TOFn
    {
        int _indexNumber;
        string _inputColumn;
        DbColumn _outputColumn;

        [FindMissing]
        public string InputColumn { get { return _inputColumn; } set { OnPropertyChanged(ref _inputColumn, value); } }

        public DbColumn OutputColumn { get { return _outputColumn; } set { OnPropertyChanged(ref _outputColumn, value); } }

        #region Implementation of IDev2TOFn

        public int IndexNumber { get { return _indexNumber; } set { OnPropertyChanged(ref _indexNumber, value); } }

        public bool CanRemove()
        {
            return false;
        }

        public bool CanAdd()
        {
            return false;
        }

        public void ClearRow()
        {
        }

        public bool Inserted { get; set; }

        #endregion

        public override IRuleSet GetRuleSet(string propertyName)
        {
            return new RuleSet();
        }
    }
}
