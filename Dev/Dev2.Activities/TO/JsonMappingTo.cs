#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Infrastructure.Providers.Validation;
using Dev2.Common.Interfaces.Interfaces;
using Dev2.Data.Util;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;
using Dev2.Validation;

namespace Dev2.TO
{
	public class JsonMappingTo : ValidatedObject, IDev2TOFn
	{
		string _sourceName, _destinationName;

		[FindMissing]
		public string SourceName
		{
			get { return _sourceName; }
			set
			{
				OnPropertyChanged(ref _sourceName, value);
				RaiseCanAddRemoveChanged();
			}
		}

		
		public string GetDestinationWithName(string sourceName)
			
		{
			string destName = null;
			if (DataListUtil.IsFullyEvaluated(sourceName))
			{

                destName = DataListUtil.IsValueRecordset(sourceName) || DataListUtil.IsValueRecordsetWithFields(sourceName) ? DataListUtil.ExtractRecordsetNameFromValue(sourceName) : DataListUtil.StripBracketsFromValue(sourceName);
            }
            return destName;
        }

		public string DestinationName
		{
			get
			{
				return _destinationName;
			}
			set
			{
				OnPropertyChanged(ref _destinationName, value);
				RaiseCanAddRemoveChanged();
			}
		}

        #region Implementation of IDev2TOFn
        int _indexNumber;
        bool _isSourceNameFocused;
        bool _isDestinationNameFocused;
        public int IndexNumber { get => _indexNumber; set => OnPropertyChanged(ref _indexNumber, value); }

		public JsonMappingTo()
		{
		}

		public JsonMappingTo(string sourceName, int indexNumber, bool inserted)
		{
			SourceName = sourceName;
			_indexNumber = indexNumber;
			Inserted = inserted;
			DestinationName = GetDestinationWithName(SourceName);
		}

        
        public bool IsSourceNameFocused { get => _isSourceNameFocused; set => OnPropertyChanged(ref _isSourceNameFocused, value); }

        public bool IsDestinationNameFocused { get => _isDestinationNameFocused; set => OnPropertyChanged(ref _isDestinationNameFocused, value); }

        public bool CanRemove() => string.IsNullOrEmpty(DestinationName) && string.IsNullOrEmpty(SourceName);

        public bool CanAdd()
		{
			return !CanRemove();
		}

		public void ClearRow()
		{
			SourceName = string.Empty;
			DestinationName = string.Empty;
		}

		void RaiseCanAddRemoveChanged()
		{
			
			OnPropertyChanged("CanRemove");
			OnPropertyChanged("CanAdd");
			
		}
		public bool Inserted { get; set; }

		#endregion

        public override IRuleSet GetRuleSet(string propertyName, string datalist)
        {
            var ruleSet = new RuleSet();
            if (String.IsNullOrEmpty( SourceName))
            {
                return ruleSet;
            }
            if(propertyName == "SourceName")
            {
                ruleSet.Add(new IsValidJsonCreateMappingSourceExpression(() => SourceName));
            }
            if(propertyName == "DestinationName")
            {
                ruleSet.Add(new IsStringEmptyOrWhiteSpaceRule(()=>DestinationName));
                ruleSet.Add(new ShouldNotBeVariableRule(()=>DestinationName));
            }
            return ruleSet;
        }
    }
}
