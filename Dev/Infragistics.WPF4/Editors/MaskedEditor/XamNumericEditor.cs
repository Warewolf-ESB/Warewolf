using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Licensing;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Editors.Events;
using Infragistics.Windows.Controls.Events;


namespace Infragistics.Windows.Editors
{
	#region XamNumericEditor Class

	/// <summary>
	/// Allows editing of numeric data based on a mask.
	/// </summary>
	/// <remarks>
	/// <p class="body">The <b>XamNumericEditor</b> is a derived <see cref="XamMaskedEditor"/> that is designed to 
	/// display and edit numeric data. By default, its <see cref="ValueEditor.ValueType"/> property is set to 
	/// <see cref="double"/>.</p>
	/// </remarks>
	
	
	//[Description( "Masked editor for editing numeric values." )]
	public class XamNumericEditor : XamMaskedEditor
	{
		#region static constants

		#endregion //static constants

		#region Variables

		private UltraLicense _license;		

		#endregion //Variables

		#region Constructors

		static XamNumericEditor( )
		{
			// Default value type for this editor should be double.
			ValueTypeProperty.OverrideMetadata( typeof( XamNumericEditor ), new FrameworkPropertyMetadata( typeof( double ) ) );

			// Default the alignment to Right since this is editing numbers.
			HorizontalContentAlignmentProperty.OverrideMetadata( typeof( XamNumericEditor ), new FrameworkPropertyMetadata( HorizontalAlignment.Right ) );

			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( XamNumericEditor ), new FrameworkPropertyMetadata( typeof( XamNumericEditor ) ) );
		}

		/// <summary>
		/// Initializes a new <see cref="XamNumericEditor"/>
		/// </summary>
		public XamNumericEditor( )
		{
			// verify and cache the license
			//
			// Wrapped in a try/catch for a FileNotFoundException.
			// When the assembly is loaded dynamically, VS seems 
			// to be trying to reload a copy of Shared even though 
			// one is in memory. This generates a FileNotFoundException
			// when the dll is not in the gac and not in the AppBase
			// for the AppDomain.
			//
			// AS 11/7/07 BR21903
			// Always do the license checks.
			//
			//if ( DesignerProperties.GetIsInDesignMode( this ) )
			{
				try
				{
					// We need to pass our type into the method since we do not want to pass in 
					// the derived type.
					this._license = LicenseManager.Validate( typeof( XamNumericEditor ), this ) as UltraLicense;
				}
				catch ( System.IO.FileNotFoundException ) { }
			}
		}

		#endregion // Constructors
	}

	#endregion // XamNumericEditor Class
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