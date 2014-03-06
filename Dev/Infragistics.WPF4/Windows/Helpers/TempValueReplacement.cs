using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Infragistics.Windows.Helpers
{
	// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
	/// <summary>
	/// Helper class for storing a local value of a dependency property while temporarily overriding it with a new value.
	/// </summary>
	internal class TempValueReplacement : IDisposable
	{
		#region Member Variables

		private object _originalValue;
		private DependencyProperty _property;
		private DependencyObject _object;

		// AS 11/23/09 TFS24836
		private BindingExpressionBase _originalExpression;

		#endregion //Member Variables

		#region Constructor
        internal TempValueReplacement(DependencyObject d, DependencyProperty property) :
            this(d, property, d != null ? d.GetValue(property) : null)
        {
        }

        internal TempValueReplacement(DependencyObject d, DependencyProperty property, object newValue)
		{
			this._object = d;
			this._property = property;

			if (null != d)
			{
				this._originalValue = d.ReadLocalValue(property);

				// AS 11/23/09 TFS24836
				_originalExpression = BindingOperations.GetBindingExpressionBase(d, property);

				d.SetValue(property, newValue);
			}
		}
		#endregion //Constructor

		#region IDisposable Members

		public void Dispose()
		{
			if (null != this._object)
			{
				RestoreValue(this._object, this._property, this._originalValue, _originalExpression );
			}
		}

		#endregion

		#region RestoreValue
		private static void RestoreValue(DependencyObject d, DependencyProperty property, object value, BindingExpressionBase originalExpression )
		{
			// AS 11/23/09 TFS24836
			// The value could have been an expression. Basically if the original value came 
			// from a binding then we need to restore the binding even if the property is of 
			// type object.
			//
			if (null != originalExpression)
				BindingOperations.SetBinding(d, property, originalExpression.ParentBindingBase);
			else
			if (value == null || property.PropertyType.IsAssignableFrom(value.GetType()))
				d.SetValue(property, value);
			else if (value is BindingBase)
				BindingOperations.SetBinding(d, property, (BindingBase)value);
			else
				d.ClearValue(property);
		}
		#endregion //RestoreValue
	}

    internal class GroupTempValueReplacement : IDisposable
    {
        #region Member Variables

        private List<TempValueReplacement> _replacements;

        #endregion //Member Variables

        #region Constructor
        internal GroupTempValueReplacement()
        {
            this._replacements = new List<TempValueReplacement>();
        } 
        #endregion //Constructor

        #region Methods

        internal void Add(TempValueReplacement replacement)
        {
            if (null == replacement)
                throw new ArgumentNullException("replacement");

            this._replacements.Add(replacement);
        }

        internal void Add(FrameworkElement element, DependencyProperty property)
        {
            this._replacements.Add(new TempValueReplacement(element, property));
        }

        internal void Add(FrameworkElement element, DependencyProperty property, object newValue)
        {
            this._replacements.Add(new TempValueReplacement(element, property, newValue));
        }

        #region IDisposable Members

        public void Dispose()
        {
            for (int i = 0, count = this._replacements.Count; i < count; i++)
                this._replacements[i].Dispose();
        }

        #endregion //IDisposable Members

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