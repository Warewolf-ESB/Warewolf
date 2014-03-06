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
using System.ComponentModel;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that controls summary settings for a particular <see cref="Column"/> object.
	/// </summary>
	public class SummaryColumnSettings : ColumnSettings, IDisposable
	{
		#region Members

		SummaryOperandCollection _summaryOperands;
        SummaryDefinitionCollection _groupBySummaryDefinitions;

		SummaryOperandCollection _summaryOperandsFromXaml = new SummaryOperandCollection();

		bool _autopopulatedSummaryOperands;

		#endregion // Members

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryColumnSettings"/> class.
		/// </summary>
		/// <param name="column">The <see cref="Column"/> object which this settings object will be associated.</param>
		public SummaryColumnSettings(Column column)
		{
			base.Column = column;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryColumnSettings"/> class.
		/// </summary>
		public SummaryColumnSettings()
		{

		}

		#endregion // Constructor

		#region Properties

		#region SummaryOperands

		/// <summary>
		/// The <see cref="SummaryOperandCollection"/> which will be allowed to be selected by the user.
		/// </summary>
        [Browsable(false)]
		public SummaryOperandCollection SummaryOperands
		{
			get
			{
				if (this._summaryOperands == null)
				{
					this._summaryOperands = new SummaryOperandCollection();

					this._summaryOperands.CollectionChanged += SummaryOperands_CollectionChanged;

					if (this.Column != null)
					{
						this.Column.FillAvailableSummaries(this._summaryOperands);
						this._autopopulatedSummaryOperands = this._summaryOperands.Count > 0;
					}
				}
				return this._summaryOperands;
			}
			set
			{
				if (this._summaryOperands != value)
				{
					if (this._summaryOperands != null)
						this._summaryOperands.CollectionChanged -= SummaryOperands_CollectionChanged;

					this._summaryOperands = value;

					if (value != null)
						this._summaryOperands.CollectionChanged += SummaryOperands_CollectionChanged;
				}
			}
		}

		#endregion // SummaryOperands

        #region GroupBySummaryOperands

        /// <summary>
        /// The SummaryDefinitions that will be applied when this particular column is grouped.
        /// </summary>
        public SummaryDefinitionCollection GroupBySummaryDefinitions
        {
            get
            {
                if (this._groupBySummaryDefinitions == null)
                {
                    this._groupBySummaryDefinitions = new SummaryDefinitionCollection();
                    this._groupBySummaryDefinitions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(GroupBySummaryDefinitions_CollectionChanged);
                }
                return this._groupBySummaryDefinitions;
            }           
        }      

        #endregion // GroupBySummaryOperands

        #region SummaryRowCellStyle

        /// <summary>
        /// Identifies the <see cref="SummaryRowCellStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SummaryRowCellStyleProperty = DependencyProperty.Register("SummaryRowCellStyle", typeof(Style), typeof(SummaryColumnSettings), new PropertyMetadata(new PropertyChangedCallback(SummaryRowCellStyleChanged)));

        /// <summary>
        /// Gets/Sets the <see cref="Style"/> that will be used for the <see cref="CellControl"/> objects on the <see cref="Infragistics.Controls.Grids.Primitives.SummaryRow"/> for this <see cref="ColumnBase"/>.
        /// </summary>        
        /// <remarks>
        /// This style should target a <see cref="Infragistics.Controls.Grids.Primitives.SummaryRowCellControl"/>.
        /// </remarks>
        public Style SummaryRowCellStyle
        {
            get { return (Style)this.GetValue(SummaryRowCellStyleProperty); }
            set { this.SetValue(SummaryRowCellStyleProperty, value); }
        }

        private static void SummaryRowCellStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            SummaryColumnSettings ctrl = (SummaryColumnSettings)obj;
            if (ctrl.Column != null)
            {
                ctrl.Column.OnStyleChanged();
            }
            ctrl.OnPropertyChanged("SummaryRowCellStyle");
        }

        #endregion // SummaryRowCellStyle 

		#endregion // Properties

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="SummaryColumnSettings"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="SummaryColumnSettings"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param name="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._summaryOperands != null)
			{
				this._summaryOperands.Dispose();
				this._summaryOperands = null;
			}

			if (this._summaryOperandsFromXaml != null)
			{
				this._summaryOperandsFromXaml.Dispose();
				this._summaryOperandsFromXaml = null;
			}
		}

		#endregion

		#region Methods

		#region Protected

		#region OnLoaded_CatchUp

		/// <summary>
		/// Designed to be called during the <see cref="XamGrid"/> OnLoaded to allow any processing 
		/// due to objects added in the XAML but not able to be processed until other objects populated.
		/// </summary>
		protected internal override void OnLoadedCatchUp()
		{
			if (this._summaryOperandsFromXaml != null && this._summaryOperandsFromXaml.Count > 0)
			{
				foreach (SummaryOperandBase soe in _summaryOperandsFromXaml)
				{
					this.ModifySummaryDefinitionCollectionFromOnApplied(soe);
				}
			}
		}

		#endregion //OnLoaded_CatchUp

		#region ValidateSummaryOperands

		/// <summary>
        /// Called to ensure that the operand list that is set, if it was autogenerated, is correct with respect to the <see cref="Infragistics.Controls.Grids.ColumnBase.DataType"/>.
		/// </summary>
		protected internal void ValidateSummaryOperands()
		{
			if (this._autopopulatedSummaryOperands && this.Column != null)
			{
				this._autopopulatedSummaryOperands = true;
				this._summaryOperands.Clear();
				this.Column.FillAvailableSummaries(this._summaryOperands);
			}
		}

		#endregion // ValidateFilterOperands

		#endregion // Protected

		#region Private

		private void AddSummaryDefinition(SummaryRowSettingsOverride srso, SummaryOperandBase sob)
		{
			if (srso.ColumnLayout != null && srso.ColumnLayout.Grid != null && srso.SummaryScopeResolved == SummaryScope.ColumnLayout)
			{
				bool found = false;
				foreach (SummaryDefinition sd in srso.SummaryDefinitionCollection)
				{
					if (sd.SummaryOperand == sob)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					if (srso.ColumnLayout.Grid.IsLoaded)
						srso.SummaryDefinitionCollection.Add(new SummaryDefinition() { SummaryOperand = sob, ColumnKey = this.Column.Key });
					else
						srso.SummaryDefinitionCollection.AddItemSilently(new SummaryDefinition() { SummaryOperand = sob, ColumnKey = this.Column.Key });
				}
			}
		}

		private static void RemoveSummaryDefinition(SummaryRowSettingsOverride srso, SummaryOperandBase sob)
		{
			if (srso.ColumnLayout != null && srso.ColumnLayout.Grid != null && srso.SummaryScopeResolved == SummaryScope.ColumnLayout)
			{
				SummaryDefinition foundSummaryDef = null;

				foreach (SummaryDefinition sd in srso.SummaryDefinitionCollection)
				{
					if (sd.SummaryOperand == sob)
					{
						foundSummaryDef = sd;
						break;
					}
				}

				if (foundSummaryDef != null)
				{
					srso.SummaryDefinitionCollection.Remove(foundSummaryDef);
				}
			}
		}

		private void ModifySummaryDefinitionCollectionFromOnApplied(SummaryOperandBase sob)
		{
			bool fromXaml = this.Column == null || this.Column.ColumnLayout == null;

			if (!fromXaml)
			{
				SummaryRowSettingsOverride srso = this.Column.ColumnLayout.SummaryRowSettings;
				if (sob.IsApplied)
				{
					this.AddSummaryDefinition(srso, sob);
				}
				else
				{
					SummaryColumnSettings.RemoveSummaryDefinition(srso, sob);
				}
			}
			else
			{
				if (sob.IsApplied)
					_summaryOperandsFromXaml.Add(sob);
				else
                {
                    if (_summaryOperandsFromXaml.Contains(sob))
                        _summaryOperandsFromXaml.Remove(sob);
                }
			}
		}

		#endregion // Private

        #region Internal

        internal void RepopulateSummaryDefinitionCollection()
        {
            SummaryRowSettingsOverride srso = this.Column.ColumnLayout.SummaryRowSettings;
            foreach (SummaryOperandBase sob in this.SummaryOperands)
            {
                if (sob.IsApplied)
                {
                    this.AddSummaryDefinition(srso, sob);
                }
            }
        }

        #endregion // Internal

        #endregion // Methods

        #region Event Handlers

        void SummaryOperands_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
			{
				foreach (object o in e.NewItems)
				{
					SummaryOperandBase sob = (SummaryOperandBase)o;

					sob.PropertyChanged += SummaryOperandBase_PropertyChanged;

					this.ModifySummaryDefinitionCollectionFromOnApplied(sob);
				}
			}
			else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
			{
				foreach (object o in e.OldItems)
				{
					SummaryOperandBase sob = (SummaryOperandBase)o;

					sob.PropertyChanged -= SummaryOperandBase_PropertyChanged;

					SummaryColumnSettings.RemoveSummaryDefinition(this.Column.ColumnLayout.SummaryRowSettings, sob);
				}
			}
		}

		void SummaryOperandBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			SummaryOperandBase sob = (SummaryOperandBase)sender;

			if (e.PropertyName == "IsApplied")
			{
				this.ModifySummaryDefinitionCollectionFromOnApplied(sob);
			}
			else
			{
				base.OnPropertyChanged(e.PropertyName);
			}
		}

        void GroupBySummaryDefinitions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this.Column != null && this.Column.IsGroupBy && this.Column.ColumnLayout != null && this.Column.ColumnLayout.Grid != null)
            {
                if (this.Column.ColumnLayout.Grid.IsLoaded)
                {
                    this.Column.ColumnLayout.Grid.InvalidateData();
                }
            }
                
        }

		#endregion // Event Handlers
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