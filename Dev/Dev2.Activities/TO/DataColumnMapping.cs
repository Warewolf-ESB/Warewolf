
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;

namespace Dev2.TO
{
    public class DataColumnMapping : ValidatedObject, IDev2TOFn
    {
        int _indexNumber;
        string _inputColumn;
        IDbColumn _outputColumn;

        [FindMissing]
        public string InputColumn { get { return _inputColumn; } set { OnPropertyChanged(ref _inputColumn, value); } }

        public IDbColumn OutputColumn { get { return _outputColumn; } set { OnPropertyChanged(ref _outputColumn, value); } }

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

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            return new RuleSet();
        }
    }
}
