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

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A cell that represents a <see cref="Cell"/> in a <see cref="AddNewRow"/>
	/// </summary>
	public class AddNewRowCell : Cell
	{
		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="AddNewRowCell"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="AddNewRow"/> object that owns the <see cref="AddNewRowCell"/></param>
		/// <param propertyName="column">The <see cref="Column"/> object that the <see cref="AddNewRowCell"/> represents.</param>
		protected internal AddNewRowCell(RowBase row, Column column)
			: base(row, column)
		{
		}
		#endregion // Constructor

		#region Overrides

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="AddNewRowCellControl"/> when it's attached.
		/// </summary>
		protected override Style ResolveStyle
		{
			get
			{
				if (this.Style != null)
					return this.Style;

                if (this.Column != null && this.Column.AddNewRowCellStyle != null)
                    return this.Column.AddNewRowCellStyle;

				return this.Row.ColumnLayout.AddNewRowSettings.StyleResolved;
			}
		}

		#endregion // ResolveStyle

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of a <see cref="AddNewRowCellControl"/> for the <see cref="AddNewRowCell"/>.
		/// </summary>
		/// <returns>A new <see cref="AddNewRowCellControl"/></returns>
		/// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
		protected override CellControlBase CreateInstanceOfRecyclingElement()
		{
			return new AddNewRowCellControl();
		}

		#endregion // CreateInstanceOfRecyclingElement

		#region EditingSettings
		/// <summary>
		/// Gets the <see cref="EditingSettingsBaseOverride"/> object that controls the settings for this object.
		/// </summary>
		protected internal override EditingSettingsBaseOverride EditingSettings
		{
			get
			{
				return this.Row.ColumnLayout.AddNewRowSettings;
			}
		}
		#endregion // EditingSettings

        #region ResolveIsCellEditable

        /// <summary>
        /// Gets if the particular Cell has a field attribute that says it can edit.
        /// </summary>
        protected override bool ResolveIsCellEditable
        {
            get
            {
                return this.Column.DataField.AllowEditingForAddNew;
            }
        }
        #endregion // ResolveIsCellEditable

        #region IsEditable
        /// <summary>
        /// Gets whether a particular <see cref="Cell"/> can enter edit mode.
        /// </summary>
        public override bool IsEditable
        {
            get
            {
                if (this.Column.AddNewRowEditorTemplate != null)
                    return true;

                return base.IsEditable;
            }
        }
        #endregion // IsEditable

        #region EnableCustomEditorBehaviors

        /// <summary>
        /// Allows a Cell to disable Editor Behavior Support if they choose to.
        /// </summary>
        /// <remarks>
        /// See <see cref="CellBase.EnableCustomEditorBehaviors"/> for more information.
        /// </remarks>
        protected internal override bool EnableCustomEditorBehaviors
        {
            get { return false; }
        }

        #endregion

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