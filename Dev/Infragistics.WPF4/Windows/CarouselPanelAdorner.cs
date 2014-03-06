using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// The Adorner created by the <see cref="XamCarouselPanel"/> and used to hold various elements such as the <see cref="CarouselPanelNavigator"/>
	/// </summary>
	/// <remarks>
	/// <p class="body">The adorner is automatically created by the <see cref="XamCarouselPanel"/> and allows the <see cref="CarouselPanelNavigator"/> to float on top of all the items in the <see cref="XamCarouselPanel"/></p>
	/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamCarouselPanel"/> when needed.  You do not ordinarily need to create an instance of this class directly.</p>
	/// </remarks>
	/// <seealso cref="XamCarouselPanel"/>
	/// <seealso cref="CarouselPanelNavigator"/>
    // JJD 8/20/08 - BR35341
    // Added AdornerEx abstract base class to handle adornerlayer re-creations based on template changes from higher level elements 
    //public class CarouselPanelAdorner : Adorner
	public class CarouselPanelAdorner : AdornerEx
	{
		#region Member Variables

		private List<UIElement>						_children;

		#endregion //Member Variables	
    
		#region Constructor

        // JJD 8/20/08 - BR35341
        // AdornerEx abstract base class contructor requires a FrameworkElement 
		//internal CarouselPanelAdorner(UIElement adornedElement) : base(adornedElement)
		internal CarouselPanelAdorner(FrameworkElement adornedElement) : base(adornedElement)
		{
		}

		#endregion //Constructor	

		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			for (int i = 0; i < this.Children.Count; i++)
			{
				this.Children[i].Arrange(new Rect(new Point(0, 0), finalSize));
			}

			return base.ArrangeOverride(finalSize);
		}

			#endregion //ArrangeOverride	
    
			#region GetVisualChild

		/// <summary>
		/// Gets the parent child at a specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the specific child parent.</param>
		/// <returns>The parent child at the specified index.</returns>
		protected override Visual GetVisualChild(int index)
		{
			if (index < 0 || index >= this.Children.Count)
				return null;

			return this.Children[index];
		}

			#endregion //GetVisualChild	
    
			#region VisualChildrenCount

		/// <summary>
		/// Returns the number of Visual Children contained within the element.
		/// </summary>
		protected override int VisualChildrenCount
		{
			get { return this.Children.Count; }
		}

			#endregion //VisualChildrenCount	
    
		#endregion //Base Class Overrides

		#region Methods

			#region Internal Methods

				#region AddChildElement

		internal void AddChildElement(UIElement childElement)
		{
			this.AddVisualChild(childElement);
			this.Children.Add(childElement);
		}

				#endregion //AddChildElement

				// JM 01-05-12 TFS96730 Added.
				#region RemoveChildElement

		internal void RemoveChildElement(UIElement childElement)
		{
			this.RemoveVisualChild(childElement);
			this.Children.Remove(childElement);
		}

				#endregion //RemoveChildElement

			#endregion //Internal Methods	
    
		#endregion //Methods

		#region Properties

			#region Children

		private List<UIElement> Children
		{
			get
			{
				if (this._children == null)
					this._children = new List<UIElement>(3);

				return this._children;
			}
		}

			#endregion //Children	
    
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