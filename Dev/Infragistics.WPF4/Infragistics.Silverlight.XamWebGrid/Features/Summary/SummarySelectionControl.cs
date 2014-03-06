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
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A control which will allows the user to select which summaries should be applied to the column.
	/// </summary>
	public class SummarySelectionControl : SelectionControl
	{
		#region Constructor


        /// <summary>
        /// Static constructor for the <see cref="SummarySelectionControl"/> class.
        /// </summary>
        static SummarySelectionControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SummarySelectionControl), new FrameworkPropertyMetadata(typeof(SummarySelectionControl)));
        }


		/// <summary>
		/// Initializes a new instance of the <see cref="SummarySelectionControl"/> class.
		/// </summary>
		public SummarySelectionControl()
		{



			this.Loaded += new RoutedEventHandler(SummarySelectionControl_Loaded);
            this.IsTabStop = false;
            this.OKCaption = SRGrid.GetString("OKCaption");
            this.CancelCaption = SRGrid.GetString("CancelCaption");
		}

		#endregion // Constructor

		#region EventHandlers

		void SummarySelectionControl_Loaded(object sender, RoutedEventArgs e)
		{
			this.SetupSelectionBox();
		}

		#endregion // EventHandlers

		#region Overrides

		#region OnApplyTemplate
		/// <summary>
		/// Builds the visual tree for the <see cref="SummarySelectionControl"/> when a new template is applied. 
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.SetupSelectionBox();
		}
		#endregion // OnApplyTemplate

		#region AcceptChanges

		/// <summary>
		/// Processes the elements in the <see cref="Panel"/>.
		/// </summary>
		protected internal override void AcceptChanges()
		{
			CellBase cell = this.Cell;
			string columnKey = cell.Column.Key;

			SummaryDefinitionCollection sdc = ((RowsManager)cell.Row.Manager).SummaryDefinitionCollectionResolved;
			ReadOnlyCollection<SummaryDefinition> readOnlyCollection = sdc.GetDefinitionsByKey(columnKey);

			foreach (CheckBox cb in this.Panel.Children)
			{
				SummaryOperandBase searchForOperand = (SummaryOperandBase)cb.Tag;

				bool isChecked = (bool)cb.IsChecked;
				if (isChecked)
				{
					bool found = false;

					foreach (SummaryDefinition sd in readOnlyCollection)
					{
						if (sd.SummaryOperand == searchForOperand)
						{
							found = true;
							break;
						}
					}

					if (!found)
                    {                        
						sdc.Add(new SummaryDefinition() { ColumnKey = columnKey, SummaryOperand = searchForOperand });
                        searchForOperand.IsApplied = true;
					}
				}
				else
				{
					SummaryDefinition cachedSummaryDef = null;
					foreach (SummaryDefinition sd in readOnlyCollection)
					{
						if (sd.SummaryOperand == searchForOperand)
						{
							cachedSummaryDef = sd;
							break;
						}
					}
					if (cachedSummaryDef != null)
					{
						sdc.Remove(cachedSummaryDef);
                        cachedSummaryDef.SummaryOperand.IsApplied = false;
					}
				}
			}

			XamGrid grid = cell.Row.ColumnLayout.Grid;
			grid.ResetPanelRows();
			grid.InvalidateScrollPanel(false);
		}

		#endregion // AcceptChanges


		#region SetupSelectionBox

		/// <summary>
		/// Used to set up the the controls which will go into the <see cref="Panel"/>.
		/// </summary>
		protected override void SetupSelectionBox()
		{
			if (this.Panel != null && this.Cell != null && this.Cell.Column != null)
			{
				this.Panel.Children.Clear();

				SummaryOperandCollection soc = this.Cell.Column.SummaryColumnSettings.SummaryOperands;
				foreach (SummaryOperandBase sob in soc)
				{
					this.Panel.Children.Add(new CheckBox() { Content = sob.SelectionDisplayLabelResolved, Tag = sob, Style = this.CheckBoxStyle });
				}

				SummaryDefinitionCollection sdc = ((RowsManager)this.Cell.Row.Manager).SummaryDefinitionCollectionResolved;
				string columnKey = this.Cell.Column.Key;
				foreach (SummaryDefinition sd in sdc)
				{
					if (sd.ColumnKey == columnKey)
					{
						int count = this.Panel.Children.Count;
						for (int i = 0; i < count; i++)
						{
							CheckBox cb = (CheckBox)this.Panel.Children[i];
							cb.Style = this.CheckBoxStyle;
							SummaryOperandBase sob = (SummaryOperandBase)cb.Tag;
							if (sob == sd.SummaryOperand)
							{
								cb.IsChecked = true;
								break;
							}
						}
					}
				}
			}
		}

		#endregion // SetupSelectionBox

		#endregion // Overrides

        #region Properties

        #region OKCaption

        /// <summary>
        /// Identifies the <see cref="OKCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty OKCaptionProperty = DependencyProperty.Register("OKCaption", typeof(string), typeof(SummarySelectionControl), new PropertyMetadata(new PropertyChangedCallback(OKCaptionChanged)));

        /// <summary>
        /// Gets / sets the text that will be displayed on the accept button
        /// </summary>
        public string OKCaption
        {
            get { return (string)this.GetValue(OKCaptionProperty); }
            set { this.SetValue(OKCaptionProperty, value); }
        }

        private static void OKCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            SummarySelectionControl ssc = (SummarySelectionControl)obj;
            ssc.OnPropertyChanged("OKCaption");
        }

        #endregion // OKCaption 
				
        #region CancelCaption

        /// <summary>
        /// Identifies the <see cref="CancelCaption"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CancelCaptionProperty = DependencyProperty.Register("CancelCaption", typeof(string), typeof(SummarySelectionControl), new PropertyMetadata(new PropertyChangedCallback(CancelCaptionChanged)));


        /// <summary>
        /// Gets / set the text that will be displayed on the cancel button.
        /// </summary>
        public string CancelCaption
        {
            get { return (string)this.GetValue(CancelCaptionProperty); }
            set { this.SetValue(CancelCaptionProperty, value); }
        }

        private static void CancelCaptionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            SummarySelectionControl ssc = (SummarySelectionControl)obj;
            ssc.OnPropertyChanged("CancelCaption");
        }

        #endregion // CancelCaption 
				

        #endregion // Properties
    }
}

namespace Infragistics.Controls.Grids
{
	#region SummarySelectionControlCommandSource
	/// <summary>
	/// The command source object for <see cref="SummarySelectionControl"/>.
	/// </summary>
	public class SummarySelectionControlCommandSource : CommandSource
	{
		/// <summary>
		/// Gets / sets the <see cref="SummarySelectionControlCommand"/> which is to be executed by the command.
		/// </summary>
		public SummarySelectionControlCommand CommandType
		{
			get;
			set;
		}

		/// <summary>
		/// Generates the <see cref="ICommand"/> object that will execute the command.
		/// </summary>
		/// <returns></returns>
		protected override ICommand ResolveCommand()
		{
			ICommand command = null;
			switch (this.CommandType)
			{
				case SummarySelectionControlCommand.AcceptChanges:
					{
						command = new AcceptChangesCommand();
						break;
					}
			}
			return command;
		}
	}
	#endregion // SummarySelectionControlCommandSource

	#region SummarySelectionControlCommand
	/// <summary>
	/// An enum describing the commands which can be executed on the <see cref="SummarySelectionControlCommandSource"/>
	/// </summary>
	public enum SummarySelectionControlCommand
	{
		/// <summary>
		/// Accepts the changes from the control.
		/// </summary>
		AcceptChanges
	}
	#endregion // SummarySelectionControlCommand

	#region AcceptChangesCommand
	/// <summary>
	/// A command which will accept changes made to the selected items of this control.
	/// </summary>
	public class AcceptChangesCommand : CommandBase
	{
		#region CanExecute
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param propertyName="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			return true;
		}
		#endregion // CanExecute

		#region Execute
		/// <summary>
		/// Executes the command 
		/// </summary>
		/// <param name="parameter"></param>
		public override void Execute(object parameter)
		{
			SelectionControl ssc = parameter as SelectionControl;

			if (ssc != null)
			{
				ssc.AcceptChanges();
				this.CommandSource.Handled = true;
			}

			base.Execute(parameter);

		}
		#endregion // Execute
	}
	#endregion // AcceptChangesCommand
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