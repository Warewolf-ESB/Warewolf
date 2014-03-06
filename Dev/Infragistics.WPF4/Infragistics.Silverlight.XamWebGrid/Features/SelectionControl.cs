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
using System.Collections.ObjectModel;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.ComponentModel;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A base class for controls in the <see cref="XamGrid"/> which will allow selection of multiple entries.
	/// </summary>
	[TemplatePart(Name = "Panel", Type = typeof(Panel))]
	public abstract class SelectionControl : Control, ICommandTarget, INotifyPropertyChanged
	{
		#region Properties

		/// <summary>
		/// Gets / sets the Panel which will contain the controls do the action against.
		/// </summary>
		protected Panel Panel { get; set; }

		#region Cell

		/// <summary>
		/// Identifies the <see cref="Cell"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty CellProperty = DependencyProperty.Register("Cell", typeof(CellBase), typeof(SelectionControl), new PropertyMetadata(new PropertyChangedCallback(CellChanged)));

		/// <summary>
		/// Gets / sets the <see cref="CellBase"/> object which hosts the <see cref="SummarySelectionControl"/>.
		/// </summary>
		public CellBase Cell
		{
			get { return (CellBase)this.GetValue(CellProperty); }
			set { this.SetValue(CellProperty, value); }
		}

		private static void CellChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SelectionControl ssc = (SelectionControl)obj;
            ssc.OnCellAssigned();
		}

		#endregion // Cell

		#region CheckBoxStyle

		/// <summary>
		/// Identifies the <see cref="CheckBoxStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty CheckBoxStyleProperty = DependencyProperty.Register("CheckBoxStyle", typeof(Style), typeof(SelectionControl), new PropertyMetadata(new PropertyChangedCallback(CheckBoxStyleChanged)));

		/// <summary>
		/// Gets/Sets the style that will be applied to all CheckBoxes in this control.
		/// </summary>
		public Style CheckBoxStyle
		{
			get { return (Style)this.GetValue(CheckBoxStyleProperty); }
			set { this.SetValue(CheckBoxStyleProperty, value); }
		}

		private static void CheckBoxStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SelectionControl ssc = (SelectionControl)obj;
			ssc.SetupSelectionBox();
		}

		#endregion // CheckBoxStyle

		#endregion // Properties

		#region Overrides

		#region OnApplyTemplate
		/// <summary>
		/// Builds the visual tree for the <see cref="SelectionControl"/> when a new template is applied. 
		/// </summary>
		public override void OnApplyTemplate()
		{
			this.Panel = base.GetTemplateChild("Panel") as Panel;
		}
		#endregion // OnApplyTemplate

		#endregion // Overrides

		#region Methods

		#region SetupSelectionBox

		/// <summary>
		/// Used to set up the the controls which will go into the <see cref="Panel"/>.
		/// </summary>
		protected virtual void SetupSelectionBox()
		{
		}

		#endregion // SetupSelectionBox

		#region AcceptChanges

		/// <summary>
		/// Processes the elements in the <see cref="Panel"/>.
		/// </summary>
		protected internal virtual void AcceptChanges()
		{
		}

		#endregion // AcceptChanges

		#region SupportsCommand

		/// <summary>
		/// Returns if the object will support a given command type.
		/// </summary>
		/// <param propertyName="command">The command to be validated.</param>
		/// <returns>True if the object recognizes the command as actionable against it.</returns>
		protected virtual bool SupportsCommand(ICommand command)
		{
			return command is AcceptChangesCommand;
		}

		#endregion // SupportsCommand

		#region GetParameter

		/// <summary>
		/// Returns the object that defines the parameters necessary to execute the command.
		/// </summary>
		/// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
		/// <returns>The object necessary for the command to complete.</returns>
		protected virtual object GetParameter(CommandSource source)
		{			
			return this;
		}

		#endregion // GetParameter

        #region OnCellAssigned
        /// <summary>
        /// Raised when a Cell is assigned to the control.
        /// </summary>
        protected virtual void OnCellAssigned()
        {
        }
        #endregion // OnCellAsssigned

        #endregion // Methods

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return this.SupportsCommand(command);
		}

		object ICommandTarget.GetParameter(CommandSource source)
		{
			return this.GetParameter(source);
		}

		#endregion // ICommandTarget Members

		#region INotifyPropertyChanged Members
		/// <summary>
		/// Event raised when a property on this object changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises the PropertyChanged event.
		/// </summary>
		/// <param name="name"></param>
		protected virtual void OnPropertyChanged(string name)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion
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