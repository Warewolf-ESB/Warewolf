using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.DataPresenter
{
	#region RecordScrollTip
	/// <summary>
	/// ToolTip class used to display information about the top record when <see cref="DataPresenterBase.ScrollingMode"/> is set to <b>DeferredWithScrollTips</b>.
	/// </summary>
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class RecordScrollTip : ToolTip
	{
		#region Member Variables

		private Size _lastSize;

		#endregion //Member Variables

		#region Constructor

		static RecordScrollTip()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RecordScrollTip), new FrameworkPropertyMetadata(typeof(RecordScrollTip)));
		}

		/// <summary>
		/// Initializes a new <see cref="RecordScrollTip"/>
		/// </summary>
		public RecordScrollTip()
		{
            // JJD 4/30/09 - TFS17157 
            // Set inherited attached IsInRecordScrollTip property
            this.SetValue(IsInRecordScrollTipPropertyKey, KnownBoxes.TrueBox);
		}
		#endregion //Constructor

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size newSize = base.MeasureOverride(availableSize);

			if (newSize.Width < this._lastSize.Width)
				newSize.Width = this._lastSize.Width;

			if (newSize.Height < this._lastSize.Height)
				newSize.Height = this._lastSize.Height;

			this._lastSize = newSize;

			return newSize;
		}
		#endregion //MeasureOverride

        #region Properties

            #region Public Properties

                // JJD 4/30/09 - TFS17157 - added
                #region IsInRecordScrollTip

        private static readonly DependencyPropertyKey IsInRecordScrollTipPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsInRecordScrollTip",
            typeof(bool), typeof(RecordScrollTip), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Identifies the IsInRecordScrollTip" attached inherited readonly dependency property
        /// </summary>
        /// <seealso cref="GetIsInRecordScrollTip"/>
        public static readonly DependencyProperty IsInRecordScrollTipProperty =
            IsInRecordScrollTipPropertyKey.DependencyProperty;


        /// <summary>
        /// Gets the value of the 'IsInRecordScrollTip' attached inherited readonly property
        /// </summary>
        /// <seealso cref="IsInRecordScrollTipProperty"/>
        public static bool GetIsInRecordScrollTip(DependencyObject d)
        {
            return (bool)d.GetValue(RecordScrollTip.IsInRecordScrollTipProperty);
        }

                #endregion //IsInRecordScrollTip

            #endregion //Public Properties

        #endregion //Properties

    }
	#endregion //RecordScrollTip

	#region RecordScrollTipInfo
	/// <summary>
	/// Represents the hierarchy of information 
	/// </summary>
	public class RecordScrollTipInfo
	{
		#region Member Variables

		private ReadOnlyCollection<RecordScrollTipInfo> _children;
		// JM 06-04-09 TFS 14198
		//private Record _record;
		private WeakReference _record;

		#endregion //Member Variables

		#region Constructor
		private RecordScrollTipInfo(Record record, ReadOnlyCollection<RecordScrollTipInfo> children)
		{
			if (record == null)
				throw new ArgumentNullException("record");

			this._children = children;

			// JM 06-04-09 TFS 14198
			//this._record = record;
			this._record = new WeakReference(record);
		}
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the record associated with this <see cref="RecordScrollTipInfo"/> instance.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		public Record Record
		{
			// JM 06-04-09 TFS 14198
			//get { return this._record; }
			get { return Utilities.GetWeakReferenceTargetSafe(this._record) as Record; }
		}

		/// <summary>
		/// Returns the scroll tip info for the descendants of the record for which the scroll tip is being displayed.
		/// </summary>
		public ReadOnlyCollection<RecordScrollTipInfo> Children
		{
			get { return this._children; }
		}

		#endregion //Properties

		#region Methods

		/// <summary>
		/// Creates a ScrollTipInfo based on the supplied top record.  Resolves the top record to the true top
		/// record if necessary (i.e., finds the top record's ultimate parent record).
		/// </summary>
		/// <param name="topRecord"></param>
		/// <returns></returns>
		public static RecordScrollTipInfo Create(Record topRecord)
		{
			RecordScrollTipInfo scrollTip = new RecordScrollTipInfo(topRecord, null);

			while (topRecord.ParentRecord != null)
			{
				topRecord = topRecord.ParentRecord;

				if (topRecord.VisibilityResolved == Visibility.Visible &&
					topRecord.OccupiesScrollPosition)
					scrollTip = new RecordScrollTipInfo(topRecord, new ReadOnlyCollection<RecordScrollTipInfo>(new RecordScrollTipInfo[] { scrollTip }));
			}

			return scrollTip;
		}
		#endregion //Methods
	} 
	#endregion //RecordScrollTipInfo
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