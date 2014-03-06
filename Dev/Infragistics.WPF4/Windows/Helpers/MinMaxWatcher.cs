using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Infragistics.Windows.Helpers
{
	// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
	/// <summary>
	/// Interface implemented by a class that uses a <see cref="MinMaxWatcher"/>
	/// </summary>
	internal interface IMinMaxWatcherOwner
	{
		void OnMinMaxChanged(MinMaxWatcher watcher);
	}

	/// <summary>
	/// Helper class for relaying changes to an element's Min/Max Width/Height properties
	/// </summary>
	internal class MinMaxWatcher : DependencyObject
	{
		#region Member Variables

		private IMinMaxWatcherOwner _owner;
		private FrameworkElement _element;

		#endregion //Member Variables

		#region Constructor
		internal MinMaxWatcher(IMinMaxWatcherOwner owner, FrameworkElement element)
		{
			if (null == owner)
				throw new ArgumentNullException("owner");

			if (null == element)
				throw new ArgumentNullException("element");

			BindingOperations.SetBinding(this, MinWidthProperty, Utilities.CreateBindingObject(MinWidthProperty, BindingMode.OneWay, element));
			BindingOperations.SetBinding(this, MaxWidthProperty, Utilities.CreateBindingObject(MaxWidthProperty, BindingMode.OneWay, element));
			BindingOperations.SetBinding(this, MinHeightProperty, Utilities.CreateBindingObject(MinHeightProperty, BindingMode.OneWay, element));
			BindingOperations.SetBinding(this, MaxHeightProperty, Utilities.CreateBindingObject(MaxHeightProperty, BindingMode.OneWay, element));

			this._owner = owner;
			this._element = element;
		}
		#endregion //Constructor

		#region Properties

		#region Element
		public FrameworkElement Element
		{
			get { return this._element; }
		}
		#endregion // Element

		#region MinWidth

		/// <summary>
		/// Identifies the <see cref="MinWidth"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty MinWidthProperty = FrameworkElement.MinWidthProperty.AddOwner(typeof(MinMaxWatcher));

		/// <summary>
		/// Returns/sets the minimum extent
		/// </summary>
		/// <seealso cref="MinWidthProperty"/>
		internal double MinWidth
		{
			get
			{
				return (double)this.GetValue(MinMaxWatcher.MinWidthProperty);
			}
			set
			{
				this.SetValue(MinMaxWatcher.MinWidthProperty, value);
			}
		}

		#endregion //MinWidth

		#region MaxWidth

		/// <summary>
		/// Identifies the <see cref="MaxWidth"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty MaxWidthProperty = FrameworkElement.MaxWidthProperty.AddOwner(typeof(MinMaxWatcher));

		/// <summary>
		/// Returns/sets the maximum extent
		/// </summary>
		/// <seealso cref="MaxWidthProperty"/>
		internal double MaxWidth
		{
			get
			{
				return (double)this.GetValue(MinMaxWatcher.MaxWidthProperty);
			}
			set
			{
				this.SetValue(MinMaxWatcher.MaxWidthProperty, value);
			}
		}

		#endregion //MaxWidth

		#region MinHeight

		/// <summary>
		/// Identifies the <see cref="MinHeight"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty MinHeightProperty = FrameworkElement.MinHeightProperty.AddOwner(typeof(MinMaxWatcher));

		/// <summary>
		/// Returns/sets the minimum extent
		/// </summary>
		/// <seealso cref="MinHeightProperty"/>
		internal double MinHeight
		{
			get
			{
				return (double)this.GetValue(MinMaxWatcher.MinHeightProperty);
			}
			set
			{
				this.SetValue(MinMaxWatcher.MinHeightProperty, value);
			}
		}

		#endregion //MinHeight

		#region MaxHeight

		/// <summary>
		/// Identifies the <see cref="MaxHeight"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty MaxHeightProperty = FrameworkElement.MaxHeightProperty.AddOwner(typeof(MinMaxWatcher));

		/// <summary>
		/// Returns/sets the maximum extent
		/// </summary>
		/// <seealso cref="MaxHeightProperty"/>
		internal double MaxHeight
		{
			get
			{
				return (double)this.GetValue(MinMaxWatcher.MaxHeightProperty);
			}
			set
			{
				this.SetValue(MinMaxWatcher.MaxHeightProperty, value);
			}
		}

		#endregion //MaxHeight

		#endregion //Properties

		#region Base class overrides

		#region OnPropertyChanged
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			if (null != this._owner)
				this._owner.OnMinMaxChanged(this);
		}
		#endregion //OnPropertyChanged

		#endregion //Base class overrides
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