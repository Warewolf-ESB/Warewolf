using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.ComponentModel;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Displays overflow tools that cannot fit on the <see cref="QuickAccessToolbar"/>.
	/// </summary>
	//[ToolboxItem(false)]	// JM BR28203 11-06-07 - added this here for documentation but commented out and added ToolboxBrowsableAttribute directly to DesignMetadata for the XamRibbon assembly.
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class QuickAccessToolbarOverflowPanel : Panel
	{
		#region Member Variables

		private QuickAccessToolbar								_qat;

		#endregion //Member Variables	
    
		#region Constructor

		/// <summary>
		/// Initializes an instance of the <see cref="QuickAccessToolbarOverflowPanel"/> class.
		/// </summary>
		public QuickAccessToolbarOverflowPanel()
		{
		}

		static QuickAccessToolbarOverflowPanel()
		{
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Arranges and sizes the tools contained in the QuickAccessToolbarOverflowPanel.
		/// </summary>
		/// <param name="finalSize">The size available to arrange the contained tools.</param>
		/// <returns>The size used to arrange the contained tools.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			UIElementCollection children					= base.Children;
			int					childrenCount				= children.Count;
			double				defaultQatToolHeight		= XamRibbon.DEFAULT_QAT_TOOL_SIZE.Height;
			double				defaultQatToolWidth			= XamRibbon.DEFAULT_QAT_TOOL_SIZE.Width;
			Rect				arrangeRect					= new Rect(0, 0, defaultQatToolWidth, defaultQatToolHeight);

			for (int i = 0; i < childrenCount; i++)
			{
				UIElement	child				= children[i];
				double		childDesiredWidth	= Math.Round(child.DesiredSize.Width);

				arrangeRect.Width				= childDesiredWidth;
				child.Arrange(arrangeRect);
				arrangeRect.X					+= childDesiredWidth;
			}

			return finalSize;
		}

			#endregion //ArrangeOverride	
    
			#region CreateUIElementCollection

		/// <summary>
		/// Creates a new UIElementCollection.
		/// </summary>
		/// <param name="logicalParent">The logical parent of the new collection.</param>
		/// <returns>A new collection.</returns>
		protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
		{
			return new UIElementCollection(this, (base.TemplatedParent == null) ? logicalParent : null);
		}

			#endregion //CreateUIElementCollection	

			#region MeasureOverride

		/// <summary>
		/// Measures the QuickAccessToolbarOverflowPanel and all its contained tools.
		/// </summary>
		/// <param name="availableSize">The size available to the QuickAccessToolbarOverflowPanel.</param>
		/// <returns>The size desired by the QuickAccessToolbarOverflowPanel.</returns>
		protected override Size MeasureOverride(System.Windows.Size availableSize)
		{
			UIElementCollection children					= base.Children;
			int					childrenCount				= children.Count;
			double				defaultQatToolHeight		= XamRibbon.DEFAULT_QAT_TOOL_SIZE.Height;
			double				widthUsed					= 0;
			double				heightUsed					= defaultQatToolHeight;

			for (int i = 0; i < childrenCount; i++)
			{
				UIElement child = children[i];

				widthUsed += Math.Round(child.DesiredSize.Width);
			}

			return new Size(widthUsed, heightUsed);
		}

			#endregion //MeasureOverride	
    
		#endregion //Base Class Overrides

		#region Properties

			#region Internal Properties

				#region QAT

		internal QuickAccessToolbar QAT
		{
			get { return this._qat; }
			set { this._qat = value; }
		}

				#endregion //QAT
    
			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

            #region Removed
        
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

            #endregion //Removed

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