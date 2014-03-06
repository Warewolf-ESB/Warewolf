using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that describes how the rows should be filtered.
	/// </summary>
	public class RowsFilter : IRecordFilter, IDisposable
	{
		#region Members
		ConditionCollection _conditions;
        Type _objectType;
        Column _column;
		#endregion

		#region Properties

		#region Public

		#region Conditions
		/// <summary>
		/// Gets the collection of filter conditions which will be applied to this column
		/// </summary>
        [Browsable(false)]
		public ConditionCollection Conditions
		{
			get
			{
				if (this._conditions == null)
				{
					this._conditions = new ConditionCollection();
					this._conditions.CollectionItemChanged += new EventHandler<EventArgs>(Conditions_CollectionItemChanged);
					this._conditions.CollectionChanged += new NotifyCollectionChangedEventHandler(Conditions_CollectionChanged);
					this._conditions.PropertyChanged += new PropertyChangedEventHandler(Conditions_PropertyChanged);
				}

				if (this._conditions.Parent != this)
					this._conditions.Parent = this;

				return this._conditions;
			}
		}
		#endregion // Conditions

		#region ObjectType

		/// <summary>
		/// Gets the Type of the object that is being processed.
		/// </summary>
		public Type ObjectType 
        {
            get { return this._objectType; }
            set
            {
                if (this._objectType == null)
                {
                    this._objectType = value;
                }
            }
        }

		#endregion // ObjectType

        #region ObjectTypedInfo

        /// <summary>
        /// The Type of the object along with any PropertyDescriptors.
        /// </summary>
        public CachedTypedInfo ObjectTypedInfo
        {
            get;
            set;
        }

		#endregion // ObjectType

		#region Column
		/// <summary>
		/// Gets the <see cref="Column"/> which is being processed on.
		/// </summary>
        public Column Column
        {
            get { return this._column; }
            set
            {
                if (this._column == null)
                {
                    this._column = value;
                }
            }
        }

		#endregion // Column

		#region FieldName

		/// <summary>
		/// Gets the field name of the property that is being filtered on.
		/// </summary>
		public virtual string FieldName
		{
			get
			{
				string fieldName = null;
				if (this.Column != null)
				{
					fieldName = this.Column.Key;
				}
				return fieldName;
			}
		}

		#endregion // FieldName

		#region FieldType

		/// <summary>
		/// Gets the Type of the FieldName property.
		/// </summary>
		public virtual Type FieldType
		{
			get
			{
				Type type = null;
				if (this.Column != null)
				{
					type = this.Column.DataType;
				}
				return type;
			}
		}
		#endregion // FieldType

		#endregion // Public

		#endregion // Properties

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="RowsFilter"/> class.
		/// </summary>
		/// <param propertyName="column">The <see cref="Column"/> object these filters will be applied to.</param>
		/// <param propertyName="objectType">The type of the object being processed.</param>
		public RowsFilter(Type objectType, Column column)
		{
			this.ObjectType = objectType;
			this.Column = column;

            if (this.Column != null && this.Column.ColumnLayout != null)
                this.ObjectTypedInfo = this.Column.ColumnLayout.ObjectDataTypeInfo;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="RowsFilter"/> class.
        /// </summary>
        public RowsFilter()
        {

        }

		#endregion

		#region GetCurrentExpression

		/// <summary>
		/// Generates the current expression for this <see cref="RowsFilter"/>.
		/// </summary>
		protected virtual Expression GetCurrentExpression()
		{
			Expression exp = null;

			if (this.Column != null && this.Column.ColumnLayout != null)
			{
                this.ObjectTypedInfo = this.Column.ColumnLayout.ObjectDataTypeInfo;

				FilterContext context = FilterContext.CreateGenericFilter(this.Column.ColumnLayout.ObjectDataTypeInfo, 
					this.Column.DataType, 
					this.Column.FilterColumnSettings.FilterCaseSensitive,
					(this.Column is DateColumn && this.Column.ColumnLayout.FilteringSettings.AllowFilteringResolved != FilterUIType.FilterMenu)
					);

				exp = GetCurrentExpression(context);
			}

			return exp;
		}

		/// <summary>
		/// Generates the current expression for this <see cref="RowsFilter"/> using the inputted context.
		/// </summary>
		protected virtual Expression GetCurrentExpression(FilterContext context)
		{
            if (this.Column != null && this.Column.ColumnLayout != null)
                this.ObjectTypedInfo = this.Column.ColumnLayout.ObjectDataTypeInfo;

			return context.CreateExpression(this.Conditions);
		}

		#endregion // GetCurrentExpression

		#region IRecordFilter Members

		string IRecordFilter.FieldName
		{
			get
			{
				return this.FieldName;
			}
		}

		ConditionCollection IRecordFilter.Conditions
		{
			get { return this.Conditions; }
		}

		Type IRecordFilter.ObjectType
		{
			get
			{
				return this.ObjectType;
			}
		}

		Type IRecordFilter.FieldType
		{
			get
			{
				return this.FieldType;
			}
		}

        CachedTypedInfo IRecordFilter.ObjectTypedInfo
        {
            get
            {
                return this.ObjectTypedInfo;
            }
        }

		#endregion

		#region IExpressConditions Members

		Expression IExpressConditions.GetCurrentExpression(FilterContext context)
		{
			return this.GetCurrentExpression(context);
		}

		Expression IExpressConditions.GetCurrentExpression()
		{
			return this.GetCurrentExpression();
		}

		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raises the <see cref="PropertyChanged"/> event on this object.
		/// </summary>
		/// <param propertyName="propertyName">The propertyName of the property changed.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Fired when a property changes on the <see cref="RowsFilter"/>.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region IDisposable Members

		/// <summary>
        /// Releases the unmanaged resources used by the <see cref="Infragistics.Collections.CollectionBase&lt;T&gt;"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param propertyName="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			if (this._conditions != null)
			{
				this._conditions.CollectionItemChanged -= Conditions_CollectionItemChanged;
				this._conditions.CollectionChanged -= Conditions_CollectionChanged;
				this._conditions.PropertyChanged -= Conditions_PropertyChanged;
				this._conditions.Dispose();
				this._conditions.Parent = null;
				this._conditions = null;
			}
		}

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="RowsFilter"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region EventHandlers
		void Conditions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			this.OnPropertyChanged("Conditions");
		}

		void Conditions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.OnPropertyChanged("ConditionsChanged");
		}

		void Conditions_CollectionItemChanged(object sender, EventArgs e)
		{
			this.OnPropertyChanged("ConditionsItemChanged");
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