using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;




namespace Infragistics.Controls.Editors

{
    /// <summary>
    /// An object that describes how the items should be filtered.
    /// </summary>
    public class ItemsFilter : IRecordFilter, IDisposable
    {
        #region Members

        private ConditionCollection _conditions;
        
        #endregion

        #region Properties

        #region Public

        #region Conditions
        /// <summary>
        /// Gets the collection of filter conditions which will be applied to this column
        /// </summary>
        public ConditionCollection Conditions
        {
            get
            {
                if (this._conditions == null)
                {
                    this._conditions = new ConditionCollection();
                    this._conditions.CollectionItemChanged += this.Conditions_CollectionItemChanged;
                    this._conditions.CollectionChanged += this.Conditions_CollectionChanged;
                    this._conditions.PropertyChanged += this.Conditions_PropertyChanged;
                }

                if (this._conditions.Parent != this)
                {
                    this._conditions.Parent = this;
                }

                return this._conditions;
            }
        }
        #endregion // Conditions

        #region ObjectType

        /// <summary>
        /// Gets or sets the Type of the object that is being processed.
        /// </summary>
        public Type ObjectType { get; protected internal set; }

        #endregion // ObjectType

        #region FieldName

        /// <summary>
        /// Gets or sets the field name of the property that is being filtered on.
        /// </summary>
        public string FieldName
        {
            get;
            set;
        }

        #endregion // FieldName

        #region FieldType

        /// <summary>
        /// Gets or sets the Type of the FieldName property.
        /// </summary>
        public Type FieldType
        {
            get
            {
                if (this.Field != null)
                    return this.Field.FieldType;

                Type fieldType = null;

                if (this.ObjectType != null && this.FieldName != null)
                {
                    bool isDisplayMemberPathValid = false;

                    try
                    {
                        System.Linq.Expressions.Expression expression =
                            DataManagerBase.BuildPropertyExpressionFromPropertyName(
                                this.FieldName,
                                System.Linq.Expressions.Expression.Parameter(this.ObjectType, "param"));

                        if (expression != null)
                        {
                            fieldType = expression.Type;
                            isDisplayMemberPathValid = true;
                        }
                    }
                    catch
                    {
                        // probably an illegal type, or the indexer is of type object and they want to access a property off of it. 
                    }

                    if (!isDisplayMemberPathValid)
                    {

                        throw new Exception(
                            string.Format(
                                SRCombo.GetString("InvalidDisplayMember"),
                                this.FieldName,
                                this.ObjectType.Name));



                    }
                }

                return fieldType;
            }
        }

        #endregion // FieldType

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

        #endregion // Public

        #region Protected

        #region DataField

        /// <summary>
        /// Gets/Sets the DataField associated with the ItemsFilter.
        /// </summary>
        protected internal DataField Field
        {
            get;
            set;
        }

        #endregion // DataField

        #endregion // Protected

        #endregion // Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsFilter"/> class.
        /// </summary>
        public ItemsFilter()
        {
        }

        #endregion

        #region GetCurrentExpression

        /// <summary>
        /// Generates the current expression for this <see cref="ItemsFilter"/>.
        /// </summary>
        /// <returns>The new expression.</returns>
        protected virtual Expression GetCurrentExpression()
        {
            Expression exp = null;

            if (this.ObjectTypedInfo != null && this.FieldType != null)
            {
                FilterContext context = FilterContext.CreateGenericFilter(this.ObjectTypedInfo, this.FieldType, false, false);

                exp = this.GetCurrentExpression(context);
            }

            return exp;
        }

        /// <summary>
        /// Generates the current expression for this <see cref="ItemsFilter"/> using the inputted context.
        /// </summary>
        /// <param name="context">Context parameter</param>
        /// <returns>The new expression.</returns>
        protected virtual Expression GetCurrentExpression(FilterContext context)
        {
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
        /// <param name="propertyName">The propertyName of the property changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Fired when a property changes on the <see cref="ItemsFilter"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Infragistics.Collections.CollectionBase&lt;T&gt;"/> and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._conditions != null)
            {
                this._conditions.CollectionItemChanged -= this.Conditions_CollectionItemChanged;
                this._conditions.CollectionChanged -= this.Conditions_CollectionChanged;
                this._conditions.PropertyChanged -= this.Conditions_PropertyChanged;
                this._conditions.Dispose();
                this._conditions = null;
            }
        }

        /// <summary>
        /// Releases the unmanaged and managed resources used by the <see cref="ItemsFilter"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region EventHandlers

        private void Conditions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.OnPropertyChanged("Conditions");
        }

        private void Conditions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged("ConditionsChanged");
        }

        private void Conditions_CollectionItemChanged(object sender, EventArgs e)
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