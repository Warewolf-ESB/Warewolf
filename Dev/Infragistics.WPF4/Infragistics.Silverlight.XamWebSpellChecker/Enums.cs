using System;

namespace Infragistics.Controls.Interactions
{
    #region SuggestionMethod
    /// <summary>
    ///  The type of Suggestions that will be returned. 
    /// </summary>
    public enum SuggestionsMethod
    {
        /// <summary>
        /// Suggestions will be determined by hashing the word.
        /// </summary>
        HashingSuggestions,

        /// <summary>
        /// Suggestions will be determined phonetically.
        /// </summary>
        PhoneticSuggestions
    }
    #endregion

    #region Language Type
    /// <summary>Enumeration of language types.</summary>
    public enum LanguageType
    {
        /// <summary>The English language.</summary>
        English,

        /// <summary>The Dutch language.</summary>
        Dutch,

        /// <summary>The French language.</summary>
        French,

        /// <summary>The German language.</summary>
        German,

        /// <summary>The Italian language.</summary>
        Italian,

        /// <summary>The Portuguese language.</summary>
        Portuguese,

        /// <summary>The Spanish language.</summary>
        Spanish, 

        /// <summary>
        /// Defaults to the English language.
        /// </summary>
        NotSet,
    }

    #endregion

    #region SpellCheckingMode
    /// <summary>
    /// Whether the dialog will be modal. 
    /// </summary>
    public enum SpellCheckingMode
    {
        /// <summary>
        /// The dialog will be modal. 
        /// </summary>
        ModalDialog,

        /// <summary>
        /// The dialog will be modalless. 
        /// </summary>
        NonModalDialog
    }
    #endregion

    #region XamSpellCheckerDialogCommand
    /// <summary>
	/// An enumeration of available commands for the <see cref="XamSpellCheckerDialogWindow"/> object.
    /// </summary>
    public enum XamSpellCheckerDialogCommand
    {
        /// <summary>
        /// Ignores the current misspelled word.
        /// </summary>
        Ignore,

        /// <summary>
        /// Ignores the current misspelled word and continues to ignore it for future sessions.
        /// </summary>
        IgnoreAll,

        /// <summary>
		/// Changes the current misspelled word with the corrected version.
        /// </summary>
        Change,

        /// <summary>
		/// Replaces all instances of the current misspelled word with the corrected version.
        /// </summary>
        ChangeAll,

        /// <summary>
        /// Finishes the current spellcheck session.
        /// </summary>
        Done,

        /// <summary>
        /// Closes the spell checker dialog.
        /// </summary>
        CloseDialog,

        /// <summary>
        /// Closes the spellchecker dialog and passes true for the canceled parameter of the spellcheck complete event args.
        /// </summary>
        CancelDialog,

        /// <summary>
        /// Moves to the next editor to be spell checked.
        /// </summary>
        NextField,

        /// <summary>
        /// Moves to the previous spell check editor.
        /// </summary>
        PreviousField,

        /// <summary>
        /// Cancel the download process of the dictionaries
        /// </summary>
        Cancel,
        
        /// <summary>
        /// Adds the current bad word to the user dictionary.
        /// </summary>
        AddToDictionary

    }

    #endregion //XamSpellCheckerCommand

    #region XamSpellCheckerCommand
    /// <summary>
    /// An enumeration of available commands for the <see cref="XamSpellChecker"/> object.
    /// </summary>
    public enum XamSpellCheckerCommand
    {
        /// <summary>
        /// Begins a spell check operation.
        /// </summary>
        SpellCheck,

        /// <summary>
        /// Cancels the current asynchrnous dictionary download.
        /// </summary>
        CancelAsyncDictionaryDownload
    }

    #endregion //XamSpellCheckerCommand
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