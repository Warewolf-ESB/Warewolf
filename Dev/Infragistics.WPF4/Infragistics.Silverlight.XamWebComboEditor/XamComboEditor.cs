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


using Infragistics.Windows.Licensing;


namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// Represents a selection control with a drop-down list that can be shown or hidden by clicking the arrow on the control.
    /// </summary>

	
	

	public class XamComboEditor : ComboEditorBase<ComboEditorItem, ComboEditorItemControl>
	{
		#region Member Variables


		private UltraLicense _license;


		#endregion //Member Variables

		#region Constructor

		static XamComboEditor()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamComboEditor), new FrameworkPropertyMetadata(typeof(XamComboEditor)));
		}


        /// <summary>
        /// Initializes a new instance of the <see cref="XamComboEditor"/> class.
        /// </summary>
        public XamComboEditor()
        {
			// JM 05-24-11 Port to WPF.



			// verify and cache the license
			//
			// Wrapped in a try/catch for a FileNotFoundException.
			// When the assembly is loaded dynamically, VS seems 
			// to be trying to reload a copy of Shared even though 
			// one is in memory. This generates a FileNotFoundException
			// when the dll is not in the gac and not in the AppBase
			// for the AppDomain.
			//
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(XamComboEditor), this) as UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }


			// JM July 2011 - XamMultiColumnComboEditor.
			base.FilterModeResolved			= FilterMode.FilterOnPrimaryColumnOnly;

			// JM 02-21-12 TFS101861
			base.AllowDropDownResizingResolved = false;
        }
        #endregion // Constructor

        #region Overrides

        #region GenerateNewObject

        /// <summary>
        /// Creates a new instance of the <see cref="ComboEditorItem"/> object with the specified data. 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override ComboEditorItem GenerateNewObject(object data)
        {
            return new ComboEditorItem(data, this);
        }

        #endregion // GenerateNewObject

        #endregion // Overrides

		#region Properties

		#region AllowFiltering

		/// <summary>
		/// Identifies the <see cref="AllowFiltering"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowFilteringProperty = DependencyProperty.Register("AllowFiltering", typeof(bool), typeof(XamComboEditor), new PropertyMetadata(true, new PropertyChangedCallback(AllowFilteringChanged)));

		/// <summary>
		/// Gets or sets a value indicating whether the editor will filter the items when it is in edit mode.
		/// </summary>               
		public bool AllowFiltering
		{
			get { return (bool)this.GetValue(AllowFilteringProperty); }
			set { this.SetValue(AllowFilteringProperty, value); }
		}

		private static void AllowFilteringChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamComboEditor combo = obj as XamComboEditor;
			combo.AllowFilteringResolved = (bool)e.NewValue;
		}

		#endregion // AllowFiltering

		#region AutoComplete

		/// <summary>
		/// Identifies the <see cref="AutoComplete"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AutoCompleteProperty = DependencyProperty.Register("AutoComplete", typeof(bool), typeof(XamComboEditor), new PropertyMetadata(false, new PropertyChangedCallback(AutoCompleteChanged)));

		/// <summary>
		/// Gets/Sets whether or not the combo should look through the data source while the user is typing, and hilight the rest of the text
		/// that matches with what they are currently typing. 
		/// </summary>               
		public bool AutoComplete
		{
			get { return (bool)this.GetValue(AutoCompleteProperty); }
			set { this.SetValue(AutoCompleteProperty, value); }
		}

		private static void AutoCompleteChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamComboEditor combo = obj as XamComboEditor;
			combo.AutoCompleteResolved = (bool)e.NewValue;
		}

		#endregion // AutoComplete

		#region IsEditable

		/// <summary>
		/// Identifies the <see cref="IsEditable"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(XamComboEditor), new PropertyMetadata(true, new PropertyChangedCallback(IsEditableChanged)));

		/// <summary>
		/// Gets/Sets whether the XamComboEditor should allow the end user to be able to type into the combo, for selection.
		/// <para>Note: this should be set to true, for AllowFilter and AutoComplete to work.</para>
		/// </summary>
		public bool IsEditable
		{
			get { return (bool)this.GetValue(IsEditableProperty); }
			set { this.SetValue(IsEditableProperty, value); }
		}

		private static void IsEditableChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamComboEditor combo = obj as XamComboEditor;
			combo.IsEditableResolved = (bool)e.NewValue;
		}

		#endregion // IsEditable

		#region OpenDropDownOnTyping

		/// <summary>
		/// Identifies the <see cref="OpenDropDownOnTyping"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty OpenDropDownOnTypingProperty = DependencyProperty.Register("OpenDropDownOnTyping", typeof(bool), typeof(XamComboEditor), new PropertyMetadata(true, new PropertyChangedCallback(OpenDropDownOnTypingChanged)));

		// JM 10-18-11 TFS92458 Added.
		private static void OpenDropDownOnTypingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			XamComboEditor combo = obj as XamComboEditor;
			combo.OpenDropDownOnTypingResolved = (bool)e.NewValue;
		}

		/// <summary>
		/// Gets/Sets whether the DropDown will open when the user starts typing.
		/// </summary>               
		public bool OpenDropDownOnTyping
		{
			get { return (bool)this.GetValue(OpenDropDownOnTypingProperty); }
			set { this.SetValue(OpenDropDownOnTypingProperty, value); }
		}

		#endregion // OpenDropDownOnTyping

		#endregion //Properties

		#region Events

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region DropDownClosing
		/// <summary>
		/// Occurs when the IsDropDownOpen property is changing from true to false. 
		/// </summary>
		public new event EventHandler<CancelEventArgs> DropDownClosing
		{
			add { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DropDownClosing += value; }
			remove { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DropDownClosing -= value; }
		}
		#endregion //DropDownClosing

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region DropDownClosed
		/// <summary>
		/// Occurs when the IsDropDownOpen property was changed from true to false and the drop-down is closed.
		/// </summary>
		public new event EventHandler DropDownClosed
		{
			add { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DropDownClosed += value; }
			remove { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DropDownClosed -= value; }
		}
		#endregion //DropDownClosed

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region ItemAdding
		/// <summary>
		/// Occurs when an item is going to be added to the underlying ComboEditorItemCollection of the ComboEditorBase
		/// </summary>
		public new event EventHandler<ComboItemAddingEventArgs<ComboEditorItem>> ItemAdding
		{
			add { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).ItemAdding += value; }
			remove { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).ItemAdding -= value; }
		}
		#endregion //ItemAdding

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region ItemAdded
		/// <summary>
		/// Occurs when an item is added to the underlying ComboEditorItemCollection of the ComboEditorBase
		/// </summary>
		public new event EventHandler<ComboItemAddedEventArgs<ComboEditorItem>> ItemAdded
		{
			add { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).ItemAdded += value; }
			remove { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).ItemAdded -= value; }
		}
		#endregion //ItemAdded

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region DropDownOpening
		/// <summary>
		/// Occurs when the value of the IsDropDownOpen property is changing from false to true. 
		/// </summary>
		public new event EventHandler<CancelEventArgs> DropDownOpening
		{
			add { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DropDownOpening += value; }
			remove { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DropDownOpening -= value; }
		}
		#endregion //DropDownOpening

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region DropDownOpened
		/// <summary>
		/// Occurs when the value of the IsDropDownOpen property has changed from false to true and the drop-down is open.
		/// </summary>
		public new event EventHandler DropDownOpened
		{
			// JM 11-30-11 TFS96911 - Fix incorrect event name
			//add { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DropDownOpening += value; }
			//remove { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DropDownOpening -= value; }
			add { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DropDownOpened += value; }
			remove { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DropDownOpened -= value; }
		}
		#endregion //DropDownOpened

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region SelectionChanged
		/// <summary>
		/// Occurs when the selection of the ComboEditorBase changes.
		/// </summary>
		public new event EventHandler SelectionChanged
		{
			add { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).SelectionChanged += value; }
			remove { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).SelectionChanged -= value; }
		}
		#endregion //SelectionChanged

		//JM 11-22-11 TFS96503, TFS96504 - Added.
		#region DataObjectRequested
		/// <summary>
		/// This event is raised, when the ComboEditorBase needs to create a new data object.
		/// </summary>
		public new event EventHandler<HandleableObjectGenerationEventArgs> DataObjectRequested
		{
			add { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DataObjectRequested += value; }
			remove { ((ComboEditorBase<ComboEditorItem, ComboEditorItemControl>)this).DataObjectRequested -= value; }
		}
		#endregion //DataObjectRequested

		#endregion //Events
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