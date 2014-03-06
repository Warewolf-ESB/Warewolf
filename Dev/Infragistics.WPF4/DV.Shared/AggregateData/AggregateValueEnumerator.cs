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
using System.Collections;

namespace Infragistics.Collections
{
    internal class AggregateValueEnumerator : IEnumerator
    {
        #region Constructor
        public AggregateValueEnumerator(AggregateValueCollection parent)
            : base()
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            this.Parent = parent;
        }
        #endregion //Constructor

        #region Properties

        #region Private 
        
        #region Parent
        private AggregateValueCollection Parent { get; set; }
        #endregion //Parent

        #region CurrentValueSourceIndex
        private int CurrentValueSourceIndex { get; set; }
        #endregion //CurrentValueSourceIndex

        #region CurrentValueSourceEnumerator
        private IEnumerator CurrentValueSourceEnumerator { get; set; }
        #endregion //CurrentValueSourceEnumerator

        #endregion //Private

        #endregion //Properties

        #region IEnumerator Members

        #region Current
        public object Current
        {
            get
            {
                return this.CurrentValueSourceEnumerator == null ? null : this.CurrentValueSourceEnumerator.Current;
            }
        }
        #endregion //Current

        #region MoveNext
        public bool MoveNext()
        {
            if (this.Parent.ItemsSources == null)
            {
                return false;
            }
            if (this.CurrentValueSourceEnumerator != null)
            {
                bool hasNext = this.CurrentValueSourceEnumerator.MoveNext();
                if (hasNext)
                {
                    return true;
                }
                else
                {
                    this.CurrentValueSourceIndex++;
                }
            }
            if (this.Parent.ItemsSources.Count > this.CurrentValueSourceIndex)
            {
                AggregateValueSource currentValueSource = this.Parent.ItemsSources[this.CurrentValueSourceIndex];
                if (currentValueSource.ValueColumn != null)
                {
                    this.CurrentValueSourceEnumerator = currentValueSource.ValueColumn.GetEnumerator();
                    return this.MoveNext();
                }
            }
            return false;
        }
        #endregion //MoveNext

        #region Reset
        public void Reset()
        {
            this.CurrentValueSourceEnumerator = null;
            this.CurrentValueSourceIndex = 0;
        }
        #endregion //Reset

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