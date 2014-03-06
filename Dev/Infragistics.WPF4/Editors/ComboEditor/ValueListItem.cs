using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Editors.Events;
using Infragistics.Windows.Helpers;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Editors;

namespace Infragistics.Windows.Editors
{
	#region ComboBoxDataItem Class

	/// <summary>
	/// <b>ComboBoxDataItem</b> is a class for holding value, display text and optionally an image.
	/// A default template is defined for ComboBoxDataItem that displays the image and display text.
	/// Main purpose of this object is to let you easily map value and display text and optionally
	/// display an image for each item inside an items control, like XamComboEditor.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// You can populate an items control like XamComboEditor with <b>ComboBoxDataItem</b> instances.
	/// This lets you easily map values to display texts and optionally display an image inside
	/// each item in the items control. All without having to create a custom object or control
	/// template for the item.
	/// </para>
	/// <seealso cref="XamComboEditor"/>
	/// </remarks>
	public class ComboBoxDataItem : INotifyPropertyChanged
	{
		#region Private Vars

		private object _dataValue;
		private string _displayText;
		private ImageSource _image;
		private object _tag;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public ComboBoxDataItem( ) : this( null, null )
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="value">Value associated with this combobox data item</param>
		public ComboBoxDataItem( object value )
			: this( value, null )
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="value">Value associated with this combobox data item</param>
		/// <param name="displayText">Text to display for this item</param>
		public ComboBoxDataItem( object value, string displayText )
		{
			_dataValue = value;
			_displayText = displayText;
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region DisplayText

		/// <summary>
		/// Gets or sets the display text associated with the item.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Display text associated with the item is displayed for each item in both the drop down
		/// and combo portion. If display text is not specified, text representation of <see cref="Value"/>
		/// will be used.
		/// </para>
		/// </remarks>
		//[Description( "Gets or sets the display text" )]
		[Bindable( true )]
		public string DisplayText
		{
			get
			{
				return _displayText;
			}
			set
			{
				if ( _displayText != value )
				{
					_displayText = value;

					this.RaisePropertyChanged( "DisplayText" );
				}
			}
		}

		/// <summary>
		/// Returns true if the DisplayText property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeDisplayText( )
		{
			return null != _displayText && _displayText.Length > 0;
		}

		/// <summary>
		/// Resets the DisplayText property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetDisplayText( )
		{
			this.DisplayText = null;
		}

		#endregion // DisplayText

		#region Image

		/// <summary>
		/// Gets or sets the image to display in the item.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can display an image in each item by setting this property to an ImageSource instance.
		/// </para>
		/// </remarks>
		//[Description( "Gets or sets the source of image to display in the item" )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		public ImageSource Image
		{
			get
			{
				return _image;
			}
			set
			{
				if ( _image != value )
				{
					_image = value;

					this.RaisePropertyChanged( "Image" );
				}
			}
		}

		/// <summary>
		/// Returns true if the Image property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeImage( )
		{
			return null != _image;
		}

		/// <summary>
		/// Resets the Image property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetImage( )
		{
			this.Image = null;
		}

		#endregion // Image

		#region Tag

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Tag is not used by the item in any way. It simply provides a way for you to associate
		/// a piece of data with an item for your reference.
		/// </para>
		/// </remarks>
		//[Description( "Gets or sets the tag" )]
		//[Category( "Data" )]
		[Bindable( true )]
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				if ( value != _tag )
				{
					_tag = value;

					this.RaisePropertyChanged( "Tag" );
				}
			}
		}

		/// <summary>
		/// Returns true if the Tag property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeTag( )
		{
			return null != _tag;
		}

		/// <summary>
		/// Resets the Tag property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetTag( )
		{
			this.Tag = null;
		}

		#endregion // Tag

		#region Value

		/// <summary>
		/// Gets or sets the value associated with this item.
		/// </summary>
		//[Description( "The value associated with this item" )]
		//[Category( "Data" )]
		[Bindable( true )]
		public object Value
		{
			get
			{
				return _dataValue;
			}
			set
			{
				if ( _dataValue != value )
				{
					_dataValue = value;

					this.RaisePropertyChanged( "Value" );
				}
			}
		}

		/// <summary>
		/// Returns true if the Value property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeValue( )
		{
			return null != _dataValue;
		}

		/// <summary>
		/// Resets the Value property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetValue( )
		{
			this.Value = null;
		}

		#endregion // Value

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Base Overrides

		#region ToString

		/// <summary>
		/// Returns the display text.
		/// </summary>
		/// <returns>The value of the <see cref="DisplayText"/> property.</returns>
		public override string ToString( )
		{
			return this.DisplayText;
		}

		#endregion // ToString

		#endregion // Base Overrides

		#endregion // Methods

		#region INotifyPropertyChanged Members

		private void RaisePropertyChanged( string propertyName )
		{
			INotifyPropertyChanged npc = this;

			PropertyChangedEventHandler handler = this.PropertyChanged;

			if ( null != handler )
				handler( this, new PropertyChangedEventArgs( propertyName ) );
		}

		/// <summary>
		/// IBindingList.ListChanged event implementation.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}

	#endregion // ComboBoxDataItem Class
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