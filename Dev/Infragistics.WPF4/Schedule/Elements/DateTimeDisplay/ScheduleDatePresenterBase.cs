using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using Infragistics.AutomationPeers;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Base class for custom elements used to display formatted date and time information.
	/// </summary>
	[DesignTimeVisible(false)]
	public class ScheduleDatePresenterBase : Control
	{
		#region Constructor
		static ScheduleDatePresenterBase()
		{

			ScheduleDatePresenterBase.DefaultStyleKeyProperty.OverrideMetadata(typeof(ScheduleDatePresenterBase), new FrameworkPropertyMetadata(typeof(ScheduleDatePresenterBase)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(ScheduleDatePresenterBase), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

		}

		/// <summary>
		/// Initializes a new <see cref="ScheduleDatePresenterBase"/>
		/// </summary>
		public ScheduleDatePresenterBase()
		{



		}
		#endregion //Constructor

		#region Properties

		#region Public properties

		#region FormattedText

		#region FormattedText

		private static readonly DependencyPropertyKey FormattedTextPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("FormattedText",
			typeof(string), typeof(ScheduleDatePresenterBase), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="FormattedText"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FormattedTextProperty = FormattedTextPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the text to display after it has been formatted
		/// </summary>
		/// <seealso cref="FormattedTextProperty"/>
		public string FormattedText
		{
			get
			{
				return (string)this.GetValue(ScheduleDatePresenterBase.FormattedTextProperty);
			}
			internal set
			{
				this.SetValue(ScheduleDatePresenterBase.FormattedTextPropertyKey, value);
			}
		}

		#endregion //FormattedText

		#endregion //FormattedText

		#endregion //Public properties	
    
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