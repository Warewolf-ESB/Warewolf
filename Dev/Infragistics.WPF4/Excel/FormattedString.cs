using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	// MD 4/12/11 - TFS67084
	// Refactored a lot of this code and removed the FormattedStringProxy class. Now the FormattedString will hold the elements directly.
	#region Old Code

	//    /// <summary>
	//    /// Represents a string with mixed formatting in a cell, cell comment, or shape.
	//    /// </summary>
	//    /// <remarks>
	//    /// <p class="body">
	//    /// The formatting of the string is controlled in a similar fashion as it would be in Microsoft Excel. In Excel, the user
	//    /// must select a portion of the text and set the various formatting properties of that selected text. 
	//    /// </p>
	//    /// <p class="body">
	//    /// With the FormattedString, a portion of the string is "selected" by calling either <see cref="GetFont(int)">GetFont(int)</see> or 
	//    /// <see cref="GetFont(int,int)">GetFont(int,int)</see>. Formatting properties are then set on the returned 
	//    /// <see cref="FormattedStringFont"/> and all characters in the font's selection range are given these properties.
	//    /// </p>
	//    /// <p class="body">
	//    /// Getting the formatting properties of a <see cref="FormattedStringFont"/> will return the formatting of the first 
	//    /// character in font's selection range. This is similar to Excel, which will update the formatting interface to 
	//    /// reflect the formatting of the first character in a selection range when a cell's text is selected.
	//    /// </p>
	//    /// </remarks>
	//    // MD 11/3/10 - TFS49093
	//    // Moved the unformattedString member variable to the new FormattedStringElement class.
	//    //[DebuggerDisplay( "{unformattedString}" )]
	//    [DebuggerDisplay("{UnformattedString}")]
	//    public class FormattedString : 
	//        IWorksheetCellOwnedValue,
	//#if !SILVERLIGHT
	//        ICloneable,
	//#endif
	//        IComparable<FormattedString> // MD 6/11/07 - BR23706
	//    {
	//        #region Member Variables

	//        // MD 11/3/10 - TFS49093
	//        // Moved these member variables to the new FormattedStringElement class.
	//        //private List<FormattedStringRun> formattingRuns;
	//        //private string unformattedString;

	//        // MD 9/2/08 - Cell Comments
	//        // Formatted strings can be owned by cells or shapes now, so a new interface was created for the owner type.
	//        //private IWorksheetCell owningCell;
	//        private IFormattedStringOwner owner;

	//        // MD 11/3/10 - TFS49093
	//        // This class is now going to be a pointer to the actual formatted string and we will use the proxy/element architecture already
	//        // in place for formats and fonts so we can share strings.
	//        private FormattedStringProxy proxy;

	//        #endregion Member Variables

	//        #region Constructor

	//        // MD 11/3/10 - TFS49093
	//        // Added a default constructor for clone operations.
	//        private FormattedString() { }

	//        // MD 11/3/10 - TFS49093
	//        // Added a constructor to take a default element value.
	//        internal FormattedString(FormattedStringElement element)
	//        {
	//            GenericCachedCollection<FormattedStringElement> collection = null;

	//            if (element.Workbook != null)
	//                collection = element.Workbook.SharedStringTable;

	//            this.proxy = new FormattedStringProxy(element, collection);
	//        }

	//        /// <summary>
	//        /// Creates a new instance of the <see cref="FormattedString"/> class.
	//        /// </summary>
	//        /// <exception cref="ArgumentNullException">
	//        /// <paramref name="unformattedString"/> is null.
	//        /// </exception>
	//        /// <param name="unformattedString">The string that will be displayed in the cell with the formatting.</param>
	//        public FormattedString( string unformattedString )
	//        {
	//            if ( unformattedString == null )
	//                throw new ArgumentNullException( "unformattedString", "The unformatted string cannot be null." );

	//            // MD 11/3/10 - TFS49093
	//            // This class is now going to be a pointer to the actual formatted string and we will use the proxy/element architecture already
	//            // in place for formats and fonts so we can share strings.
	//            //this.formattingRuns = new List<FormattedStringRun>();
	//            //this.unformattedString = unformattedString;
	//            this.proxy = new FormattedStringProxy(new FormattedStringElement(null, unformattedString), null);
	//        }

	//        #endregion Constructor

	//        #region Base Class Overrides

	//        #region Equals

	//        /// <summary>
	//        /// Determines whether the specified <see cref="Object"/> is equal to this <see cref="FormattedString"/>.
	//        /// </summary>
	//        /// <param name="obj">The value to test for equality to this FormattedString.</param>
	//        /// <returns>
	//        /// True if the <paramref name="obj"/> is a FormattedString instance and it contains the same unformatted 
	//        /// string and formatting as this FormattedString; False otherwise.
	//        /// </returns>
	//        public override bool Equals( object obj )
	//        {
	//            // MD 11/3/10 - TFS49093
	//            // This class is now going to be a pointer to the actual formatted string, so forward the Equals call to the proxy which
	//            // points to the formatted string data.
	//            //FormattedString formattedString = obj as FormattedString;
	//            //
	//            //if ( formattedString == null )
	//            //    return false;
	//            //
	//            //if ( formattedString.unformattedString != this.unformattedString )
	//            //    return false;
	//            //
	//            //if ( this.FormattingRuns.Count != formattedString.FormattingRuns.Count )
	//            //    return false;
	//            //
	//            //for ( int i = 0; i < this.FormattingRuns.Count; i++ )
	//            //{
	//            //    if ( this.FormattingRuns[ i ].Equals( formattedString.FormattingRuns[ i ] ) == false )
	//            //        return false;
	//            //}
	//            //
	//            //return true;
	//            FormattedString formattedString = obj as FormattedString;

	//            if (formattedString == null)
	//                return false;

	//            return this.proxy.Equals(formattedString.proxy);
	//        }

	//        #endregion Equals

	//        #region GetHashCode

	//        /// <summary>
	//        /// Calculates the has code for this <see cref="FormattedString"/>.
	//        /// </summary>
	//        /// <returns>A number which can be used in hashing functions.</returns>
	//        public override int GetHashCode()
	//        {
	//            // MD 11/3/10 - TFS49093
	//            // This class is now going to be a pointer to the actual formatted string, so forward the GetHashCode call to the proxy which
	//            // points to the formatted string data.
	//            //int hashCode = this.unformattedString.GetHashCode();
	//            //
	//            //if ( this.formattingRuns != null )
	//            //    hashCode += this.formattingRuns.Count;
	//            //
	//            //return hashCode;
	//            return this.proxy.GetHashCode();
	//        }

	//        #endregion GetHashCode

	//        #region ToString

	//        /// <summary>
	//        /// Returns the <see cref="String"/> that represents this <see cref="FormattedString"/>.
	//        /// This is just the unformatted string.
	//        /// </summary>
	//        /// <remarks>
	//        /// <p class="body">
	//        /// This will return the same value as <see cref="UnformattedString"/>.
	//        /// </p>
	//        /// </remarks>
	//        /// <returns>The String that represents this FormattedString.</returns>
	//        public override string ToString()
	//        {
	//            // MD 11/3/10 - TFS49093
	//            // Moved the unformattedString member variable to the new FormattedStringElement class.
	//            //return this.unformattedString;
	//            return this.UnformattedString;
	//        }

	//        #endregion ToString

	//        #endregion Base Class Overrides

	//        #region Interfaces

	//        #region ICloneable Members
	//#if !SILVERLIGHT
	//        object ICloneable.Clone()
	//        {
	//            return this.Clone();
	//        }
	//#endif
	//        #endregion

	//        #region IComparable<FormattedString> Members

	//        int IComparable<FormattedString>.CompareTo( FormattedString other )
	//        {
	//            // MD 11/3/10 - TFS49093
	//            // This class is now going to be a pointer to the actual formatted string, so forward the CompareTo call to the proxy which
	//            // points to the formatted string data.
	//            //// MD 10/22/07 - BR27652
	//            //// There is a bug with CurrentCulture comparing that Ordinal comparing does not have.
	//            //// This causes a problem when sorting and doing binary searches.
	//            ////int result = String.Compare( this.unformattedString, other.unformattedString, StringComparison.CurrentCulture );
	//            //int result = String.Compare( this.unformattedString, other.unformattedString, StringComparison.Ordinal );
	//            //
	//            //if ( result != 0 )
	//            //    return result;
	//            //
	//            //if ( this.HasFormatting == false )
	//            //{
	//            //    if ( other.HasFormatting == false )
	//            //        return 0;
	//            //
	//            //    return -1;
	//            //}
	//            //
	//            //result = this.formattingRuns.Count - other.formattingRuns.Count;
	//            //
	//            //if ( result != 0 )
	//            //    return result;
	//            //
	//            //for ( int i = 0; i < this.formattingRuns.Count; i++ )
	//            //{
	//            //    result = 
	//            //        this.formattingRuns[ i ].FirstFormattedChar - 
	//            //        other.formattingRuns[ i ].FirstFormattedChar;
	//            //
	//            //    if ( result != 0 )
	//            //        return result;
	//            //
	//            //    result =
	//            //        this.formattingRuns[ i ].GetHashCode() -
	//            //        other.formattingRuns[ i ].GetHashCode();
	//            //
	//            //    if ( result != 0 )
	//            //        return result;
	//            //}
	//            //
	//            //return result;
	//            return this.proxy.CompareTo(other.proxy);
	//        }

	//        #endregion

	//        #region IWorksheetCellValue Members

	//        // MD 9/2/08 - Cell Comments
	//        // Renamed parameter for clarity because there is now a member variable named owner
	//        //void IWorksheetCellOwnedValue.VerifyNewOwner( IWorksheetCell owner )
	//        void IWorksheetCellOwnedValue.VerifyNewOwner( IWorksheetCell newOwner )
	//        {
	//            // MD 9/2/08 - Cell Comments
	//            // Moved all code to the new VerifyNewOwner method
	//            this.VerifyNewOwner( newOwner );
	//        }

	//        // MD 9/2/08 - Cell Comments
	//        internal void VerifyNewOwner( IFormattedStringOwner newOwner )
	//        {
	//            // MD 9/2/08 - Cell Comments
	//            //if ( this.owningCell != null && this.owningCell != owner )
	//            if ( this.owner != null && this.owner != newOwner )
	//                throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_FormattedStringAlreadyOwned" ) );
	//        }

	//        // MD 8/12/08 - Excel formula solving
	//        bool IWorksheetCellOwnedValue.IsOwnedByAllCellsAppliedTo
	//        {
	//            get { return true; }
	//        }

	//        IWorksheetCell IWorksheetCellOwnedValue.OwningCell
	//        {
	//            // MD 9/2/08 - Cell Comments
	//            // Formatted strings can be owned by cells or shapes now, so a new interface was created for the owner type.
	//            //get { return this.OwningCell; }
	//            //set { this.OwningCell = value; }
	//            get { return this.Owner as IWorksheetCell; }
	//            set { this.Owner = value; }
	//        }

	//        #endregion

	//        #endregion Interfaces

	//        #region Methods

	//        #region Clone

	//        /// <summary>
	//        /// Creates a new <see cref="FormattedString"/> that is a copy of this one.
	//        /// </summary>
	//        /// <remarks>
	//        /// <p class="body">
	//        /// This should be used if the same formatted string needs to be used in multiple cells.
	//        /// The FormattedString class can only exist as the <see cref="WorksheetCell.Value"/>
	//        /// of one cell at a time. If the FormattedString is already the value of a cell, and needs
	//        /// to be set as the value of another cell, clone the FormattedString and set the returned
	//        /// clone as value of the cell.
	//        /// </p>
	//        /// <p class="body">
	//        /// The cloned FormattedString only takes its original configuration for this instance.
	//        /// If this instance is cloned and than changed, the clone will not be changed as well; it will
	//        /// remain as it was when it was cloned.
	//        /// </p>
	//        /// </remarks>
	//        /// <returns>A new FormattedString that is a copy of this one.</returns>
	//        public FormattedString Clone()
	//        {
	//            // MD 11/3/10 - TFS49093
	//            // This class is now going to be a pointer to the actual formatted string and we will use the proxy/element architecture already
	//            // in place for formats and fonts so we can share strings. So just pass the element used by this formatted string to the clone so
	//            // they point to the same data initially.
	//            //FormattedString clone = new FormattedString( this.unformattedString );
	//            //
	//            //if ( this.HasFormatting )
	//            //{
	//            //    foreach ( FormattedStringRun run in this.formattingRuns )
	//            //        clone.FormattingRuns.Add( run.Clone( clone ) );
	//            //}
	//            //
	//            //return clone;
	//            FormattedString clone = new FormattedString();
	//            clone.proxy = new FormattedStringProxy(this.proxy.Element, this.proxy.Collection);
	//            return clone;
	//        }

	//        #endregion Clone

	//        #region GetFont ( int )

	//        /// <summary>
	//        /// Gets the font which controls the formatting properties in the string from the specified start index to 
	//        /// the end of the string.
	//        /// </summary>
	//        /// <remarks>
	//        /// <p class="body">
	//        /// If the start index is greater than or equal to the length of the unformatted string, no exception 
	//        /// will be thrown. It will be thrown later when one of the formatting properties of the returned
	//        /// <see cref="FormattedStringFont"/> is set.
	//        /// </p>
	//        /// </remarks>
	//        /// <param name="startIndex">The index of the first character the returned font controls.</param>
	//        /// <exception cref="ArgumentOutOfRangeException">
	//        /// <paramref name="startIndex"/> is less than zero.
	//        /// </exception>
	//        /// <returns>
	//        /// A FormattedStringFont instance which controls the formatting of the end portion of the string.
	//        /// </returns>
	//        public FormattedStringFont GetFont( int startIndex )
	//        {
	//            if ( startIndex < 0 )
	//                throw new ArgumentOutOfRangeException( "startIndex", startIndex, SR.GetString( "LE_ArgumentOutOfRangeException_NegativeStartIndex" ) );

	//            return new FormattedStringFont( this, startIndex, 0 );
	//        }

	//        #endregion GetFont ( int )

	//        #region GetFont ( int, int )

	//        /// <summary>
	//        /// Gets the font which controls the formatting properties in the string from the specified start index for
	//        /// the specified number of characters.
	//        /// </summary>
	//        /// <remarks>
	//        /// <p class="body">
	//        /// If the start index is greater than or equal to the length of the unformatted string, no exception 
	//        /// will be thrown. It will be thrown later when one of the formatting properties of the returned
	//        /// <see cref="FormattedStringFont"/> is set.
	//        /// </p>
	//        /// </remarks>
	//        /// <param name="startIndex">The index of the first character the returned font controls.</param>
	//        /// <param name="length">The number of characters after the start index controlled by the returned font.</param>
	//        /// <exception cref="ArgumentOutOfRangeException">
	//        /// <paramref name="startIndex"/> is less than zero.
	//        /// </exception>
	//        /// <exception cref="ArgumentOutOfRangeException">
	//        /// <paramref name="length"/> is less than one. A zero length string cannot be controlled by a formatting font.
	//        /// </exception>
	//        /// <returns>
	//        /// A FormattedStringFont instance which controls the formatting of a portion of the string.
	//        /// </returns>
	//        public FormattedStringFont GetFont( int startIndex, int length )
	//        {
	//            if ( startIndex < 0 )
	//                throw new ArgumentOutOfRangeException( "startIndex", startIndex, SR.GetString( "LE_ArgumentOutOfRangeException_NegativeStartIndex" ) );

	//            if ( length < 1 )
	//                throw new ArgumentOutOfRangeException( "length", length, SR.GetString( "LE_ArgumentOutOfRangeException_LengthMustBePositive" ) );

	//            return new FormattedStringFont( this, startIndex, length );
	//        }

	//        #endregion GetFont ( int, int )

	//        // MD 9/26/08
	//        #region GetFormattingRuns

	//#if DEBUG
	//        /// <summary>
	//        /// For unit testing purposes only.
	//        /// </summary>  
	//#endif
	//        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
	//        internal FormattedStringRun[] GetFormattingRuns()
	//        {
	//            // MD 11/3/10 - TFS49093
	//            //return this.formattingRuns.ToArray();
	//            return this.proxy.Element.FormattingRuns.ToArray();
	//        }

	//        #endregion GetFormattingRuns

	//        // MD 11/3/10 - TFS49093
	//        // This is no longer needed because the shared formatted strings will be iterated over and their fonts will be placed 
	//        // in the manager when saving.
	//        #region Removed

	//        // MD 9/2/08 - Cell Comments
	//        //#region InitSerializationCache
	//        //
	//        //internal void InitSerializationCache( WorkbookSerializationManager serializationManager )
	//        //{
	//        //    // Add the formatted fonts from the formatted string to the serialization manager
	//        //    if ( this.HasFormatting == false )
	//        //        return;
	//        //
	//        //    foreach ( FormattedStringRun run in this.FormattingRuns )
	//        //    {
	//        //        if ( run.HasFont )
	//        //            serializationManager.AddFont( run.Font.Element, true );
	//        //    }
	//        //} 
	//        //
	//        //#endregion InitSerializationCache 

	//        #endregion // Removed

	//        // MD 11/3/10 - TFS49093
	//        // This has been moved to the FormattedStringElement class.
	//        #region Moved

	//        //        // MD 5/2/08 - BR32461/BR01870
	//        //        #region RemoveCarriageReturns
	//        //
	//        //#if DEBUG
	//        //        /// <summary>
	//        //        /// Gets a FormattedString similar to the current one which has no carriage returns. If this FormattedString
	//        //        /// already has no carriage returns, this instance will be returned.
	//        //        /// </summary> 
	//        //#endif
	//        //        internal FormattedString RemoveCarriageReturns()
	//        //        {
	//        //            int carraigeReturnIndex = this.unformattedString.IndexOf( '\r' );
	//        //
	//        //            // If no carraige return is contained in this instance, return it
	//        //            if ( carraigeReturnIndex < 0 )
	//        //                return this;
	//        //
	//        //            // Otherwise, we need to clone this instance and return the carriage returns from it
	//        //            FormattedString clonedString = this.Clone();
	//        //
	//        //            // The formatting runs are sorted in the order they appear in the string, we keep track of the formatting run
	//        //            // occurring after the carriage return in the string. This will be updated when we have to iterate and update the
	//        //            // formatting runs.
	//        //            int nextFormattingRunIndex = 0;
	//        //
	//        //            while ( carraigeReturnIndex >= 0 )
	//        //            {
	//        //                // Remove the carriage return from the string.
	//        //                clonedString.unformattedString = clonedString.unformattedString.Remove( carraigeReturnIndex, 1 );
	//        //
	//        //                // If the carraige return did not have a newline character after it, insert a newline where the carriage return 
	//        //                // was. If we do this, the overall length of the string has not changes, so the formatting doesn't need to be 
	//        //                // updated.
	//        //                if ( carraigeReturnIndex < clonedString.unformattedString.Length &&
	//        //                    clonedString.unformattedString[ carraigeReturnIndex ] != '\n' )
	//        //                {
	//        //                    clonedString.unformattedString = clonedString.unformattedString.Insert( carraigeReturnIndex, "\n" );
	//        //                }				
	//        //                // If the carriage return did have a newline after it, we will not be inserting a new character. This has changed
	//        //                // the length of the string and the formatting runs after the removed carriage return must be updated to be at the 
	//        //                // correct place in the new string.
	//        //                else if ( clonedString.HasFormatting )
	//        //                {
	//        //                    // Start iterating at the first formatting run that was on or after the last carraige return.
	//        //                    for ( int i = nextFormattingRunIndex; i < clonedString.formattingRuns.Count; i++ )
	//        //                    {
	//        //                        FormattedStringRun formattingRun = clonedString.formattingRuns[ i ];
	//        //
	//        //                        // If the formatting run is before the current carriage return, skip it and increment the 
	//        //                        // nextFormattingRunIndex pointer so it will end up pointing to the first formatting run on or after the 
	//        //                        // current carriage return.
	//        //                        if ( formattingRun.FirstFormattedChar < carraigeReturnIndex )
	//        //                        {
	//        //                            nextFormattingRunIndex++;
	//        //                            continue;
	//        //                        }
	//        //
	//        //                        // If the formatting run was on or after the current carriage return, update it so it still points to the
	//        //                        // same character after removing the carriage return.
	//        //                        formattingRun.FirstFormattedChar--;
	//        //                    }
	//        //                }
	//        //
	//        //                // Find the index of the next carriage return
	//        //                carraigeReturnIndex = clonedString.unformattedString.IndexOf( '\r', carraigeReturnIndex );
	//        //            }
	//        //
	//        //            return clonedString;
	//        //        } 
	//        //
	//        //        #endregion RemoveCarriageReturns 

	//        #endregion // Moved

	//        // MD 11/3/10 - TFS49093
	//        // Moved this code from the Owner property setter and modified it to work with the new architecture of FormattedStrings.
	//        #region SetWorksheet

	//        internal void SetWorksheet(Worksheet worksheet)
	//        {
	//            Workbook workbook = null;
	//            if (worksheet != null)
	//                workbook = worksheet.Workbook;

	//            this.proxy.SetWorkbook(workbook);
	//        } 

	//        #endregion // SetWorksheet

	//        #endregion Methods

	//        #region Properties

	//        #region Public Properties

	//        #region UnformattedString

	//        /// <summary>
	//        /// Gets or sets the unformatted string.
	//        /// </summary>
	//        /// <remarks>
	//        /// <p class="body">
	//        /// If the new unformatted string assigned is shorter than the old unformatted string, all formatting
	//        /// outside the range of the new value will be lost.
	//        /// </p>
	//        /// </remarks>
	//        /// <exception cref="ArgumentNullException">
	//        /// The value assigned is a null string.
	//        /// </exception>
	//        /// <value>The unformatted string.</value>
	//        public string UnformattedString
	//        {
	//            // MD 9/26/08
	//            // This will never be null so this is not needed.
	//            //get { return this.unformattedString != null ? this.unformattedString : string.Empty; }
	//            // MD 11/3/10 - TFS49093
	//            // Moved the unformattedString member variable to the new FormattedStringElement class.
	//            //get { return this.unformattedString; }
	//            get { return this.proxy.UnformattedString; }
	//            set
	//            {
	//                //  BF 9/9/08
	//                //  Externalized the set logic; I needed a way to set
	//                //  this property without triggering the notification.
	//                #region Refactored
	//                //if ( this.unformattedString != value )
	//                //{
	//                //    if ( value == null )
	//                //        throw new ArgumentNullException( "unformattedString", SR.GetString( "LE_ArgumentNullException_UnformattedString" ) );

	//                //    this.unformattedString = value;

	//                //    if ( this.HasFormatting )
	//                //    {
	//                //        for ( int i = this.formattingRuns.Count - 1; i >= 0; i-- )
	//                //        {
	//                //            if ( this.unformattedString.Length <= this.formattingRuns[ i ].FirstFormattedChar )
	//                //            {
	//                //                FormattedStringRun run = this.formattingRuns[ i ];

	//                //                if ( run.HasFont )
	//                //                    run.Font.OnUnrooted();

	//                //                this.formattingRuns.RemoveAt( i );
	//                //            }
	//                //            else
	//                //                break;
	//                //        }
	//                //    }

	//                //    // MD 9/2/08 - Cell Comments
	//                //    // The owner of the string may have to do some processing when the unformatted string changes.
	//                //    if ( this.Owner != null )
	//                //        this.Owner.OnUnformattedStringChanged();
	//                //}
	//                #endregion Refactored

	//                // MD 11/3/10 - TFS49093
	//                // We no longer need the helper method becasue callers who dn't want notifications to go out will just set the formatted 
	//                // string on the element directly.
	//                //this.SetUnformattedStringHelper( value, true );
	//                if (this.UnformattedString != value)
	//                {
	//                    if (value == null)
	//                        throw new ArgumentNullException("unformattedString", SR.GetString("LE_ArgumentNullException_UnformattedString"));

	//                    this.proxy.UnformattedString = value;

	//                    // MD 9/2/08 - Cell Comments
	//                    // The owner of the string may have to do some processing when the unformatted string changes.
	//                    if (this.Owner != null)
	//                        this.Owner.OnUnformattedStringChanged();
	//                }
	//            }
	//        }

	//        // MD 11/3/10 - TFS49093
	//        // Removed - this is no longer needed now that the FormattedStringElement stores the actual data. 
	//        // Moved most of this code back to the UnformattedString setter.
	//        #region Removed

	//        //internal void SetUnformattedStringHelper( string value, bool notify )
	//        //{
	//        //    if ( this.unformattedString != value )
	//        //    {
	//        //        if ( value == null )
	//        //            throw new ArgumentNullException( "unformattedString", SR.GetString( "LE_ArgumentNullException_UnformattedString" ) );
	//        //
	//        //        this.unformattedString = value;
	//        //
	//        //        if (this.HasFormatting)
	//        //        {
	//        //            for (int i = this.formattingRuns.Count - 1; i >= 0; i--)
	//        //            {
	//        //                if (this.unformattedString.Length <= this.formattingRuns[i].FirstFormattedChar)
	//        //                {
	//        //                    FormattedStringRun run = this.formattingRuns[i];
	//        //
	//        //                    if (run.HasFont)
	//        //                        run.Font.OnUnrooted();
	//        //
	//        //                    this.formattingRuns.RemoveAt(i);
	//        //                }
	//        //                else
	//        //                    break;
	//        //            }
	//        //        }
	//        //
	//        //        if ( notify )
	//        //        {
	//        //            // MD 9/2/08 - Cell Comments
	//        //            // The owner of the string may have to do some processing when the unformatted string changes.
	//        //            if ( this.Owner != null )
	//        //                this.Owner.OnUnformattedStringChanged();
	//        //        }
	//        //    }
	//        //} 

	//        #endregion // Removed

	//        #endregion UnformattedString

	//        #endregion Public Properties

	//        #region Internal Properties

	//        // MD 11/3/10 - TFS49093
	//        // Moved to the FormattedStringElement.
	//        #region Moved

	//        //#region FormattingRuns
	//        //
	//        //internal List<FormattedStringRun> FormattingRuns
	//        //{
	//        //    get
	//        //    {
	//        //        // MD 9/26/08
	//        //        // This will never be null so this is not needed.
	//        //        //if ( this.formattingRuns == null )
	//        //        //	this.formattingRuns = new List<FormattedStringRun>();
	//        //
	//        //        return this.formattingRuns;
	//        //    }
	//        //}
	//        //
	//        //#endregion FormattingRuns
	//        //
	//        //#region HasFormatting
	//        //
	//        //internal bool HasFormatting
	//        //{
	//        //    get
	//        //    {
	//        //        // MD 9/26/08
	//        //        // This will never be null so this is not needed.
	//        //        //return this.formattingRuns != null && this.formattingRuns.Count > 0;
	//        //        return this.formattingRuns.Count > 0;
	//        //    }
	//        //}
	//        //
	//        //#endregion HasFormatting 

	//        #endregion // Moved

	//        #region Owner

	//        // MD 9/2/08 - Cell Comments
	//        // Formatted strings can be owned by cells or shapes now, so a new interface was created for the owner type.
	//        //internal IWorksheetCell OwningCell
	//        internal IFormattedStringOwner Owner
	//        {
	//            // MD 9/2/08 - Cell Comments
	//            //get { return this.owningCell; }
	//            get { return this.owner; }
	//            set 
	//            {
	//                // MD 9/2/08 - Cell Comments
	//                //this.owningCell = value;
	//                this.owner = value;

	//                // MD 11/3/10 - TFS49093
	//                // Moved this to the SetWorksheet helper method so it could be called from other places.
	//                //if ( this.HasFormatting )
	//                //{
	//                //    GenericCachedCollection<WorkbookFontData> newCollection = null;
	//                //
	//                //    // MD 9/2/08 - Cell Comments
	//                //    //if ( this.owningCell != null && this.owningCell.IsOnWorksheet )
	//                //    //    newCollection = this.owningCell.Worksheet.Workbook.Fonts;
	//                //    // MD 7/26/10 - TFS34398
	//                //    // Now that the Row is stored on the cell, the Worksheet getter is now a bit slower, so cache it.
	//                //    // Also, we should be checking the Workbook for null.
	//                //    //if ( this.owner != null && this.owner.Worksheet != null )
	//                //    //    newCollection = this.owner.Worksheet.Workbook.Fonts;
	//                //    if (this.owner != null)
	//                //    {
	//                //        Worksheet worksheet = this.owner.Worksheet;
	//                //
	//                //        if (worksheet != null)
	//                //        {
	//                //            Workbook workbook = worksheet.Workbook;
	//                //            if (workbook != null)
	//                //                newCollection = workbook.Fonts;
	//                //        }
	//                //    }
	//                //
	//                //    foreach ( FormattedStringRun run in this.formattingRuns )
	//                //    {
	//                //        if ( run.HasFont )
	//                //            run.Font.OnRooted( newCollection );
	//                //    }
	//                //}
	//                Worksheet worksheet = this.owner == null ? null : this.owner.Worksheet;
	//                this.SetWorksheet(worksheet);
	//            }
	//        }

	//        #endregion Owner

	//        #region Proxy

	//        internal FormattedStringProxy Proxy
	//        {
	//            get { return this.proxy; }
	//        } 

	//        #endregion // Proxy

	//        #endregion Internal Properties

	//        #endregion Properties
	//    }


	//    // MD 11/3/10 - TFS49093
	//    // Formatted strings will now use the proxy/element architecture already in place for formats and fonts so we can share strings.
	//    #region FormattedStringElement class

	//    internal class FormattedStringElement : GenericCacheElement,
	//        IComparable<FormattedStringElement>
	//    {
	//        #region Member Variables

	//        private int indexInStringTable;
	//        private List<FormattedStringRun> formattingRuns;
	//        private string unformattedString; 

	//        #endregion // Member Variables

	//        #region Constructor

	//        public FormattedStringElement(Workbook workbook, string unformattedString)
	//            : base(workbook)
	//        {
	//            this.unformattedString = unformattedString;
	//        } 

	//        #endregion // Constructor

	//        #region Base Class Overrides

	//        #region Clone

	//        public override object Clone()
	//        {
	//            FormattedStringElement clone = new FormattedStringElement(this.Workbook, this.unformattedString);

	//            if (this.HasFormatting)
	//            {
	//                foreach (FormattedStringRun run in this.formattingRuns)
	//                    clone.FormattingRuns.Add(run.Clone(clone));
	//            }

	//            return clone;
	//        }

	//        #endregion // Clone

	//        #region Equals

	//        public override bool Equals(object obj)
	//        {
	//            return this.HasSameData(obj as GenericCacheElement);
	//        }

	//        #endregion Equals

	//        #region GetHashCode

	//        public override int GetHashCode()
	//        {
	//            int hashCode = this.unformattedString.GetHashCode();

	//            if (this.formattingRuns != null)
	//                hashCode += this.formattingRuns.Count;

	//            return hashCode;
	//        }

	//        #endregion // GetHashCode

	//        #region HasSameData

	//        public override bool HasSameData(GenericCacheElement otherElement)
	//        {
	//            FormattedStringElement formattedString = otherElement as FormattedStringElement;

	//            if (formattedString == null)
	//                return false;

	//            if (formattedString.unformattedString != this.unformattedString)
	//                return false;

	//            int formattingRunsCount = this.formattingRuns == null ? 0 : this.formattingRuns.Count;
	//            int otherFormattingRunsCount = formattedString.formattingRuns == null ? 0 : formattedString.formattingRuns.Count;

	//            if (formattingRunsCount != otherFormattingRunsCount)
	//                return false;

	//            if (formattingRunsCount > 0)
	//            {
	//                for (int i = 0; i < formattingRunsCount; i++)
	//                {
	//                    if (this.formattingRuns[i].Equals(formattedString.formattingRuns[i]) == false)
	//                        return false;
	//                }
	//            }

	//            return true;
	//        }

	//        #endregion // HasSameData

	//        #region RemoveUsedColorIndicies

	//        public override void RemoveUsedColorIndicies(List<int> unusedIndicies)
	//        {
	//            // We don't have to do anything here because the fonts on the formatting runs are stored in the workbook fonts collcetion.
	//        }

	//        #endregion // RemoveUsedColorIndicies

	//        #endregion // Base Class Overrides

	//        #region Interfaces

	//        #region IComparable<FormattedStringElement> Members

	//        public int CompareTo(FormattedStringElement other)
	//        {
	//            int result = String.Compare(this.unformattedString, other.unformattedString, StringComparison.Ordinal);

	//            if (result != 0)
	//                return result;

	//            int otherFormattingRunsCount = other.formattingRuns == null ? 0 : other.formattingRuns.Count;

	//            if (this.formattingRuns == null)
	//            {
	//                if (otherFormattingRunsCount == 0)
	//                    return 0;

	//                return -1;
	//            }

	//            result = this.formattingRuns.Count - otherFormattingRunsCount;

	//            if (result != 0)
	//                return result;

	//            for (int i = 0; i < this.formattingRuns.Count; i++)
	//            {
	//                result =
	//                    this.formattingRuns[i].FirstFormattedChar -
	//                    other.formattingRuns[i].FirstFormattedChar;

	//                if (result != 0)
	//                    return result;

	//                result =
	//                    this.formattingRuns[i].GetHashCode() -
	//                    other.formattingRuns[i].GetHashCode();

	//                if (result != 0)
	//                    return result;
	//            }

	//            return result;
	//        }

	//        #endregion

	//        #endregion // Interfaces

	//        #region Methods

	//        #region InitSerializationCache

	//        internal void InitSerializationCache(WorkbookSerializationManager serializationManager)
	//        {
	//            // Add the formatted fonts from the formatted string to the serialization manager
	//            if (this.HasFormatting == false)
	//                return;

	//            foreach (FormattedStringRun run in this.FormattingRuns)
	//            {
	//                if (run.HasFont)
	//                    serializationManager.AddFont(run.Font.Element, true);
	//            }
	//        }

	//        #endregion InitSerializationCache

	//        #region RemoveCarriageReturns

	//#if DEBUG
	//        /// <summary>
	//        /// Gets a FormattedString similar to the current one which has no carriage returns. If this FormattedString
	//        /// already has no carriage returns, this instance will be returned.
	//        /// </summary> 
	//#endif
	//        internal FormattedStringElement RemoveCarriageReturns()
	//        {
	//            int carraigeReturnIndex = this.unformattedString.IndexOf('\r');

	//            // If no carraige return is contained in this instance, return it
	//            if (carraigeReturnIndex < 0)
	//                return this;

	//            // Otherwise, we need to clone this instance and return the carriage returns from it
	//            FormattedStringElement clonedString = (FormattedStringElement)this.Clone();

	//            // The formatting runs are sorted in the order they appear in the string, we keep track of the formatting run
	//            // occurring after the carriage return in the string. This will be updated when we have to iterate and update the
	//            // formatting runs.
	//            int nextFormattingRunIndex = 0;

	//            while (carraigeReturnIndex >= 0)
	//            {
	//                // Remove the carriage return from the string.
	//                clonedString.unformattedString = clonedString.unformattedString.Remove(carraigeReturnIndex, 1);

	//                // If the carraige return did not have a newline character after it, insert a newline where the carriage return 
	//                // was. If we do this, the overall length of the string has not changes, so the formatting doesn't need to be 
	//                // updated.
	//                if (carraigeReturnIndex < clonedString.unformattedString.Length &&
	//                    clonedString.unformattedString[carraigeReturnIndex] != '\n')
	//                {
	//                    clonedString.unformattedString = clonedString.unformattedString.Insert(carraigeReturnIndex, "\n");
	//                }
	//                // If the carriage return did have a newline after it, we will not be inserting a new character. This has changed
	//                // the length of the string and the formatting runs after the removed carriage return must be updated to be at the 
	//                // correct place in the new string.
	//                else if (clonedString.HasFormatting)
	//                {
	//                    // Start iterating at the first formatting run that was on or after the last carraige return.
	//                    for (int i = nextFormattingRunIndex; i < clonedString.formattingRuns.Count; i++)
	//                    {
	//                        FormattedStringRun formattingRun = clonedString.formattingRuns[i];

	//                        // If the formatting run is before the current carriage return, skip it and increment the 
	//                        // nextFormattingRunIndex pointer so it will end up pointing to the first formatting run on or after the 
	//                        // current carriage return.
	//                        if (formattingRun.FirstFormattedChar < carraigeReturnIndex)
	//                        {
	//                            nextFormattingRunIndex++;
	//                            continue;
	//                        }

	//                        // If the formatting run was on or after the current carriage return, update it so it still points to the
	//                        // same character after removing the carriage return.
	//                        formattingRun.FirstFormattedChar--;
	//                    }
	//                }

	//                // Find the index of the next carriage return
	//                carraigeReturnIndex = clonedString.unformattedString.IndexOf('\r', carraigeReturnIndex);
	//            }

	//            return clonedString;
	//        }

	//        #endregion RemoveCarriageReturns

	//        #endregion // Methods

	//        #region Properties

	//        #region FormattingRuns

	//        public List<FormattedStringRun> FormattingRuns
	//        {
	//            get
	//            {
	//                if (this.formattingRuns == null)
	//                    this.formattingRuns = new List<FormattedStringRun>();

	//                return this.formattingRuns;
	//            }
	//        }

	//        #endregion // FormattingRuns

	//        #region FormattingRunsInternal

	//        public List<FormattedStringRun> FormattingRunsInternal
	//        {
	//            get { return this.formattingRuns; }
	//        }

	//        #endregion // FormattingRunsInternal

	//        #region HasFormatting

	//        public bool HasFormatting
	//        {
	//            get
	//            {
	//                return this.formattingRuns != null && this.formattingRuns.Count > 0;
	//            }
	//        }

	//        #endregion HasFormatting

	//        #region IndexInStringTable

	//        public int IndexInStringTable
	//        {
	//            get { return this.indexInStringTable; }
	//            set { this.indexInStringTable = value; }
	//        }

	//        #endregion // IndexInStringTable

	//        #region UnformattedString

	//        public string UnformattedString
	//        {
	//            get { return this.unformattedString; }
	//            set
	//            {
	//                this.unformattedString = value;

	//                if (this.HasFormatting)
	//                {
	//                    for (int i = this.formattingRuns.Count - 1; i >= 0; i--)
	//                    {
	//                        if (this.unformattedString.Length <= this.formattingRuns[i].FirstFormattedChar)
	//                        {
	//                            FormattedStringRun run = this.formattingRuns[i];

	//                            if (run.HasFont)
	//                                run.Font.OnUnrooted();

	//                            this.formattingRuns.RemoveAt(i);
	//                        }
	//                        else
	//                            break;
	//                    }
	//                }
	//            }
	//        }

	//        #endregion // UnformattedString

	//        #endregion // Properties
	//    } 

	//    #endregion // FormattedStringElement class

	//    // MD 11/3/10 - TFS49093
	//    // Formatted strings will now use the proxy/element architecture already in place for formats and fonts so we can share strings.
	//    #region FormattedStringProxy class

	//    internal class FormattedStringProxy : GenericCacheElementProxy<FormattedStringElement>,
	//        IComparable<FormattedStringProxy>
	//    {
	//        #region Constructor

	//        public FormattedStringProxy(FormattedStringElement element, GenericCachedCollection<FormattedStringElement> collection)
	//            : base(element, collection) { } 

	//        #endregion // Constructor

	//        #region Interfaces

	//        #region IComparable<FormattedStringProxy> Members

	//        public int CompareTo(FormattedStringProxy other)
	//        {
	//            return this.Element.CompareTo(other.Element);
	//        }

	//        #endregion

	//        #endregion // Interfaces

	//        #region Base Class Overrides

	//        #region ToString

	//        public override string ToString()
	//        {
	//            return this.UnformattedString;
	//        }

	//        #endregion // ToString

	//        #region Workbook

	//        public override Workbook Workbook
	//        {
	//            get { return this.Collection.Workbook; }
	//        }

	//        #endregion // Workbook

	//        #endregion // Base Class Overrides

	//        #region SetWorksheet

	//        public void SetWorkbook(Workbook workbook)
	//        {
	//            GenericCachedCollection<FormattedStringElement> sharedStringTable =
	//                workbook == null ? null : workbook.SharedStringTable;
	//            GenericCachedCollection<WorkbookFontData> newCollection =
	//                workbook == null ? null : workbook.Fonts;

	//            FormattedStringElement element = this.Element;
	//            element.Workbook = workbook;

	//            if (element.HasFormatting)
	//            {
	//                foreach (FormattedStringRun run in element.FormattingRuns)
	//                {
	//                    if (run.HasFont)
	//                        run.Font.OnRooted(newCollection);
	//                }
	//            }

	//            this.OnRooted(sharedStringTable);
	//        } 

	//        #endregion // SetWorksheet

	//        #region UnformattedString

	//        public string UnformattedString
	//        {
	//            get { return this.Element.UnformattedString; }
	//            set
	//            {
	//                this.BeforeSet();
	//                this.Element.UnformattedString = value;
	//                this.AfterSet();
	//            }
	//        } 

	//        #endregion // UnformattedString
	//    } 

	//    #endregion // FormattedStringProxy class

	//    // MD 11/3/10 - TFS49093
	//    // This class will store the string representations of objects along with the object which created them.
	//    #region FormattedStringValueReference class

	//    internal class FormattedStringValueReference
	//    {
	//        private FormattedStringProxy proxy;
	//        private object value;

	//        public FormattedStringValueReference(object value, WorksheetCell cell)
	//        {
	//            this.value = value;

	//            Workbook workbook = cell.Worksheet.Workbook;
	//            GenericCachedCollection<FormattedStringElement> sharedStringTable = workbook == null ? null : workbook.SharedStringTable;

	//            this.proxy = new FormattedStringProxy(new FormattedStringElement(workbook, value.ToString()), sharedStringTable);
	//        }

	//        public override string ToString()
	//        {
	//            return this.proxy.ToString();
	//        }

	//        public FormattedStringProxy Proxy
	//        {
	//            get { return this.proxy; }
	//        }

	//        public object Value
	//        {
	//            get { return this.value; }
	//        }
	//    } 

	//    #endregion // FormattedStringValueReference class 

	#endregion  // Old Code
	/// <summary>
	/// Represents a string with mixed formatting in a cell or cell comment.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The formatting of the string is controlled in a similar fashion as it would be in Microsoft Excel. In Excel, the user
	/// must select a portion of the text and set the various formatting properties of that selected text. 
	/// </p>
	/// <p class="body">
	/// With the FormattedString, a portion of the string is "selected" by calling either <see cref="GetFont(int)">GetFont(int)</see> or 
	/// <see cref="GetFont(int,int)">GetFont(int,int)</see>. Formatting properties are then set on the returned 
	/// <see cref="FormattedStringFont"/> and all characters in the font's selection range are given these properties.
	/// </p>
	/// <p class="body">
	/// Getting the formatting properties of a <see cref="FormattedStringFont"/> will return the formatting of the first 
	/// character in font's selection range. This is similar to Excel, which will update the formatting interface to 
	/// reflect the formatting of the first character in a selection range when a cell's text is selected.
	/// </p>
	/// </remarks>
	[DebuggerDisplay("FormattedString: {UnformattedString}")]



	public

		 class FormattedString : 
		IWorksheetCellOwnedValue,

	    ICloneable,

		IComparable<FormattedString>,
		IFormattedString,
		IFormattedItem			// MD 11/9/11 - TFS85193
	{
		#region Member Variables

		private StringElement element;
		private IFormattedStringOwner owner;
		private GenericCachedCollection<StringElement> sharedStringTable;

		#endregion Member Variables

		#region Constructor

		// MD 2/2/12 - TFS100573
		// Since the StringElement no longer has a Workbook reference, it needs to be passed into the constructor.
		//internal FormattedString(StringElement element)
		//    : this(element, true) { }
		internal FormattedString(Workbook workbook, StringElement element)
			: this(workbook, element, false, true) { }

		// MD 2/2/12 - TFS100573
		// Since the StringElement no longer has a Workbook reference, it needs to be passed into the constructor.
		//internal FormattedString(StringElement element, bool addElementToCache)
		internal FormattedString(Workbook workbook, StringElement element, bool isElementInCache, bool addElementToCache)
		{
			// MD 2/2/12 - TFS100573
			//if (element.Workbook != null)
			//    this.sharedStringTable = element.Workbook.SharedStringTable;
			if (isElementInCache || addElementToCache)
			{
				if (workbook != null)
					this.sharedStringTable = workbook.SharedStringTable;
			}

			if (addElementToCache)
				this.element = GenericCacheElement.FindExistingOrAddToCache(element, this.sharedStringTable);
			else
				this.element = element;
		}

		/// <summary>
		/// Creates a new instance of the <see cref="FormattedString"/> class.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="unformattedString"/> is null.
		/// </exception>
		/// <param name="unformattedString">The string that will be displayed in the cell with the formatting.</param>
		public FormattedString( string unformattedString )
		{
			if ( unformattedString == null )
				throw new ArgumentNullException( "unformattedString", SR.GetString("LE_ArgumentNullException_UnformattedString") );

			// MD 1/6/12 - TFS98536
			// Fix up any invalid characters in the string.
			unformattedString = Utilities.FixupCellString(unformattedString);

			// MD 2/2/12 - TFS100573
			// The StringElement no longer needs a Workbook reference.
			//this.element = new StringElement(null, unformattedString);
			this.element = new StringElement(unformattedString);
		}

		#endregion Constructor

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether the specified <see cref="Object"/> is equal to this <see cref="FormattedString"/>.
		/// </summary>
		/// <param name="obj">The value to test for equality to this FormattedString.</param>
		/// <returns>
		/// True if the <paramref name="obj"/> is a FormattedString instance and it contains the same unformatted 
		/// string and formatting as this FormattedString; False otherwise.
		/// </returns>
		public override bool Equals( object obj )
		{
			FormattedString formattedString = obj as FormattedString;

			if (formattedString == null)
				return false;

			return this.element.Equals(formattedString.element);
		}

		#endregion Equals

		#region GetHashCode

		/// <summary>
		/// Calculates the has code for this <see cref="FormattedString"/>.
		/// </summary>
		/// <returns>A number which can be used in hashing functions.</returns>
		public override int GetHashCode()
		{
			return this.element.GetHashCode();
		}

		#endregion GetHashCode

		#region ToString

		/// <summary>
		/// Returns the <see cref="String"/> that represents this <see cref="FormattedString"/>.
		/// This is just the unformatted string.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This will return the same value as <see cref="UnformattedString"/>.
		/// </p>
		/// </remarks>
		/// <returns>The String that represents this FormattedString.</returns>
		public override string ToString()
		{
			return this.UnformattedString;
		}

		#endregion ToString

		#endregion Base Class Overrides

		#region Interfaces

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return this.Clone();
		}

		#endregion

		#region IComparable<FormattedString> Members

		int IComparable<FormattedString>.CompareTo( FormattedString other )
		{
			return this.element.CompareTo(other.element);
		}

		#endregion

		// MD 11/9/11 - TFS85193
		#region IFormattedItem Members

		object IFormattedItem.Owner
		{
			get { return this.owner; }
		}

		Workbook IFormattedItem.Workbook
		{
			get { return this.Workbook; }
		}

		IFormattedRunOwner IFormattedItem.GetOwnerAt(int startIndex)
		{
			// MD 1/31/12 - TFS100573
			//return this.Element;
			return this.ConvertToFormattedStringElement();
		}

		void IFormattedItem.OnFormattingChanged() { }

		#endregion

		#region IFormattedString Members

		void IFormattedString.SetWorkbook(Workbook workbook)
		{
			// MD 8/23/11 - TFS84306
			// Use the new overload which takes an owner now.
			//FormattedStringElement.SetWorkbook(workbook, ref this.sharedStringTable, ref this.element);
			StringElement.SetWorkbook(workbook, this.owner, ref this.sharedStringTable, ref this.element);
		}

		#endregion

		#region IWorksheetCellOwnedValue Members

		bool IWorksheetCellOwnedValue.IsOwnedByAllCellsAppliedTo
		{
			get { return true; }
		}

		void IWorksheetCellOwnedValue.SetOwningCell(WorksheetRow row, short columnIndex)
		{
			this.SetOwningCell(row, columnIndex);
		}

		internal void SetOwningCell(WorksheetRow row, short columnIndex)
		{
			if (row == null)
				this.Owner = null;
			else
				this.Owner = row.Cells[columnIndex];
		}

		#endregion

		#region IWorksheetCellValue Members

		void IWorksheetCellOwnedValue.VerifyNewOwner(WorksheetRow ownerRow, short ownerColumnIndex)
		{
			this.VerifyNewOwner(ownerRow.Cells[ownerColumnIndex]);
		}

		internal void VerifyNewOwner( IFormattedStringOwner newOwner )
		{
			// Make sure this string is still owned by the cell before we verify the new owner.
			this.VerifyOwner();

			// MD 7/21/11 - TFS82017
			// WorksheetCells no longer have to be the same instance to represent the same logical cell. And even though the == operator
			// is overloaded, it will only work if both variables being compared are declared as WorksheetCells. In this case, they are
			// IFormattedStringOwner, so the operator overload is not used and their references are compared instead, which incorrectly
			// results in the cells not being equal. Use the Equals method instead.
			//if ( this.owner != null && this.owner != newOwner )
			if (this.owner != null && this.owner.Equals(newOwner) == false)
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_FormattedStringAlreadyOwned" ) );
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region AfterSet

		internal void AfterSet()
		{
			// MD 2/2/12 - TFS100573
			//GenericCacheElementProxy<StringElement>.AfterSet(sharedStringTable, ref this.element);
			GenericCacheElement.AfterSet(sharedStringTable, ref this.element);
		}

		#endregion  // AfterSet

		#region BeforeSet

		internal void BeforeSet()
		{
			// Verify that this string is still owned by the same owner before performing the modification.
			this.VerifyOwner();

			// MD 4/18/11 - TFS62026
			// Pass in True for the willModifyElement parameter.
			//GenericCacheElementProxy<FormattedStringElement>.BeforeSet(this.sharedStringTable, ref this.element);
			// MD 2/2/12 - TFS100573
			//GenericCacheElementProxy<FormattedStringElement>.BeforeSet(this.sharedStringTable, ref this.element, true);
			GenericCacheElement.BeforeSet(this.sharedStringTable, ref this.element, true);
		}

		#endregion  // BeforeSet

		#region ClearCollectionOnInvalidOwner

		private void ClearCollectionOnInvalidOwner()
		{
			if (this.sharedStringTable != null)
			{
				this.sharedStringTable = null;
				this.element = GenericCacheElement.FindExistingOrAddToCache(this.element, null);
			}
		}

		#endregion // ClearCollectionOnInvalidOwner

		#region Clone

		/// <summary>
		/// Creates a new <see cref="FormattedString"/> that is a copy of this one.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This should be used if the same formatted string needs to be used in multiple cells.
		/// The FormattedString class can only exist as the <see cref="WorksheetCell.Value"/>
		/// of one cell at a time. If the FormattedString is already the value of a cell, and needs
		/// to be set as the value of another cell, clone the FormattedString and set the returned
		/// clone as value of the cell.
		/// </p>
		/// <p class="body">
		/// The cloned FormattedString only takes its original configuration for this instance.
		/// If this instance is cloned and than changed, the clone will not be changed as well; it will
		/// remain as it was when it was cloned.
		/// </p>
		/// </remarks>
		/// <returns>A new FormattedString that is a copy of this one.</returns>
		public FormattedString Clone()
		{
			// MD 2/2/12 - TFS100573
			//return new FormattedString(this.element);
			Workbook workbook = this.sharedStringTable == null ? null : this.sharedStringTable.Workbook;
			return new FormattedString(workbook, this.element);
		}

		#endregion Clone

		// MD 1/31/12 - TFS100573
		#region ConvertToFormattedStringElement

		internal FormattedStringElement ConvertToFormattedStringElement()
		{
			FormattedStringElement formattedStringElement = this.element as FormattedStringElement;
			if (formattedStringElement != null)
				return formattedStringElement;

			Debug.Assert(this.element.ReferenceCount == 0, "This element should be detached.");
			formattedStringElement = new FormattedStringElement(this.element.UnformattedStringUTF8);
			this.element = formattedStringElement;
			return formattedStringElement;
		}

		#endregion // ConvertToFormattedStringElement

		#region GetFont ( int )

		/// <summary>
		/// Gets the font which controls the formatting properties in the string from the specified start index to 
		/// the end of the string.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the start index is greater than or equal to the length of the unformatted string, no exception 
		/// will be thrown. It will be thrown later when one of the formatting properties of the returned
		/// <see cref="FormattedStringFont"/> is set.
		/// </p>
		/// </remarks>
		/// <param name="startIndex">The index of the first character the returned font controls.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is less than zero.
		/// </exception>
		/// <returns>
		/// A FormattedStringFont instance which controls the formatting of the end portion of the string.
		/// </returns>
		public FormattedStringFont GetFont( int startIndex )
		{
			if ( startIndex < 0 )
				throw new ArgumentOutOfRangeException( "startIndex", startIndex, SR.GetString( "LE_ArgumentOutOfRangeException_NegativeStartIndex" ) );

			return new FormattedStringFont( this, startIndex, 0 );
		}

		#endregion GetFont ( int )

		#region GetFont ( int, int )

		/// <summary>
		/// Gets the font which controls the formatting properties in the string from the specified start index for
		/// the specified number of characters.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the start index is greater than or equal to the length of the unformatted string, no exception 
		/// will be thrown. It will be thrown later when one of the formatting properties of the returned
		/// <see cref="FormattedStringFont"/> is set.
		/// </p>
		/// </remarks>
		/// <param name="startIndex">The index of the first character the returned font controls.</param>
		/// <param name="length">The number of characters after the start index controlled by the returned font.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex"/> is less than zero.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="length"/> is less than one. A zero length string cannot be controlled by a formatting font.
		/// </exception>
		/// <returns>
		/// A FormattedStringFont instance which controls the formatting of a portion of the string.
		/// </returns>
		public FormattedStringFont GetFont( int startIndex, int length )
		{
			if ( startIndex < 0 )
				throw new ArgumentOutOfRangeException( "startIndex", startIndex, SR.GetString( "LE_ArgumentOutOfRangeException_NegativeStartIndex" ) );

			if ( length < 1 )
				throw new ArgumentOutOfRangeException( "length", length, SR.GetString( "LE_ArgumentOutOfRangeException_LengthMustBePositive" ) );

			return new FormattedStringFont( this, startIndex, length );
		}

		#endregion GetFont ( int, int )

		#region GetFormattingRuns






		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
		// MD 11/9/11 - TFS85193
		//internal FormattingRun[] GetFormattingRuns()
		internal FormattingRunBase[] GetFormattingRuns()
		{
			// MD 1/31/12 - TFS100573
			// The element now has a derived type which holds the formatting runs.
			//return this.element.FormattingRuns.ToArray();
			if (this.element is FormattedStringElement)
				return ((FormattedStringElement)this.element).FormattingRuns.ToArray();

			return new FormattingRunBase[0];
		}

		#endregion GetFormattingRuns

		#region OnRooted

		internal void OnRooted(GenericCachedCollection<StringElement> sharedStringTable)
		{
			// MD 2/2/12 - TFS100573
			//GenericCacheElementProxy<FormattedStringElement>.SetCollection(sharedStringTable, ref this.sharedStringTable, ref this.element);
			GenericCacheElement.SetCollection(sharedStringTable, ref this.sharedStringTable, ref this.element);
		}

		#endregion  // OnRooted

		#region SetWorksheet

		internal void SetWorksheet(Worksheet worksheet)
		{
			Workbook workbook = null;
			if (worksheet != null)
				workbook = worksheet.Workbook;

			((IFormattedString)this).SetWorkbook(workbook);
		} 

		#endregion // SetWorksheet

		#region VerifyOwner

		internal void VerifyOwner()
		{
			WorksheetCell cell = this.owner as WorksheetCell;
			if (cell != null)
			{
				FormattedString fs = cell.Value as FormattedString;
				if (fs == null || fs.Element != this.Element)
				{
					this.owner = null;
					this.ClearCollectionOnInvalidOwner();
				}
			}
		} 

		#endregion  // VerifyOwner

		#endregion Methods

		#region Properties

		#region Public Properties

		#region UnformattedString

		/// <summary>
		/// Gets or sets the unformatted string.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the new unformatted string assigned is shorter than the old unformatted string, all formatting
		/// outside the range of the new value will be lost.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is a null string.
		/// </exception>
		/// <value>The unformatted string.</value>
		public string UnformattedString
		{
			get { return this.element.UnformattedString; }
			set
			{
				if (this.UnformattedString == value)
					return;

				if (value == null)
					throw new ArgumentNullException("unformattedString", SR.GetString("LE_ArgumentNullException_UnformattedString"));

				this.BeforeSet();
				this.element.UnformattedString = value;
				this.AfterSet();

				// The owner of the string may have to do some processing when the unformatted string changes.
				if (this.Owner != null)
					this.Owner.OnUnformattedStringChanged(this);
			}
		}

		#endregion UnformattedString

		#endregion Public Properties

		#region Internal Properties

		#region Element

		internal StringElement Element
		{
			get { return this.element; }
		}

		#endregion // Proxy

		#region Owner

		internal IFormattedStringOwner Owner
		{
			get { return this.owner; }
			set 
			{
				if (this.owner == value)
					return;

				this.owner = value;

				Worksheet worksheet = this.owner == null ? null : this.owner.Worksheet;
				this.SetWorksheet(worksheet);
			}
		}

		#endregion Owner

		// MD 11/9/11 - TFS85193
		#region Workbook

		internal Workbook Workbook
		{
			get
			{
				if (this.owner == null)
					return null;

				Worksheet worksheet = this.owner.Worksheet;
				if (worksheet == null)
					return null;

				return worksheet.Workbook;
			}
		}

		#endregion  // Workbook

		#endregion Internal Properties

		#endregion Properties
	}

	#region IFormattedString interface

	internal interface IFormattedString
	{
		void SetWorkbook(Workbook workbook);
	} 

	#endregion  // IFormattedString interface

	#region StringElement class

	// MD 1/31/12 - TFS100573
	// Renamed to StringElement because this class no longer stored the formatting runs. 
	// It has a derived type to hold the formatting runs.
	//internal class FormattedStringElement : GenericCacheElement,
	//    IComparable<FormattedStringElement>
	[DebuggerDisplay("StringElement: {UnformattedString}")]		// MD 11/10/11 - TFS8519
	internal class StringElement : GenericCacheElement,
		IComparable<StringElement>
	{
		public static readonly byte[] EmptyStringUTF8 = new byte[0];

		#region Member Variables

		// MD 1/31/12 - TFS100573
		// Only the formatted strings need a formattingRuns collection, so this has been moved to the FormattedStringElement
		//// MD 11/9/11 - TFS85193
		////private List<FormattedStringRun> formattingRuns;
		//private List<FormattingRunBase> formattingRuns;

		// MD 2/1/12 - TFS100573
		// Instead of storing this here, we can now easily get the index from the collection as save time. 
		//private int indexInStringTable;

		private uint key;

		// MD 2/1/12 - TFS100573
		// For most strings in English, we can save twice as much memory by storing UTF8 encoded strings instead of .NET strings.
		//private string unformattedString;
		private byte[] unformattedStringUTF8;

		#endregion // Member Variables

		#region Constructor

		// MD 2/1/12 - TFS100573
		//public StringElement(Workbook workbook, string unformattedString)
		//    : base(workbook)
		//{
		//    this.unformattedString = unformattedString;
		//} 
		public StringElement(byte[] unformattedStringUTF8)
		{
			this.unformattedStringUTF8 = unformattedStringUTF8;
		}

		public StringElement(string unformattedString)
			: this(Encoding.UTF8.GetBytes(unformattedString)) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region Clone

		// MD 2/2/12 - TFS100573
		//public override object Clone()
		public override object Clone(Workbook workbook)
		{
			// MD 1/31/12 - TFS100573
			// There is no formatting on this class anymore.
			//FormattedStringElement clone = new FormattedStringElement(this.Workbook, this.unformattedString);
			//
			//if (this.HasFormatting)
			//{
			//    // MD 11/9/11 - TFS85193
			//    //foreach (FormattingRun run in this.formattingRuns)
			//    foreach (FormattedStringRun run in this.formattingRuns)
			//        clone.FormattingRuns.Add(run.Clone(clone));
			//}
			//
			//return clone;
			// MD 2/1/12 - TFS100573
			//return new StringElement(this.Workbook, this.unformattedString);
			return new StringElement(this.unformattedStringUTF8);
		}

		#endregion // Clone

		#region Equals

		public override bool Equals(object obj)
		{
			return this.HasSameData(obj as GenericCacheElement);
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			// MD 1/31/12 - TFS100573
			// There is no formatting on this class anymore.
			//int hashCode = this.unformattedString.GetHashCode();
			//
			//if (this.formattingRuns != null)
			//    hashCode += this.formattingRuns.Count;
			//
			//return hashCode;
			// MD 2/1/12 - TFS100573
			//return this.unformattedString.GetHashCode();
			unchecked
			{
				const int p = 16777619;
				int hash = (int)2166136261;

				for (int i = 0; i < this.unformattedStringUTF8.Length; i++)
					hash = (hash ^ this.unformattedStringUTF8[i]) * p;

				hash += hash << 13;
				hash ^= hash >> 7;
				hash += hash << 3;
				hash ^= hash >> 17;
				hash += hash << 5;
				return hash;
			}
		}

		#endregion // GetHashCode

		#region HasSameData

		public override bool HasSameData(GenericCacheElement otherElement)
		{
			// MD 1/31/12 - TFS100573
			// Added this as a performance enhancement.
			if (Object.ReferenceEquals(this, otherElement))
				return true;

			StringElement formattedString = otherElement as StringElement;

			if (formattedString == null)
				return false;

			// MD 2/1/12 - TFS100573
			//if (formattedString.unformattedString != this.unformattedString)
			//    return false;
			if (formattedString.unformattedStringUTF8 != this.unformattedStringUTF8)
			{
				if (formattedString.unformattedStringUTF8.Length != this.unformattedStringUTF8.Length)
					return false;

				for (int i = 0; i < this.unformattedStringUTF8.Length; i++)
				{
					if (formattedString.unformattedStringUTF8[i] != this.unformattedStringUTF8[i])
						return false;
				}
			}

			// MD 1/31/12 - TFS100573
			// There is no formatting on this class anymore.
			//int formattingRunsCount = this.formattingRuns == null ? 0 : this.formattingRuns.Count;
			//int otherFormattingRunsCount = formattedString.formattingRuns == null ? 0 : formattedString.formattingRuns.Count;
			//
			//if (formattingRunsCount != otherFormattingRunsCount)
			//    return false;
			//
			//if (formattingRunsCount > 0)
			//{
			//    for (int i = 0; i < formattingRunsCount; i++)
			//    {
			//        if (this.formattingRuns[i].Equals(formattedString.formattingRuns[i]) == false)
			//            return false;
			//    }
			//}
			if (formattedString.HasFormatting != this.HasFormatting)
				return false;

			return true;
		}

		#endregion // HasSameData

		// MD 1/16/12 - 12.1 - Cell Format Updates
		// This is no longer needed.
		#region Removed

		//#region RemoveUsedColorIndicies

		//public override void RemoveUsedColorIndicies(List<int> unusedIndicies)
		//{
		//    // We don't have to do anything here because the fonts on the formatting runs are stored in the workbook fonts collection.
		//}

		//#endregion // RemoveUsedColorIndicies

		#endregion // Removed

		#region ToString

		public override string ToString()
		{
			return this.UnformattedString;
		} 

		#endregion // ToString

		#endregion // Base Class Overrides

		#region Interfaces

		#region IComparable<FormattedStringElement> Members

		public int CompareTo(StringElement other)
		{
			// MD 2/1/12 - TFS100573
			//int result = String.Compare(this.unformattedString, other.unformattedString, StringComparison.Ordinal);
			int result = String.Compare(this.UnformattedString, other.UnformattedString, StringComparison.Ordinal);

			if (result != 0)
				return result;

			// MD 1/31/12 - TFS100573
			// Rewrote this now the the formatting has been moved to a derived type.
			#region Old Code

			//int otherFormattingRunsCount = other.formattingRuns == null ? 0 : other.formattingRuns.Count;

			//if (this.formattingRuns == null)
			//{
			//    if (otherFormattingRunsCount == 0)
			//        return 0;

			//    return -1;
			//}

			//result = this.formattingRuns.Count - otherFormattingRunsCount;

			//if (result != 0)
			//    return result;

			//for (int i = 0; i < this.formattingRuns.Count; i++)
			//{
			//    result =
			//        this.formattingRuns[i].FirstFormattedChar -
			//        other.formattingRuns[i].FirstFormattedChar;

			//    if (result != 0)
			//        return result;

			//    result =
			//        this.formattingRuns[i].GetHashCode() -
			//        other.formattingRuns[i].GetHashCode();

			//    if (result != 0)
			//        return result;
			//}

			//return result;

			#endregion // Old Code
			FormattedStringElement otherFormattedStringElement = other as FormattedStringElement;
			int otherFormattingRunsCount = other.HasFormatting
				? 0
				: otherFormattedStringElement.FormattingRuns.Count;

			if (this.HasFormatting == false)
			{
				if (otherFormattingRunsCount == 0)
					return 0;

				return -1;
			}

			FormattedStringElement formattedStringElement = (FormattedStringElement)this;
			result = formattedStringElement.FormattingRuns.Count - otherFormattingRunsCount;

			if (result != 0)
				return result;

			for (int i = 0; i < formattedStringElement.FormattingRuns.Count; i++)
			{
				result =
					formattedStringElement.FormattingRuns[i].FirstFormattedCharInOwner -
					otherFormattedStringElement.FormattingRuns[i].FirstFormattedCharInOwner;

				if (result != 0)
					return result;

				result =
					formattedStringElement.FormattingRuns[i].GetHashCode() -
					otherFormattedStringElement.FormattingRuns[i].GetHashCode();

				if (result != 0)
					return result;
			}

			return result;
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		#region InitSerializationCache

		// MD 1/31/12 - TFS100573
		// Made this virtual and moved the implementation to the derived class override.
		//// MD 1/18/12 - 12.1 - Cell Format Updates
		//// We once again need a IWorkbookFontDefaultsResolver because the comments have different font defaults from the shapes.
		////internal void InitSerializationCache(WorkbookSerializationManager serializationManager)
		////{
		////
		////    // MD 11/11/11 - TFS85193
		////    // This new overload is no longer needed now that shapes own FormattedText instead of FormatedStrings
		//////    // MD 8/23/11 - TFS84306
		//////    // Moved all code to this new overload.
		//////    this.InitSerializationCache(serializationManager, null);
		//////}
		//////
		//////// MD 8/23/11 - TFS84306
		//////// Added a new overload to take an instance to resolve font property defaults.
		//////internal void InitSerializationCache(WorkbookSerializationManager serializationManager, IWorkbookFontDefaultsResolver fontDefaultsResolver)
		//////{
		//internal void InitSerializationCache(WorkbookSerializationManager serializationManager, IWorkbookFontDefaultsResolver fontDefaultsResolver)
		//{
		//    // Add the formatted fonts from the formatted string to the serialization manager
		//    if (this.HasFormatting == false)
		//        return;
		//
		//    // MD 11/9/11 - TFS85193
		//    //foreach (FormattingRun run in this.FormattingRuns)
		//    foreach (FormattingRunBase run in this.FormattingRuns)
		//    {
		//        if (run.HasFont)
		//        {
		//            // MD 8/23/11 - TFS84306
		//            // Pass off the instance to resolve font property defaults.
		//            //serializationManager.AddFont(run.Font.Element, true);
		//            // MD 11/9/11 - TFS85193
		//            //serializationManager.AddFont(run.Font.Element, fontDefaultsResolver, true);
		//            // MD 1/10/12 - 12.1 - Cell Format Updates
		//            //serializationManager.AddFont(run.GetFontInternal(serializationManager.Workbook).Element, true);
		//            serializationManager.AddFont(run.GetFontInternal(serializationManager.Workbook), fontDefaultsResolver);
		//        }
		//    }
		//}
		internal virtual void InitSerializationCache(WorkbookSerializationManager serializationManager, IWorkbookFontDefaultsResolver fontDefaultsResolver) { }

		#endregion InitSerializationCache

		// MD 1/31/12 - TFS100573
		#region OnUnformattedStringChanged

		protected virtual void OnUnformattedStringChanged() { }

		#endregion // OnUnformattedStringChanged

		#region RemoveCarriageReturns







		// MD 2/2/12 - TFS100573
		//internal StringElement RemoveCarriageReturns()
		internal StringElement RemoveCarriageReturns(Workbook workbook)
		{
			// MD 2/1/12 - TFS100573
			//int carraigeReturnIndex = this.unformattedString.IndexOf('\r');
			string unformattedString = this.UnformattedString;
			int carraigeReturnIndex = unformattedString.IndexOf('\r');

			// If no carriage return is contained in this instance, return it
			if (carraigeReturnIndex < 0)
				return this;

			// Otherwise, we need to clone this instance and return the carriage returns from it
			// MD 2/2/12 - TFS100573
			//StringElement clonedString = (StringElement)this.Clone();
			StringElement clonedString = (StringElement)this.Clone(workbook);

			// MD 1/31/12 - TFS100573
			FormattedStringElement clonedFormattedStringElement = clonedString as FormattedStringElement;

			// The formatting runs are sorted in the order they appear in the string, we keep track of the formatting run
			// occurring after the carriage return in the string. This will be updated when we have to iterate and update the
			// formatting runs.
			int nextFormattingRunIndex = 0;

			while (carraigeReturnIndex >= 0)
			{
				// Remove the carriage return from the string.
				// MD 2/1/12 - TFS100573
				// Use the local string variable.
				//clonedString.unformattedString = clonedString.unformattedString.Remove(carraigeReturnIndex, 1);
				unformattedString = unformattedString.Remove(carraigeReturnIndex, 1);

				// If the carriage return did not have a newline character after it, insert a newline where the carriage return 
				// was. If we do this, the overall length of the string has not changes, so the formatting doesn't need to be 
				// updated.
				// MD 2/1/12 - TFS100573
				// Use the local string variable.
				//if (carraigeReturnIndex < clonedString.unformattedString.Length &&
				//    clonedString.unformattedString[carraigeReturnIndex] != '\n')
				//{
				//    clonedString.unformattedString = clonedString.unformattedString.Insert(carraigeReturnIndex, "\n");
				//}
				if (carraigeReturnIndex < unformattedString.Length &&
					unformattedString[carraigeReturnIndex] != '\n')
				{
					unformattedString = unformattedString.Insert(carraigeReturnIndex, "\n");
				}
				// If the carriage return did have a newline after it, we will not be inserting a new character. This has changed
				// the length of the string and the formatting runs after the removed carriage return must be updated to be at the 
				// correct place in the new string.
				// MD 1/31/12 - TFS100573
				// The element with formatting may be null now.
				//else if (clonedString.HasFormatting)
				else if (clonedFormattedStringElement != null && clonedFormattedStringElement.HasFormatting)
				{
					// Start iterating at the first formatting run that was on or after the last carriage return.
					for (int i = nextFormattingRunIndex; i < clonedFormattedStringElement.FormattingRuns.Count; i++)
					{
						// MD 11/9/11 - TFS85193
						//FormattedStringRun formattingRun = clonedString.formattingRuns[i];
						FormattingRunBase formattingRun = clonedFormattedStringElement.FormattingRuns[i];

						// If the formatting run is before the current carriage return, skip it and increment the 
						// nextFormattingRunIndex pointer so it will end up pointing to the first formatting run on or after the 
						// current carriage return.
						// MD 11/9/11 - TFS85193
						//if (formattingRun.FirstFormattedChar < carraigeReturnIndex)
						if (formattingRun.FirstFormattedCharInOwner < carraigeReturnIndex)
						{
							nextFormattingRunIndex++;
							continue;
						}

						// If the formatting run was on or after the current carriage return, update it so it still points to the
						// same character after removing the carriage return.
						// MD 11/9/11 - TFS85193
						//formattingRun.FirstFormattedChar--;
						formattingRun.FirstFormattedCharInOwner--;
					}
				}

				// Find the index of the next carriage return
				// MD 2/1/12 - TFS100573
				// Use the local string variable.
				//carraigeReturnIndex = clonedString.unformattedString.IndexOf('\r', carraigeReturnIndex);
				carraigeReturnIndex = unformattedString.IndexOf('\r', carraigeReturnIndex);
			}

			// MD 2/1/12 - TFS100573
			// Update the string on the clone.
			clonedString.UnformattedString = unformattedString;

			return clonedString;
		}

		#endregion RemoveCarriageReturns

		#region SetWorksheet

		internal static void SetWorkbook(Workbook workbook, ref GenericCachedCollection<StringElement> sharedStringTable, ref StringElement element)
		{
			// MD 8/23/11 - TFS84306
			// Moved all code to the new overload.
			StringElement.SetWorkbook(workbook, null, ref sharedStringTable, ref element);
		}

		// MD 8/23/11 - TFS84306
		// Added a new overload to take the owner.
		internal static void SetWorkbook(Workbook workbook, IFormattedStringOwner owner, ref GenericCachedCollection<StringElement> sharedStringTable, ref StringElement element)
		{
			GenericCachedCollection<StringElement> newSharedStringTable =
				// MD 8/23/11 - TFS84306
				// Strings on shapes should not go in the shared string table.
				//workbook == null ? null : workbook.SharedStringTable;
				(workbook == null || owner is WorksheetShape) ? null : workbook.SharedStringTable;

			GenericCachedCollection<WorkbookFontData> newCollection =
				workbook == null ? null : workbook.Fonts;

			// MD 2/2/12 - TFS100573
			// The StringElement no longer has a reference to the Workbook.
			//element.Workbook = workbook;

			// MD 1/31/12 - TFS100573
			//if (element.HasFormatting)
			//{
			//    foreach (FormattingRun run in element.FormattingRuns)
			if (element.HasFormatting)
			{
				// MD 11/9/11 - TFS85193
				//foreach (FormattingRun run in element.FormattingRuns)
				foreach (FormattingRunBase run in ((FormattedStringElement)element).FormattingRuns)
				{
					if (run.HasFont)
					{
						// MD 11/9/11 - TFS85193
						//run.Font.OnRooted(newCollection);
						run.GetFontInternal(workbook).OnRooted(newCollection);
					}
				}
			}

			// MD 2/2/12 - TFS100573
			//GenericCacheElementProxy<FormattedStringElement>.SetCollection(newSharedStringTable, ref sharedStringTable, ref element);
			GenericCacheElement.SetCollection(newSharedStringTable, ref sharedStringTable, ref element);
		}

		#endregion // SetWorksheet

		#endregion // Methods

		#region Properties

		// MD 1/31/12 - TFS100573
		// These have been moved to the derived class which stores the formatting.
		#region Removed

		//#region FormattingRuns

		//public virtual List<FormattingRun> FormattingRuns
		//{
		//    get { return null; }
		//}

		//public virtual bool HasFormatting
		//{
		//    get { return false; }
		//}

		//#endregion // FormattingRuns

		#endregion // Removed

		// MD 1/31/12 - TFS100573
		#region HasFormatting

		public virtual bool HasFormatting
		{
			get { return false; }
		}

		#endregion // HasFormatting

		// MD 2/1/12 - TFS100573
		// Instead of storing this here, we can now easily get the index from the collection as save time. 
		#region Removed

		//#region IndexInStringTable

		//public int IndexInStringTable
		//{
		//    get { return this.indexInStringTable; }
		//    set { this.indexInStringTable = value; }
		//}

		//#endregion // IndexInStringTable

		#endregion // Removed

		#region Key

		public uint Key
		{
			get { return this.key; }
			set { this.key = value; }
		}

		#endregion // Key

		#region UnformattedString

		public string UnformattedString
		{
			// MD 2/1/12 - TFS100573
			// We need to convert back from the UTF8 encoded bytes to a string.
			//get { return this.unformattedString; }
			get { return Encoding.UTF8.GetString(this.unformattedStringUTF8); }
			set
			{
				// MD 2/1/12 - TFS100573
				// Store the UTF8 bytes from the string.
				//this.unformattedString = value;
				this.unformattedStringUTF8 = Encoding.UTF8.GetBytes(value);

				// MD 1/31/12 - TFS100573
				// Moved this code to an override of the new OnUnformattedStringChanged method.
				#region Moved

				//if (this.HasFormatting)
				//{
				//    // MD 11/10/11 - TFS85193
				//    // This code was moved to a helper method so it could be used in other places.
				//    #region Moved

				//    //for (int i = this.formattingRuns.Count - 1; i >= 0; i--)
				//    //{
				//    //    if (this.unformattedString.Length <= this.formattingRuns[i].FirstFormattedCharAbsolute)
				//    //    {
				//    //        // MD 11/9/11 - TFS85193
				//    //        //FormattedStringRun run = this.formattingRuns[i];
				//    //        FormattedStringRun run = (FormattedStringRun)this.formattingRuns[i];
				//    //
				//    //        if (run.HasFont)
				//    //        {
				//    //            // MD 11/9/11 - TFS85193
				//    //            //run.Font.OnUnrooted();
				//    //            run.GetFontInternal(this.Workbook).OnUnrooted();
				//    //        }
				//    //
				//    //        this.formattingRuns.RemoveAt(i);
				//    //    }
				//    //    else
				//    //        break;
				//    //}

				//    #endregion  // Moved
				//    Utilities.TrimFormattingRuns(this);
				//}

				#endregion // Moved
				this.OnUnformattedStringChanged();
			}
		}

		#endregion // UnformattedString

		// MD 2/1/12 - TFS100573
		#region UnformattedStringUTF8

		public byte[] UnformattedStringUTF8
		{
			get { return this.unformattedStringUTF8; }
		}

		#endregion // UnformattedStringUTF8

		#endregion // Properties
	} 

	#endregion // StringElement class

	// MD 1/31/12 - TFS100573
	#region FormattedStringElement class

	internal class FormattedStringElement : StringElement,
		IFormattedRunOwner		// MD 11/9/11 - TFS85193
	{
		#region Member Variables

		private List<FormattingRunBase> formattingRuns;

		#endregion // Member Variables

		#region Constructor

		// MD 2/1/12 - TFS100573
		public FormattedStringElement(byte[] unformattedStringUTF8)
			: base(unformattedStringUTF8) { }

		public FormattedStringElement(string unformattedString)
			: base(unformattedString) { }

		#endregion // Constructor

		#region Base Class Overrides

		#region Clone

		public override object Clone(Workbook workbook)
		{
			FormattedStringElement clone = new FormattedStringElement(this.UnformattedStringUTF8);

			if (this.HasFormatting)
			{
				foreach (FormattingRunBase run in this.FormattingRuns)
					clone.FormattingRuns.Add(run.Clone(workbook, clone));
			}

			return clone;
		}

		#endregion // Clone

		#region Equals

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		#endregion Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			int hashCode = this.UnformattedString.GetHashCode();

			if (this.HasFormatting)
				hashCode += this.FormattingRuns.Count;

			return hashCode;
		}

		#endregion // GetHashCode

		#region HasSameData

		public override bool HasSameData(GenericCacheElement otherElement)
		{
			if (Object.ReferenceEquals(this, otherElement))
				return true;

			FormattedStringElement formattedString = otherElement as FormattedStringElement;

			if (formattedString == null)
				return false;

			if (formattedString.UnformattedString != this.UnformattedString)
				return false;

			int formattingRunsCount = this.HasFormatting == false ? 0 : this.FormattingRuns.Count;
			int otherFormattingRunsCount = formattedString.HasFormatting == false ? 0 : formattedString.FormattingRuns.Count;

			if (formattingRunsCount != otherFormattingRunsCount)
				return false;

			if (formattingRunsCount > 0)
			{
				for (int i = 0; i < formattingRunsCount; i++)
				{
					if (this.FormattingRuns[i].Equals(formattedString.FormattingRuns[i]) == false)
						return false;
				}
			}

			return true;
		}

		#endregion // HasSameData

		#region InitSerializationCache

		internal override void InitSerializationCache(WorkbookSerializationManager serializationManager, IWorkbookFontDefaultsResolver fontDefaultsResolver)
		{
			// Add the formatted fonts from the formatted string to the serialization manager
			if (this.HasFormatting == false)
				return;

			foreach (FormattingRunBase run in this.FormattingRuns)
			{
				if (run.HasFont)
					serializationManager.AddFont(run.GetFontInternal(serializationManager.Workbook), fontDefaultsResolver);
			}
		}

		#endregion InitSerializationCache

		#region OnUnformattedStringChanged

		protected override void OnUnformattedStringChanged()
		{
			if (this.HasFormatting)
				Utilities.TrimFormattingRuns(this);
		}

		#endregion // OnUnformattedStringChanged

		#endregion // Base Class Overrides

		#region Interfaces

		// MD 11/9/11 - TFS85193
		#region IFormattedRunOwner Members

		void IFormattedRunOwner.AddRun(FormattingRunBase run)
		{
			FormattedStringRun stringRun = run as FormattedStringRun;
			if (stringRun == null)
			{
				Utilities.DebugFail("The run should be a FormattedStringRun instance.");
				return;
			}

			this.FormattingRuns.Add(stringRun);
		}

		FormattingRunBase IFormattedRunOwner.CreateRun(int absoluteStartIndex)
		{
			return new FormattedStringRun(this, absoluteStartIndex);
		}

		List<FormattingRunBase> IFormattedRunOwner.GetFormattingRuns(Workbook workbook)
		{
			return this.FormattingRuns;
		}

		void IFormattedRunOwner.InsertRun(int runIndex, FormattingRunBase run)
		{
			FormattedStringRun stringRun = run as FormattedStringRun;
			if (stringRun == null)
			{
				Utilities.DebugFail("The run should be a FormattedStringRun instance.");
				return;
			}

			this.FormattingRuns.Insert(runIndex, stringRun);
		}

		int IFormattedRunOwner.StartIndex
		{
			get { return 0; }
		}

		#endregion // Interfaces

		#endregion // Interfaces

		// MD 1/29/12 - 12.1 - Cell Format Updates
		#region ClearFormatting

		internal void ClearFormatting()
		{
			if (this.HasFormatting == false)
				return;

			for (int i = 0; i < this.formattingRuns.Count; i++)
			{
				FormattingRunBase run = this.formattingRuns[i];
				if (run.HasFont)
					run.GetFontInternal().OnUnrooted();
			}

			this.formattingRuns = null;
		}

		#endregion // ClearFormatting

		#region FormattingRuns

		public List<FormattingRunBase> FormattingRuns
		{
			get
			{
				if (this.formattingRuns == null)
					this.formattingRuns = new List<FormattingRunBase>();

				return this.formattingRuns;
			}
		}

		public override bool HasFormatting
		{
			get { return this.formattingRuns != null && this.formattingRuns.Count > 0; }
		}

		#endregion // FormattingRuns
	}

	#endregion // FormattedStringElement class

	#region FormattedStringValueReference class






	internal class FormattedStringValueReference :
		IFormattedString
	{
		#region Member Variables

		private StringElement element;
		private GenericCachedCollection<StringElement> sharedStringTable;

		private object value;

		#endregion  // Member Variables

		#region Constructors

		public FormattedStringValueReference(object value, Workbook workbook)
		{
			this.sharedStringTable = workbook == null ? null : workbook.SharedStringTable;

			this.value = value;

			// MD 11/11/11 - TFS94281
			// A char value of 0 should be written out as an empty string because a string with a 0 character is not valid in Excel.
			//this.element = GenericCacheElement.FindExistingOrAddToCache(
			//    new FormattedStringElement(workbook, this.value.ToString()),
			//    this.sharedStringTable);
			string valueString = this.value.ToString();
			if (value is char && (char)value == 0)
				valueString = string.Empty;

			this.element = GenericCacheElement.FindExistingOrAddToCache(
				// MD 2/2/12 - TFS100573
				// The StringElement no longer has a reference to the Workbook.
				//new StringElement(workbook, valueString),
				new StringElement(valueString),
				this.sharedStringTable);
		}

		#endregion  // Constructors

		#region IFormattedString Members

		void IFormattedString.SetWorkbook(Workbook workbook)
		{
			StringElement.SetWorkbook(workbook, ref this.sharedStringTable, ref this.element);
		}

		#endregion

		#region Base Class Overrides

		public override string ToString()
		{
			return this.element.ToString();
		}

		#endregion  // Base Class Overrides

		#region Properties

		#region Element

		public StringElement Element
		{
			get { return this.element; }
		}

		#endregion  // Element

		#region Value

		public object Value
		{
			get { return this.value; }
		}

		#endregion  // Value

		#endregion  // Properties
	}

	#endregion // FormattedStringValueReference class

	// MD 11/9/11 - TFS85193
	internal interface IFormattedItem
	{
		object Owner { get; }
		Workbook Workbook { get; }

		IFormattedRunOwner GetOwnerAt(int startIndex);
		void OnFormattingChanged();
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