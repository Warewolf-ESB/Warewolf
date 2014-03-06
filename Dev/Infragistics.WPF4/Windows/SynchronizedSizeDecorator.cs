using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Controls
{
	// AS 10/22/09 TFS24142
	/// <summary>
	/// Custom class used to synchronize the extent of two elements separated within the visual tree.
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SynchronizedSizeDecorator : Decorator
	{
		#region Member Variables

		private bool _isMeasuringTarget;
		private Size _lastActualDesired;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="SynchronizedSizeDecorator"/>
		/// </summary>
		public SynchronizedSizeDecorator()
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size desired = base.MeasureOverride(availableSize);

			// cache the actual desired
			_lastActualDesired = desired;

			SynchronizedSizeDecorator target = this.Target;

			// if we're not told to ignore our content...
			if (null != target && target != this)
			{
				// if the target is in the process of measuring its 
				// target then we don't want to have it get into its 
				// measure recursively
				if (!target.IsMeasureValid && !target._isMeasuringTarget)
				{
					try
					{
						this._isMeasuringTarget = true;
						target.Measure(availableSize);
					}
					finally
					{
						this._isMeasuringTarget = false;
					}
				}

				// if the target is in the middle of measuring then we can't rely 
				// on its desired size since it won't have returned from its 
				// measure call yet and so just use the size it would have return 
				// if it had no target
				Size targetSize = target._isMeasuringTarget ? target._lastActualDesired : target.DesiredSize;
				bool dirtyTarget = false;

				if (this.SynchronizeWidth)
				{
					if (!Utilities.AreClose(desired.Width, targetSize.Width))
					{
						if (desired.Width < targetSize.Width)
							desired.Width = targetSize.Width;
						else
							dirtyTarget = true;
					}
				}

				if (this.SynchronizeHeight)
				{
					if (!Utilities.AreClose(desired.Height, targetSize.Height))
					{
						if (desired.Height < targetSize.Height)
							desired.Height = targetSize.Height;
						else
							dirtyTarget = true;
					}
				}

				if (dirtyTarget)
					target.InvalidateMeasure();
			}

			return desired;
		}
		#endregion //MeasureOverride

		#region OnChildDesiredSizeChanged
		/// <summary>
		/// Invoked when the <see cref="UIElement.DesiredSize"/> of an element changes.
		/// </summary>
		/// <param name="child">The child whose size is being changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			SynchronizedSizeDecorator target = this.Target;

			if (null != target)
				target.InvalidateMeasure();

			base.OnChildDesiredSizeChanged(child);
		} 
		#endregion //OnChildDesiredSizeChanged

		#endregion //Base class overrides

		#region Properties

		#region SynchronizeHeight

		/// <summary>
		/// Identifies the <see cref="SynchronizeHeight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SynchronizeHeightProperty = DependencyProperty.Register("SynchronizeHeight",
			typeof(bool), typeof(SynchronizedSizeDecorator), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns or sets a boolean indicating if the height should be synchronized with the <see cref="Target"/>.
		/// </summary>
		/// <seealso cref="SynchronizeHeightProperty"/>
		/// <seealso cref="SynchronizeWidth"/>
		/// <seealso cref="Target"/>
		//[Description("Returns or sets a boolean indicating if the height should be synchronized with the Target.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool SynchronizeHeight
		{
			get
			{
				return (bool)this.GetValue(SynchronizedSizeDecorator.SynchronizeHeightProperty);
			}
			set
			{
				this.SetValue(SynchronizedSizeDecorator.SynchronizeHeightProperty, value);
			}
		}

		#endregion //SynchronizeHeight

		#region SynchronizeWidth

		/// <summary>
		/// Identifies the <see cref="SynchronizeWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SynchronizeWidthProperty = DependencyProperty.Register("SynchronizeWidth",
			typeof(bool), typeof(SynchronizedSizeDecorator), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns or sets a boolean indicating if the width should be synchronized with the <see cref="Target"/>.
		/// </summary>
		/// <seealso cref="SynchronizeWidthProperty"/>
		/// <seealso cref="SynchronizeHeight"/>
		/// <seealso cref="Target"/>
		//[Description("Returns or sets a boolean indicating if the width should be synchronized with the Target.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool SynchronizeWidth
		{
			get
			{
				return (bool)this.GetValue(SynchronizedSizeDecorator.SynchronizeWidthProperty);
			}
			set
			{
				this.SetValue(SynchronizedSizeDecorator.SynchronizeWidthProperty, value);
			}
		}

		#endregion //SynchronizeWidth

		#region Target

		/// <summary>
		/// Identifies the <see cref="Target"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target",
			typeof(SynchronizedSizeDecorator), typeof(SynchronizedSizeDecorator), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, null, new CoerceValueCallback(CoerceTarget)));

		private static object CoerceTarget(DependencyObject d, object newValue)
		{
			SynchronizedSizeDecorator src = (SynchronizedSizeDecorator)d;
			SynchronizedSizeDecorator target = (SynchronizedSizeDecorator)newValue;

			if (newValue != null)
			{
				if (target.IsAncestorOf(src) || target.IsDescendantOf(src))
					throw new InvalidOperationException();
			}

			return newValue;
		}

		/// <summary>
		/// Returns or sets the other SynchronizedSizeDecorator whose extent is to be synchronized.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b>The Target cannot be an ancestor or descendant of this element.</p>
		/// </remarks>
		/// <seealso cref="TargetProperty"/>
		/// <seealso cref="SynchronizeWidth"/>
		/// <seealso cref="SynchronizeHeight"/>
		//[Description("Returns or sets the other SynchronizedSizeDecorator whose extent is to be synchronized.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public SynchronizedSizeDecorator Target
		{
			get
			{
				return (SynchronizedSizeDecorator)this.GetValue(SynchronizedSizeDecorator.TargetProperty);
			}
			set
			{
				this.SetValue(SynchronizedSizeDecorator.TargetProperty, value);
			}
		}

		#endregion //Target

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