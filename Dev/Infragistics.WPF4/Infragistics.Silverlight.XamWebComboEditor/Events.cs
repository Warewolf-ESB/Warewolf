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
using Infragistics.Controls.Editors;

namespace Infragistics.Controls.Editors
{
    #region ComboItemAddingEventArgs

    /// <summary>
    /// An object used in the ItemAdding event of the ComboEditorBase
    /// </summary>
    public class ComboItemAddingEventArgs<T> : CancellableEventArgs
    {
        /// <summary>
        /// Creates a new instance of the ComboItemAddingEventArgs object.
        /// </summary>
        /// <param name="item"></param>
        public ComboItemAddingEventArgs(T item)
        {
            this.Item = item;
        }

        /// <summary>
        /// The item being added to the ComboEditorBase dropdown.
        /// </summary>
        public T Item
        {
            get;
            private set;
        }
    }
    #endregion // ComboItemAddingEventArgs

    #region ComboItemAddedEventArgs
    /// <summary>
    /// An object used in the ItemAdded event of the ComboEditorBase
    /// </summary>
    public class ComboItemAddedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the ComboItemAddedEventArgs object.
        /// </summary>
        /// <param name="item"></param>
        public ComboItemAddedEventArgs(T item)
        {
            this.Item = item;
        }

        /// <summary>
        /// The item that was added to the ComboEditorBase dropdown.
        /// </summary>
        public T Item
        {
            get;
            private set;
        }
    }
    #endregion // ComboItemAddedEventArgs

    #region ValidateInputTextEventArgs
    /// <summary>
    /// An object used in the SpecializedTextBox ValidateInputText event of the <see cref="Infragistics.Controls.Editors.Primitives.SpecializedTextBox"/>
    /// </summary>
    internal class ValidateInputTextEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ValidateInputTextEventArgs"/> object.
        /// </summary>
        /// <param name="text"></param>
        public ValidateInputTextEventArgs(string text)
        {
            this.IsValid = true;
            this.TextToValidate = text;
        }

        /// <summary>
        /// The text to validate.
        /// </summary>
        public string TextToValidate
        {
            get;
            private set;
        }

        /// <summary>
        /// Where or not the text is valid.
        /// </summary>
        public bool IsValid
        {
            get;
            set;
        }
    }
    #endregion // ValidateInputTextEventArgs
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