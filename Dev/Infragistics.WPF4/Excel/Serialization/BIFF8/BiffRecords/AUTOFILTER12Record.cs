using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Filtering;
using Infragistics.Documents.Excel.Serialization.Excel2007;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	// MD 2/21/12 - 12.1 - Table Support
	// http://msdn.microsoft.com/en-us/library/dd947970(v=office.12).aspx
	internal class AUTOFILTER12Record : Biff8RecordBase
	{
		public const int CFTNotCustom = 0x00000000;
		public const int CFTAboveAverage = 0x00000001;
		public const int CFTBelowAverage = 0x00000002;
		public const int CFTTomorrow = 0x00000008;
		public const int CFTToday = 0x00000009;
		public const int CFTYesterday = 0x0000000A;
		public const int CFTNextWeek = 0x0000000B;
		public const int CFTThisWeek = 0x0000000C;
		public const int CFTLastWeek = 0x0000000D;
		public const int CFTNextMonth = 0x0000000E;
		public const int CFTThisMonth = 0x0000000F;
		public const int CFTLastMonth = 0x00000010;
		public const int CFTNextQuarter = 0x00000011;
		public const int CFTThisQuarter = 0x00000012;
		public const int CFTLastQuarter = 0x00000013;
		public const int CFTNextYear = 0x00000014;
		public const int CFTThisYear = 0x00000015;
		public const int CFTLastYear = 0x00000016;
		public const int CFTYearToDate = 0x00000017;
		public const int CFT1stQuarter = 0x00000018;
		public const int CFT2ndQuarter = 0x00000019;
		public const int CFT3rdQuarter = 0x0000001A;
		public const int CFT4thQuarter = 0x0000001B;
		public const int CFT1stMonth = 0x0000001C;
		public const int CFT2ndMonth = 0x0000001D;
		public const int CFT3rdMonth = 0x0000001E;
		public const int CFT4thMonth = 0x0000001F;
		public const int CFT5thMonth = 0x00000020;
		public const int CFT6thMonth = 0x00000021;
		public const int CFT7thMonth = 0x00000022;
		public const int CFT8thMonth = 0x00000023;
		public const int CFT9thMonth = 0x00000024;
		public const int CFT10thMonth = 0x00000025;
		public const int CFT11thMonth = 0x00000026;
		public const int CFT12thMonth = 0x00000027;

		private const double DatePeriodFilterCondition1Value = 6.50121220736663E-319;
		private const double DatePeriodFilterCondition2Value = 1.66431042389899E-316;

		public override void Load(BIFF8WorkbookSerializationManager manager)
		{
			Worksheet worksheet = manager.ContextStack.Get<Worksheet>();
			if (worksheet == null)
			{
				Utilities.DebugFail("There is no worksheet in the context stack.");
				return;
			}

			WorksheetRegion region = manager.CurrentRecordStream.ReadFrtRefHeader(worksheet);
			ushort iEntry = manager.CurrentRecordStream.ReadUInt16();
			uint fHideArrow = manager.CurrentRecordStream.ReadUInt32();
			uint ft = manager.CurrentRecordStream.ReadUInt32();
			uint cft = manager.CurrentRecordStream.ReadUInt32();
			uint cCriteria = manager.CurrentRecordStream.ReadUInt32();
			uint cDateGroupings = manager.CurrentRecordStream.ReadUInt32();

			ushort temp16 = manager.CurrentRecordStream.ReadUInt16();
			bool fWorksheetAutoFilter = Utilities.TestBit(temp16, 3);

			uint unused2 = manager.CurrentRecordStream.ReadUInt32();
			uint idList = manager.CurrentRecordStream.ReadUInt32();
			if (idList == 0xFFFFFFFF)
			{
				Utilities.DebugFail("Worksheet filtering is not supported yet.");
				return;
			}

			WorksheetTable table = worksheet.Tables.GetTableById(idList);
			if (table == null)
			{
				Utilities.DebugFail("Cannot find the table.");
				return;
			}

			Debug.Assert(table.FilterRegion.Equals(region), "Something is wrong here.");
			if (table.Columns.Count <= iEntry)
			{
				Utilities.DebugFail("The iEntry value is out of range.");
				return;
			}

			WorksheetTableColumn column = table.Columns[iEntry];

			byte[] guidSview = manager.CurrentRecordStream.ReadBytes(16);

			Filter filter = null;

			double? value = null;
			double? maxValue = null;
			switch (cft)
			{
				case CFTNotCustom:
					break;

				case CFTAboveAverage:
				case CFTBelowAverage:
					{
						Debug.Assert(cCriteria == 1, "There should be one comparison condition here.");

						Biff8RecordStream.AFDOper condition1 = null;
						if (cCriteria >= 1)
							condition1 = manager.CurrentRecordStream.ReadAF12Criteria(worksheet);

						ST_FilterOperator expectedFilterOperator;
						if (cft == CFTAboveAverage)
							expectedFilterOperator = ST_FilterOperator.greaterThan;
						else
							expectedFilterOperator = ST_FilterOperator.lessThan;

						if (condition1 != null &&
							condition1.vt == Biff8RecordStream.AFDOper.VTDouble &&
							condition1.grbitSign == expectedFilterOperator)
						{
							value = (double)condition1.vtValue;
						}
						else
						{
							Utilities.DebugFail("Couldn't get the minValue");
						}
					}
					break;

				case CFTTomorrow:
				case CFTToday:
				case CFTYesterday:
				case CFTNextWeek:
				case CFTThisWeek:
				case CFTLastWeek:
				case CFTNextMonth:
				case CFTThisMonth:
				case CFTLastMonth:
				case CFTNextQuarter:
				case CFTThisQuarter:
				case CFTLastQuarter:
				case CFTNextYear:
				case CFTThisYear:
				case CFTLastYear:
				case CFTYearToDate:
					{
						Debug.Assert(cCriteria == 2, "There should be two comparison conditions here.");

						Biff8RecordStream.AFDOper condition1 = null;
						Biff8RecordStream.AFDOper condition2 = null;
						if (cCriteria >= 1)
						{
							condition1 = manager.CurrentRecordStream.ReadAF12Criteria(worksheet);
							if (cCriteria >= 2)
								condition2 = manager.CurrentRecordStream.ReadAF12Criteria(worksheet);
						}

						if (condition1 != null &&
							condition1.vt == Biff8RecordStream.AFDOper.VTDouble &&
							condition1.grbitSign == ST_FilterOperator.greaterThanOrEqual)
						{
							value = (double)condition1.vtValue;
						}
						else
						{
							Utilities.DebugFail("Couldn't get the minValue");
						}

						if (condition2 != null &&
							condition2.vt == Biff8RecordStream.AFDOper.VTDouble &&
							condition2.grbitSign == ST_FilterOperator.lessThan)
						{
							maxValue = (double)condition2.vtValue;
						}
						else
						{
							Utilities.DebugFail("Couldn't get the maxValue");
						}
					}
					break;

				case CFT1stQuarter:
				case CFT2ndQuarter:
				case CFT3rdQuarter:
				case CFT4thQuarter:
				case CFT1stMonth:
				case CFT2ndMonth:
				case CFT3rdMonth:
				case CFT4thMonth:
				case CFT5thMonth:
				case CFT6thMonth:
				case CFT7thMonth:
				case CFT8thMonth:
				case CFT9thMonth:
				case CFT10thMonth:
				case CFT11thMonth:
				case CFT12thMonth:
					{
						Debug.Assert(cCriteria == 2, "This is unexpected.");

						if (cCriteria >= 1)
						{
							Biff8RecordStream.AFDOper condition1 = manager.CurrentRecordStream.ReadAF12Criteria(worksheet);

							Debug.Assert(
								condition1.vt == Biff8RecordStream.AFDOper.VTDouble &&
								condition1.grbitSign == ST_FilterOperator.greaterThanOrEqual &&
								(double)condition1.vtValue == AUTOFILTER12Record.DatePeriodFilterCondition1Value, 
								"This is unexpected.");

							if (cCriteria >= 2)
							{
								Biff8RecordStream.AFDOper condition2 = manager.CurrentRecordStream.ReadAF12Criteria(worksheet);

								Debug.Assert(
									condition2.vt == Biff8RecordStream.AFDOper.VTDouble &&
									condition2.grbitSign == ST_FilterOperator.lessThan &&
									(double)condition2.vtValue == AUTOFILTER12Record.DatePeriodFilterCondition2Value,
									"This is unexpected.");
							}
						}
					}
					break;

				default:
					Utilities.DebugFail("Unknown cft value in the AutoFilter12 record.");
					break;
			}

			filter = DynamicValuesFilter.CreateDynamicValuesFilter(manager, column, (ST_DynamicFilterType)cft, value, maxValue);
			if (filter == null)
			{
				switch (ft)
				{
					case 0:
						{
							Debug.Assert(cCriteria != 0 || cDateGroupings != 0, "Either cCriteria or cDateGroupings must be non-zero.");
							FixedValuesFilter fixedValuesFilter = null;
							if (cCriteria != 0)
							{
								if (fixedValuesFilter == null)
									fixedValuesFilter = new FixedValuesFilter(column);

								for (int i = 0; i < cCriteria; i++)
								{
									Biff8RecordStream.AFDOper doper = manager.CurrentRecordStream.ReadAF12Criteria(worksheet);
									if (doper.vt == Biff8RecordStream.AFDOper.VTBlanks)
										fixedValuesFilter.IncludeBlanks = true;
									else if (doper.ResolvedValue != null)
										fixedValuesFilter.DisplayValues.Add(doper.ResolvedValue.ToString());
									else
										Utilities.DebugFail("This is unexpected.");
								}
							}

							if (cDateGroupings != 0)
							{
								if (fixedValuesFilter == null)
									fixedValuesFilter = new FixedValuesFilter(column);

								for (int i = 0; i < cDateGroupings; i++)
								{
									FixedDateGroup fixedDateGroup = manager.CurrentRecordStream.ReadAF12DateInfo(worksheet);
									if (fixedDateGroup != null)
										fixedValuesFilter.DateGroups.Add(fixedDateGroup);
								}
							}

							filter = fixedValuesFilter;
						}
						break;

					case 1:
						{
							WorksheetCellFormatData format = manager.CurrentRecordStream.ReadDXFN12NoCB();
							filter = FillFilter.CreateFillFilter(column, format);
						}
						break;

					case 2:
						{
							WorksheetCellFormatData format = manager.CurrentRecordStream.ReadDXFN12NoCB();
							filter = FontColorFilter.CreateFontColorFilter(column, format);
						}
						break;

					case 3:
						{
							ST_IconSetType? iconSet;
							uint iconIndex;
							manager.CurrentRecordStream.ReadAF12CellIcon(out iconSet, out iconIndex);

							if (iconSet.HasValue)
								filter = new IconFilter(column, iconSet.Value, iconIndex);
						}
						break;

					default:
						Utilities.DebugFail("Unknown ft value in the AutoFilter12 record.");
						break;
				}
			}

			if (filter == null)
			{
				Utilities.DebugFail("No filter was loaded.");
				return;
			}

			column.Filter = filter;
		}

		public override void Save(BIFF8WorkbookSerializationManager manager)
		{
			TableColumnFilterData filterData = manager.ContextStack.Get<TableColumnFilterData>();
			if (filterData == null)
			{
				Utilities.DebugFail("There is no TableColumnFilterData in the context stack.");
				return;
			}

			WorksheetTableColumn column = filterData.Column;
			WorksheetRegion filterRegion = column.Table.FilterRegion;

			uint ft = 0;
			IColorFilter colorFilter = column.Filter as IColorFilter;
			if (colorFilter != null)
			{
				if (colorFilter.IsCellColorFilter)
					ft = 1;
				else
					ft = 2;
			}

			IconFilter iconFilter = column.Filter as IconFilter;
			if (iconFilter != null)
			{
				ft = 3;
			}

			uint cft = 0;

			
			List<Biff8RecordStream.AFDOper> criteria = new List<Biff8RecordStream.AFDOper>();
			IList<FixedDateGroup> dateGroupings;
			FixedValuesFilter fixedValuesFilter = column.Filter as FixedValuesFilter;
			if (fixedValuesFilter != null)
			{
				if (filterData.AllowedTextValues != null)
				{
					for (int i = 0; i < filterData.AllowedTextValues.Count; i++)
						criteria.Add(new Biff8RecordStream.AFDOper(filterData.AllowedTextValues[i]));
				}

				if (fixedValuesFilter.IncludeBlanks)
				{
					Biff8RecordStream.AFDOper blanksDoper = new Biff8RecordStream.AFDOper();
					blanksDoper.vt = Biff8RecordStream.AFDOper.VTBlanks;
					criteria.Add(blanksDoper);
				}

				dateGroupings = fixedValuesFilter.DateGroups;
			}
			else
			{
				dateGroupings = new FixedDateGroup[0];

				DynamicValuesFilter dynamicValuesFilter = column.Filter as DynamicValuesFilter;
				if (dynamicValuesFilter != null)
				{
					cft = (uint)dynamicValuesFilter.Type2003;

					AverageFilter averageFilter = column.Filter as AverageFilter;
					DatePeriodFilter datePeriodFilter = column.Filter as DatePeriodFilter;
					DateRangeFilter dateRangeFilter = column.Filter as DateRangeFilter;
					if (averageFilter != null)
					{
						ST_FilterOperator filterOperator = averageFilter.Type == AverageFilterType.AboveAverage
							? ST_FilterOperator.greaterThan
							: ST_FilterOperator.lessThan;
						criteria.Add(new Biff8RecordStream.AFDOper(averageFilter.Average, filterOperator));
					}
					else if (datePeriodFilter != null)
					{
						criteria.Add(new Biff8RecordStream.AFDOper(AUTOFILTER12Record.DatePeriodFilterCondition1Value, ST_FilterOperator.greaterThanOrEqual));
						criteria.Add(new Biff8RecordStream.AFDOper(AUTOFILTER12Record.DatePeriodFilterCondition2Value, ST_FilterOperator.lessThan));
					}
					else if (dateRangeFilter != null)
					{
						double? startValue = ExcelCalcValue.DateTimeToExcelDate(manager.Workbook, dateRangeFilter.Start);
						double? endValue = ExcelCalcValue.DateTimeToExcelDate(manager.Workbook, dateRangeFilter.End);
						if (startValue.HasValue && endValue.HasValue)
						{
							criteria.Add(new Biff8RecordStream.AFDOper(startValue.Value, ST_FilterOperator.greaterThanOrEqual));
							criteria.Add(new Biff8RecordStream.AFDOper(endValue.Value, ST_FilterOperator.lessThan));
						}
						else
						{
							Utilities.DebugFail("Something is wrong here.");
						}
					}
				}
			}

			bool fWorksheetAutoFilter = column == null;
			uint idList = 0xFFFFFFFF;
			if (fWorksheetAutoFilter == false)
				idList = column.Table.Id;

			manager.CurrentRecordStream.WriteFrtRefHeader(filterRegion); // frtRefHeader 
			manager.CurrentRecordStream.Write((ushort)column.Index); // iEntry
			manager.CurrentRecordStream.Write((uint)0); // fHideArrow
			manager.CurrentRecordStream.Write((uint)ft);
			manager.CurrentRecordStream.Write((uint)cft);
			manager.CurrentRecordStream.Write((uint)criteria.Count); // cCriteria
			manager.CurrentRecordStream.Write((uint)dateGroupings.Count); // cDateGroupings

			ushort temp16 = 0;
			Utilities.SetBit(ref temp16, fWorksheetAutoFilter, 3);
			manager.CurrentRecordStream.Write(temp16);

			manager.CurrentRecordStream.Write((uint)0); // unused2
			manager.CurrentRecordStream.Write(idList);
			manager.CurrentRecordStream.Write(new byte[16]); // guidSview 

			switch (ft)
			{
				case 0:
					{
						for (int i = 0; i < criteria.Count; i++)
						{
							manager.CurrentRecordStream.CapCurrentBlock();
							manager.CurrentRecordStream.WriteAF12Criteria(filterRegion, criteria[i]);
						}

						for (int i = 0; i < dateGroupings.Count; i++)
						{
							manager.CurrentRecordStream.CapCurrentBlock();
							manager.CurrentRecordStream.WriteAF12DateInfo(filterRegion, dateGroupings[i]);
						}
					}
					break;

				case 1:
				case 2:
					{
						WorksheetCellFormatData format = colorFilter.GetDxf(manager);
						manager.CurrentRecordStream.WriteDXFN12NoCB(format);
					}
					break;

				case 3:
					manager.CurrentRecordStream.WriteAF12CellIcon(iconFilter.IconSet, iconFilter.IconIndex);
					break;

				default:
					Utilities.DebugFail("Unknown ft value in the AutoFilter12 record.");
					break;
			}
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.AUTOFILTER12; }
		}
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