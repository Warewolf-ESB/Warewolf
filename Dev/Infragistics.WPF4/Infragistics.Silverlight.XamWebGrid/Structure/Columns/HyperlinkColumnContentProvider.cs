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
using System.Security;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// An object that provides content to <see cref="Cell"/> objects that belong to a <see cref="HyperlinkColumn"/>
	/// </summary>
	public class HyperlinkColumnContentProvider : ColumnContentProviderBase
	{
		#region Members
        

        Hyperlink _hyperlink;
        TextBlock _textBlock;
        Run _run;




		#endregion // Members

		#region Constructor

		/// <summary>
		/// Instantiates a new instance of the <see cref="HyperlinkColumnContentProvider"/>.
		/// </summary>
		public HyperlinkColumnContentProvider()
		{

            this._run = new Run();
            this._hyperlink = new Hyperlink(this._run);
            this._hyperlink.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(Hyperlink_RequestNavigate);
            this._textBlock = new TextBlock();
            this._textBlock.Inlines.Add(this._hyperlink);



		}             

		#endregion // Constructor

		#region Methods

		#region ResolveDisplayElement

		/// <summary>
		/// Sets up the element that will be displayed in a <see cref="Cell"/>, when the cell is not in edit mode. 
		/// </summary>
		/// <param propertyName="cell">The cell that the display element will be displayed in.</param>
		/// <param propertyName="cellBinding">A <see cref="Binding"/> object that can be applied to the cell. Note: this binding can be null.</param>
		/// <returns>The element that should be displayed.</returns>
		public override FrameworkElement ResolveDisplayElement(Cell cell, Binding cellBinding)
		{
			HyperlinkColumn column = (HyperlinkColumn)cell.Column;      


            


            if (cell.Row.RowType == RowType.FilterRow)
            {
                this._textBlock.IsHitTestVisible = false;
            }

            this._textBlock.Style = column.HyperlinkButtonStyle;

            this._hyperlink.TargetName = column.TargetName;

            this.ApplyBindingToDisplayElement(cell, cellBinding);

            return this._textBlock;


#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

        }

		#endregion // ResolveDisplayElement

        #region ApplyBindingToDisplayElement

        /// <summary>
        /// This is where a ColumnContentProvider should apply the Binding to their Display element.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="cellBinding"></param>
        private void ApplyBindingToDisplayElement(Cell cell, Binding cellBinding)
        {
            HyperlinkColumn column = (HyperlinkColumn)cell.Column;   

            if (column.Key != null)
            {
                if (cellBinding != null)
                    this._hyperlink.SetBinding(Hyperlink.NavigateUriProperty, cellBinding);

                Binding binding = new Binding(column.Key);
                if (column.Content == null)
                {
                    binding.Mode = BindingMode.OneWay;
                    if (column.ContentBinding != null)
                    {
                        this._run.SetBinding(Run.TextProperty, column.ContentBinding);
                    }
                    else
                    {
                        this._run.SetBinding(Run.TextProperty, binding);
                    }
                }
                else
                {
                    binding = new Binding("Content");
                    binding.Source = column;
                    this._run.SetBinding(Run.TextProperty, binding);
                }               
            }


#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

        }

        #endregion // ApplyBindingToDisplayElement

		#region ResolveEditorControl
		/// <summary>
		/// Sets up the edtior control that will be displayed in a <see cref="Cell"/> when the the cell is in edit mode.
		/// </summary>
		/// <param propertyName="cell">The <see cref="Cell"/> entering edit mode.</param>
		/// <param propertyName="editorValue">The value that should be put in the editor.</param>
		/// <param propertyName="availableWidth">The amount of horizontal space available.</param>
		/// <param propertyName="availableHeight">The amound of vertical space available.</param>
		/// <param propertyName="editorBinding">Provides a <see cref="Binding"/> that can be used for setting up the editor.</param>
		/// <returns></returns>
		protected override FrameworkElement ResolveEditorControl(Cell cell, object editorValue, double availableWidth, double availableHeight, Binding editorBinding)
		{
			return null;
		}
		#endregion // ResolveEditorControl

		#region ResolveValueFromEditor

		/// <summary>
		/// Resolves the value of the editor control, so that the cell's underlying data can be updated. 
		/// </summary>
		/// <param propertyName="cell">The <see cref="Cell"/> that the editor id being displayed in.</param>
		/// <returns>The value that should be displayed in the cell.</returns>
		public override object ResolveValueFromEditor(Cell cell)
		{
			return null;
		}

		#endregion // ResolveValueFromEditor

		#endregion // Methods

        #region EventHandlers


        void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            this.NavigateToLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        [SecuritySafeCritical]
        private void NavigateToLink(string uri)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(uri));
        }


        #endregion // EventHandlers
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