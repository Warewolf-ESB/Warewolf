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
	/// Visual object for the <see cref="SummaryRowCell"/> object.
	/// </summary>
	[TemplatePart(Name = "SummaryDisplay", Type = typeof(Panel))]
	public class SummaryRowCellControl : CellControl
	{
		#region Members

		Panel _panel;
        StackPanel _toolTipPanel;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryRowCellControl"/> class.
		/// </summary>
		public SummaryRowCellControl()
		{
			base.DefaultStyleKey = typeof(SummaryRowCellControl);
		}

		#endregion // Constructor

		#region Properties

		#region SummaryDisplayTemplate

		/// <summary>
		/// Identifies the <see cref="SummaryDisplayTemplate"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty SummaryDisplayTemplateProperty = DependencyProperty.Register("SummaryDisplayTemplate", typeof(DataTemplate), typeof(SummaryRowCellControl), new PropertyMetadata(new PropertyChangedCallback(SummaryDisplayTemplateChanged)));

		/// <summary>
		/// Gets / sets the <see cref="DataTemplate"/> that will be used to display the items in the <see cref="SummaryRowCellControl"/>
		/// </summary>
		public DataTemplate SummaryDisplayTemplate
		{
			get { return (DataTemplate)this.GetValue(SummaryDisplayTemplateProperty); }
			set { this.SetValue(SummaryDisplayTemplateProperty, value); }
		}

		private static void SummaryDisplayTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryRowCellControl src = (SummaryRowCellControl)obj;
			src.OnPropertyChanged("SummaryDisplayTemplate");
		}

		#endregion // SummaryDisplayTemplate

		#endregion // Properties

		#region Overrides

		#region OnApplyTemplate

		/// <summary>
		/// Builds the visual tree for the <see cref="SummaryRowCellControl"/> when a new template is applied. 
		/// </summary>
		public override void OnApplyTemplate()
		{
			this._panel = base.GetTemplateChild("SummaryDisplay") as Panel;

            if (_panel != null)
            {
                this.FillPanelWithSummaryResults(this._panel);
            }

			base.OnApplyTemplate();
		}

		#endregion // OnApplyTemplate

		#region EnsureContent
		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>	
		protected internal override void EnsureContent()
		{
			base.EnsureContent();
			if (_panel != null)
			{
                this.FillPanelWithSummaryResults(this._panel);
			}
		}

		#endregion // EnsureContent

		#region ReleaseContent

		/// <summary>
		/// Invoked before content is released from the control.
		/// </summary>
		protected override void ReleaseContent()
		{
			if (_panel != null)
			{
				_panel.Children.Clear();
			}

			base.ReleaseContent();
		}

		#endregion // ReleaseContent

        #region ToolTipContent

        /// <summary>
        ///  Allows a <see cref="CellControl"/> to provide different content for it's Tooltip.
        /// </summary>
        protected override object ToolTipContent
        {
            get
            {
                if (this._toolTipPanel == null)
                {
                    this._toolTipPanel = new StackPanel();
                    this._toolTipPanel.Orientation = Orientation.Vertical;
                }
                this.FillPanelWithSummaryResults(this._toolTipPanel);

                if (this._toolTipPanel.Children.Count > 0)
                    return this._toolTipPanel;
                else
                    return null;
            }
        }
        #endregion // ToolTipContent

        #endregion // Overrides

        #region Methods

        #region Protected

        #region FillPanelWithSummaryResults

        /// <summary>
        /// Fills the specified panel with all of the Summaries for a particular ColumnLayout
        /// </summary>
        /// <param name="panel"></param>
        protected virtual void FillPanelWithSummaryResults(Panel panel)
        {
            panel.Children.Clear();

            if (this.Cell != null)
            {
                SummaryResultCollection src = ((RowsManager)this.Cell.Row.Manager).SummaryResultCollectionInternal;
                foreach (SummaryResult sr in src)
                {
                    if (sr.SummaryDefinition.ColumnKey == this.Cell.Column.Key)
                    {
                        ContentControl cc = new ContentControl();
                        cc.ContentTemplate = this.SummaryDisplayTemplate;
                        cc.Content = sr;
                        panel.Children.Add(cc);
                    }
                }
            }
        }

        #endregion // FillPanelWithSummaryResults

        #endregion // Protected

        #endregion // Methods
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