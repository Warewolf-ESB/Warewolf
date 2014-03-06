using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter.Events;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// Class used to hold information about a <see cref="Cell"/> value change.
	/// </summary>
	/// <remarks>
	/// <p class="body">One or more instances of this class are returned in an IList to represent the history of value changes for a particular <see cref="Cell"/>.</p>
	/// </remarks>
	/// <seealso cref="DataPresenterBase.DataValueChanged"/>
	/// <seealso cref="DataValueChangedEventArgs"/>
	/// <seealso cref="DataRecord.GetDataValueHistory(string)"/>
	/// <seealso cref="DataRecord.GetDataValueHistory(Field)"/>
	/// <seealso cref="CellValuePresenter.ValueHistory"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
	public class DataValueInfo : PropertyChangeNotifier
	{
		#region Member Variables

		private object						_value;
		private DateTime					_timeStamp;
		private object						_tag;

		// JJD 3/11/11 - TFS67970 - Optimization
		// In order to prevent fragmentation of the heap we want to cache info objects for later re-use
		[ThreadStatic()]
		private static List<DataValueInfo> _InfoCache;			

		#endregion //Member Variables

		#region Constructors

		static DataValueInfo()
		{
			// JJD 3/11/11 - TFS67970 - Optimization
			// In order to prevent fragmentation of the heap we want to cache info objects for later re-use
			// allocate the cache list in the static ctro since the member is thread static
			_InfoCache = new List<DataValueInfo>(100);
		}

		// JJD 3/11/11 - TFS67970 - Optimization
		// Make ctor private to force the use of the Create static method
		//internal DataValueInfo(object value, DateTime timeStamp, object tag)
		private DataValueInfo(object value, DateTime timeStamp, object tag)
		{
			this._value		= value;
			this._timeStamp	= timeStamp;
			this._tag		= tag;
		}

		#endregion //Constructors

		#region Properties

			#region Tag

			/// <summary>
			/// Returns/sets an arbitrary object value that can be used to store custom information about this DataValueInfo.
			/// </summary>
			public object Tag
			{ 
				get 
				{ 
					return this._tag; 
				}
				set
				{
					this._tag = value;
					this.RaisePropertyChangedEvent("Tag");
				}
			}

			#endregion //Tag

			#region TimeStamp

			/// <summary>
			/// Returns the date/time when the associated data value changed. (read-only)
			/// </summary>
			public DateTime TimeStamp
			{ 
				get 
				{ 
					return this._timeStamp; 
				}
			}

			#endregion //TimeStamp

			#region Value

			/// <summary>
			/// Returns the value captured when the associated data value changed. (read-only)
			/// </summary>
			public object Value
			{ 
				get 
				{ 
					return this._value; 
				} 
			}

			#endregion //Value

		#endregion //Properties

		#region Methods
			
			#region Static Methods

			// JJD 3/11/11 - TFS67970 - Optimization
			#region Create

			internal static DataValueInfo Create(object value, DateTime timeStamp, object tag)
			{
				DataValueInfo info;

				int index = _InfoCache.Count - 1;

				// pop one off the end of the list
				if (index >= 0)
				{
					info = _InfoCache[index];
					_InfoCache.RemoveAt(index);

					info._tag = tag;
					info._timeStamp = timeStamp;
					info._value = value;
				}
				else
				{
					info = new DataValueInfo(value, timeStamp, tag);
				}
				return info;
			}

			#endregion //Create	
    
			// JJD 3/11/11 - TFS67970 - Optimization
			#region Release

			internal static void Release(DataValueInfo info)
			{
				// cache up to a certain anumber of these objects
				if (info == null || _InfoCache.Count > 300)
					return;

				info._value = null;
				info._tag = null;
				_InfoCache.Add(info);
			}

			#endregion //Release	
    
			#endregion //Static Methods	
    
			#region SetTimeStamp

			internal void SetTimeStamp(DateTime newTimeStamp)
			{
				this._timeStamp = newTimeStamp;
				this.RaisePropertyChangedEvent("TimeStamp");
			}

			#endregion //SetTimeStamp	
    
			#region SetValue

			internal void SetValue(object newValue)
			{
				this._value = newValue;
				this.RaisePropertyChangedEvent("Value");
			}

			#endregion //SetValue	
    
		#endregion //Methods
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