using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;

namespace Infragistics.Controls.Primitives
{
	/// <summary>
	/// Element used to rotate its content
	/// </summary>
	[DesignTimeVisible(false)]
	public class RotateDecorator : ContentControl
	{
		#region Private Members

		private RotateTransform _rotateTransform;

		#endregion //Private Members	
    
		#region Constructor
		static RotateDecorator()
		{

			RotateDecorator.DefaultStyleKeyProperty.OverrideMetadata(typeof(RotateDecorator), new FrameworkPropertyMetadata(typeof(RotateDecorator)));

		}

		/// <summary>
		/// Initializes a new <see cref="RotateDecorator"/>
		/// </summary>
		public RotateDecorator()
		{




			// AS 9/29/10 TFS49515
			// WPF seems to be freezing the transform we are setting on the root element so if 
			// we bind the Angle instead of setting it in the change callback they see that it 
			// is not freezable and don't freeze it.
			//
			_rotateTransform = new RotateTransform();
			BindingOperations.SetBinding(_rotateTransform, RotateTransform.AngleProperty, new Binding("Angle") { Source = this });
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="arrangeBounds">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			FrameworkElement child = this.GetSingleChild();
			if (child == null)
				return base.ArrangeOverride(arrangeBounds);

			double angleInRadians = ConvertAngleToRadians(this.Angle);

			Size sizeToArrange = ConvertSizeToChildSize(arrangeBounds, child.DesiredSize, angleInRadians);

			Size boundingSize = ConvertChildSizeToSize(child.DesiredSize, angleInRadians);

			this._rotateTransform.CenterX = sizeToArrange.Width / 2;
			this._rotateTransform.CenterY = sizeToArrange.Height / 2;

			Point topLeft = new Point((boundingSize.Width - sizeToArrange.Width) / 2,
										   (boundingSize.Height - sizeToArrange.Height) / 2);

			child.Arrange(new Rect(topLeft, sizeToArrange));

			return boundingSize;

		}

		#endregion //ArrangeOverride	
    
		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			FrameworkElement child = this.GetSingleChild();
			if (child == null)
				return base.MeasureOverride(constraint);

			double angle = ConvertAngleToRadians(this.Angle);

			child.Measure(ConvertSizeToChildSize(angle, constraint));

			Size childSize = child.DesiredSize;

			return ConvertChildSizeToSize(childSize, angle);
		}

		#endregion //MeasureOverride	
    
		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			FrameworkElement child = this.GetSingleChild();
			Debug.Assert(child != null, "RotateDecorator has ivalid child count");

			if (child != null)
				child.RenderTransform = this._rotateTransform;
		}

		#endregion //OnApplyTemplate	
    
		#endregion //Base class overrides	
    
		#region Properties

		#region Public Properties

		#region Angle

		/// <summary>
		/// Identifies the <see cref="Angle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AngleProperty = DependencyPropertyUtilities.Register("Angle",
			typeof(double), typeof(RotateDecorator),
			DependencyPropertyUtilities.CreateMetadata(0d, new PropertyChangedCallback(OnAngleChanged))
			);

		private static void OnAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			RotateDecorator instance = (RotateDecorator)d;

			// AS 9/29/10 TFS49515
			// Bind instead - see notes in the ctor.
			//
			//instance._rotateTransform.Angle = (double)e.NewValue;
			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets the angle to rotate the content
		/// </summary>
		/// <seealso cref="AngleProperty"/>
		public double Angle
		{
			get
			{
				return (double)this.GetValue(RotateDecorator.AngleProperty);
			}
			set
			{
				this.SetValue(RotateDecorator.AngleProperty, value);
			}
		}

		#endregion //Angle

		#endregion //Public Properties	
    
		#endregion //Properties	

		#region Methods

		#region Private Methods

		#region ConvertAngleToRadians

		private static double ConvertAngleToRadians(double angleInDegrees)
		{
			return angleInDegrees * Math.PI / 180;
		}

		#endregion //ConvertAngleToRadians	
    
		#region ConvertChildSizeToSize

		private static Size ConvertChildSizeToSize(Size childSize, double angleInRadians)
		{
			double sin = Math.Abs(Math.Sin(angleInRadians));
			double cos = Math.Abs(Math.Cos(angleInRadians));

			Size size = new Size(Math.Abs((sin * childSize.Height) + (cos * childSize.Width)),
								  Math.Abs((sin * childSize.Width) + (cos * childSize.Height)));

			return size;
		}

		#endregion //ConvertChildSizeToSize	

		#region ConvertSizeToChildSize

		private Size ConvertSizeToChildSize(double angleInRadians, Size constraint)
		{
			double sin = Math.Abs(Math.Sin(angleInRadians));

			// handle the horizontal case
			if (CoreUtilities.AreClose(sin, 0))
				return constraint;

			// handle the vertical case
			if (CoreUtilities.AreClose(sin, 1))
				return new Size(constraint.Height, constraint.Width);

			if (double.IsPositiveInfinity(constraint.Width) ||
				double.IsPositiveInfinity(constraint.Height))
				return constraint;

			double cos = Math.Abs(Math.Cos(angleInRadians));
			double aspectRatio = constraint.Width / constraint.Height;

			double h1 = Math.Abs( constraint.Height / ((sin * aspectRatio) + cos));
			double h2 = Math.Abs(constraint.Width / (sin + (cos * aspectRatio)));
			double height = Math.Min(h1, h2);

			return new Size(height * aspectRatio, height);
		}

		private Size ConvertSizeToChildSize(Size size, Size childDesiredSize, double angleInRadians)
		{
			double sin = Math.Abs(Math.Sin(angleInRadians));

			// handle the horizontal case
			if (CoreUtilities.AreClose(sin, 0))
				return size;

			// handle the vertical case
			if (CoreUtilities.AreClose(sin, 1))
				return new Size(size.Height, size.Width);

			if (childDesiredSize.Width == 0 ||
				childDesiredSize.Height == 0 ||
				double.IsPositiveInfinity(size.Width) ||
				double.IsPositiveInfinity(size.Height))
				return childDesiredSize;
			
			double cos = Math.Abs( Math.Cos(angleInRadians));
			double aspectRatio = childDesiredSize.Width / childDesiredSize.Height;

			double h1 = Math.Abs( size.Height / ((sin * aspectRatio) + cos));
			double h2 = Math.Abs( size.Width / (sin + (cos * aspectRatio)));
			double height = Math.Min( Math.Min(h1, h2), childDesiredSize.Height);

			return new Size(height * aspectRatio, height);
		}

		#endregion //ConvertSizeToChildSize	
    
		#region GetSingleChild

		private FrameworkElement GetSingleChild()
		{
			int count = VisualTreeHelper.GetChildrenCount(this);

			if (count == 1)
				return VisualTreeHelper.GetChild(this, 0) as FrameworkElement;

			return null;
		}

		#endregion //GetSingleChild

		#endregion //Private Methods	
    
		#endregion //Methods	
            
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