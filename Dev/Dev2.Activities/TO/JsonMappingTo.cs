
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Interfaces;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;

namespace Dev2.TO
{
    public class JsonMappingTo : ValidatedObject, IDev2TOFn
    {
        string _sourceName, _destinationName;

        [FindMissing]
        public string SourceName { get { return _sourceName; } set { OnPropertyChanged(ref _sourceName, value); } }

        public string DestinationName { get { return _destinationName; } set { OnPropertyChanged(ref _destinationName, value); } }

        #region Implementation of IDev2TOFn
        int _indexNumber;
        public int IndexNumber { get { return _indexNumber; } set { OnPropertyChanged(ref _indexNumber, value); } }

        public JsonMappingTo()
        {
        }

        public JsonMappingTo(string sourceName, string destinationName, int indexNumber, bool inserted)
        {
            _sourceName = sourceName;
            _destinationName = destinationName;
            _indexNumber = indexNumber;
            Inserted = inserted;
        }

        public string WatermarkTextKeyName { get; set; }
        public string WatermarkTextInput { get; set; }

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
