using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.ComponentModel;


using Infragistics.Windows.Controls;


namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Displays a UI for choosing a TimeZone Id.
	/// </summary>
	// AS 2/28/11
	// Removed the attributes. We'll still look for the element we needed but the buttons need not be there.
	// Actually we never used/looked for the cancel button.
	//
	//[TemplatePart(Name = PartOkButton, Type = typeof(Button))]
	//[TemplatePart(Name = PartCancelButton, Type = typeof(Button))]
	[TemplatePart(Name = PartSelector, Type = typeof(Selector))]
	[DesignTimeVisible(false)]
	public class TimeZoneChooserDialog : ScheduleDialogBase<TimeZoneToken>
	{
		#region Member Variables

		// Template part names
		private const string PartOkButton = "OkButton";
		// this wasn't being used
		//private const string PartCancelButton = "CancelButton";
		private const string PartSelector = "Selector";

		private TimeZoneInfoProvider _provider;
		private Button _btnOk;
		private Selector _selector;
		private XamScheduleDataManager _dataManager;

		#endregion //Member Variables

		#region Constructor

		static TimeZoneChooserDialog()
		{

			TimeZoneChooserDialog.DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeZoneChooserDialog), new FrameworkPropertyMetadata(typeof(TimeZoneChooserDialog)));

		}

		// AS 6/14/12 TFS113929
		/// <summary>
		/// Constructor used at design time to style the control.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public TimeZoneChooserDialog()
			: base()
		{



			_provider = TimeZoneInfoProvider.DefaultProvider;
		}

		/// <summary>
		/// Creates an instance of the TimeZoneChooserDialog which lets the user choose a TimeZone id.
		/// </summary>
		/// <param name="dataManager">The XamScheduleDataManager for which the dialog is being displayed.</param>
		/// <param name="tzProvider">The time zone info provcider</param>
		/// <param name="chooserResult">A reference to a <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult"/> instance. The dialog will set 
		/// the <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult.Choice"/> property when the dialog closes to reflect the user's choice.</param>
		public TimeZoneChooserDialog(XamScheduleDataManager dataManager, TimeZoneInfoProvider tzProvider, ChooserResult chooserResult) : base(chooserResult)
		{



			CoreUtilities.ValidateNotNull(tzProvider, "tzProvider");
			CoreUtilities.ValidateNotNull(dataManager, "dataManager");

			this._dataManager = dataManager;
			this._provider = tzProvider;

		}
		#endregion //Constructor

		#region Base Class Overrides

		#region CanSaveAndClose
		internal override bool CanSaveAndClose
		{
			get { return true; }
		} 
		#endregion //CanSaveAndClose

		#region Initialize
		internal override void Initialize()
		{
			if (this._btnOk != null)
				this._btnOk.Click -= new RoutedEventHandler(_btnOk_Click);

			this._btnOk = this.GetTemplateChild(PartOkButton) as Button;

			if (this._btnOk != null)
			{
				if (Commanding.GetCommand(_btnOk) == null)
					this._btnOk.Click += new RoutedEventHandler(_btnOk_Click);
			}

			if (null != _selector)
				_selector.SelectionChanged -= new SelectionChangedEventHandler(_selector_SelectionChanged);

			this._selector = this.GetTemplateChild(PartSelector) as Selector;

			if (this._selector != null)
			{
				this._selector.SelectionChanged += new SelectionChangedEventHandler(_selector_SelectionChanged);
				TimeZoneToken token = _provider != null ? _provider.LocalToken : null;

				if (token != null)
					_selector.SelectedItem = token;
				else
				{
					if (_provider.TimeZoneTokens.Count > 0)
					{
						TimeSpan baseOffset = TimeZoneInfo.Local.BaseUtcOffset;

						foreach (TimeZoneToken tzToken in _provider.TimeZoneTokens)
						{
							if (_provider.GetBaseUtcOffset(tzToken) == baseOffset)
							{
								_selector.SelectedItem = tzToken;
								break;
							}
						}

						if (_selector.SelectedItem == null)
							_selector.SelectedItem = _provider.TimeZoneTokens[0];
					}
				}

				this._selector.Focus();
			}

			base.Initialize(); // AS 4/11/11 TFS71618
		}

		#endregion //Initialize

		#region InitializeLocalizedStrings
		internal override void InitializeLocalizedStrings(Dictionary<string, string> localizedStrings)
		{
			base.InitializeLocalizedStrings(localizedStrings);

			localizedStrings.Add("DLG_TimeZoneChooser_Literal_Message", ScheduleUtilities.GetString("DLG_TimeZoneChooser_Literal_Message"));
			localizedStrings.Add("DLG_TimeZoneChooser_Literal_Selector", ScheduleUtilities.GetString("DLG_TimeZoneChooser_Literal_Selector"));
			localizedStrings.Add("DLG_ScheduleDialog_Btn_Ok", ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Ok"));
			// note we're keeping the previously existing key names in case someone copied the 10.3 templates
			localizedStrings.Add("DLG_TimeZoneChooser_OK", ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Ok"));
		} 
		#endregion //InitializeLocalizedStrings

		#region Save
		internal override void Save()
		{
			// nothing to do here as we set it in the selection change
		} 
		#endregion //Save

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

		#region DataManager
		/// <summary>
		/// Returns the <see cref="XamScheduleDataManager"/> associated with the dialog.
		/// </summary>
		public XamScheduleDataManager DataManager
		{
			get { return this._dataManager; }
		}
		#endregion //DataManager

		#region SelectedTimeZoneToken

		/// <summary>
		/// Identifies the <see cref="SelectedTimeZoneToken"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedTimeZoneTokenProperty = DependencyPropertyUtilities.Register("SelectedTimeZoneToken",
			typeof(TimeZoneToken), typeof(TimeZoneChooserDialog),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnSelectedTimeZoneTokenChanged))
			);

		private static void OnSelectedTimeZoneTokenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeZoneChooserDialog instance = (TimeZoneChooserDialog)d;

		}

		/// <summary>
		/// Returns or sets the selected time zone token
		/// </summary>
		/// <seealso cref="SelectedTimeZoneTokenProperty"/>
		public TimeZoneToken SelectedTimeZoneToken
		{
			get
			{
				return (TimeZoneToken)this.GetValue(TimeZoneChooserDialog.SelectedTimeZoneTokenProperty);
			}
			set
			{
				this.SetValue(TimeZoneChooserDialog.SelectedTimeZoneTokenProperty, value);
			}
		}

		#endregion //SelectedTimeZoneToken

		#region TimeZoneTokens
		/// <summary>
		/// Returns a read only collectio of time zone tokens.
		/// </summary>
		public ReadOnlyObservableCollection<TimeZoneToken> TimeZoneTokens
		{
			get
			{
				return this._provider.TimeZoneTokens;
			}
		}
		#endregion //TimeZoneTokens
		
		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#endregion //Methods

		#region Event Handlers

		#region _btnOk_Click
		void _btnOk_Click(object sender, RoutedEventArgs e)
		{
			((IScheduleDialog)this).SaveAndClose();
		}
		#endregion //_btnOk_Click

		#region _selector_SelectionChanged

		private void _selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this._selector != null)
				this.Result.Choice = _selector.SelectedItem as TimeZoneToken;
		}

		#endregion //_selector_SelectionChanged	
    
		#endregion //Event Handlers
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