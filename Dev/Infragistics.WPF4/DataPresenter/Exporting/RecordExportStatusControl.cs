using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Automation.Peers.DataPresenter;

namespace Infragistics.Windows.DataPresenter
{
	// AS 3/3/11 NA 2011.1 - Async Exporting
	/// <summary>
	/// Custom element used to provide status information about a <see cref="DataPresenterBase"/> asynchronous export that is in progress.
	/// </summary>
	[DesignTimeVisible(false)]	// JJD 4/11/11 - TFS72200 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	[InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_WordWriter)]
	public class RecordExportStatusControl : Control
	{
		#region Constructor
		static RecordExportStatusControl()
		{
			Control.DefaultStyleKeyProperty.OverrideMetadata(typeof(RecordExportStatusControl), new FrameworkPropertyMetadata(typeof(RecordExportStatusControl)));
		}

		/// <summary>
		/// Initializes a new <see cref="RecordExportStatusControl"/>
		/// </summary>
		public RecordExportStatusControl()
		{
		} 
		#endregion //Constructor

		#region Base class overrides

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="RecordExportStatusControl"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="RecordExportStatusControlAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new RecordExportStatusControlAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#endregion //Base class overrides

		#region Properties

		#region CancelExportButtonStyleKey

		/// <summary>
		/// The key that identifies the style for a button within the template of this control used to cancel the current export operation.
		/// </summary>
		public static readonly ResourceKey CancelExportButtonStyleKey = new StaticPropertyResourceKey(typeof(RecordExportStatusControl), "CancelExportButtonStyleKey");

		#endregion CancelExportButtonStyleKey

		#region DataPresenter

		/// <summary>
		/// Identifies the <see cref="DataPresenter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataPresenterProperty = DependencyProperty.Register("DataPresenter",
			typeof(DataPresenterBase), typeof(RecordExportStatusControl), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnDataPresenterChanged)));

		private static void OnDataPresenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RecordExportStatusControl ctrl = d as RecordExportStatusControl;
			DataPresenterBase oldDp = e.OldValue as DataPresenterBase;
			DataPresenterBase newDp = e.NewValue as DataPresenterBase;

			if (null != oldDp)
				oldDp.ExportHelper.RemoveStatusControl(ctrl);

			if (null != newDp)
				newDp.ExportHelper.AddStatusControl(ctrl);
		}

		/// <summary>
		/// Returns or sets the DataPresenter whose export status is being displayed.
		/// </summary>
		/// <seealso cref="DataPresenterProperty"/>
		[Bindable(true)]
		public DataPresenterBase DataPresenter
		{
			get
			{
				return (DataPresenterBase)this.GetValue(RecordExportStatusControl.DataPresenterProperty);
			}
			set
			{
				this.SetValue(RecordExportStatusControl.DataPresenterProperty, value);
			}
		}

		#endregion //DataPresenter

		#region ExportInfo

		private static readonly DependencyPropertyKey ExportInfoPropertyKey =
			DependencyProperty.RegisterReadOnly("ExportInfo",
			typeof(ExportInfo), typeof(RecordExportStatusControl), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="ExportInfo"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExportInfoProperty =
			ExportInfoPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns additional information about the current export process
		/// </summary>
		/// <seealso cref="ExportInfoProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public ExportInfo ExportInfo
		{
			get
			{
				return (ExportInfo)this.GetValue(RecordExportStatusControl.ExportInfoProperty);
			}
			internal set
			{
				this.SetValue(RecordExportStatusControl.ExportInfoPropertyKey, value);
			}
		}

		#endregion //ExportInfo

		#region ExportedRecordCount

		private static readonly DependencyPropertyKey ExportedRecordCountPropertyKey =
			DependencyProperty.RegisterReadOnly("ExportedRecordCount",
			typeof(int), typeof(RecordExportStatusControl), new FrameworkPropertyMetadata(0));

		/// <summary>
		/// Identifies the <see cref="ExportedRecordCount"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ExportedRecordCountProperty =
			ExportedRecordCountPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the number of records that have been exported
		/// </summary>
		/// <seealso cref="ExportedRecordCountProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public int ExportedRecordCount
		{
			get
			{
				return (int)this.GetValue(RecordExportStatusControl.ExportedRecordCountProperty);
			}
			internal set
			{
				this.SetValue(RecordExportStatusControl.ExportedRecordCountPropertyKey, value);
			}
		}

		#endregion //ExportedRecordCount

		#region IndeterminateProgressBarStyleKey

		// SSP 4/21/09 NAS9.2 IDataErrorInfo Support
		// 
		/// <summary>
		/// The key that identifies the style for an indeterminate <see cref="ProgressBar"/> used in the template of this control.
		/// </summary>
		public static readonly ResourceKey IndeterminateProgressBarStyleKey = new StaticPropertyResourceKey(typeof(RecordExportStatusControl), "IndeterminateProgressBarStyleKey");

		#endregion IndeterminateProgressBarStyleKey

		#region Status

		private static readonly DependencyPropertyKey StatusPropertyKey =
			DependencyProperty.RegisterReadOnly("Status",
			typeof(RecordExportStatus), typeof(RecordExportStatusControl), new FrameworkPropertyMetadata(RecordExportStatus.NotExporting));

		/// <summary>
		/// Identifies the <see cref="Status"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StatusProperty =
			StatusPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the current <see cref="DataPresenterBase.ExportStatus"/> of the associated <see cref="DataPresenterBase"/>
		/// </summary>
		/// <seealso cref="StatusProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public RecordExportStatus Status
		{
			get
			{
				return (RecordExportStatus)this.GetValue(RecordExportStatusControl.StatusProperty);
			}
			internal set
			{
				this.SetValue(RecordExportStatusControl.StatusPropertyKey, value);
			}
		}

		#endregion //Status

		#endregion //Properties
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