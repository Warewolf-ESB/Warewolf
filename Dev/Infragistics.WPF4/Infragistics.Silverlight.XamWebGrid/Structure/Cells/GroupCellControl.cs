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
    /// The visual representation of the <see cref="GroupCell"/>
    /// </summary>
    public class GroupCellControl : CellControlBase
    {
        #region Members

        GroupColumnPanel _panel;

        #endregion // Members

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCellControl"/> class.
        /// </summary>
        public GroupCellControl()
        {
            base.DefaultStyleKey = typeof(GroupCellControl);
        }
        #endregion // Constructor

        #region Overrides

        #region OnApplyTemplate

        /// <summary>
        /// Builds the visual tree for the <see cref="GroupCellControl"/>
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._panel = base.GetTemplateChild("Panel") as GroupColumnPanel;
        }

        #endregion // OnApplyTemplate

        #region ManuallyInvokeMeasure

        /// <summary>
        /// Invoked when a CellControl's measure was called, but its MeasureOverride was not invoked. 
        /// This method should be overriden on a derived class, if it's neccessary to take some extra steps to ensure Measure has been called.
        /// </summary>
        /// <param name="sizeToBeMeasured"></param>
        protected internal override void ManuallyInvokeMeasure(Size sizeToBeMeasured)
        {
            if (this._panel != null)
                this._panel.InvalidateMeasure();
        }

        #endregion // ManuallyInvokeMeasure

        #region ShowResizingArrow
        /// <summary>
        /// Gets if a resizing arrow should be shown.
        /// </summary>
        protected override bool ShowResizingArrow
        {
            get
            {
                return this.Cell.Column.ColumnLayout.ColumnResizingSettings.AllowCellAreaResizingResolved;
            }
        }
        #endregion // ShowResizingArrow

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