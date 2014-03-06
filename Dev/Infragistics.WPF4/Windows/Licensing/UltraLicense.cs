using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;

#if WPF
	namespace Infragistics.Windows.Licensing
#else
	namespace Infragistics.Shared
#endif
{
	#region UltraLicense class
	/// <summary>
	/// The license object for Infragistics components and controls
	/// </summary>
	[ EditorBrowsable( EditorBrowsableState.Never ) ] 
	// JJD 12/05/02 - FxCop
	// Added ComVisible attribute to avoid fxcop violation
	[System.Runtime.InteropServices.ComVisible(false)]
    public class UltraLicense : License 
	{
		#region Member Fields
        /* AS 3/9/09 Performance
         * Moved members to a struct so we can more easily cache the information.
         * 
        private UltraProductInfoBase productInfo;
		private string key;
		private string componentName;

		//	BF 9/21/06	Office2007UI Licensing
		private LicenseOptions licenseOptions = LicenseOptions.None;
        */
        private UltraLicenseInfo licenseInfo;
        #endregion

        #region Constructors

        //	BF 9/21/06	Office2007UI Licensing
		#region Obsolete code
		//	// ctor
		//    internal UltraLicense( string componentName, UltraProductInfoBase productInfo ) 
		//	{
		//        this.productInfo	= productInfo;
		//		this.componentName	= componentName;
		//    }
		//
		//	// ctor
		//    internal UltraLicense( string componentName, UltraProductInfoBase productInfo, string key ) 
		//	{
		//        this.productInfo	= productInfo;
		//		this.key			= key;
		//		this.componentName	= componentName;
		//    }
		#endregion	//Obsolete code

		//	BF 9/21/06	Office2007UI Licensing
		//	Added the 'licenseOptions' parameter to both constructors
		// ctor
        

		// ctor
        

        // AS 3/9/09 Performance
        
		#endregion //Constructors

		#region Public Properties
		/// <summary>
		/// Returns the product info object
		/// </summary>
		public UltraProductInfoBase ProductInfo
		{
			get
			{
                // AS 3/9/09 Performance
                //return this.productInfo;
                return this.licenseInfo.ProductInfo;
			}
		}

		/// <summary>
		/// Returns the display name of the component (read-only)
		/// </summary>
		public string ComponentName
		{
			get
			{
                // AS 3/9/09 Performance
                //return this.componentName;
                return this.licenseInfo.ComponentName;
			}
		}

		/// <summary>
		/// Returns the license key (read-only)
		/// </summary>
		public override string LicenseKey 
		{ 
            get 
			{
				//	BF 9/21/06	Office2007UI Licensing
				#region Obsolete code
				//	if ( this.key != null )
				//		return this.key;
				//
				//	return this.productInfo.Key;
				#endregion //Obsolete code

				//	BF 9/21/06	Office2007UI Licensing
				//	Add the tokens for additional license options (example: Office2007UI)
				//	to the key that gets returned to the .NET runtime's license checking.
				string licenseKey = this.LicenseKeyForDisplay;

				//	If the key is null, we know we are in the design-time environment,
				//	in which case we need to add the tokens - but otherwise we don't.
                // AS 3/9/09 Performance
                //if ( this.key == null )
				//	UltraLicense.AddLicenseOptionsTokensToKey( ref licenseKey, this.licenseOptions );
                if (this.licenseInfo.Key == null)
					UltraLicense.AddLicenseOptionsTokensToKey( ref licenseKey, this.licenseInfo.LicenseOptions );

				return licenseKey;
            }
        }

		//	BF 9/21/06	Office2007UI Licensing
		/// <summary>
		/// Returns the license key, without any license option tokens (read-only)
		/// </summary>
		public string LicenseKeyForDisplay
		{ 
            get 
			{
                /* AS 3/9/09 Performance
				if ( this.key != null )
					return this.key;

				return this.productInfo.Key;
                */
                return this.licenseInfo.Key ?? this.licenseInfo.ProductInfo.Key;
            }
        }

		//	BF 9/21/06	Office2007UI Licensing
		/// <summary>
		/// Returns the additional licensed options for this instance (e.g., Office2007UI)
		/// </summary>
		public LicenseOptions LicenseOptions
		{ 
            get 
			{
				// AS 12/18/09
				// We don't need to force the status to be updated if the license 
				// already knows it supports all the options. This will avoid the 
				// case where something like the ribbon checks the status and we 
				// try to load the Infragistics.License dll.
				//
				if (this.licenseInfo.LicenseOptions == LicenseOptions.All)
					return LicenseOptions.All;

				//	BF 9/22/06	Office2007UI Licensing
				//	Beta participants get full permission
				UltraProductInfoBase productInfo = this.ProductInfo;
				// AS 6/9/11
				// Don't ask for the status of the UltraProductInfo since that will do a design time check.
				//
				//if ( productInfo.Status == LicenseStatus.BetaValid )
				if (  productInfo.Status == LicenseStatus.BetaValid )
					return LicenseOptions.All;

                // AS 3/9/09 Performance
                //return this.licenseOptions;
                return this.licenseInfo.LicenseOptions;
            }
        }

		#endregion

        #region Internal Properties

        // AS 3/9/09 Performance
        #region LicenseInfo
        internal UltraLicenseInfo LicenseInfo
        {
            get { return this.licenseInfo; }
        }
        #endregion //LicenseInfo 

        #endregion //Internal Properties

		#region Public Methods
		/// <summary>
		/// Disposes the object
		/// </summary>
		public override void Dispose() 
		{
        }
		#endregion

		#region Helper routines

		//	BF 9/21/06	Office2007UI Licensing
			#region AddLicenseOptionsTokensToKey
#if DEBUG
		/// <summary>
		/// Adds the appropriate string tokens to the specified key based on
		/// the specified license options.
		/// </summary>
#endif
		static internal void AddLicenseOptionsTokensToKey( ref string key, LicenseOptions licenseOptions )
		{
			if ( key != null )
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder( key );

				Array values = Enum.GetValues( typeof(LicenseOptions) );
				string[] names = Enum.GetNames( typeof(LicenseOptions) );

				for ( int i = 0; i < values.Length; i ++ )
				{
					LicenseOptions currentOption = (LicenseOptions)values.GetValue(i);

					if ( currentOption == LicenseOptions.None || currentOption == LicenseOptions.All )
						continue;

					if ( (licenseOptions & currentOption) == currentOption )
					{
						sb.Append( "_" );
						sb.Append( names[i] );
					}
				}

				key = sb.ToString();
			}
		}
			#endregion //AddLicenseOptionsTokensToKey

		//	BF 9/21/06	Office2007UI Licensing
			#region ParseLicenseOptionsTokensFromKey
#if DEBUG
		/// <summary>
		/// Parses the license options string tokens out of the specified key,
		/// returns a LicenseOptions value describing which license options
		/// are available, and modifies the specified key.
		/// </summary>
#endif
		static internal LicenseOptions ParseLicenseOptionsTokensFromKey( ref string key )
		{
			//	Note that when the specified key is null, we assume lack of sufficient
			//	IO permissions on the local machine, and consider the local machine to
			//	be fully licensed.
			LicenseOptions retVal = LicenseOptions.All;

			if ( key != null )
			{
				retVal = LicenseOptions.None;

				Array values = Enum.GetValues( typeof(LicenseOptions) );
				string[] names = Enum.GetNames( typeof(LicenseOptions) );

				//	Iterate through each member of the LicenseOptions enumeration,
				//	and see if the associated token is present in the key; if it is,
				//	logically OR the value of the associated constant into the return
				//	value, and also strip the token out of the string.
				for ( int i = 0; i < values.Length; i ++ )
				{
					LicenseOptions currentOption = (LicenseOptions)values.GetValue(i);

					if ( currentOption == LicenseOptions.None || currentOption == LicenseOptions.All )
						continue;

					string searchString = string.Format("_{0}", names[i] );

					if ( key.IndexOf(searchString) >= 0 )
					{
						retVal |= currentOption;
						key = key.Replace( searchString, string.Empty );
					}
				}
			}

			return retVal;
		}
			#endregion //ParseLicenseOptionsTokensFromKey

		#endregion //Helper routines

    }
	#endregion //UltraLicense class

	//	BF 9/21/06	Office2007UI Licensing
	#region OptionalLicensePermissions enumeration
	/// <summary>
	/// Contains bitflags which describe any additional options
	/// for which the associated product is licensed; e.g., Office2007UI
	/// </summary>
	[Flags()]
	public enum LicenseOptions
	{
		/// <summary>
		/// No additional license options.
		/// </summary>
		None			=	0x0000,

		/// <summary>
		/// Licensed to use the Office2007 user interface features.
		/// </summary>
		Office2007UI	=	0x0001,

		/// <summary>
		/// Licensed to use all optional features.
		/// </summary>
		All	=	0x7FFFFFFF,
	}
	#endregion //OptionalLicensePermissions enumeration

    // AS 3/9/09 Performance
    #region UltraLicenseInfo
    internal struct UltraLicenseInfo
    {
        #region Member Variables

        public UltraProductInfoBase ProductInfo;
        public string Key;
        public string ComponentName;
        public LicenseOptions LicenseOptions;

        #endregion //Member Variables

        #region Constructor
        internal UltraLicenseInfo(string componentName, UltraProductInfoBase productInfo, LicenseOptions licenseOptions) :
            this(componentName, productInfo, null, licenseOptions)
        {
        }

        internal UltraLicenseInfo(string componentName, UltraProductInfoBase productInfo, string key, LicenseOptions licenseOptions)
        {
            ProductInfo = productInfo;
            ComponentName = componentName;
            Key = key;
            LicenseOptions = licenseOptions;
        }
        #endregion //Constructor
    } 
    #endregion //UltraLicenseInfo
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