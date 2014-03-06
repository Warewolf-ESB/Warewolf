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
using System.Windows.Data;
using Infragistics.Controls.Primitives;

namespace Infragistics.Controls.Grids.Primitives
{

	/// <summary>
	/// Visual object for the <see cref="ExpansionIndicatorCell"/> object.
	/// </summary>
	[TemplatePart(Name = "Indicator", Type = typeof(ExpansionIndicator))]
	public class ExpansionIndicatorCellControl : CellControlBase
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpansionIndicatorCellControl"/> class.
		/// </summary>
		public ExpansionIndicatorCellControl()
		{
			base.DefaultStyleKey = typeof(ExpansionIndicatorCellControl);
		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region ExpansionIndicator

		/// <summary>
		/// Gets the <see cref="ExpansionIndicator"/> for the <see cref="ExpansionIndicatorCellControl"/>
		/// </summary>
		public ExpansionIndicator ExpansionIndicator
		{
			get;
			private set;
		}

		#endregion // ExpansionIndicator

		#endregion // Public

		#endregion // Properties

		#region Overrides

		#region OnApplyTemplate

		/// <summary>
		/// Builds the visual tree for the <see cref="ExpansionIndicatorCellControl"/>
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			this.ExpansionIndicator = base.GetTemplateChild("Indicator") as ExpansionIndicator;

            // Apparently there is a case where OnApplyTemplate could get called after EnsureContent
            // So just in case, call ensureContent after we have an ExpansionIndicator, so that it definitely shows. 
            this.EnsureContent();
		}

		#endregion // OnApplyTemplate

		#region OnLoaded

		/// <summary>
		/// Raised when a <see cref="ExpansionIndicatorCellControl"/> is loaded. 
		/// </summary>
		protected override void OnLoaded()
		{
			base.OnLoaded();

			this.EnsureContent();
		}

		#endregion // OnLoaded

		#region EnsureContent

		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>
		protected internal override void EnsureContent()
		{
			if (this.ExpansionIndicator != null && this.Cell != null)
			{
				if (((ExpandableRowBase)this.Cell.Row).HasChildren)
				{
					this.ExpansionIndicator.Visibility = Visibility.Visible;
					Binding binding = new Binding("IsExpanded");
					binding.Mode = BindingMode.TwoWay;
					binding.Source = this.Cell.Row;
					this.ExpansionIndicator.SetBinding(ExpansionIndicator.IsExpandedProperty, binding);
				}
				else
					this.ExpansionIndicator.Visibility = Visibility.Collapsed;
			}

			base.EnsureContent();
		}

		#endregion // EnsureContent

		#endregion // Overrides
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