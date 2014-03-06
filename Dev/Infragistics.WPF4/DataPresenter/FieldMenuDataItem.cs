using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Collections;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// ViewModel object that represents a menu item.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	public class FieldMenuDataItem : PropertyChangeNotifier
	{
		#region Member Variables

		private bool _isChecked;
		private bool _isCheckable;
		private ICommand _command;
		private object _commandParameter;
		private object _header;
		private FieldMenuDataItemCollection _items;
		private ImageSource _imageSource;
		private bool _isSeparator;
		private bool _isResizable;
		private bool _hasResizableItems;
		private bool _staysOpenOnClick;
		private bool _isControlHost; // AS 8/19/11 TFS84468

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="FieldMenuDataItem"/>
		/// </summary>
		public FieldMenuDataItem()
		{
		}
		#endregion //Constructor

		#region Properties

		#region Command
		/// <summary>
		/// Returns or sets the command that should be executed when the item is clicked.
		/// </summary>
		public ICommand Command
		{
			get { return _command; }
			set { this.SetField(ref _command, value, "Command"); }
		}
		#endregion //Command

		#region CommandParameter
		/// <summary>
		/// Returns or sets the object to use as the parameter when invoking the <see cref="Command"/>
		/// </summary>
		public object CommandParameter
		{
			get { return _commandParameter; }
			set { this.SetField(ref _commandParameter, value, "CommandParameter"); }
		}
		#endregion //CommandParameter

		#region HasResizableItems
		/// <summary>
		/// Returns a boolean indicating if any of the items in the <see cref="Items"/> collection have their <see cref="IsResizable"/> set to true.
		/// </summary>
		public bool HasResizableItems
		{
			get { return _hasResizableItems; }
			private set { this.SetField(ref _hasResizableItems, value, "HasResizableItems"); }
		}
		#endregion //HasResizableItems

		#region Header
		/// <summary>
		/// Returns or sets a value to display as the header for the item.
		/// </summary>
		public object Header
		{
			get { return _header; }
			set { this.SetField(ref _header, value, "Header"); }
		}
		#endregion //Header

		#region ImageSource
		/// <summary>
		/// Returns or sets the image that should be displayed within the menu.
		/// </summary>
		public ImageSource ImageSource
		{
			get { return _imageSource; }
			set { this.SetField(ref _imageSource, value, "ImageSource"); }
		}
		#endregion //ImageSource

		#region IsCheckable

		/// <summary>
		/// Returns or sets a boolean indicating if the item is considered checkable, that is that its <see cref="IsChecked"/> property may be changed.
		/// </summary>
		public bool IsCheckable
		{
			get { return _isCheckable; }
			set { this.SetField(ref _isCheckable, value, "IsCheckable"); }
		}
		#endregion //IsCheckable

		#region IsChecked

		/// <summary>
		/// Returns or sets a boolean indicating if the item is considered checked.
		/// </summary>
		public bool IsChecked
		{
			get { return _isChecked; }
			set { this.SetField(ref _isChecked, value, "IsChecked"); }
		}
		#endregion //IsChecked

		// AS 8/19/11 TFS84468
		#region IsControlHost

		/// <summary>
		/// Returns or sets a boolean indicating if the item contains a control that should receive focus.
		/// </summary>
		public bool IsControlHost
		{
			get { return _isControlHost; }
			set { this.SetField(ref _isControlHost, value, "IsControlHost"); }
		}
		#endregion //IsControlHost

		#region IsResizable
		/// <summary>
		/// Returns or sets a boolean indicating if the contents of the item may be resized.
		/// </summary>
		public bool IsResizable
		{
			get { return _isResizable; }
			set { this.SetField(ref _isResizable, value, "IsResizable"); }
		}
		#endregion //IsResizable

		#region IsSeparator
		/// <summary>
		/// Returns or sets a boolean indicating if the item represents a separator.
		/// </summary>
		public bool IsSeparator
		{
			get { return _isSeparator; }
			set { this.SetField(ref _isSeparator, value, "IsSeparator"); }
		}
		#endregion //IsSeparator

		#region Items
		/// <summary>
		/// Returns a collection of child menu items.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public IList<FieldMenuDataItem> Items
		{
			get
			{
				if (null == _items)
				{
					_items = new FieldMenuDataItemCollection(this);
				}

				return _items;
			}
		}
		#endregion //Items

		#region StaysOpenOnClick
		/// <summary>
		/// Returns or sets a boolean indicating if the submenu in which the menu item is located should not close when the item is clicked.
		/// </summary>
		public bool StaysOpenOnClick
		{
			get { return _staysOpenOnClick; }
			set { this.SetField(ref _staysOpenOnClick, value, "StaysOpenOnClick"); }
		}
		#endregion //StaysOpenOnClick

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region Find
		internal FieldMenuDataItem Find(Predicate<FieldMenuDataItem> callback, bool recursive, List<FieldMenuDataItem> ancestors)
		{
			if (_items == null)
				return null;

			foreach (var item in _items)
			{
				if (callback(item))
				{
					if (null != ancestors)
						ancestors.Add(this);

					return item;
				}

				if (recursive)
				{
					var matchingDescendant = item.Find(callback, true, ancestors);

					if (null != matchingDescendant)
					{
						if (null != ancestors)
							ancestors.Add(this);

						return matchingDescendant;
					}
				}
			}

			return null;
		}
		#endregion //Find

		#endregion //Internal Methods

		#region Private Methods

		#region SetField
		private bool SetField<T>(ref T member, T value, string propertyName)
		{
			if (EqualityComparer<T>.Default.Equals(member, value))
				return false;

			member = value;
			this.OnPropertyChanged(propertyName);
			return true;
		}
		#endregion //SetField

		#endregion //Private Methods

		#endregion //Methods

		#region FieldMenuDataItemCollection class
		private class FieldMenuDataItemCollection : ObservableCollectionExtended<FieldMenuDataItem>
		{
			#region Member Variables

			private FieldMenuDataItem _owner;
			private int _resizableItemCount;

			#endregion //Member Variables

			#region Constructor
			internal FieldMenuDataItemCollection(FieldMenuDataItem owner)
				: base(true, false)
			{
				Utilities.ValidateNotNull(owner);
				_owner = owner;
			}
			#endregion //Constructor

			#region Base class overrides

			#region NotifyItemsChanged
			protected override bool NotifyItemsChanged
			{
				get
				{
					return true;
				}
			}
			#endregion //NotifyItemsChanged

			#region OnItemAdding
			protected override void OnItemAdding(FieldMenuDataItem itemAdded)
			{
				Utilities.ValidateNotNull(itemAdded, "itemAdded");

				base.OnItemAdding(itemAdded);
			}
			#endregion //OnItemAdding

			#region OnItemAdded
			protected override void OnItemAdded(FieldMenuDataItem itemAdded)
			{
				if (itemAdded.IsResizable)
					this.ResizableItemCount++;

				base.OnItemAdded(itemAdded);
			}
			#endregion //OnItemAdded

			#region OnItemRemoved
			protected override void OnItemRemoved(FieldMenuDataItem itemRemoved)
			{
				if (itemRemoved.IsResizable)
					this.ResizableItemCount--;

				base.OnItemRemoved(itemRemoved);
			}
			#endregion //OnItemRemoved

			#region OnItemPropertyChanged
			protected override void OnItemPropertyChanged(ItemPropertyChangedEventArgs e)
			{
				if (e.PropertyName == "IsResizable")
				{
					var menuItem = e.Item as FieldMenuDataItem;
					if (menuItem.IsResizable)
						this.ResizableItemCount--;
					else
						this.ResizableItemCount++;
				}

				base.OnItemPropertyChanged(e);
			}
			#endregion //OnItemPropertyChanged

			#endregion //Base class overrides

			#region Properties

			#region ResizableItemCount
			internal int ResizableItemCount
			{
				get { return _resizableItemCount; }
				private set
				{
					if (value != _resizableItemCount)
					{
						Debug.Assert(value >= 0);
						_resizableItemCount = value;
						_owner.HasResizableItems = value > 0;
					}
				}
			}
			#endregion //ResizableItemCount

			#endregion //Properties
		}
		#endregion //FieldMenuDataItemCollection class
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