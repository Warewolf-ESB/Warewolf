using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;





using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Abstract base class which exposes the various workbook window options available which can be saved with 
	/// both a workbook and a custom view.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// This class provides a way to control how a workbook is displayed when it is viewed in Microsoft Excel.
	/// </p>
	/// </remarks>
	/// <seealso cref="CustomViewWindowOptions"/>
	/// <seealso cref="WorkbookWindowOptions"/>



	public

		 abstract class WindowOptions
    {
        #region Constants

        //  BF 6/26/08  Office2007 Format
        internal const int defaultTabBarWidth = 600;

        #endregion Constants

        #region Member Variables

        private Workbook workbook;

		private ObjectDisplayStyle objectDisplayStyle = ObjectDisplayStyle.ShowAll;
		private ScrollBars scrollBars = ScrollBars.Both;
		private Worksheet selectedWorksheet;
		private bool tabBarVisible = true;
		
        //  BF 6/26/08  Office2007 Format
        //private int tabBarWidth = 600;
        private int tabBarWidth = WindowOptions.defaultTabBarWidth;

		#endregion Member Variables

		#region Constructor

		internal WindowOptions( Workbook workbook )
		{
			this.workbook = workbook;
		}

		#endregion Constructor

		#region Methods

		#region Abstract Methods

		internal abstract DisplayOptions GetDisplayOptionsForWorksheet( Worksheet worksheet );

		#endregion Abstract Methods

		#region Public Methods

		#region Reset

		/// <summary>
		/// Resets the window options to their default settings.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The defaults used for each setting are the same defaults with which Microsoft Excel creates a blank workbook.
		/// </p>
		/// </remarks>
		public virtual void Reset()
		{
			this.objectDisplayStyle = ObjectDisplayStyle.ShowAll;
			this.scrollBars = ScrollBars.Both;
			this.tabBarVisible = true;
			this.tabBarWidth = 600;

			if ( this.AllowNullSelectedWorksheet )
				this.selectedWorksheet = null;
			else
				this.selectedWorksheet = this.workbook.Worksheets[ 0 ];
		}

		#endregion Reset

		#endregion Public Methods

		#region Internal Methods

		#region InitializeFrom

		internal void InitializeFrom( WindowOptions windowOptions )
		{
			this.objectDisplayStyle = windowOptions.objectDisplayStyle;
			this.scrollBars = windowOptions.scrollBars;
			this.tabBarVisible = windowOptions.tabBarVisible;
			this.tabBarWidth = windowOptions.tabBarWidth;

			// Only copy over the selected worksheet if it is not null.  If the selected worksheet
			// of a custom view null, the current selected worksheet should remain selected.
			if ( windowOptions.selectedWorksheet != null )
				this.selectedWorksheet = windowOptions.selectedWorksheet;
		}

		#endregion InitializeFrom

		#endregion Internal Methods

		#region Private Methods

		// MD 9/27/08
		#region VerifySelectedWorksheet

		private void VerifySelectedWorksheet()
		{
			if ( this.selectedWorksheet == null )
				return;

			// Get the display options which apply to the selected worksheet in this context
			DisplayOptions displayOptions = this.GetDisplayOptionsForWorksheet( this.selectedWorksheet );

			if ( displayOptions.Visibility == WorksheetVisibility.Visible )
				return;

			int selectedIndex = this.selectedWorksheet.Index;

			if ( selectedIndex < 0 )
			{
				Utilities.DebugFail( "The worksheet should exist in the workbook." );

				if ( this.workbook.Worksheets.Count == 0 )
				{
					this.selectedWorksheet = null;
					return;
				}
			}

			// If the selected worksheet is not visible, find the next visible worksheet after this one
			for ( int i = selectedIndex + 1; i < this.workbook.Worksheets.Count; i++ )
			{
				Worksheet testSelectedWorksheet = this.workbook.Worksheets[ i ];
				displayOptions = this.GetDisplayOptionsForWorksheet( testSelectedWorksheet );

				if ( displayOptions.Visibility != WorksheetVisibility.Visible )
					continue;

				this.selectedWorksheet = testSelectedWorksheet;
				return;
			}

			// If there is no visible worksheet after the selected one, find the first visible worksheet 
			// before it
			for ( int i = selectedIndex - 1; i >= 0; i-- )
			{
				Worksheet testSelectedWorksheet = this.workbook.Worksheets[ i ];
				displayOptions = this.GetDisplayOptionsForWorksheet( testSelectedWorksheet );

				if ( displayOptions.Visibility != WorksheetVisibility.Visible )
					continue;

				this.selectedWorksheet = testSelectedWorksheet;
				return;
			}
		}

		#endregion VerifySelectedWorksheet

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region ObjectDisplayStyle

		/// <summary>
		/// Gets or sets the way the objects and shapes are displayed in the workbook.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="ObjectDisplayStyle"/> enumeration.
		/// </exception>
		/// <value>The way the objects and shapes are displayed in the workbook.</value>
		/// <seealso cref="WorksheetShape"/>
		/// <seealso cref="Worksheet.Shapes"/>
		public ObjectDisplayStyle ObjectDisplayStyle
		{
			get { return this.objectDisplayStyle; }
			set
			{
				if ( this.objectDisplayStyle != value )
				{
					if ( Enum.IsDefined( typeof( ObjectDisplayStyle ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( ObjectDisplayStyle ) );

					this.objectDisplayStyle = value;
				}
			}
		}

		#endregion ObjectDisplayStyle

		#region ScrollBars

		/// <summary>
		/// Gets or sets the scroll bars shown in the workbook window.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The vertical scroll bar occupies the entire height of the application if it is visible.
		/// </p>
		/// <p class="body">
		/// The horizontal scroll bar occupies the width of the application not used by the worksheet
		/// tab bar, if it is visible. Otherwise, it occupies the entire width of the application.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="ScrollBars"/> enumeration.
		/// </exception>
		/// <value>The scroll bars shown in the workbook window.</value>
		/// <seealso cref="TabBarWidth"/>
		/// <seealso cref="TabBarVisible"/>
		public ScrollBars ScrollBars
		{
			get { return this.scrollBars; }
			set
			{
				if ( this.scrollBars != value )
				{
					if ( Enum.IsDefined( typeof( ScrollBars ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( ScrollBars ) );

					this.scrollBars = value;
				}
			}
		}

		#endregion ScrollBars

		#region SelectedWorksheet

		/// <summary>
		/// Gets or sets the selected worksheet of the workbook.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this is an instance of <see cref="CustomViewWindowOptions"/> and the SelectedWorksheet value is null, the 
		/// workbook's selected worksheet will not be changed when the associated <see cref="CustomView"/> is applied.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> If the selected worksheet does not have its <see cref="DisplayOptions.Visibility"/> set to Visible
		/// when the workbook is saved, another worksheet will be selected.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// The assigned value is null and this is an instance of <see cref="WorkbookWindowOptions"/> whose associated 
		/// <see cref="Workbook"/> has at least one <see cref="Worksheet"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The assigned value does not belong to the workbook associated with this instance of <see cref="WindowOptions"/>.
		/// </exception>
		/// <value>The selected worksheet of the workbook.</value>
		public Worksheet SelectedWorksheet
		{
			// MD 9/27/08
			// This property will resolve itself lazily when the getter is accessed so it is correct at run-time when accessed publicly
			//get { return this.selectedWorksheet; }
			get 
			{
				this.VerifySelectedWorksheet();
				return this.selectedWorksheet; 
			}
			set
			{
				if ( this.selectedWorksheet != value )
				{
					if ( value == null && this.AllowNullSelectedWorksheet == false )
						throw new ArgumentNullException( "value", SR.GetString( "LE_ArgumentNullException_SelectedWorksheet" ) );

					// MD 9/27/08
					// Refactored to test other conditions
					//if ( value != null && value.Workbook != this.workbook )
					//	throw new ArgumentException( SR.GetString( "LE_ArgumentException_SelectedWorksheetFromOtherWorkbook" ), "value" );
					if ( value != null )
					{
						if ( value.Workbook != this.workbook )
							throw new ArgumentException( SR.GetString( "LE_ArgumentException_SelectedWorksheetFromOtherWorkbook" ), "value" );

						// Get the display options which apply to the new worksheet in this context
						DisplayOptions displayOptions = this.GetDisplayOptionsForWorksheet( value );

						if ( displayOptions.Visibility != WorksheetVisibility.Visible )
							throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_HiddenWorksheetCannotBeSelected" ) );
					}

					

					this.selectedWorksheet = value;
				}
			}
		}

		#endregion SelectedWorksheet

		#region TabBarVisible

		/// <summary>
		/// Gets or sets the value indicating whether the worksheet tab bar is visible.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the value is False, the <see cref="TabBarWidth"/> will not be used, but it will still 
		/// be serialized with the workbook.
		/// </p>
		/// </remarks>
		/// <value>The value indicating whether the worksheet tab bar is visible.</value>
		/// <seealso cref="TabBarWidth"/>
		/// <seealso cref="WorkbookWindowOptions.FirstVisibleTabIndex"/>
		public bool TabBarVisible
		{
			get { return this.tabBarVisible; }
			set { this.tabBarVisible = value; }
		}

		#endregion TabBarVisible

		#region TabBarWidth

		/// <summary>
		/// Gets or sets the width of the worksheet tab bar, expressed in 1/1000ths of the application width.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This value is only used if <see cref="TabBarVisible"/> is True. Regardless of whether the tab bar is 
		/// visible, the width value is always saved with the workbook.
		/// </p>
		/// <p class="body">
		/// A value of 1000 indicates the worksheet tab bar occupies the entire width of the application, while
		/// a value of 0 indicates the worksheet tab bar has no width.
		/// </p>
		/// <p class="body">
		/// All space not occupied by the worksheet tab bar will be used by the horizontal scroll bar, if it is visible.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is outside the valid range of 0 and 1000.
		/// </exception>
		/// <value>The width of the worksheet tab bar, expressed in 1/1000ths of the application width.</value>
		/// <seealso cref="TabBarVisible"/>
		/// <seealso cref="ScrollBars"/>
		/// <seealso cref="WorkbookWindowOptions.FirstVisibleTabIndex"/>
		public int TabBarWidth
		{
			get { return this.tabBarWidth; }
			set
			{
				if ( this.tabBarWidth != value )
				{
					if ( this.tabBarWidth < 0 || 1000 < this.tabBarWidth )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_TabBarWidth" ) );

					this.tabBarWidth = value;
				}
			}
		}

		#endregion TabBarWidth

		#endregion Public Properties

		#region Internal Properties

		#region AllowNullSelectedWorksheet
			





		internal abstract bool AllowNullSelectedWorksheet { get;}
		
		#endregion AllowNullSelectedWorksheet

		// MD 9/27/08
		// This logic has been refactored into the VerifySelectedWorksheet method because the selected worksheet will resolve 
		// itself whenever the getter is accessed so it is correct at run-time when accessed publicly
		#region Not Used

		
#region Infragistics Source Cleanup (Region)





















































#endregion // Infragistics Source Cleanup (Region)


		#endregion Not Used

		#region Workbook

		internal Workbook Workbook
		{
			get { return this.workbook; }
		}

		#endregion Workbook

		#endregion Internal Properties

		#endregion Properties
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