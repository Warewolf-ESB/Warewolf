using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// Base class for controls that will be used for paging.  
	/// </summary>
	public abstract class PagerControlBase : ContentControl
	{
		#region Members
		private int _currentPageIndex;
		#endregion

		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="PagerControlBase"/> object.
		/// </summary>
		protected PagerControlBase()
		{
			this.DataContext = this;
		}
		#endregion // Constructor

		#region Properties

		#region Public
		#region CurrentPageIndex
		/// <summary>
		/// Gets / sets which page of data is currently visible.
		/// </summary>
		public int CurrentPageIndex
		{
			get
			{
				return this._currentPageIndex;
			}
			set
			{
				this._currentPageIndex = value;
				EnsureContent();
			}
		}
		#endregion // CurrentPageIndex

		#region TotalPages

		/// <summary>
		/// Identifies the <see cref="TotalPages"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty TotalPagesProperty = DependencyProperty.Register("TotalPages", typeof(int), typeof(PagerControlBase), new PropertyMetadata(new PropertyChangedCallback(TotalPagesPropertyChanged)));

		/// <summary>
		/// Gets the count of pages that this <see cref="PagerControlBase"/> represents.
		/// </summary>
		public int TotalPages
		{
			get { return (int)this.GetValue(TotalPagesProperty); }
			protected internal set { this.SetValue(TotalPagesProperty, value); }
		}

		private static void TotalPagesPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			PagerControl pc = (PagerControl)obj;
			pc.EnsureContent();
		}

		#endregion // TotalPages

		#endregion // Public

		#endregion // Properties

		#region Methods

		#region Protected

		#region SetCurrentPageIndexSilent
		/// <summary>
		/// Sets the CurrentPageIndex without doing all the EnsureContent calls.
		/// </summary>
		/// <param propertyName="value"></param>
		protected internal void SetCurrentPageIndexSilent(int value)
		{
			this._currentPageIndex = value;
		}
		#endregion

		#region SetRowsManagerMaximumPagesSilent
		/// <summary>
		/// Sets the RowsManagerMaximumPages without doiong the EnsureContent call.
		/// </summary>
		/// <param propertyName="value"></param>
		protected internal void SetRowsManagerMaximumPagesSilent(int value)
		{
			this.TotalPages = value;
		}
		#endregion

		#region EnsureContent
		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>
		protected internal virtual void EnsureContent()
		{
		}
		#endregion // EnsureContent

		#region CheckButtonState
		/// <summary>
		/// Calls to the CommandSourceManager to update the UI so that the controls that are going to be doing the commanding are updated appropriately.
		/// </summary>
		protected virtual void CheckButtonState()
		{
			CommandSourceManager.NotifyCanExecuteChanged(typeof(FirstPageControlCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(NextPageControlCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(PreviousPageControlCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(LastPageControlCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(GoToPageControlCommand));
		}
		#endregion

		#region InvalidateItems
		/// <summary>
		/// Recreates the Items list <see cref="PagerControl"/>
		/// </summary>
		protected internal virtual void InvalidateItems()
		{

		}
		#endregion // InvalidateItems

		#endregion

		#endregion

		#region Events

		#region FirstPage
		/// <summary>
		/// Event raised when the control wants to naviage to the first page.
		/// </summary>
		public event EventHandler FirstPage;

		/// <summary>
		/// Raises the FirstPage event.
		/// </summary>
		protected internal virtual void OnFirstPage()
		{
			if (this.FirstPage != null)
				this.FirstPage(this, EventArgs.Empty);
		}
		#endregion // FirstPage

		#region LastPage
		/// <summary>
		/// Event raised when the control wants to naviage to the last page.
		/// </summary>
		public event EventHandler LastPage;

		/// <summary>
		/// Raises the LastPage event.
		/// </summary>
		protected internal virtual void OnLastPage()
		{
			if (this.LastPage != null)
				this.LastPage(this, EventArgs.Empty);
		}
		#endregion // LastPage

		#region PreviousPage
		/// <summary>
		/// Event raised when the control wants to naviage to the previous page.
		/// </summary>
		public event EventHandler PreviousPage;

		/// <summary>
		/// Raises the PreviousPage event
		/// </summary>
		protected internal virtual void OnPreviousPage()
		{
			if (this.PreviousPage != null)
				this.PreviousPage(this, EventArgs.Empty);
		}
		#endregion // PreviousPage

		#region NextPage
		/// <summary>
		/// Event raised when the control wants to naviage to the next page.
		/// </summary>
		public event EventHandler NextPage;

		/// <summary>
		/// Raises the NextPage event.
		/// </summary>
		protected internal virtual void OnNextPage()
		{
			if (this.NextPage != null)
				this.NextPage(this, EventArgs.Empty);
		}
		#endregion // NextPage

		#region GoToPage
		/// <summary>
		/// Event raised when the control wants to naviage to a particular page.
		/// </summary>
		public event EventHandler GoToPage;

		/// <summary>
		/// Raises the GoToPage event.
		/// </summary>
		/// <param propertyName="nextPage">The 1 based index of the page to be navigated to.</param>
		protected internal virtual void OnGoToPage(int nextPage)
		{
			if (this.GoToPage != null)
				this.GoToPage(this, new PagingEventArgs() { NextPage = nextPage });
		}
		#endregion //

		#endregion // Events
	}

	/// <summary>
	/// A <see cref="PagerControlBase"/> implementation that has control buttons for paging along with a <see cref="ListBox"/> showing a window of available pages that can be naviagated to.
	/// </summary>
	[TemplatePart(Name = "PagerItems", Type = typeof(Selector))]
	public class PagerControl : PagerControlBase, ICommandTarget
	{
		#region Members
		private Selector _selectorControl;

		private int _originalPageCount = -1;
		private int _originalPageIndex = -1;

		#endregion // Members

		#region Constructor


        /// <summary>
        /// Static constructor for the <see cref="PagerControl"/> class.
        /// </summary>
        static PagerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PagerControl), new FrameworkPropertyMetadata(typeof(PagerControl)));
        }


		/// <summary>
		/// Initializes a new instance of the <see cref="PagerControl"/> class.
		/// </summary>
		public PagerControl()
		{



		}
		#endregion // Constructor

		#region Properties

		#region MaximumAvailablePagerValues

		/// <summary>
		/// Identifies the <see cref="MaximumAvailablePagerValues"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty MaximumAvailablePagerValuesProperty = DependencyProperty.Register("MaximumAvailablePagerValues", typeof(int), typeof(PagerControl), new PropertyMetadata(new PropertyChangedCallback(MaximumAvailablePagerValuesChanged)));

		/// <summary>
		/// Gets / sets the maxiumum number of page indicators that will be visible in the Selector.
		/// </summary>
		public int MaximumAvailablePagerValues
		{
			get { return (int)this.GetValue(MaximumAvailablePagerValuesProperty); }
			set { this.SetValue(MaximumAvailablePagerValuesProperty, value); }
		}

		private static void MaximumAvailablePagerValuesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			PagerControl pc = (PagerControl)obj;
			pc.EnsureContent();
		}

		#endregion // MaximumAvailablePagerValues

		#endregion // Properties

		#region Methods

		#region Protected

		#region AddItemsToList
		/// <summary>
		/// Sets the ItemSource of the ListBox to a list of values from the starting index to the ending index.
		/// </summary>
		/// <param propertyName="startPage">The first value that should appear in the ListBox.</param>
		/// <param propertyName="endPage">The last value that should appear in the ListBox.</param>
		protected internal void AddItemsToList(int startPage, int endPage)
		{
			List<int> numbers = new List<int>();

			for (int i = startPage; i <= endPage; i++)
			{
				numbers.Add(i);
			}
			this._selectorControl.ItemsSource = numbers;
		}
		#endregion // AddItemsToList

		#region InvalidateItems
		/// <summary>
		/// Recreates the Items list for the Selector control
		/// </summary>
		protected internal override void InvalidateItems()
		{
			int pageCount = this.TotalPages;
			this._selectorControl.ItemsSource = null;
			if (this.MaximumAvailablePagerValues < 0)
			{
				this.AddItemsToList(1, this.TotalPages);
			}
			// if the page count is less then the maximum available pages, just render them
			else if (pageCount < this.MaximumAvailablePagerValues)
			{
				this.AddItemsToList(1, pageCount);
			}
			else
			{
				int midPoint = this.MaximumAvailablePagerValues / 2;
				int newStartingPage = this.CurrentPageIndex - midPoint;
				if (newStartingPage <= 1)
				{
					this.AddItemsToList(1, this.MaximumAvailablePagerValues);
				}
				else if (newStartingPage + this.MaximumAvailablePagerValues >= this.TotalPages)
				{
					this.AddItemsToList(this.TotalPages - this.MaximumAvailablePagerValues + 1, this.TotalPages);
				}
				else
				{
					int endPage = newStartingPage + this.MaximumAvailablePagerValues - 1;

					if (endPage > this.TotalPages)
						endPage = this.TotalPages;

					this.AddItemsToList(newStartingPage, endPage);
				}
			}

			this._originalPageCount = pageCount;
			this._originalPageIndex = this.CurrentPageIndex;

			if (this._selectorControl.SelectedItem == null || (int)this._selectorControl.SelectedItem != this.CurrentPageIndex)
				this._selectorControl.SelectedItem = this.CurrentPageIndex;

			CheckButtonState();
		}
		#endregion // InvalidateItems

		#region EnsureContent
		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>
		protected internal override void EnsureContent()
		{
			if (this._selectorControl != null)
			{
				int pageCount = this.TotalPages;
				if (this._originalPageCount != pageCount || this._originalPageIndex != this.CurrentPageIndex)
				{
					InvalidateItems();
				}
			}
		}
		#endregion // EnsureContent

		#region SupportsCommand

		/// <summary>
		/// Returns if the object will support a given command type.
		/// </summary>
		/// <param propertyName="command">The command to be validated.</param>
		/// <returns>True if the object recognizes the command as actionable against it.</returns>
		protected virtual bool SupportsCommand(ICommand command)
		{
			return command is PagingControlCommandBase;
		}
		#endregion // SupportsCommand

		#region GetParameter
		/// <summary>
		/// Returns the object that defines the parameters necessary to execute the command.
		/// </summary>
		/// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
		/// <returns>The object necessary for the command to complete.</returns>
		protected virtual object GetParameter(CommandSource source)
		{
			SelectionChangedEventArgs args = source.OriginEventArgs as SelectionChangedEventArgs;
			if (args != null)
			{
				if (args.RemovedItems.Count > 0 && args.AddedItems.Count > 0)
				{
					return new object[] { this, (int)args.AddedItems[0] };
				}
			}
			return this;
		}
		#endregion // GetParameter

		#endregion // Protected

		#region Public

		#region OnApplyTemplate
		/// <summary>
		/// Builds the visual tree for the <see cref="PagerControl"/> when a new template is applied. 
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this._selectorControl = base.GetTemplateChild("PagerItems") as Selector;

			if (this._selectorControl != null)
				this.EnsureContent();
		}
		#endregion // OnApplyTemplate

		#endregion // Public

		#endregion // Methods

		#region ICommandTarget Members

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return this.SupportsCommand(command);
		}

		object ICommandTarget.GetParameter(CommandSource source)
		{
			return this.GetParameter(source);
		}

		#endregion
	}

    /// <summary>
    /// A control used in <see cref="PagerControl"/> for showing of the available pages.
    /// </summary>
    public class PagerItemsControl : ListBox
    {
        #region Overrides

        #region GetContainerForItemOverride

        /// <summary>
        /// Creates or identifies the element used to display a specified item.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Windows.Controls.ListBoxItem"/>.
        /// </returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new PagerItem();
        }

        #endregion // GetContainerForItemOverride

        #region OnMouseMove


        /// <summary>
        /// Called when a <see cref="T:System.Windows.Controls.ListBox"/> reports a mouse move.
        /// </summary>
        /// <param name="e">Provides data for <see cref="T:System.Windows.Input.MouseEventArgs"/>.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Overridden in order to prevent autoscroll and drag selection.
            if ((e.OriginalSource == this) && (Mouse.Captured == this))
            {
                base.ReleaseMouseCapture();
            }
        }


        #endregion // OnMouseMove

        #endregion // Overrides
    }

    /// <summary>
    /// Represents a selectable item in a <see cref="PagerItemsControl"/>. 
    /// </summary>
    public class PagerItem : ListBoxItem
    {
        #region Overrides

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            // Overridden in order to prevent drag selection.
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            // Overridden in order to prevent drag selection.
        }

        #endregion // Overrides
    }

	/// <summary>
	/// Event argument used for the PagingLocation commanding structure that will carry event information to the
	/// commands.
	/// </summary>
	public class PagingEventArgs : EventArgs
	{
		/// <summary>
		/// Gets / sets the index of the next page of data to be displayed.
		/// </summary>
		public int NextPage { get; set; }
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