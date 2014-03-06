using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Controls;

namespace Infragistics.Controls.Layouts.Primitives
{

	#region TileDragIndicator Class

	/// <summary>
	/// This control is used for displaying drag indicator when a tile is being dragged.
	/// </summary>
	/// <remarks>
	/// <b>Note</b> that there is no need for you to instantiate this directly. This control 
	/// is created automatically when the user starts a drag operation.
	/// </remarks>
	//[ToolboxItem( false )]
 	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class TileDragIndicator : ContentControl
	{
		#region Constructor

		static TileDragIndicator( )
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( TileDragIndicator ), new FrameworkPropertyMetadata( typeof( TileDragIndicator ) ) );
			ContentControl.FocusableProperty.OverrideMetadata( typeof( TileDragIndicator ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );

		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="TileDragIndicator"/> class.
		/// </summary>
		public TileDragIndicator( )
		{



		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Container

		internal static readonly DependencyPropertyKey ContainerPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Container",
			typeof(FrameworkElement), typeof(TileDragIndicator), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="Container"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContainerProperty = ContainerPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the FrameworkElement containing the item being dragged (read-only).
        /// </summary>
        [Browsable(false)]

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        public FrameworkElement Container
		{
			get
			{
				return (FrameworkElement)this.GetValue(TileDragIndicator.ContainerProperty);
			}
		}

    		#endregion //Container
	
		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // TileDragIndicator Class
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