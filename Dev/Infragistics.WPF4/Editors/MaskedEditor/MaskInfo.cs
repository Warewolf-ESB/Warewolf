using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Editors;

namespace Infragistics.Windows.Editors
{
	internal class MaskInfo
	{
		#region private variables

		private XamMaskedEditor			_maskEditor;
		private System.Type				_dataType;
		private MaskMode				_displayMode;
		private MaskMode				_dataMode;
		private MaskMode				_clipMode;
		private string					_format;
		private IFormatProvider			_formatProvider;
		// SSP 7/9/08 BR34636
		// 
		private CultureInfo				_cultureInfo;

		private SectionsCollection		_sections;
		
		private string _lastParsedMask;
		// SSP 9/26/07 BR26063
		// 
		private IFormatProvider _lastParsedMask_FormatProvider;

		// SSP 11/19/03 UWE749 / Optimizations
		// Added code to cache min and max value.
		// 
		private object _cached_minValue;
		private object _cached_maxValue;

		// MRS 12/12/05 - BR07946
		//
		private object _cached_minExclusive;
		private object _cached_maxExclusive;

        // AS 10/17/08 TFS8886
        private bool _skipDigitSeparator;

		// MD 4/22/11 - TFS73181
		private char _cachedCommaChar;
		private char _cachedDecimalSeperatorChar;
		private char _cachedPlusSignChar;
		private char _cachedMinusSignChar;

		// MD 4/25/11 - TFS73181
		private MaskDefaultType _maskDefaultType;

		#endregion //private variables

		#region Constructors

		internal MaskInfo( )
		{
		}
		





		internal MaskInfo( XamMaskedEditor maskEditor )
		{
			this.Initialize( maskEditor, true, false );
		}

		#endregion //Constructors
		
		#region Private/Internal Methods/Properties

		#region CreateMaskInfoForConverter

		internal static MaskInfo CreateMaskInfoForConverter( XamMaskedEditor maskEditor )
		{
			MaskInfo maskInfo = new MaskInfo( );
			maskInfo.Initialize( maskEditor, false, true );
			return maskInfo;
		}

		#endregion // CreateMaskInfoForConverter

		#region ReleaseMaskInfoForConverter

		// SSP 5/19/08 BR33116
		// 
		internal static void ReleaseMaskInfoForConverter( MaskInfo maskInfo )
		{
			SectionsCollection sections = maskInfo.Sections;
			if ( null != sections )
				sections.Initialize( null );
		}

		#endregion // ReleaseMaskInfoForConverter

		#region InitializeFormatProvider

		// SSP 7/9/08 BR34636
		// 

		internal void InitializeFormatProvider( IFormatProvider formatProvider, CultureInfo cultureInfo )
		{
			_formatProvider = null != formatProvider ? formatProvider : cultureInfo;
			_cultureInfo = null != cultureInfo ? cultureInfo : formatProvider as CultureInfo;

			// MD 4/22/11 - TFS73181
			// Dirty all cached literal character values, because they depend on the format provider.
			this.ResetCachedCharacters();
		}

		internal void InitializeFormatProvider( )
		{
			this.InitializeFormatProvider( _maskEditor.FormatProviderResolved, _maskEditor.CultureInfoResolved );
		}

		#endregion // InitializeFormatProvider

		#region Initialize

		private void Initialize( XamMaskedEditor maskEditor, bool setValue, bool maskInfoForConverter )
		{
			Utils.ValidateNull( maskEditor );
			_maskEditor = maskEditor;
			_format = maskEditor.Format;

			// SSP 7/9/08 BR34636
			// Use the new InitializeFormatProvider method.
			// 
			//_formatProvider = maskEditor.FormatProvider;
			this.InitializeFormatProvider( );

			_dataType = Utilities.GetUnderlyingType( maskEditor.ValueType );
			_clipMode = maskEditor.ClipMode;
			_dataMode = maskEditor.DataMode;
			_displayMode = maskEditor.DisplayMode;

            // AS 12/1/08 TFS11022
            _skipDigitSeparator = ShouldSkipDigitSeparator(_dataType);

			
			
			
			
			this.InitializedCachedMinMaxValues( );
			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			

			this.CreateSections( maskInfoForConverter );

			if ( setValue )
				this.InternalRefreshValue( maskEditor.Value );
		}

		#endregion // Initialize

		#region ClearCachedMinMaxValues







		internal void ClearCachedMinMaxValues()
		{
			_cached_minValue = null;
			_cached_maxValue = null;	
			_cached_minExclusive = null;
			_cached_maxExclusive = null;
		}

		#endregion // ClearCachedMinMaxValues

		#region InitializedCachedMinMaxValues

		
		
		
		internal void InitializedCachedMinMaxValues( )
		{
			ValueConstraint valueConstraint = null != _maskEditor ? _maskEditor.ValueConstraint : null;
			if ( null != valueConstraint )
			{
				_cached_maxExclusive = valueConstraint.MaxExclusive;
				_cached_maxValue = valueConstraint.MaxInclusive;
				_cached_minExclusive = valueConstraint.MinExclusive;
				_cached_minValue = valueConstraint.MinInclusive;
			}
			else
			{
				this.ClearCachedMinMaxValues( );
			}
		}

		#endregion // InitializedCachedMinMaxValues

		#region DataMode






		internal MaskMode DataMode
		{
			get
			{
				return _dataMode;
			}
			set
			{
				_dataMode = value;
			}
		}

		#endregion // DataMode

		#region ClipMode






		internal MaskMode ClipMode
		{
			get
			{
				return _clipMode;
			}
			set
			{
				_clipMode = value;
			}
		}

		#endregion // ClipMode

		#region DisplayMode






		internal MaskMode DisplayMode
		{
			get
			{
				// Always display the prompt characters during design time.
				//
				if ( this.DesignMode )
					return MaskMode.IncludeBoth;

				return _displayMode;
			}
			set
			{
				_displayMode = value;
			}
		}

		#endregion // DisplayMode

		#region InternalRefreshValue

		
		
		
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Gets the value from the owner and sets it on the display chars
		/// collection.
		/// </summary>
		/// <returns>
		/// If a value indicating whether the value was successfully set.
		/// </returns>
		internal bool InternalRefreshValue( object dataValue )
		{
			Exception error;
			return this.InternalRefreshValue( dataValue, out error );
		}

		/// <summary>
		/// Gets the value from the owner and sets it on the display chars
		/// collection.
		/// </summary>
		/// <returns>
		/// If there's an error (for example the specified value doesn't match the mask), 
		/// returns the error information. Otherwise returns null.
		/// </returns>
		// SSP 5/19/09 - Clipboard Support
		// Added an overload that takes in error out param.
		// 
		internal bool InternalRefreshValue( object dataValue, out Exception error )
		{
			Debug.Assert( null != this.Sections, "No sections !" );

			if ( null == this.Sections )
			{
				// SSP 5/19/09 - Clipboard Support
				// Added an overload that takes in error out param.
				// 
				//return;
				error = new InvalidOperationException( "Invalid mask." );
				return false;
			}

			// SSP 9/22/03
			//
			//object val = this.Owner.GetValue( this.OwnerContext );
			object val = dataValue;

			
			
			
			
			
			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


			XamMaskedEditor maskedEditor = this.MaskEditor;

            // SSP 6/7/10 - Optimization - TFS34031
            // We're getting the oldText and newText and comparing only for the purposes of
            // maintaining undo history. Since we only maintain undo history when in edit mode,
            // we shouldn't bother getting old text and new text if we are not in edit mode.
            // 
            // --------------------------------------------------------------------------------
            //string oldText = XamMaskedEditor.GetText( this.Sections, MaskMode.IncludeBoth, this );
            EditInfo editInfo = null != maskedEditor ? maskedEditor.EditInfo : null;

            string oldText = null;
            if ( null != editInfo )
			    oldText = XamMaskedEditor.GetText( this.Sections, MaskMode.IncludeBoth, this );
            // --------------------------------------------------------------------------------

			// SSP 5/19/09 - Clipboard Support
			// Return a value indicating whether the value was succefully set.
			// 
			error = null;

			if ( null != val )
			{
				// SSP 7/25/02 UWG1445
				// Enclosed the SetDataValue call in a try-catch block because that will
				// throw an exception of the val is an invalid value or can't be converted
				// to the data type.
				//
				try
				{
					// SSP 5/19/09 - Clipboard Support
					// Return a value indicating whether the value was succefully set.
					// 
					//XamMaskedEditor.SetDataValue( this.Sections, this.DataType, val, this );
					if ( !XamMaskedEditor.SetDataValue( this.Sections, this.DataType, val, this ) )
						error = new ArgumentException(XamMaskedEditor.GetString("LMSG_EnteredValueInvalid"));
				}
				catch ( Exception exception )
				{
					//	BF 8.20.04	UWG3607
					//
					//	In the UWG3607 scenario, an exception was being thrown
					//	in SetDataValue when we tried to convert an empty string
					//	to a DateTime. The value of 'oldText' in some cases reflected
					//	the value of some other cell (since we reused the UIElement),
					//	and that never got changed here because the string comparison
					//	below causes this method to return false when the oldText and
					//	newText are the same. Since we call EraseAllChars when the value
					//	provided by the owner is null, it would seem correct to do that
					//	when we are unable to convert the value as well.
					//
					this.EraseAllChars( );

					// SSP 5/19/09 - Clipboard Support
					// Return a value indicating whether the value was succefully set.
					// 
					error = exception;
				}
			}
			else
			{
				this.EraseAllChars( );
			}

            // SSP 6/7/10 - Optimization
            // Enclosed the existing code into the if block.
            // 
            if ( null != editInfo )
            {
                string newText = XamMaskedEditor.GetText( this.Sections, MaskMode.IncludeBoth, this );

                if ( ( null == oldText && null == newText ) ||
                    // JJD 12/10/02 - FxCop
                    // Pass the culture info in explicitly
                    //( 0 == string.Compare( newText, oldText ) ) )
                    ( 0 == string.Compare( newText, oldText, false, System.Globalization.CultureInfo.CurrentCulture ) ) )
                    
                    
                    
                    return null == error;

                // SSP 3/7/07 BR19849
                // Clear the undo history.
                // 
                
                
                
                
                if ( null != editInfo && editInfo.MaskInfo == this )
                    editInfo.ManageUndoHistory_TextChanged( false, null );
            }

			
			
			
			return null == error;
		}
		
		#endregion // InternalRefreshValue
		
		#region MaskEdior






		internal XamMaskedEditor MaskEditor
		{
			get
			{
				return _maskEditor;
			}
		}

		#endregion //MaskEditor

		#region DataType






		internal System.Type DataType
		{
			get
			{
				return _dataType;
			}
			set
			{
				// MD 4/22/11 - TFS73181
				// If the data type isn't changing, we shouldn't do anything.
				if (_dataType == value)
					return;

				_dataType = value;

                // AS 10/17/08 TFS8886
                // If we're using a converter then we're going to assume that we
                // should not skip digit separators when setting the text.
                //
                // AS 12/1/08 TFS11022
                //XamMaskedEditor.SupportsDataType(value, out _skipDigitSeparator);
                _skipDigitSeparator = ShouldSkipDigitSeparator(value);

				// MD 4/22/11 - TFS73181
				// Dirty all cached literal character values, because they depend on the data type.
				this.ResetCachedCharacters();
			}
		}

		#endregion // DataType

        // AS 10/17/08 TFS8886
        #region SkipDigitSeparator
        internal bool SkipDigitSeparator
        {
            get { return _skipDigitSeparator; }
        }
        #endregion //SkipDigitSeparator

        #region FormatProvider






		internal IFormatProvider FormatProvider
		{
			get
			{
				return _formatProvider;
			}
		}

		#endregion //FormatProvider

		#region Format



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal string Format
		{
			get
			{
				return _format;
			}
			set
			{
				_format = value;
			}
		}

		#endregion //Format		

		#region HasFormat






		internal bool HasFormat
		{
			get
			{
				return null != _format && _format.Length > 0;
			}
		}

		#endregion // HasFormat
		
		#region CultureInfo







		internal CultureInfo CultureInfo
		{
			get
			{
				
				
				return _cultureInfo;
				
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			}
		}

		#endregion //CultureInfo

		#region MinValue






		internal object MinValue
		{
			get
			{
				return _cached_minValue;
			}
		}

		#endregion // MinValue

		#region MaxValue






		internal object MaxValue
		{
			get
			{
				return _cached_maxValue;
			}
		}

		#endregion // MaxValue

		#region MinExclusive






		internal object MinExclusive
		{
			get
			{
				return _cached_minExclusive;
			}
		}

		#endregion // MinExclusive

		#region MaxExclusive






		internal object MaxExclusive
		{
			get
			{
				return _cached_maxExclusive;
			}
		}

		#endregion // MaxExclusive

		#region IsBeingEditedAndFocused

		/// <summary>
		/// Returns true if in edit mode and the associated control is focused.
		/// </summary>
		internal bool IsBeingEditedAndFocused
		{
			get
			{
				XamMaskedEditor maskedEditor = this.MaskEditor;

				return this.IsBeingEdited &&
					// SSP 7/19/07 BR22754
					// We need to appear as if we have keyboard focus when we are displaying context menu.
					// 
					//this.MaskEditor.IsKeyboardFocusWithin;
					( maskedEditor.IsKeyboardFocusWithin || maskedEditor._isDisplayingContextMenu )
					// SSP 10/17/07 UWG2076 BR27228 
					// Read the comment above transitingIntoEditModeInMouseDown declaration for
					// the explanation.
					//
					&& ! maskedEditor._transitingIntoEditModeInMouseDown;
			}
		}

		#endregion // IsBeingEditedAndFocused

		#region IsBeingEdited

		/// <summary>
		/// Returns true if in edit mode.
		/// </summary>
		internal bool IsBeingEdited
		{
			get
			{				
				return null != this.MaskEditor && this.MaskEditor.IsInEditMode;
			}
		}

		#endregion // IsBeingEdited

        // AS 12/1/08 TFS11022
        // We should have been negating the usesConverter - i.e. we only want to 
        // avoid skipping digit separators if we are using a converter.
        //
        #region ShouldSkipDigitSeparator
        private bool ShouldSkipDigitSeparator(Type dataType)
        {
            bool usesConverter;
            XamMaskedEditor.SupportsDataType(dataType, out usesConverter);
            return !usesConverter;
        }
        #endregion //ShouldSkipDigitSeparator

        #endregion // Private/Internal properties






		internal char CommaChar
		{
			get
			{
				// MD 4/22/11 - TFS73181
				// We now cache the culture characters.
				//return this.GetCultureChar( ',' );
				return this.GetCultureChar(',', ref this._cachedCommaChar);
			}
		}






		internal char DecimalSeperatorChar
		{
			get
			{
				// MD 4/22/11 - TFS73181
				// We now cache the culture characters.
				//return this.GetCultureChar( '.' );
				return this.GetCultureChar('.', ref this._cachedDecimalSeperatorChar);
			}
		}

		// MD 4/22/11 - TFS73181





		internal char MinusSignChar
		{
			get
			{
				return this.GetCultureChar('-', ref this._cachedMinusSignChar);
			}
		}

		// MD 4/22/11 - TFS73181





		internal char PlusSignChar
		{
			get
			{
				return this.GetCultureChar('+', ref this._cachedPlusSignChar);
			}
		}

		// MD 4/22/11 - TFS73181
		// We now cache the culture characters.
		//internal char GetCultureChar( char c )
		private char GetCultureChar(char c, ref char cachedCharacter)
		{
			// MD 4/22/11 - TFS73181
			//// SSP 9/30/03 UWE503
			////
			//// ------------------------------------------------------------------------------
			////return XamMaskedEditor.GetCultureChar( c, this.FormatProvider );
			//return XamMaskedEditor.GetCultureChar( c, this );
			//// ------------------------------------------------------------------------------
			//
			// If the character is not cached yet, determine the culture character and cache it.
			if (cachedCharacter == (char)0)
				cachedCharacter = XamMaskedEditor.GetCultureChar(c, this);

			return cachedCharacter;
		}

		internal void EraseAllChars( )
		{
			XamMaskedEditor.EraseAllChars( this.Sections );
		}

		internal string ResolvedMask
		{
			get
			{
				string mask;
				this.MaskEditor.ResolveMask( this.DataType, this.FormatProvider, out mask );
				Debug.Assert( null != mask && mask.Length > 0 );

				return mask;
			}
		}

		internal void RecreateSections( bool dontRefreshValue )
		{
			this.CreateSections( false );

			if ( ! dontRefreshValue )
				this.InternalRefreshValue( this.MaskEditor.Value );
		}

		private bool CreateSections( bool maskInfoForConverter )
		{
			// MD 4/25/11 - TFS73181
			// Call off to the helper method which will get the maskDefaultType
			//string mask = this.ResolvedMask;
			string mask;
			this.MaskEditor.ResolveMask(this.DataType, this.FormatProvider, out mask, out _maskDefaultType);
			
			if ( null != mask && mask.Length > 0 )
			{
				if ( null != _lastParsedMask )
				{
					if ( null != _sections && mask.Equals( _lastParsedMask )
						// SSP 9/26/07 BR26063
						// 
						&& _lastParsedMask_FormatProvider == this.FormatProvider )
					{
						// If we are already using sections with the same mask, then
						// just return

						return true;
					}
				}				
			}

			if ( null != mask && mask.Length > 0 )
				_sections = MaskInfo.GetCachedParsedMask( mask, this, ! maskInfoForConverter );
			else
				_sections = null;

			XamMaskedEditor editor = this.MaskEditor;
			if ( null != editor && editor.MaskInfoIfAllocated == this )
				// SSP 8/7/07 
				// Made sections read-only.
				// 
				//editor.Sections = this.Sections;
				editor.InternalSetSections( this.Sections );

			if ( null != _sections )
			{
				_lastParsedMask = mask;

				// SSP 9/26/07 BR26063
				// 
				_lastParsedMask_FormatProvider = this.FormatProvider;

				Type dataType = this.DataType;

				// Also according to the mask set the max values of the section if appropriate.
				// For example, if the data type is byte, then max value should be 255.
				//
				if ( null != _sections &&
					( typeof( byte ) == dataType ||
					typeof( sbyte ) == dataType ||
					typeof( short ) == dataType ||
					typeof( ushort ) == dataType ||
					typeof( int ) == dataType ||
					typeof( uint ) == dataType ||
					typeof( long ) == dataType ||
					typeof( ulong ) == dataType ) )
				{
					NumberSection section = (NumberSection)XamMaskedEditor.GetSection( this._sections, typeof( NumberSection ) );

					if ( null != section &&
						section.GetType( ) == typeof( NumberSection ) &&
						section.DisplayChars.Count > 2 )
					{
						decimal maxVal = 0;

						if ( typeof( byte ) == dataType )
							maxVal = byte.MaxValue;
						else if ( typeof( sbyte ) == dataType )
							maxVal = sbyte.MaxValue;
						else if ( typeof( short ) == dataType )
							maxVal = short.MaxValue;
						else if ( typeof( ushort ) == dataType )
							maxVal = ushort.MaxValue;
						else if ( typeof( int ) == dataType )
							maxVal = int.MaxValue;
						else if ( typeof( uint ) == dataType )
							maxVal = uint.MaxValue;
						else if ( typeof( long ) == dataType )
							maxVal = long.MaxValue;
						else if ( typeof( ulong ) == dataType )
							maxVal = ulong.MaxValue;

						if ( 0 != maxVal && section.MaxValue > maxVal )
							section.MaxValue = maxVal;
					}
				}
			}

			return true;
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal class MaskCache
		{
			private string mask;
			private IFormatProvider formatProvider;

			internal MaskCache( string mask, IFormatProvider formatProvider )
			{
				if ( null == mask )
					throw new ArgumentNullException( "mask" );

				this.mask = mask;
				this.formatProvider = formatProvider;
			}

			/// <summary>
			/// Indicates whether this object is equal to the passed in obj.
			/// </summary>
			/// <param name="obj"></param>
			/// <returns></returns>
			public override bool Equals( object obj )
			{
				MaskCache maskCache = obj as MaskCache;

				if ( null != maskCache )
				{
					if ( !maskCache.mask.Equals( this.mask ) )
						return false;

					if ( this.formatProvider == maskCache.formatProvider )
						return true;

					if ( null != this.formatProvider && null != maskCache.formatProvider )
						return this.formatProvider.Equals( maskCache.formatProvider );
				}

				return false;
			}

			/// <summary>
			/// Rerturns thehas code of the object.
			/// </summary>
			/// <returns></returns>
			public override int GetHashCode( )
			{
				return this.mask.GetHashCode( );
			}
		}

		// SSP 3/1/12 - Optimizations
		// Changed to use Dictionary instead of Hashtable.
		// 
		[ThreadStatic( )]
		private static Dictionary<MaskCache, SectionsCollection> g_parsedMaskCache;
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		internal static SectionsCollection GetCachedParsedMask( string mask, MaskInfo maskInfo, bool cloneSections )
		{
			IFormatProvider formatProvider = maskInfo.FormatProvider;
			SectionsCollection sections = null;

			Debug.Assert( null != mask && mask.Length > 0, "can not parse empty mask" );
			if ( null == mask || mask.Length == 0 )
				return sections;

			// Allow for localized masks. Since the mask now also depends on the format provider,
			// we need to take that into account in our caching mechanism.
			//
			MaskCache maskCache = new MaskCache( mask, formatProvider );

			// SSP 3/1/12 - Optimizations
			// 
			//if ( MaskInfo.ParsedMaskCache.Contains( maskCache ) )
			if ( null == g_parsedMaskCache )
				g_parsedMaskCache = new Dictionary<MaskCache, SectionsCollection>( );

			// SSP 3/1/12 TFS92791
			// 
			//if ( MaskInfo.ParsedMaskCache.Contains( maskCache ) )
			if ( g_parsedMaskCache.TryGetValue( maskCache, out sections ) && ! sections._isCacheInvalid )
			{
				sections = (SectionsCollection)g_parsedMaskCache[maskCache];
				if ( cloneSections )
					sections = sections.Clone( maskInfo, false );
				else
					sections.Initialize( maskInfo );
			}
			else
			{
				MaskParser.Parse( mask, out sections, formatProvider );

				Debug.Assert( null != sections, "MaskParser.Parse(" + mask + ") did not create sections collection" );
				if ( null == sections )
					return sections;

				sections.Initialize( maskInfo );

				// Allow for localized masks. Since the mask now also depends on the format provider,
				// we need to take that into account in our caching mechanism.
				//
				// Make sure that the cached sections collection doesn't hold a reference back to this
				// mask info. To do that clone the sections and cache that. If sections held back a
				// reference to the mask info then there is a potential for the grid to not release
				// rows and bands etc... when the data source is reset until the next paint.
				//
				g_parsedMaskCache[maskCache] = sections.Clone( null, false );

				// Make sure the cache doesn't grow unbounded.
				// 
				if ( g_parsedMaskCache.Count > 100 )
				{
					try
					{
						int countOfItemsToRemove = g_parsedMaskCache.Count - 100;
						List<MaskCache> itemsToRemove = new List<MaskCache>( countOfItemsToRemove );

						foreach ( MaskCache key in g_parsedMaskCache.Keys )
						{
							itemsToRemove.Add( key );

							if ( itemsToRemove.Count >= countOfItemsToRemove )
								break;
						}

						foreach ( MaskCache key in itemsToRemove )
							g_parsedMaskCache.Remove( key );
					}
					catch ( Exception )
					{
					}
				}
			}

			return sections;
		}






		internal char PromptChar
		{
			get
			{
				return this.MaskEditor.PromptChar;
			}
		}




#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal char PadChar
		{
			get
			{
				return this.MaskEditor.PadChar;
			}
		}






		internal SectionsCollection Sections
		{
			get
			{
				return this._sections;
			}
		}






		internal bool DesignMode
		{
			get
			{
				
				return false;
			}
		}

		internal object ValueToDataValue( object value )
		{
			return Utilities.ConvertDataValue( value, this.MaskEditor.ValueType, this.FormatProvider, this.Format );
		}

		// MD 4/22/11 - TFS73181
		#region ResetCachedCharacters

		private void ResetCachedCharacters()
		{
			this._cachedCommaChar =
				this._cachedDecimalSeperatorChar =
				this._cachedPlusSignChar =
				this._cachedMinusSignChar = (char)0;
		}

		#endregion // ResetCachedCharacters

		// MD 4/25/11 - TFS73181
		#region GetDefaultTypeForSyncTextFromValue

		internal Type GetDefaultTypeForSyncTextFromValue(object newValue)
		{
			if (newValue == null)
				return null;

			Type expectedValueType;
			int expectedNumberOfSections;

			switch (_maskDefaultType)
			{
				case MaskDefaultType.None:
					return null;

				case MaskDefaultType.Byte:
					expectedValueType = typeof(byte);
					expectedNumberOfSections = 1;
					break;

				case MaskDefaultType.SByte:
					expectedValueType = typeof(sbyte);
					expectedNumberOfSections = 1;
					break;

				case MaskDefaultType.Int16:
					expectedValueType = typeof(short);
					expectedNumberOfSections = 1;
					break;

				case MaskDefaultType.UInt16:
					expectedValueType = typeof(ushort);
					expectedNumberOfSections = 1;
					break;

				case MaskDefaultType.Int32:
					expectedValueType = typeof(int);
					expectedNumberOfSections = 1;
					break;

				case MaskDefaultType.UInt32:
					expectedValueType = typeof(uint);
					expectedNumberOfSections = 1;
					break;

				case MaskDefaultType.Int64:
					expectedValueType = typeof(long);
					expectedNumberOfSections = 1;
					break;

				case MaskDefaultType.UInt64:
					expectedValueType = typeof(ulong);
					expectedNumberOfSections = 1;
					break;

				case MaskDefaultType.Float:
					expectedValueType = typeof(float);
					expectedNumberOfSections = 3;
					break;

				case MaskDefaultType.Double:
					expectedValueType = typeof(double);
					expectedNumberOfSections = 3;
					break;

				case MaskDefaultType.Currency:
					expectedValueType = typeof(decimal);
					expectedNumberOfSections = 4;
					break;

				case MaskDefaultType.DateTime:
					expectedValueType = typeof(DateTime);
					// The sections for the date time is variable, so don't expect any specific value.
					expectedNumberOfSections = this.Sections.Count;
					break;

				default:
					Debug.Fail("Unknown MaskDefaultType: " + _maskDefaultType);
					return null;
			}

			if (newValue.GetType() != expectedValueType)
				return null;

			if (this.Sections.Count != expectedNumberOfSections)
			{
				Debug.Fail("Incorrect number of sections.");
				return null;
			}

			return expectedValueType;
		} 

		#endregion // GetDefaultTypeForSyncTextFromValue
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