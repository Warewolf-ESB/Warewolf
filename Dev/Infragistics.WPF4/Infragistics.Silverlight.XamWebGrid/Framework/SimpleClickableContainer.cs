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
using System.Windows.Markup;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A container control that has an event that fires a <see cref="SimpleClickableContainer.SimpleClick"/> when a MouseDown and MouseUp event
	/// have been triggered without the mouse moving.
	/// </summary>
	[ContentProperty("Child")]
	[TemplatePart(Name="Border", Type=typeof(Border))]
	public class SimpleClickableContainer : Control
	{
		#region Constructor


        static SimpleClickableContainer()
        {
            Style style = new Style();
            style.Seal();
            Control.FocusVisualStyleProperty.OverrideMetadata(typeof(SimpleClickableContainer), new FrameworkPropertyMetadata(style));
        }


		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleClickableContainer"/> class.
		/// </summary>
		public SimpleClickableContainer()
		{
			base.DefaultStyleKey = typeof(SimpleClickableContainer);
		}
		#endregion // Constructor

		#region Members
		private bool _fireClick;
		private Border _border;
		private UIElement _child;
		#endregion // Members

		#region Properties

		#region Child

		/// <summary>
		/// Identifies the <see cref="Child"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ChildProperty = DependencyProperty.Register("Child", typeof(UIElement), typeof(SimpleClickableContainer), new PropertyMetadata(new PropertyChangedCallback(ChildChanged)));

		/// <summary>
		/// Gets/Sets the child element that will be the content of this control.
		/// </summary>
		public UIElement Child
		{
			get { return (UIElement)this.GetValue(ChildProperty); }
			set { this.SetValue(ChildProperty, value); }
		}

		private static void ChildChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SimpleClickableContainer c = (SimpleClickableContainer)obj;
			if (c._border != null)
				c._border.Child = e.NewValue as UIElement;

			c._child = e.NewValue as UIElement;
		}

		#endregion // Child 

		#endregion // Properties

		#region Overrides

		#region MouseLeftButtonDown
		/// <summary>
		/// Called before the <see cref="UIElement.MouseLeftButtonDown"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			this._fireClick = true;
		}
		#endregion // MouseLeftButtonDown

		#region OnApplyTemplate

		/// <summary>
		/// Builds the visual tree for the <see cref="SimpleClickableContainer"/> when a new template is applied. 
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			this._border = base.GetTemplateChild("Border") as Border;

			if (this._child != null && this._border != null)
				this._border.Child = this._child;

		}
		#endregion // OnApplyTemplate

		#region OnMouseMove
		/// <summary>
		/// Called before the <see cref="UIElement.MouseMove"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			this._fireClick = false;
		}
		#endregion // OnMouseMove

		#region MouseLeftButtonUp
		/// <summary>
		/// Called before the <see cref="UIElement.MouseLeftButtonUp"/> event occurs.
		/// </summary>
		/// <param propertyName="e">The data for the event.</param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);

			if (this._fireClick)
			{
				this.OnSimpleClick();
			}

			this._fireClick = false;
		}

		#endregion // MouseLeftButtonUp

		#endregion // Overrides

		#region Events

		/// <summary>
		/// An event that is triggered when a MouseDown and MouseUp occur without the mouse moving.
		/// </summary>
		public event EventHandler SimpleClick;

		/// <summary>
		/// Raises the <see cref="SimpleClick"/> event.
		/// </summary>
		protected virtual void OnSimpleClick()
		{
			if (this.SimpleClick != null)
				this.SimpleClick(this, EventArgs.Empty);
		}
		#endregion // Events
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