using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
//using System.Windows.Events;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Data;
using Infragistics.Shared;
//using Infragistics.Windows.Input;
using Infragistics.Windows;
using Infragistics.Windows.Controls;
using System.Windows.Markup;
using System.Threading;

namespace Infragistics.Windows.DataPresenter
{






    internal static class GroupByEvaluatorFactory
    {

        // JJD 2/29/08
        // a static array of slots to hold up to 2 cached evaluators
        // for each FieldGroupByMode enum value.
        // Note: the reason we need 2 for each is because some of the 
        // enums require a case senstive one as well as a case insensitive one
        private static IGroupByEvaluator[] g_CachedEvaluators;

        #region Static Constructor

        // JJD 2/29/08 - added
        static GroupByEvaluatorFactory()
        {
            int maxValue = 1;

            // JJD 2/29/08
            // get the highest value for the enum
            foreach (object value in Enum.GetValues(typeof(FieldGroupByMode)))
            {
                int ival = (int)value;

                if (ival > maxValue)
                    maxValue = ival;
            }

            // JJD 2/29/08
            // allocate a static array of slots to hold up to 2 cached evaluators
            // for each enum value.
            // Note: the reason we need 2 for each is because some of the 
            // enums require a case senstive one as well as a case insensitive one
            g_CachedEvaluators = new IGroupByEvaluator[(maxValue + 1) * 2];
        }

        #endregion //Static Constructor	
    
        #region Static GetEvaluator Method

        // JJD 2/29/08 - added
        private static IGroupByEvaluator GetEvaluator(FieldGroupByMode mode, FieldSortComparisonType sortCompareType)
        {
            // calculate the index by 1st doubling the value of the enum.
            // This will allow for up to 2 slots per enum value
            int index = (int)mode * 2;

            // if the sort compare is not case sensitive add 1 to the index for the 2nd slot
            if (sortCompareType != FieldSortComparisonType.CaseSensitive)
                index++;

            // if the slot is empty then create it now based on the passed
            // in mode and sortCompareType
			
			
			
			IGroupByEvaluator evaluator = g_CachedEvaluators[index];
			if ( evaluator == null )
            {
                switch (mode)
                {

                    case FieldGroupByMode.FirstCharacter:
                        evaluator = new FirstCharactersEvaluator(1, sortCompareType);
                        break;

                    case FieldGroupByMode.First2Characters:
                        evaluator = new FirstCharactersEvaluator(2, sortCompareType);
                        break;

                    case FieldGroupByMode.First3Characters:
                        evaluator = new FirstCharactersEvaluator(3, sortCompareType);
                        break;

                    case FieldGroupByMode.First4Characters:
                        evaluator = new FirstCharactersEvaluator(4, sortCompareType);
                        break;

                    case FieldGroupByMode.Date:
                        evaluator = new DateEvaluator();
                        break;

                    case FieldGroupByMode.Hour:
                        evaluator = new HourEvaluator();
                        break;

                    case FieldGroupByMode.Minute:
                        evaluator = new MinuteEvaluator();
                        break;

                    case FieldGroupByMode.Month:
                        evaluator = new MonthEvaluator();
                        break;

                    case FieldGroupByMode.OutlookDate:
                        evaluator = new OutlookDateEvaluator();
                        break;

                    case FieldGroupByMode.Quarter:
                        evaluator = new QuarterEvaluator();
                        break;

                    case FieldGroupByMode.Second:
                        evaluator = new SecondEvaluator();
                        break;

                    case FieldGroupByMode.Text:
                        evaluator = new CellTextEvaluator(sortCompareType);
                        break;

                    // JJD 2/29/08 - added
                    case FieldGroupByMode.Value:
                        evaluator = new ValueEvaluator();
                        break;

                    case FieldGroupByMode.Year:
                        evaluator = new YearEvaluator();
                        break;

                    default:
                        Debug.Fail("Unrecognized FieldGroupByMode value: " + mode.ToString());
                        evaluator = null;
                        break;
                }

                g_CachedEvaluators[index] = evaluator;
            }

			
			
            
			return evaluator;
        }

        #endregion //Static GetEvaluator Method	
    
        #region CreateEvaluator



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static IGroupByEvaluator CreateEvaluator(Field fieldBeingGrouped)
        {
            if (fieldBeingGrouped == null)
                return null;

            Debug.Assert(fieldBeingGrouped.HasSettings == false || fieldBeingGrouped.Settings.GroupByEvaluator == null, "CreateEvaluator should not be called if the field has been assigned a custom evaluator already!");

            FieldGroupByMode mode = fieldBeingGrouped.GroupByModeResolved;

            // If the FieldGroupByMode resolves to 'Value' then just return null so that the grid's
            // default evaluation logic will be used.
            //
            // JJD 2/29/08
            // Let Value go thru because now we are supplying an evaluator for it 
            //if (mode == FieldGroupByMode.Value)
            //    return null;

            // JJD 2/29/08
            // Instead of creating a new instance of the evaluator every time
            // this method is called we want to get one that we cache in a static
            // table
            #region Old code

            //IGroupByEvaluator evaluator;

            //switch (mode)
            //{
            //    case FieldGroupByMode.Date:
            //        evaluator = new DateEvaluator();
            //        break;

            //    case FieldGroupByMode.FirstCharacter:
            //        evaluator = new FirstCharactersEvaluator(1, fieldBeingGrouped.SortComparisonTypeResolved);
            //        break;

            //    case FieldGroupByMode.First2Characters:
            //        evaluator = new FirstCharactersEvaluator(2, fieldBeingGrouped.SortComparisonTypeResolved);
            //        break;

            //    case FieldGroupByMode.First3Characters:
            //        evaluator = new FirstCharactersEvaluator(3, fieldBeingGrouped.SortComparisonTypeResolved);
            //        break;

            //    case FieldGroupByMode.First4Characters:
            //        evaluator = new FirstCharactersEvaluator(4, fieldBeingGrouped.SortComparisonTypeResolved);
            //        break;

            //    case FieldGroupByMode.Hour:
            //        evaluator = new HourEvaluator();
            //        break;

            //    case FieldGroupByMode.Minute:
            //        evaluator = new MinuteEvaluator();
            //        break;

            //    case FieldGroupByMode.Month:
            //        evaluator = new MonthEvaluator();
            //        break;

            //    case FieldGroupByMode.OutlookDate:
            //        evaluator = new OutlookDateEvaluator();
            //        break;

            //    case FieldGroupByMode.Quarter:
            //        evaluator = new QuarterEvaluator();
            //        break;

            //    case FieldGroupByMode.Second:
            //        evaluator = new SecondEvaluator();
            //        break;

            //    case FieldGroupByMode.Text:
            //        evaluator = new CellTextEvaluator(fieldBeingGrouped.SortComparisonTypeResolved);
            //        break;

            //    case FieldGroupByMode.Year:
            //        evaluator = new YearEvaluator();
            //        break;

            //    default:
            //        Debug.Fail("Unrecognized FieldGroupByMode value: " + mode.ToString());
            //        evaluator = null;
            //        break;
            //}

            //return evaluator;
            #endregion //Old code	

            FieldSortComparisonType sortComparisonType = FieldSortComparisonType.CaseSensitive;

            // JJD 2/29/08
            // Get the appropriate sortComparisonType for text type modes
            switch (mode)
            {
                case FieldGroupByMode.FirstCharacter:
                case FieldGroupByMode.First2Characters:
                case FieldGroupByMode.First3Characters:
                case FieldGroupByMode.First4Characters:
                case FieldGroupByMode.Text:
                    {
                        sortComparisonType = fieldBeingGrouped.SortComparisonTypeResolved;
                        break;
                    }
            }

            // JJD 2/29/08
            // Call the new static GetEvaluator method that will lazily create and
            // cache an instance of an evaluator based on the mode and the sortComparisonType.
            return GetEvaluator(mode, sortComparisonType);
        }

        #endregion // CreateEvaluator

        #region Evaluator Classes

        // JJD 10/09/08 - TFS6745 - added
        #region DateComparerBase abstract class

        // JJD 10/09/08 - TFS6745
        // Create comparers to use during a groupby operation that that will
        // freturn 0 for all values that would end up in the same group
        private abstract class DateComparerBase : IComparer,
				// SSP 5/29/09 - TFS17233 - Optimization
				// Implemented IComparer<RecordManager.SameFieldRecordsSortComparer.CellInfo> so we don't
				// have to allocate cells and also we can cache converted cell text.
				// 
				IComparer<RecordManager.SameFieldRecordsSortComparer.CellInfo>
        {
			// SSP 5/29/09 - TFS17233 - Optimization
			// Implemented IComparer<RecordManager.SameFieldRecordsSortComparer.CellInfo> so we don't
			// have to allocate cells and also we can cache converted cell text.
			// 
			public virtual int Compare(
				RecordManager.SameFieldRecordsSortComparer.CellInfo x,
				RecordManager.SameFieldRecordsSortComparer.CellInfo y )
			{
				return this.CompareHelper( x._value, y._value );
			}

            public virtual int Compare(object x, object y)
            {
                Cell xxCell = x as Cell;
                Cell yyCell = y as Cell;

                // JJD 5/29/09 - TFS18063 
                // Use the new overload to GetCellValue which will return the value 
                // converted into EditAsType
                //object xValue = xxCell.Record.GetCellValue(xxCell.Field);
                //object yValue = yyCell.Record.GetCellValue(yyCell.Field);
                object xValue = xxCell.Record.GetCellValue(xxCell.Field, CellValueType.EditAsType);
                object yValue = yyCell.Record.GetCellValue(yyCell.Field, CellValueType.EditAsType);

				// SSP 5/29/09 - TFS17233 - Optimization
				// Refactored. Moved existing code from here into the new CompareHelper method.
				// 
				return this.CompareHelper( xValue, yValue );
            }

			// SSP 5/29/09 - TFS17233 - Optimization
			// Refactored. Moved code from Compare method above into the new CompareHelper method.
			// 
			private int CompareHelper( object xValue, object yValue )
			{
				if ( xValue is DateTime )
				{
					if ( yValue is DateTime )
					{
						int rtn = ( (IComparable)xValue ).CompareTo( yValue );

						// if the return is zero (i.e. the exact same value) or the
						// AreInSameGroupBy method returns true. then return 0 so
						// they are treated as equal values so that sub field sorts
						// work property
						if ( rtn == 0 || this.AreInSameGroupBy( (DateTime)xValue, (DateTime)yValue ) )
							return 0;

						return rtn;
					}
					else
						return 1;
				}

				if ( yValue is DateTime )
					return -1;

				return 0;
			}

            protected abstract bool AreInSameGroupBy(DateTime x, DateTime y);
        }

        #endregion //DateComparerBase abstract class	
    
        #region CellTextEvaluator

        // Evaluates the text of a cell, not the value of a cell.
        //
        internal class CellTextEvaluator : IGroupByEvaluator
        {
            #region Data

            private readonly bool isCaseInsensitive;
			private CellTextComparer _sortComparer;

            #endregion // Data

            #region CellTextEvaluator

            internal CellTextEvaluator(FieldSortComparisonType sortComparisonType)
            {
                this.isCaseInsensitive = sortComparisonType == FieldSortComparisonType.CaseInsensitive;
            }

            #endregion // CellTextEvaluator

            #region GetGroupByValue

            object IGroupByEvaluator.GetGroupByValue(GroupByRecord groupByRecord, DataRecord record)
            {
                string text = record.GetCellText(groupByRecord.GroupByField);
                return text == null ? "" : text;
            }

            #endregion // GetGroupByValue

            #region DoesGroupContainRecord

            bool IGroupByEvaluator.DoesGroupContainRecord(GroupByRecord groupByRecord, DataRecord record)
            {
                string cellText = record.GetCellText(groupByRecord.GroupByField);

                cellText = cellText == null ? "" : cellText;

                return String.Compare(groupByRecord.Value.ToString(), cellText, this.isCaseInsensitive) == 0;
            }

            #endregion // DoesGroupContainRecord

			#region SortComparer

			IComparer IGroupByEvaluator.SortComparer
			{
				get
				{
					if (this._sortComparer == null)
						this._sortComparer = new CellTextComparer(this);

					return this._sortComparer;
				}
			}

			#endregion //SortComparer

			#region CellTextComparer

			// JJD 2/23/07 - BR19943
			// Added support for groupby evaluators to specify their own sort comparer
			private class CellTextComparer : IComparer, 
				// SSP 5/29/09 - TFS17233 - Optimization
				// Implemented IComparer<RecordManager.SameFieldRecordsSortComparer.CellInfo> so we don't
				// have to allocate cells and also we can cache converted cell text.
				// 
				IComparer<RecordManager.SameFieldRecordsSortComparer.CellInfo>
			{
				private CellTextEvaluator _evaluator;

				internal CellTextComparer(CellTextEvaluator evaluator)
				{
					this._evaluator = evaluator;
				}

				// SSP 5/29/09 - TFS17233 - Optimization
				// Implemented IComparer<RecordManager.SameFieldRecordsSortComparer.CellInfo> so we don't
				// have to allocate cells and also we can cache converted cell text.
				// 
				public int Compare(
					RecordManager.SameFieldRecordsSortComparer.CellInfo x,
					RecordManager.SameFieldRecordsSortComparer.CellInfo y )
				{
					string xx = x.GetCellText( );
					string yy = y.GetCellText( );

					return String.Compare( xx, yy, this._evaluator.isCaseInsensitive );
				}

				#region IComparer Members

				int IComparer.Compare(object x, object y)
				{
					Cell xxCell = x as Cell;
					Cell yyCell = y as Cell;

					string xx = xxCell.Record.GetCellText(xxCell.Field);
					string yy = yyCell.Record.GetCellText(yyCell.Field);

					return String.Compare(xx, yy, this._evaluator.isCaseInsensitive);
				}

				#endregion
			}

			#endregion //CellTextComparer
		}

        #endregion // CellValueEvaluator

        #region DateEvaluator

        // Evaluates dates (not times) for fields of type DateTime.
        //
        private class DateEvaluator : IGroupByEvaluator
        {
            #region GetGroupByValue

            object IGroupByEvaluator.GetGroupByValue(GroupByRecord groupByRecord, DataRecord record)
            {
                // JJD 3/25/08
                // Strip off the time portion before returning the value
                //return DateEvaluator.GetGroupByValueHelper(groupByRecord, record);
                DateTime dt = DateEvaluator.GetGroupByValueHelper(groupByRecord, record);

                return dt.Date;
            }

            #endregion // GetGroupByValue

            #region GetGroupByValueHelper







            internal static DateTime GetGroupByValueHelper(GroupByRecord groupByRecord, DataRecord record)
            {
                return groupByRecord.Value is DateTime ? (DateTime)groupByRecord.Value : DateTime.MinValue;
            }

            #endregion // GetGroupByValueHelper

            #region DoesGroupContainRecord

            bool IGroupByEvaluator.DoesGroupContainRecord(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dateInCell, dateInGrp;
                if (DateEvaluator.DoesGroupContainRecordHelper(groupByRecord, record, out dateInGrp, out dateInCell))
                    return true;

                return dateInCell.Date == dateInGrp.Date;
            }

            #endregion // DoesGroupContainRecord

            #region DoesGroupContainRecordHelper



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            internal static bool DoesGroupContainRecordHelper(GroupByRecord groupByRecord, DataRecord record, out DateTime dateInGroup, out DateTime dateInCell)
            {
                // JJD 5/29/09 - TFS18063 
                // Use the new overload to GetCellValue which will return the value 
                // converted into EditAsType
                //object cellValue = record.GetCellValue(groupByRecord.GroupByField, true);
                object cellValue = record.GetCellValue(groupByRecord.GroupByField, CellValueType.EditAsType);

                dateInGroup = (DateTime)groupByRecord.Value;

                // If the cell's value is not a DateTime then the GroupBy record's value will be
                // DateTime.MinValue because the GetGroupByValue method says so.
                //
                if (!(cellValue is DateTime))
                {
                    // JJD 3/25/08
                    // Before we return true we need to test if the groupByRecord's value is MinValue
                    // If it is then return true, otherwise set the dateInCell to MinValue and return false
                    //Debug.Assert((DateTime)groupByRecord.Value == DateTime.MinValue, "groupByRecord.Value should be DateTime.MinValue when the cell's value is null, but it is not.");
                    //dateInCell = dateInGroup;
                    if ((DateTime)groupByRecord.Value == DateTime.MinValue)
                    {
                        dateInCell = dateInGroup;
                        return true;
                    }

                    dateInCell = DateTime.MinValue;

                    return false;
                }

                dateInCell = (DateTime)cellValue;

                return false;
            }

            #endregion // DoesGroupContainRecordHelper

			#region SortComparer

			/// <summary>
			/// The comparer to use for sorting the records.
			/// </summary>
			/// <value>Returns an object that implements IComparer or null to use the default sort comparer.</value>
			public IComparer SortComparer
			{
				get { return null; }
			}

			#endregion //SortComparer
        }

        #endregion // DateEvaluator

        #region FirstCharactersEvaluator

        // Evaluates the first N characters of a cell's value.
        //
        private class FirstCharactersEvaluator : IGroupByEvaluator
        {
            #region Data

            private readonly int numChars;
            private readonly bool isCaseInsensitive;
			private FirstCharactersComparer _sortComparer;

            #endregion // Data

            #region Constructor

            internal FirstCharactersEvaluator(int numChars, FieldSortComparisonType sortComparisonType)
            {
                Debug.Assert(numChars > 0, "'numChars' should not be less than or equal to zero.");

                this.numChars = numChars;
                this.isCaseInsensitive = sortComparisonType == FieldSortComparisonType.CaseInsensitive;
            }

            #endregion // Constructor

            #region GetGroupByValue

            object IGroupByEvaluator.GetGroupByValue(GroupByRecord groupByRecord, DataRecord record)
            {
                return this.GetFirstCharactersOfCellText(groupByRecord, record);
            }

            #endregion // GetGroupByValue

            #region DoesGroupContainRecord

            bool IGroupByEvaluator.DoesGroupContainRecord(GroupByRecord groupByRecord, DataRecord record)
            {
                string groupByVal = groupByRecord.Value == null ? "" : groupByRecord.Value.ToString();
                string firstLetters = this.GetFirstCharactersOfCellText(groupByRecord, record);

                return String.Compare(groupByVal, firstLetters, this.isCaseInsensitive) == 0;
            }

            #endregion // DoesGroupContainRecord

            #region GetFirstCharactersOfCellText

            // Helper method.
            //
            private string GetFirstCharactersOfCellText(GroupByRecord groupByRecord, DataRecord record)
            {
                string text;

                // Get the text of the cell.
                //
                if (groupByRecord.GroupByField == null)
                    text = "";
                else
                    text = this.GetFirstCharactersOfText( record.GetCellText(groupByRecord.GroupByField));

                return text;
            }

            #endregion // GetFirstCharactersOfCellText

			#region GetFirstCharactersOfText

			private string GetFirstCharactersOfText(string text)
			{
				if (null == text)
					text = string.Empty;
				else if (this.numChars < text.Length)
					text = text.Substring(0, this.numChars);

				return text;
			}

			#endregion // GetFirstCharactersOfText

			#region SortComparer

			IComparer IGroupByEvaluator.SortComparer
			{
				get 
				{
					if (this._sortComparer == null)
						this._sortComparer = new FirstCharactersComparer(this);

					return this._sortComparer; 
				}
			}

			#endregion //SortComparer

			#region FirstCharactersComparer

			// JJD 2/23/07 - BR19943
			// Added support for groupby evaluators to specify their own sort comparer
			private class FirstCharactersComparer : IComparer,
				// SSP 5/29/09 - TFS17233 - Optimization
				// Implemented IComparer<RecordManager.SameFieldRecordsSortComparer.CellInfo> so we don't
				// have to allocate cells and also we can cache converted cell text.
				// 
				IComparer<RecordManager.SameFieldRecordsSortComparer.CellInfo>
			{
				private FirstCharactersEvaluator _evaluator;

				internal FirstCharactersComparer(FirstCharactersEvaluator evaluator)
				{
					this._evaluator = evaluator;
				}

				// SSP 5/29/09 - TFS17233 - Optimization
				// Implemented IComparer<RecordManager.SameFieldRecordsSortComparer.CellInfo> so we don't
				// have to allocate cells and also we can cache converted cell text.
				// 
				public int Compare(
					RecordManager.SameFieldRecordsSortComparer.CellInfo x,
					RecordManager.SameFieldRecordsSortComparer.CellInfo y )
				{
					string xx = x.GetCellText( );
					string yy = y.GetCellText( );

					return this.CompareHelper( xx, yy );
				}

				
				
				
				private int CompareHelper( string x, string y )
				{
					string xx = this._evaluator.GetFirstCharactersOfText( x );
					string yy = this._evaluator.GetFirstCharactersOfText( y );

					return String.Compare( xx, yy, this._evaluator.isCaseInsensitive );
				}

				#region IComparer Members

				int IComparer.Compare(object x, object y)
				{
					Cell xxCell = x as Cell;
					Cell yyCell = y as Cell;

					
					
					
					string xx = xxCell.Record.GetCellText( xxCell.Field );
					string yy = yyCell.Record.GetCellText( yyCell.Field );

					return this.CompareHelper( xx, yy );

					
					

					
					
				}

				#endregion
			}

			#endregion //FirstCharactersComparer
		}

        #endregion // FirstCharactersEvaluator

        #region HourEvaluator

        // Evaluates the Date and Hour portions of a DateTime.
        //
        private class HourEvaluator : IGroupByEvaluator
        {
            #region Private Members

            // JJD 10/09/08 - TFS6745 - added
            private Comparer _comparer;

            #endregion //Private Members	
    
            #region GetGroupByValue

            object IGroupByEvaluator.GetGroupByValue(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dt = DateEvaluator.GetGroupByValueHelper(groupByRecord, record);

                // Set the Minute and Second portions to 0 for every group so that the arbitrary Minute 
                // and Second values found in cells do not get displayed in the group descriptions.
                //
                return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
            }

            #endregion // GetGroupByValue

            #region DoesGroupContainRecord

            bool IGroupByEvaluator.DoesGroupContainRecord(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dateInCell, dateInGrp;
                if (DateEvaluator.DoesGroupContainRecordHelper(groupByRecord, record, out dateInGrp, out dateInCell))
                    return true;

                return dateInCell.Date == dateInGrp.Date &&
                    dateInCell.Hour == dateInGrp.Hour;
            }

            #endregion // DoesGroupContainRecord

			#region SortComparer

			IComparer IGroupByEvaluator.SortComparer
			{
				get 
                {
                    // JJD 10/09/08 - TFS6745 - added
                    if (this._comparer == null)
                        this._comparer = new Comparer();

                    return this._comparer; 
                }
			}

			#endregion //SortComparer
            
            // JJD 10/09/08 - TFS6745 - added
            #region Comparer private class

            private class Comparer : DateComparerBase
            {
                protected override bool AreInSameGroupBy(DateTime x, DateTime y)
                {
                    return x.Date == y.Date &&
                        x.Hour == y.Hour;
                }
            }

            #endregion //Comparer private class
        }

        #endregion // HourEvaluator

        #region MinuteEvaluator

        // Evaluates the Date, Hour, and Minute portions of a DateTime.
        //
        private class MinuteEvaluator : IGroupByEvaluator
        {
            #region Private Members

            // JJD 10/09/08 - TFS6745 - added
            private Comparer _comparer;

            #endregion //Private Members	
    
            #region GetGroupByValue

            object IGroupByEvaluator.GetGroupByValue(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dt = DateEvaluator.GetGroupByValueHelper(groupByRecord, record);

                // Set the Second portion to 0 for every group so that the arbitrary Second
                // values found in cells do not get displayed in the group descriptions.
                //
                return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
            }

            #endregion // GetGroupByValue

            #region DoesGroupContainRecord

            bool IGroupByEvaluator.DoesGroupContainRecord(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dateInCell, dateInGrp;
                if (DateEvaluator.DoesGroupContainRecordHelper(groupByRecord, record, out dateInGrp, out dateInCell))
                    return true;

                return dateInCell.Date == dateInGrp.Date &&
                    dateInCell.Hour == dateInGrp.Hour &&
                    dateInCell.Minute == dateInGrp.Minute;
            }

            #endregion // DoesGroupContainRecord

			#region SortComparer

			IComparer IGroupByEvaluator.SortComparer
			{
				get 
                {
                    // JJD 10/09/08 - TFS6745 - added
                    if (this._comparer == null)
                        this._comparer = new Comparer();

                    return this._comparer; 
                }
			}

			#endregion //SortComparer
            
            // JJD 10/09/08 - TFS6745 - added
            #region Comparer private class

            private class Comparer : DateComparerBase
            {
                protected override bool AreInSameGroupBy(DateTime x, DateTime y)
                {
                    return x.Date == y.Date &&
                        x.Hour == y.Hour &&
                        x.Minute == y.Minute;
                }
            }

            #endregion //Comparer private class
        }

        #endregion // MinuteEvaluator

        #region MonthEvaluator

        // Evaluates the Year and Month portion of a DateTime.
        //
        private class MonthEvaluator : IGroupByEvaluator
        {
            #region Private Members

            // JJD 10/09/08 - TFS6745 - added
            private Comparer _comparer;

            #endregion //Private Members	
    
            #region GetGroupByValue

            object IGroupByEvaluator.GetGroupByValue(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dt = DateEvaluator.GetGroupByValueHelper(groupByRecord, record);

                // Set the Day portion to 1 for every group so that the arbitrary Day values
                // found in cells do not get displayed in the group descriptions.
                //
                return new DateTime(dt.Year, dt.Month, 1);
            }

            #endregion // GetGroupByValue

            #region DoesGroupContainRecord

            bool IGroupByEvaluator.DoesGroupContainRecord(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dateInCell, dateInGrp;
                if (DateEvaluator.DoesGroupContainRecordHelper(groupByRecord, record, out dateInGrp, out dateInCell))
                    return true;

                return dateInCell.Year == dateInGrp.Year &&
                    dateInCell.Month == dateInGrp.Month;
            }

            #endregion // DoesGroupContainRecord

			#region SortComparer

			IComparer IGroupByEvaluator.SortComparer
			{
				get 
                {
                    // JJD 10/09/08 - TFS6745 - added
                    if (this._comparer == null)
                        this._comparer = new Comparer();

                    return this._comparer; 
                }
			}

			#endregion //SortComparer
            
            // JJD 10/09/08 - TFS6745 - added
            #region Comparer private class

            private class Comparer : DateComparerBase
            {
                protected override bool AreInSameGroupBy(DateTime x, DateTime y)
                {
                    return x.Year == y.Year &&
                        x.Month == y.Month;
                }
            }

            #endregion //Comparer private class
        }

        #endregion // MonthEvaluator

        #region OutlookDateEvaluator

        // Performs Outlook 2003-style grouping of dates.
        //
        private class OutlookDateEvaluator : IGroupByEvaluator
        {
            #region Private Members

            // JJD 10/09/08 - TFS6745 - added
            private Comparer _comparer;

            #endregion //Private Members	
    
            #region Data

            // JJD 4/10/08
            // Added support for Culture caching
            #region Old code - moved into CultureCache class

//            private readonly string BEYOND_NEXT_MONTH;
//            private readonly string NEXT_MONTH;
//            private readonly string LATER_THIS_MONTH;
//            private readonly string THREE_WEEKS_AWAY;
//            private readonly string TWO_WEEKS_AWAY;
//            private readonly string NEXT_WEEK;
//            private readonly string TODAY;
//            private readonly string YESTERDAY;
//            private readonly string SUNDAY;
//            private readonly string MONDAY;
//            private readonly string TUESDAY;
//            private readonly string WEDNESDAY;
//            private readonly string THURSDAY;
//            private readonly string FRIDAY;
//            private readonly string SATURDAY;
//            private readonly string LAST_WEEK;
//            private readonly string TWO_WEEKS_AGO;
//            private readonly string THREE_WEEKS_AGO;
//            private readonly string EARLIER_THIS_MONTH;
//            private readonly string LAST_MONTH;
//            private readonly string OLDER;
//            private readonly string NONE;

//#if DEBUG
//            /// <summary>
//            /// Contains a list of DateInfo objects used when determining which date range a specific DateTime falls into.
//            /// </summary>
//#endif
//            private ArrayList dateInfos;

//#if DEBUG
//            /// <summary>
//            /// A timestamp which is used to ensure that the 'dateInfos' member variable does not contain yesterday's data.
//            /// </summary>
//#endif
//            private DateTime lastUpdated;

//#if DEBUG
//            /// <summary>
//            /// Do not use this member variable, use the DayMap property instead.
//            /// </summary>
//#endif
            //            private Hashtable dayMap;

            #endregion //Old code - moved into CultureCache class

            private static Dictionary<CultureInfo, CultureCache> g_cultureCache;
            #endregion // Data

            #region Constructor

            internal OutlookDateEvaluator()
            {
                // JJD 4/10/08
                // Added support for Culture caching
                #region Old code - moved into CultureCache class

                //// Get a reference to a Sunday.
                ////
                //DateTime sunday = DateTime.Today;
                //while (sunday.DayOfWeek != DayOfWeek.Sunday)
                //    sunday = sunday.AddDays(1);

                //// Load day names using a format string stored in the resource file.
                ////
                //SUNDAY = SR.GetString("Outlook_GroupByMode_Description_DayOfWeekFormatString", sunday);
                //MONDAY = SR.GetString("Outlook_GroupByMode_Description_DayOfWeekFormatString", sunday.AddDays(1));
                //TUESDAY = SR.GetString("Outlook_GroupByMode_Description_DayOfWeekFormatString", sunday.AddDays(2));
                //WEDNESDAY = SR.GetString("Outlook_GroupByMode_Description_DayOfWeekFormatString", sunday.AddDays(3));
                //THURSDAY = SR.GetString("Outlook_GroupByMode_Description_DayOfWeekFormatString", sunday.AddDays(4));
                //FRIDAY = SR.GetString("Outlook_GroupByMode_Description_DayOfWeekFormatString", sunday.AddDays(5));
                //SATURDAY = SR.GetString("Outlook_GroupByMode_Description_DayOfWeekFormatString", sunday.AddDays(6));

                //BEYOND_NEXT_MONTH = SR.GetString("Outlook_GroupByMode_Description_BeyondNextMonth");
                //NEXT_MONTH = SR.GetString("Outlook_GroupByMode_Description_NextMonth");
                //LATER_THIS_MONTH = SR.GetString("Outlook_GroupByMode_Description_LaterThisMonth");
                //THREE_WEEKS_AWAY = SR.GetString("Outlook_GroupByMode_Description_ThreeWeeksAway");
                //TWO_WEEKS_AWAY = SR.GetString("Outlook_GroupByMode_Description_TwoWeeksAway");
                //NEXT_WEEK = SR.GetString("Outlook_GroupByMode_Description_NextWeek");
                //TODAY = SR.GetString("Outlook_GroupByMode_Description_Today");
                //YESTERDAY = SR.GetString("Outlook_GroupByMode_Description_Yesterday");
                //LAST_WEEK = SR.GetString("Outlook_GroupByMode_Description_LastWeek");
                //TWO_WEEKS_AGO = SR.GetString("Outlook_GroupByMode_Description_TwoWeeksAgo");
                //THREE_WEEKS_AGO = SR.GetString("Outlook_GroupByMode_Description_ThreeWeeksAgo");
                //EARLIER_THIS_MONTH = SR.GetString("Outlook_GroupByMode_Description_EarlierThisMonth");
                //LAST_MONTH = SR.GetString("Outlook_GroupByMode_Description_LastMonth");
                //OLDER = SR.GetString("Outlook_GroupByMode_Description_Older");
                //NONE = SR.GetString("Outlook_GroupByMode_Description_None");

                #endregion //Old code - moved into CultureCache class
            }

            #endregion // Constructor

            #region GetGroupByValue

            object IGroupByEvaluator.GetGroupByValue(GroupByRecord groupByRecord, DataRecord record)
            {
                // JJD 5/29/09 - TFS18063 
                // Use the new overload to GetCellValue which will return the value 
                // converted into EditAsType
                //object cellValue = record.GetCellValue(groupByRecord.GroupByField, true);
                object cellValue = record.GetCellValue(groupByRecord.GroupByField, CellValueType.EditAsType);

                // JJD 4/10/08
                // Added support for culture caching
                //string dateLabel = String.Empty;
                DynamicResourceString dateLabel = null;
                DateTime targetDate = DateTime.MinValue;

                // JJD 4/10/08
                // Added support for culture caching
                CultureCache cic = GetCultureInfoCache(groupByRecord);

                if (cellValue == null || cellValue is DBNull)
                {
                    // JJD 4/10/08
                    // Added support for culture caching
                    //dateLabel = this.NONE;
                    dateLabel = cic.NONE;
                }
                else if (cellValue is DateTime)
                {
                    targetDate = (DateTime)cellValue;

                    // JJD 4/10/08
                    // Added support for culture caching
                    //dateLabel = this.GetDateLabel(targetDate);
                    dateLabel = cic.GetDateLabel(targetDate);
                }
                else
                {
                    // JJD 4/10/08
                    // If it is not null or a DateTime then place it in the null group
                    dateLabel = cic.NONE;
                }

                // Set the groupby record's description to the appropriate date label.
                // We use that description in the DoesGroupContainRecord method to determine
                // which records belong in that group.
                //
                //groupByRecord.Description = dateLabel;
                groupByRecord.DynamicDesriptionString = dateLabel;

                // JJD 11/10/09 - TFS24195
                // Return only the date portion as the GroupByValue
                //return targetDate;
                return targetDate.Date;
            }

            #endregion // GetGroupByValue

            // JJD 4/10/08
            // Added support for culture caching
            #region GetCultureInfoCache

            // JJD 10/09/08 - TFS6745 
            // Changed param to base Record
            //private static CultureCache GetCultureInfoCache(GroupByRecord grp)
            private static CultureCache GetCultureInfoCache(Record rcd)
            {
                DataPresenterBase dp = rcd.DataPresenter;

                CultureInfo culture = null;

                Debug.Assert(dp != null);

                if (dp != null)
                    culture = dp.DefaultConverterCulture;

                if (culture == null)
                {
                    // JJD 10/09/08 - TFS6745 
                    // Use CurrentCulture instead of CurrentUICulture
                    //culture = Thread.CurrentThread.CurrentUICulture;
                    culture = Thread.CurrentThread.CurrentCulture;

                    if (culture == null)
                        culture = CultureInfo.InvariantCulture;
                }
 
                if (g_cultureCache == null)
                    g_cultureCache = new Dictionary<CultureInfo, CultureCache>();

                CultureCache cic;

                if (g_cultureCache.TryGetValue(culture, out cic))
                    return cic;

                cic = new CultureCache(culture);

                g_cultureCache.Add(culture, cic);

                return cic;
            }

            #endregion //GetCultureInfoCache	
    
            #region DoesGroupContainRecord

            bool IGroupByEvaluator.DoesGroupContainRecord(GroupByRecord groupByRecord, DataRecord record)
            {
                // JJD 4/10/08
                // Added support for culture caching
                CultureCache cic = GetCultureInfoCache(groupByRecord);

                object grpValue = groupByRecord.Value;

                // JJD 5/29/09 - TFS18063 
                // Use the new overload to GetCellValue which will return the value 
                // converted into EditAsType
                //object cellValue = record.GetCellValue(groupByRecord.GroupByField, true);
                object cellValue = record.GetCellValue(groupByRecord.GroupByField, CellValueType.EditAsType);
               
                DynamicResourceString currentDateLabel = groupByRecord.DynamicDesriptionString;

                // Empty cells fall into the 'None' category.
                //
                if (cellValue == null || cellValue is DBNull)
                {
                    // JJD 4/10/08
                    //return currentDateLabel == this.NONE;
                    return currentDateLabel == cic.NONE;
                }

                if (!(cellValue is DateTime))
                {
                    // JJD 4/10/08
                    // Return true only if the grpValue is not a DateTime
                    //return true;
                    return (currentDateLabel == cic.NONE);
                }
                else
                {
                    // JJD 4/10/08
                    // return false if the currentDateLabel is for the null group
                    if (currentDateLabel == cic.NONE)
                        return false;
                }

                // JJD 4/10/08
                // Added support for culture caching
                // string dateLabel = this.GetDateLabel((DateTime)cellValue);
                DynamicResourceString cellLabel = cic.GetDateLabel((DateTime)cellValue);
                DynamicResourceString grpLabel = cic.GetDateLabel((DateTime)grpValue);

                // JJD 4/10/08
                // Compare the DynamicResourceString as objects
                //// Since the groupby record's Value property must store a DateTime object if the field's DataType
                //// is DateTime, we determine if the current value is in the group via comparing the Description
                //// of the group against the Description (a.k.a "date label") associated with the current value.
                //
                //return dateLabel == currentDateLabel;
                if (Object.ReferenceEquals(cellLabel, grpLabel))
                {
                    if (currentDateLabel != grpLabel)
                        groupByRecord.DynamicDesriptionString = grpLabel;

                    return true;
                }

                return false;
            }

            #endregion // DoesGroupContainRecord

            #region GetDateLabel - JJD 4/10/08 - moved into CultureCache

//#if DEBUG
//            /// <summary>
//            /// Returns the appropriate text to be displayed in the description of a groupby record which contains this date.
//            /// </summary>
//#endif
//            //private string GetDateLabel(DateTime targetDate)
//            private string GetDateLabel(DateTime targetDate, GroupByRecord groupByRecord)
//            {
//                // If the list of date information is stale or empty, rebuild it.
//                //
//                if (this.lastUpdated.Date < DateTime.Today)
//                    this.InitDateInfos();

//                foreach (DateInfo info in this.dateInfos)
//                    if (info.Contains(targetDate))
//                        return info.DateLabel;

//                Debug.Fail("Could not find a date range for the target date: " + targetDate.ToString());

//                return String.Empty;
//            }

            #endregion // GetDateLabel

            #region InitDateInfos - JJD 4/10/08 - moved into CultureCache

//#if DEBUG
//            /// <summary>
//            /// Populates the 'dateInfos' member variable with DateInfo objects.  Those
//            /// objects are used by the GetDateLabel method as it attempts to determine
//            /// which date range a specified date falls into.
//            /// </summary>
//#endif
//            // JJD 4/10/08 - added cultureInfo param
//            //private void InitDateInfos()
//            private void InitDateInfos(CultureInfo cultureInfo)
//            {
//                if ( cultureInfo == null )
//                    cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;

//                const int daysPerWeek = 7;
//                //DayOfWeek firstDayOfWeek = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
//                DayOfWeek firstDayOfWeek =  cultureInfo.DateTimeFormat.FirstDayOfWeek;

//                DateTime today = DateTime.Today;
//                DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
//                DateTime startOfWeek = today;

//                // Travel back in time to the first day of the week.
//                //
//                while (startOfWeek.DayOfWeek != firstDayOfWeek)
//                    startOfWeek = startOfWeek.AddDays(-1);

//                this.dateInfos = new ArrayList();
//                DateTime begin, end;

//                #region The Past

//                // OLDER THAN LAST MONTH
//                //
//                begin = DateTime.MinValue;
//                end = startOfMonth.AddMonths(-1).AddDays(-1);
//                this.dateInfos.Add(new DateInfo(begin, end, this.OLDER));

//                // THREE WEEKS AGO
//                //
//                begin = startOfWeek.AddDays(-(daysPerWeek * 3));
//                end = begin.AddDays(daysPerWeek - 1);
//                this.dateInfos.Add(new DateInfo(begin, end, this.THREE_WEEKS_AGO));

//                // TWO WEEKS AGO
//                //
//                begin = startOfWeek.AddDays(-(daysPerWeek * 2));
//                end = begin.AddDays(daysPerWeek - 1);
//                this.dateInfos.Add(new DateInfo(begin, end, this.TWO_WEEKS_AGO));

//                // LAST WEEK
//                //
//                begin = startOfWeek.AddDays(-daysPerWeek);
//                end = begin.AddDays(daysPerWeek - 1);
//                this.dateInfos.Add(new DateInfo(begin, end, this.LAST_WEEK));

//                // EARLIER THIS MONTH 
//                // This catches all days which slip between the cracks of "3 weeks ago" and "last month", if any.
//                // This entry must come after "N Weeks Ago" entries for this to work properly.
//                //
//                begin = startOfMonth;
//                end = startOfWeek.AddDays(-1);
//                this.dateInfos.Add(new DateInfo(begin, end, this.EARLIER_THIS_MONTH));

//                // LAST MONTH - Note: Add this after the "N Weeks Ago" and "Last Week" entries
//                // because the collection of DateInfo objects is read from beginning to end, and
//                // the "Last Month" entry does not account for a date existing in the "N Weeks Ago"
//                // categories.
//                //
//                begin = startOfMonth.AddMonths(-1);
//                end = startOfMonth.AddDays(-1);
//                this.dateInfos.Add(new DateInfo(begin, end, this.LAST_MONTH));

//                #endregion // The Past

//                #region The Present Week

//                DateTime currentDay = startOfWeek;

//                for (int i = 0; i < daysPerWeek; ++i)
//                {
//                    bool isToday = currentDay.DayOfWeek == today.DayOfWeek;
//                    bool isYesterday =
//                        (int)currentDay.DayOfWeek + 1 == (int)today.DayOfWeek
//                        || // If the week does not start on Sunday, then Sunday follows Saturday in the same week.
//                        currentDay.DayOfWeek == DayOfWeek.Sunday && today.DayOfWeek == DayOfWeek.Saturday;

//                    string label;

//                    if (isToday)
//                        label = this.TODAY;
//                    else if (isYesterday)
//                        label = this.YESTERDAY;
//                    else
//                        label = this.DayMap[currentDay.DayOfWeek] as string;

//                    Debug.Assert(label != null && label.Length > 0, "The label value was not determined.");

//                    this.dateInfos.Add(new DateInfo(currentDay, currentDay, label));

//                    // Bump the current day up so that the next iteration through the loop 
//                    // will calculate the next day.
//                    //
//                    currentDay = currentDay.AddDays(1);

//                    // If the day that we just added was the last day of the week,
//                    // we must break out of the loop.
//                    //
//                    if (currentDay.DayOfWeek == firstDayOfWeek)
//                        break;
//                }

//                #endregion // The Present Week

//                #region The Future

//                // NEXT WEEK
//                //
//                begin = startOfWeek.AddDays(daysPerWeek);
//                end = begin.AddDays(daysPerWeek - 1);
//                this.dateInfos.Add(new DateInfo(begin, end, this.NEXT_WEEK));

//                // TWO WEEKS AWAY
//                //
//                begin = startOfWeek.AddDays(daysPerWeek * 2);
//                end = begin.AddDays(daysPerWeek - 1);
//                this.dateInfos.Add(new DateInfo(begin, end, this.TWO_WEEKS_AWAY));

//                // THREE WEEKS AWAY
//                //
//                begin = startOfWeek.AddDays(daysPerWeek * 3);
//                end = begin.AddDays(daysPerWeek - 1);
//                this.dateInfos.Add(new DateInfo(begin, end, this.THREE_WEEKS_AWAY));

//                // LATER THIS MONTH 
//                // This catches all days which slip between the cracks of "3 weeks away" and "next month", if any.
//                // This entry must come after the "N Weeks Away" entries for this to work properly.
//                //
//                begin = startOfMonth;
//                end = startOfMonth.AddMonths(1).AddDays(-1);
//                this.dateInfos.Add(new DateInfo(begin, end, this.LATER_THIS_MONTH));

//                // NEXT MONTH
//                // This entry must come after the "N Weeks Away" entries for this to work properly.
//                //
//                begin = startOfMonth.AddMonths(1);
//                end = startOfMonth.AddMonths(2).AddDays(-1);
//                this.dateInfos.Add(new DateInfo(begin, end, this.NEXT_MONTH));

//                // BEYOND NEXT MONTH
//                //
//                begin = startOfMonth.AddMonths(2);
//                end = DateTime.MaxValue;
//                this.dateInfos.Add(new DateInfo(begin, end, this.BEYOND_NEXT_MONTH));

//                #endregion // The Future

//                // Cache the date that this list of DateInfo was last updated.
//                //
//                this.lastUpdated = today;
//            }

            #endregion // InitDateInfos

			#region SortComparer

			IComparer IGroupByEvaluator.SortComparer
			{
				get 
                {
                    // JJD 10/09/08 - TFS6745 - added
                    if (this._comparer == null)
                        this._comparer = new Comparer(this);
 
                    return this._comparer; 
                }
			}

			#endregion //SortComparer
            
            // JJD 10/09/08 - TFS6745 - added
            #region Comparer private class

            private class Comparer : DateComparerBase
            {
                private OutlookDateEvaluator _evaluator;
                private CultureCache _cic;

                internal Comparer(OutlookDateEvaluator evaluator)
                {
                    this._evaluator = evaluator;
                }

				public override int Compare( object x, object y )
				{
					Cell xCell = x as Cell;

					// JJD 4/10/08
					// Added support for culture caching
					this._cic = GetCultureInfoCache( xCell.Record );

					return base.Compare( x, y );
				}

				// SSP 5/29/09 - TFS17233 - Optimization
				// Overrode the new overload as well so we can cache the culture info cache.
				// 
				public override int Compare(
					RecordManager.SameFieldRecordsSortComparer.CellInfo x, 
					RecordManager.SameFieldRecordsSortComparer.CellInfo y)
                {
					// JJD 4/10/08
					// Added support for culture caching
					this._cic = GetCultureInfoCache( x._record );

                    return base.Compare(x, y);
                }

                protected override bool AreInSameGroupBy(DateTime x, DateTime y)
                {
                    DynamicResourceString xLabel = this._cic.GetDateLabel(x);
                    DynamicResourceString yLabel = this._cic.GetDateLabel(y);

                    // Compare the DynamicResourceString as objects
                    return (Object.ReferenceEquals(xLabel, yLabel));
                }
            }

            #endregion //Comparer private class

            #region DayMap - JJD 4/10/08 - moved into CultureCache

//#if DEBUG
//            /// <summary>
//            /// Returns a Hashtable where the keys are DayOfWeek enum members and the values 
//            /// are the corresponding localized day names.
//            /// </summary>
//#endif
//            private Hashtable DayMap
//            {
//                get
//                {
//                    if (this.dayMap == null)
//                    {
//                        Debug.Assert(this.SUNDAY != null, "DayMap should not be accessed before the day name strings have been given values.");

//                        this.dayMap = new Hashtable(7);
//                        this.dayMap.Add(DayOfWeek.Sunday, this.SUNDAY);
//                        this.dayMap.Add(DayOfWeek.Monday, this.MONDAY);
//                        this.dayMap.Add(DayOfWeek.Tuesday, this.TUESDAY);
//                        this.dayMap.Add(DayOfWeek.Wednesday, this.WEDNESDAY);
//                        this.dayMap.Add(DayOfWeek.Thursday, this.THURSDAY);
//                        this.dayMap.Add(DayOfWeek.Friday, this.FRIDAY);
//                        this.dayMap.Add(DayOfWeek.Saturday, this.SATURDAY);
//                    }

//                    return this.dayMap;
//                }
//            }

            #endregion // DayMap

            #region CultureCache private class

            private class CultureCache
            {
                #region Private Members

                private CultureInfo _culture;
                internal readonly DynamicResourceString BEYOND_NEXT_MONTH;
                internal readonly DynamicResourceString NEXT_MONTH;
                internal readonly DynamicResourceString LATER_THIS_MONTH;
                internal readonly DynamicResourceString THREE_WEEKS_AWAY;
                internal readonly DynamicResourceString TWO_WEEKS_AWAY;
                internal readonly DynamicResourceString NEXT_WEEK;
                internal readonly DynamicResourceString TODAY;
                internal readonly DynamicResourceString YESTERDAY;
                internal readonly DynamicResourceString SUNDAY;
                internal readonly DynamicResourceString MONDAY;
                internal readonly DynamicResourceString TUESDAY;
                internal readonly DynamicResourceString WEDNESDAY;
                internal readonly DynamicResourceString THURSDAY;
                internal readonly DynamicResourceString FRIDAY;
                internal readonly DynamicResourceString SATURDAY;
                internal readonly DynamicResourceString LAST_WEEK;
                internal readonly DynamicResourceString TWO_WEEKS_AGO;
                internal readonly DynamicResourceString THREE_WEEKS_AGO;
                internal readonly DynamicResourceString EARLIER_THIS_MONTH;
                internal readonly DynamicResourceString LAST_MONTH;
                internal readonly DynamicResourceString OLDER;
                internal readonly DynamicResourceString NONE;






                private ArrayList dateInfos;






                private DateTime lastUpdated;






                private Dictionary<DayOfWeek, DynamicResourceString> dayMap;

                #endregion // Private Members

                #region Contructor

                internal CultureCache(CultureInfo culture)
                {
                    this._culture = culture;

                    // Get a reference to a Sunday.
                    //
                    DateTime sunday = DateTime.Today;
                    while (sunday.DayOfWeek != DayOfWeek.Sunday)
                        sunday = sunday.AddDays(1);

                    // Load day names using a format string stored in the resource file.
                    //
                    SUNDAY = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_DayOfWeekFormatString", new object[] { sunday }, this._culture);
                    MONDAY = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_DayOfWeekFormatString", new object[] { sunday.AddDays(1) }, this._culture);
                    TUESDAY = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_DayOfWeekFormatString", new object[] { sunday.AddDays(2) }, this._culture);
                    WEDNESDAY = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_DayOfWeekFormatString", new object[] { sunday.AddDays(3) }, this._culture);
                    THURSDAY = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_DayOfWeekFormatString", new object[] { sunday.AddDays(4) }, this._culture);
                    FRIDAY = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_DayOfWeekFormatString", new object[] { sunday.AddDays(5) }, this._culture);
                    SATURDAY = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_DayOfWeekFormatString", new object[] { sunday.AddDays(6) }, this._culture);

                    BEYOND_NEXT_MONTH = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_BeyondNextMonth", new object[] { }, this._culture);
                    NEXT_MONTH = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_NextMonth", new object[] { }, this._culture);
                    LATER_THIS_MONTH = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_LaterThisMonth", new object[] { }, this._culture);
                    THREE_WEEKS_AWAY = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_ThreeWeeksAway", new object[] { }, this._culture);
                    TWO_WEEKS_AWAY = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_TwoWeeksAway", new object[] { }, this._culture);
                    NEXT_WEEK = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_NextWeek", new object[] { }, this._culture);
                    TODAY = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_Today", new object[] { }, this._culture);
                    YESTERDAY = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_Yesterday", new object[] { }, this._culture);
                    LAST_WEEK = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_LastWeek", new object[] { }, this._culture);
                    TWO_WEEKS_AGO = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_TwoWeeksAgo", new object[] { }, this._culture);
                    THREE_WEEKS_AGO = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_ThreeWeeksAgo", new object[] { }, this._culture);
                    EARLIER_THIS_MONTH = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_EarlierThisMonth", new object[] { }, this._culture);
                    LAST_MONTH = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_LastMonth", new object[] { }, this._culture);
                    OLDER = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_Older", new object[] { }, this._culture);
                    NONE = Infragistics.Windows.DataPresenter.Resources.GetDynamicResourceString("Outlook_GroupByMode_Description_None", new object[] { }, this._culture);

                }

                #endregion //Contructor

                #region DayMap







                internal Dictionary<DayOfWeek, DynamicResourceString> DayMap
                {
                    get
                    {
                        if (this.dayMap == null)
                        {
                            Debug.Assert(this.SUNDAY != null, "DayMap should not be accessed before the day name strings have been given values.");

                            this.dayMap = new Dictionary<DayOfWeek, DynamicResourceString>(7);
                            this.dayMap.Add(DayOfWeek.Sunday, this.SUNDAY);
                            this.dayMap.Add(DayOfWeek.Monday, this.MONDAY);
                            this.dayMap.Add(DayOfWeek.Tuesday, this.TUESDAY);
                            this.dayMap.Add(DayOfWeek.Wednesday, this.WEDNESDAY);
                            this.dayMap.Add(DayOfWeek.Thursday, this.THURSDAY);
                            this.dayMap.Add(DayOfWeek.Friday, this.FRIDAY);
                            this.dayMap.Add(DayOfWeek.Saturday, this.SATURDAY);
                        }

                        return this.dayMap;
                    }
                }

                #endregion // DayMap

                #region GetDateLabel






                internal DynamicResourceString GetDateLabel(DateTime targetDate)
                {
                    // If the list of date information is stale or empty, rebuild it.
                    //
                    if (this.lastUpdated.Date < DateTime.Today)
                        this.InitDateInfos();

                    foreach (DateInfo info in this.dateInfos)
                        if (info.Contains(targetDate))
                            return info.DateLabel;

                    Debug.Fail("Could not find a date range for the target date: " + targetDate.ToString());

                    return null;
                }

                #endregion // GetDateLabel

                #region InitDateInfos



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                // JJD 4/10/08 - added cultureInfo param
                //private void InitDateInfos()
                private void InitDateInfos()
                {
                    const int daysPerWeek = 7;
                    //DayOfWeek firstDayOfWeek = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

                    DayOfWeek firstDayOfWeek = this._culture.DateTimeFormat.FirstDayOfWeek;

                    DateTime today = DateTime.Today;
                    DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
                    DateTime startOfWeek = today;

                    // Travel back in time to the first day of the week.
                    //
                    while (startOfWeek.DayOfWeek != firstDayOfWeek)
                        startOfWeek = startOfWeek.AddDays(-1);

                    this.dateInfos = new ArrayList();
                    DateTime begin, end;

					// AS 8/15/11 TFS84145
					// The handling for yesterday is incorrect.
					//
					DateTime yesterday = today.AddDays(-1);

                    #region The Past

                    // OLDER THAN LAST MONTH
                    //
                    begin = DateTime.MinValue;
                    end = startOfMonth.AddMonths(-1).AddDays(-1);
                    this.dateInfos.Add(new DateInfo(begin, end, this.OLDER));

                    // THREE WEEKS AGO
                    //
                    begin = startOfWeek.AddDays(-(daysPerWeek * 3));
                    end = begin.AddDays(daysPerWeek - 1);
                    this.dateInfos.Add(new DateInfo(begin, end, this.THREE_WEEKS_AGO));

                    // TWO WEEKS AGO
                    //
                    begin = startOfWeek.AddDays(-(daysPerWeek * 2));
                    end = begin.AddDays(daysPerWeek - 1);
                    this.dateInfos.Add(new DateInfo(begin, end, this.TWO_WEEKS_AGO));

                    // LAST WEEK
                    //
                    begin = startOfWeek.AddDays(-daysPerWeek);
                    end = begin.AddDays(daysPerWeek - 1);

					// AS 8/15/11 TFS84145
					// If Today is the 1st day of the week then we should still include a Yesterday and 
					// last week should not include that date.
					//
					if (end == yesterday)
					{
						this.dateInfos.Add(new DateInfo(end, end, this.YESTERDAY));
						end = end.AddDays(-1);
					}

                    this.dateInfos.Add(new DateInfo(begin, end, this.LAST_WEEK));

                    // EARLIER THIS MONTH 
                    // This catches all days which slip between the cracks of "3 weeks ago" and "last month", if any.
                    // This entry must come after "N Weeks Ago" entries for this to work properly.
                    //
                    begin = startOfMonth;
                    end = startOfWeek.AddDays(-1);
                    this.dateInfos.Add(new DateInfo(begin, end, this.EARLIER_THIS_MONTH));

                    // LAST MONTH - Note: Add this after the "N Weeks Ago" and "Last Week" entries
                    // because the collection of DateInfo objects is read from beginning to end, and
                    // the "Last Month" entry does not account for a date existing in the "N Weeks Ago"
                    // categories.
                    //
                    begin = startOfMonth.AddMonths(-1);
                    end = startOfMonth.AddDays(-1);
                    this.dateInfos.Add(new DateInfo(begin, end, this.LAST_MONTH));

                    #endregion // The Past

                    #region The Present Week

                    DateTime currentDay = startOfWeek;

                    for (int i = 0; i < daysPerWeek; ++i)
                    {
						// AS 8/15/11 TFS84145
						// When the FirstDayOfWeek is not Sunday then this can generate
						// incorrect results.
						//
						//bool isToday = currentDay.DayOfWeek == today.DayOfWeek;
						//bool isYesterday =
						//    (int)currentDay.DayOfWeek + 1 == (int)today.DayOfWeek
						//    || // If the week does not start on Sunday, then Sunday follows Saturday in the same week.
						//    currentDay.DayOfWeek == DayOfWeek.Sunday && today.DayOfWeek == DayOfWeek.Saturday;

                        DynamicResourceString label;

						// AS 8/15/11 TFS84145
						// Instead of using the bools above just compare the dates.
						//
                        if (currentDay == today)
                            label = this.TODAY;
                        else if (currentDay == yesterday)
                            label = this.YESTERDAY;
                        else
                            label = this.DayMap[currentDay.DayOfWeek];

                        Debug.Assert(label != null && label.Value.Length > 0, "The label value was not determined.");

                        this.dateInfos.Add(new DateInfo(currentDay, currentDay, label));

                        // Bump the current day up so that the next iteration through the loop 
                        // will calculate the next day.
                        //
                        currentDay = currentDay.AddDays(1);

                        // If the day that we just added was the last day of the week,
                        // we must break out of the loop.
                        //
                        if (currentDay.DayOfWeek == firstDayOfWeek)
                            break;
                    }

                    #endregion // The Present Week

                    #region The Future

                    // NEXT WEEK
                    //
                    begin = startOfWeek.AddDays(daysPerWeek);
                    end = begin.AddDays(daysPerWeek - 1);
                    this.dateInfos.Add(new DateInfo(begin, end, this.NEXT_WEEK));

                    // TWO WEEKS AWAY
                    //
                    begin = startOfWeek.AddDays(daysPerWeek * 2);
                    end = begin.AddDays(daysPerWeek - 1);
                    this.dateInfos.Add(new DateInfo(begin, end, this.TWO_WEEKS_AWAY));

                    // THREE WEEKS AWAY
                    //
                    begin = startOfWeek.AddDays(daysPerWeek * 3);
                    end = begin.AddDays(daysPerWeek - 1);
                    this.dateInfos.Add(new DateInfo(begin, end, this.THREE_WEEKS_AWAY));

                    // LATER THIS MONTH 
                    // This catches all days which slip between the cracks of "3 weeks away" and "next month", if any.
                    // This entry must come after the "N Weeks Away" entries for this to work properly.
                    //
                    begin = startOfMonth;
                    end = startOfMonth.AddMonths(1).AddDays(-1);
                    this.dateInfos.Add(new DateInfo(begin, end, this.LATER_THIS_MONTH));

                    // NEXT MONTH
                    // This entry must come after the "N Weeks Away" entries for this to work properly.
                    //
                    begin = startOfMonth.AddMonths(1);
                    end = startOfMonth.AddMonths(2).AddDays(-1);
                    this.dateInfos.Add(new DateInfo(begin, end, this.NEXT_MONTH));

                    // BEYOND NEXT MONTH
                    //
                    begin = startOfMonth.AddMonths(2);
                    end = DateTime.MaxValue;
                    this.dateInfos.Add(new DateInfo(begin, end, this.BEYOND_NEXT_MONTH));

                    #endregion // The Future

                    // Cache the date that this list of DateInfo was last updated.
                    //
                    this.lastUpdated = today;
                }

                #endregion // InitDateInfos
            }

            #endregion //CultureCache private class	
    
            #region DateInfo class

            private class DateInfo
            {
                #region Data

                private readonly DateTime beginDate;
                private readonly DateTime endDate;

                // JJD 4/10/08 changed to use DynamicResourceString so 
                // changes wiull get picked up
                //private readonly string dateLabel;
                private readonly DynamicResourceString dateLabel;

                #endregion // Data

                #region Ctor

                // JJD 4/10/08 changed to use DynamicResourceString so 
                // changes wiull get picked up
                //internal DateInfo(DateTime begin, DateTime end, string label)
                internal DateInfo(DateTime begin, DateTime end, DynamicResourceString label)
                {
                    this.beginDate = begin;
                    this.endDate = end;
                    this.dateLabel = label;
                }

                #endregion // Ctor

                #region Contains






                internal bool Contains(DateTime date)
                {
                    return this.beginDate.Date <= date.Date
                        &&
                        date.Date <= this.endDate.Date;
                }

                #endregion // Contains

                #region DateLabel






                internal DynamicResourceString DateLabel
                {
                    get { return this.dateLabel; }
                }

                #endregion // DateLabel
            }

            #endregion // DateInfo class
        }

        #endregion // OutlookDateEvaluator

        #region QuarterEvaluator

        // Groups dates based on which quarter of which year they fall into.
        //
        private class QuarterEvaluator : IGroupByEvaluator
        {
            #region Private Members

            // JJD 10/09/08 - TFS6745 - added
            private Comparer _comparer;

            #endregion //Private Members
	
            #region GetGroupByValue

            object IGroupByEvaluator.GetGroupByValue(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dt = DateEvaluator.GetGroupByValueHelper(groupByRecord, record);

                // In case this evaluator is being used on a field which is not of type DateTime
                // then just return the dummy value MinValue.
                //
                if (dt == DateTime.MinValue)
                    return dt;

                // Determine the month which represents the quarter that the date falls into.
                //
                int month = dt.Month - (dt.Month - 1) % 3;

                // Set the Day portion to 1 for every group so that the arbitrary Day values
                // found in cells do not get displayed in the group descriptions.
                //
                return new DateTime(dt.Year, month, 1);
            }

            #endregion // GetGroupByValue

            #region DoesGroupContainRecord

            bool IGroupByEvaluator.DoesGroupContainRecord(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dateInCell, dateInGrp;
                if (DateEvaluator.DoesGroupContainRecordHelper(groupByRecord, record, out dateInGrp, out dateInCell))
                    return true;

                return dateInCell.Year == dateInGrp.Year &&
                    dateInCell.Month >= dateInGrp.Month &&
                    dateInCell.Month < dateInGrp.Month + 3;
            }

            #endregion // DoesGroupContainRecord

			#region SortComparer

			IComparer IGroupByEvaluator.SortComparer
			{
				get 
                {
                    // JJD 10/09/08 - TFS6745 - added
                    if (this._comparer == null)
                        this._comparer = new Comparer();

                    return this._comparer; 
                }
			}

			#endregion //SortComparer
            
            // JJD 10/09/08 - TFS6745 - added
            #region Comparer private class

            private class Comparer : DateComparerBase
            {
                protected override bool AreInSameGroupBy(DateTime x, DateTime y)
                {
                    if (x.Year == y.Year)
                        return (x.Month - 1) / 3 == (y.Month - 1) / 3;

                    return false;
                 }
            }

            #endregion //Comparer private class
        }

        #endregion // QuarterEvaluator

        #region SecondEvaluator

        // Evaluates the Date, Hour, Minute, and Second portions of a DateTime.
        //
        private class SecondEvaluator : IGroupByEvaluator
        {
            #region Private Members

            // JJD 10/09/08 - TFS6745 - added
            private Comparer _comparer;

            #endregion //Private Members	
    
            #region GetGroupByValue

            object IGroupByEvaluator.GetGroupByValue(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dt = DateEvaluator.GetGroupByValueHelper(groupByRecord, record);

                // Create a new DateTime object so that the Millisecond component gets eliminated.
                //
                return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }

            #endregion // GetGroupByValue

            #region DoesGroupContainRecord

            bool IGroupByEvaluator.DoesGroupContainRecord(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dateInCell, dateInGrp;
                if (DateEvaluator.DoesGroupContainRecordHelper(groupByRecord, record, out dateInGrp, out dateInCell))
                    return true;

                return dateInCell.Date == dateInGrp.Date &&
                    dateInCell.Hour == dateInGrp.Hour &&
                    dateInCell.Minute == dateInGrp.Minute &&
                    dateInCell.Second == dateInGrp.Second;
            }

            #endregion // DoesGroupContainRecord

			#region SortComparer

			IComparer IGroupByEvaluator.SortComparer
			{
				get 
                {
                    // JJD 10/09/08 - TFS6745 - added
                    if (this._comparer == null)
                        this._comparer = new Comparer();

                    return this._comparer; 
                }
			}

			#endregion //SortComparer
            
            // JJD 10/09/08 - TFS6745 - added
            #region Comparer private class

            private class Comparer : DateComparerBase
            {
                protected override bool AreInSameGroupBy(DateTime x, DateTime y)
                {
                    return x.Date == y.Date &&
                        x.Hour == y.Hour &&
                        x.Minute == y.Minute &&
                        x.Second == y.Second;
                }
            }

            #endregion //Comparer private class
        }

        #endregion // SecondEvaluator

        // JJD 2/29/08 - added
        #region ValueEvaluator

        // Evaluates values .
        //
        private class ValueEvaluator : IGroupByEvaluator
        {
            #region GetGroupByValue

            object IGroupByEvaluator.GetGroupByValue(GroupByRecord groupByRecord, DataRecord record)
            {
                return groupByRecord.Value;
            }

            #endregion // GetGroupByValue

            #region DoesGroupContainRecord

            bool IGroupByEvaluator.DoesGroupContainRecord(GroupByRecord groupByRecord, DataRecord record)
            {
                Field field = groupByRecord.GroupByField;

                // JJD 5/29/09 - TFS18063 
                // Use the new overload to GetCellValue which will return the value 
                // converted into EditAsType
                //object cellValue = record.GetCellValue(field, true);
                object cellValue = record.GetCellValue(field, CellValueType.EditAsType);

                IComparer comparer = field.SortComparerResolved;

                // If they have specified a sort comparer then get the cell from
                // the first record in the collection and use that for comparison.
                //
                if (null != comparer)
                {
                    return comparer.Compare(cellValue, groupByRecord.Value) == 0;
                }

                return RecordManager.RecordsSortComparer.DefaultCompare(cellValue, groupByRecord.Value, false, false) == 0;
            }

            #endregion // DoesGroupContainRecord

			#region SortComparer

			/// <summary>
			/// The comparer to use for sorting the records.
			/// </summary>
			/// <value>Returns an object that implements IComparer or null to use the default sort comparer.</value>
			public IComparer SortComparer
			{
				get { return null; }
			}

			#endregion //SortComparer
        }

        #endregion // DateEvaluator

        #region YearEvaluator

        // Evaluates the Year portion of a DateTime.
        //
        private class YearEvaluator : IGroupByEvaluator
        {
            #region Private Members

            // JJD 10/09/08 - TFS6745 - added
            private Comparer _comparer;

            #endregion //Private Members	
    
            #region GetGroupByValue

            object IGroupByEvaluator.GetGroupByValue(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dt = DateEvaluator.GetGroupByValueHelper(groupByRecord, record);

                // Set the Month and Day portions to 1 for every group so that the arbitrary Month 
                // and Day values found in cells do not get displayed in the group descriptions.
                //
                return new DateTime(dt.Year, 1, 1);
            }

            #endregion // GetGroupByValue

            #region DoesGroupContainRecord

            bool IGroupByEvaluator.DoesGroupContainRecord(GroupByRecord groupByRecord, DataRecord record)
            {
                DateTime dateInCell, dateInGrp;
                if (DateEvaluator.DoesGroupContainRecordHelper(groupByRecord, record, out dateInGrp, out dateInCell))
                    return true;

                return dateInCell.Year == dateInGrp.Year;
            }

            #endregion // DoesGroupContainRecord

			#region SortComparer

			IComparer IGroupByEvaluator.SortComparer
			{
				get 
                {
                    // JJD 10/09/08 - TFS6745 - added
                    if (this._comparer == null)
                        this._comparer = new Comparer();

                    return this._comparer; 
                }
			}

			#endregion //SortComparer
            
            // JJD 10/09/08 - TFS6745 - added
            #region Comparer private class

            private class Comparer : DateComparerBase
            {
                protected override bool AreInSameGroupBy(DateTime x, DateTime y)
                {
                    return x.Year == y.Year;
                }
            }

            #endregion //Comparer private class
        }

        #endregion // YearEvaluator

        #endregion // Evaluator Classes
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved