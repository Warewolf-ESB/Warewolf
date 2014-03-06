using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Infragistics.Controls.Editors
{
	/// <summary>
	/// A visual representation of a <see cref="RowSelectionCheckBoxCell"/>
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	[TemplatePart(Name = "Checkbox", Type = typeof(CheckBox))]
	public class RowSelectionCheckBoxCellControl : ComboCellControlBase
	{
		#region Member Variables

		private CheckBox _elementCheckBox;

		#endregion //Member Variables

		#region Constructor


		static RowSelectionCheckBoxCellControl()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(RowSelectionCheckBoxCellControl), new FrameworkPropertyMetadata(typeof(RowSelectionCheckBoxCellControl)));
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="RowSelectionCheckBoxCellControl"/> class.
        /// </summary>
		public RowSelectionCheckBoxCellControl()
        {



		}

		#endregion //Constructor

		#region Overrides

		#region AttachContent

		/// <summary>
		/// Invoked when content is attached to the Control.
		/// </summary>
		protected override void AttachContent()
		{
            this.DataContext = this.Cell.Row;

            // Since we remove the Binding in OnReleased, we should re-attach it in AttachContent
            this.AttachIsSelectedToCheckBox();
		}

		#endregion // AttachContent

        #region OnReleased

        /// <summary>
        /// Cleanup anything that we shouldn't be using anymore
        /// </summary>
        /// <param name="cell"></param>
        protected internal override void OnReleased(ComboCellBase cell)
        {
            base.OnReleased(cell);

            // Make sure we clear out the binding before we remove the DataContext
            // Otherwise, we'll be in an invalid state for our binding, and receive binding errors in the output window.
            if (this._elementCheckBox != null)
                this._elementCheckBox.ClearValue(CheckBox.IsCheckedProperty);

            this.DataContext = null;
        }

        #endregion // OnReleased

        #region MeasureOverride
        private static int measureCount;
		/// <summary>
		/// Allows a Cell to ensure it was propely measured. 
		/// </summary>
		/// <param name="availableSize"></param>
		/// <returns></returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			measureCount++;

			return base.MeasureOverride(availableSize);
		}

		#endregion // MeasureOverride

        #region OnMouseLeftButtonDown
        /// <summary>
        /// Called before the MouseLeftButtonDown event is raised.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

			// JM 09-14-11 TFS87617
            //e.Handled = true;

            this.Cell.Row.ComboEditor.OnComboEditorItemClicked((ComboRow)this.Cell.Row);
        }
        #endregion // OnMouseLeftButtonDown

		#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
		/// it to get the focus site from the control template whenever template gets applied to the control.
		/// </p>
		/// </remarks>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this._elementCheckBox = this.GetTemplateChild("Checkbox") as CheckBox;

            this.AttachIsSelectedToCheckBox();

			this.EnsureVisualStates();
		}

		#endregion //OnApplyTemplate

		#endregion //Overrides

		#region Methods

		#region Private

		#region AttachIsSelectedToCheckBox

		private void AttachIsSelectedToCheckBox()
		{
			if (this._elementCheckBox != null)
			{
				Binding b = new Binding("IsSelected");
				b.Mode = BindingMode.TwoWay;
				this._elementCheckBox.SetBinding(CheckBox.IsCheckedProperty, b);
			}
		}

		#endregion // AttachIsSelectedToCheckBox

		#endregion //Private 

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