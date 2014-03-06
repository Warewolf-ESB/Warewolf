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
using System.Resources;

namespace Infragistics
{
    /// <summary>
    /// A resource class, for looking up localized strings, etc.
    /// </summary>
    public class DialogStringResources
    {
        #region Members
        string _notInDictionary;
        string _change;
        string _changeAll;
        string _ignore;
        string _ignoreAll;
        string _context;
        string _suggestions;
        string _useThisInstead;
        string _previousField;
        string _nextField;
        string _ok;
        string _addToDictionary;
        string _spellCheckComplete;
        string _loading;



        #endregion  //Members

        /// <summary>
        /// Not in dictionary string.
        /// </summary>
        public string NotInDictionary
        {
            get
            {
                if (_notInDictionary != null)
                    return this._notInDictionary;
                else
                    return SRSP.GetString("NotInDictionaryString");
            }

            set
            {
                this._notInDictionary = value;
            }
        }

        /// <summary>
        /// Context string.
        /// </summary>
        public string Context
        {
            get
            {
                if (_context != null)
                    return this._context;
                else
                    return SRSP.GetString("ContextString");
            }

            set
            {
                this._context = value;
            }
        }
        /// <summary>
        /// Loading string.
        /// </summary>
        public string Loading
        {
            get
            {
                if (_loading != null)
                    return this._loading;
                else
                    return SRSP.GetString("LoadingDictionaryString");
            }

            set
            {
                this._loading = value;
            }
        }

        /// <summary>
        /// Use this instead string.
        /// </summary>
        public string UseThisInstead
        {
            get
            {
                if (_useThisInstead != null)
                    return this._useThisInstead;
                else
                    return SRSP.GetString("UseThisInsteadString");
            }

            set
            {
                this._useThisInstead = value;
            }
        }

        /// <summary>
        /// Change string.
        /// </summary>
        public string Change
        {
            get
            {
                if (_change != null)
                    return this._change;
                else
                    return SRSP.GetString("ChangeButtonString");
            }

            set
            {
                this._change = value;
            }
        }

        /// <summary>
        /// Change all string.
        /// </summary>
        public string ChangeAll
        {
            get
            {
                if (_changeAll != null)
                    return this._changeAll;
                else
                    return SRSP.GetString("ChangeAllButtonString");
            }

            set
            {
                this._changeAll = value;
            }
        }

        /// <summary>
        /// Ignore string.
        /// </summary>
        public string Ignore
        {
            get
            {
                if (_ignore != null)
                    return this._ignore;
                else
                    return SRSP.GetString("IgnoreButtonString");
            }

            set
            {
                this._ignore = value;
            }
        }

        /// <summary>
        /// Ignore all string.
        /// </summary>
        public string IgnoreAll
        {
            get
            {
                if (_ignoreAll != null)
                    return this._ignoreAll;
                else
                    return SRSP.GetString("IgnoreAllButtonString");
            }

            set
            {
                this._ignoreAll = value;
            }
        }

        /// <summary>
        /// Suggestions string.
        /// </summary>
        public string Suggestions
        {
            get
            {
                if (_suggestions != null)
                    return this._suggestions;
                else
                    return SRSP.GetString("SuggestionsString");
            }

            set
            {
                this._suggestions = value;
            }
        }

        /// <summary>
        /// Previous field string.
        /// </summary>
        public string PreviousField
        {
            get
            {
                if (_previousField != null)
                    return this._previousField;
                else
                    return SRSP.GetString("PreviousFieldButtonString");
            }

            set
            {
                this._previousField = value;
            }
        }

        /// <summary>
        /// Next field string.
        /// </summary>
        public string NextField
        {
            get
            {
                if (_nextField != null)
                    return this._nextField;
                else
                    return SRSP.GetString("NextFieldButtonString");
            }

            set
            {
                this._nextField = value;
            }
        }

        /// <summary>
        /// Ok string.
        /// </summary>
        public string Ok
        {
            get
            {
                if (_ok != null)
                    return this._ok;
                else
                    return SRSP.GetString("OkButtonString");
            }

            set
            {
                this._ok = value;
            }
        }

        /// <summary>
        /// Add to dictionary string.
        /// </summary>
        public string AddToDictionary
        {
            get
            {
                if (_addToDictionary != null)
                    return this._addToDictionary;
                else
                    return SRSP.GetString("AddToDictionaryButtonString");
            }

            set
            {
                this._addToDictionary = value;
            }
        }

        /// <summary>
        /// Spellcheck complete string.,
        /// </summary>
        public string SpellCheckComplete
        {
            get
            {
                if (_spellCheckComplete != null)
                    return this._spellCheckComplete;
                else
                    return SRSP.GetString("SpellCheckCompleteString");
            }

            set
            {
                this._spellCheckComplete = value;
            }
            
        }
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