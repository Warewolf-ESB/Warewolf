
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Diagnostics;
using Infragistics.Shared;
using System.Windows;

		
namespace Infragistics.Windows.Licensing
{
	#region LicenseStatus
	/// <summary>
	/// An enumerator that describes the status of the license
	/// </summary>
	[ EditorBrowsable( EditorBrowsableState.Never ) ] 
	public enum LicenseStatus 
	{
		/// <summary>
		/// The license verification logic hasn't run yet
		/// </summary>
		Unchecked		= 0,

		/// <summary>
		/// The license file could not be located.
		/// </summary>
		UnableToLocateLicenseAssembly = 1,

		/// <summary>
		/// Unable to determine license status due to security permission restrictions.
		/// </summary>
		InsufficientSecurityPermissions = 2,

		/// <summary>
		/// Couldn't find the necessary registry keys
		/// </summary>
		RegKeyNotFound	= 3,    

		/// <summary>
		/// Foound an invalid CDkey
		/// </summary>
		CDKeyInvalid	= 4,    

		/// <summary>
		/// The beta period has expired
		/// </summary>
		BetaExpired		= 5,

		/// <summary>
		/// The beta period has not expired
		/// </summary>
		BetaValid		= 6,

		/// <summary>
		/// The trial period has expired
		/// </summary>
		TrialExpired	= 7,
    
		/// <summary>
		/// The activation grace period has expired.
		/// </summary>
		ActivationGracePeriodExpired	= 8,    

		/// <summary>
		/// A valid CDKey was found
		/// </summary>
		CDKeyValid		= 9,
    
		/// <summary>
		/// The trial period has not expired
		/// </summary>
		TrialValid		= 10,

		/// <summary>
		/// The activation grace period has not expired.
		/// </summary>
		ActivationGracePeriodValid		= 11,    
    
		/// <summary>
		/// The component is fully licensed
		/// </summary>
		Licensed		= 12				    
	};
	#endregion LicenseStatus

	#region ProductType enum

	/// <summary>
	/// An enumerator that describes the product type.
	/// </summary>
	[ EditorBrowsable( EditorBrowsableState.Never ) ] 
	public enum ProductType 
	{
		/// <summary>
		/// The product is a winforms component.
		/// </summary>
		WinFormsComponent	= 0,

		/// <summary>
		/// The product is a webforms component.
		/// </summary>
		WebFormsComponent	= 1,
	}

	#endregion ProductType enum

	#region UltraProductInfoBase

	/// <summary>
	/// Abstract base class that holds product info.
	/// </summary>
	[ EditorBrowsable( EditorBrowsableState.Never ) ] 
	public abstract class UltraProductInfoBase
	{
		private string			productName;

		
		
		
		private ProductType		productType = ProductType.WinFormsComponent;

		
		internal UltraProductInfoBase( string productName )
		{
			this.productName	= productName;
		}

		
		
		internal UltraProductInfoBase( string productName, ProductType productType )
		{
			this.productName	= productName;
			this.productType	= productType;
		}

		/// <summary>
		/// Returns the expiration description.
		/// </summary>
		
		
		public abstract string Expiration { get; }

		/// <summary>
		/// Returns the expiration date.
		/// </summary>
		public abstract bool RequiresActivation { get; }

		/// <summary>
		/// Returns the product name
		/// </summary>
		public string ProductName { get { return this.productName; } }

		/// <summary>
		/// Returns true if the license has expired (read-only)
		/// </summary>
		
		public abstract bool Expired { get; }

		/// <summary>
		/// Returns the statuc of the license
		/// </summary>
		public abstract LicenseStatus Status { get; }
		
		/// <summary>
		/// Returns the Key 
		/// </summary>
		public abstract string Key { get; }

		/// <summary>
		/// Returns the company name
		/// </summary>
		public abstract string CompanyName { get; }

		/// <summary>
		/// Returns the user name
		/// </summary>
		public abstract string UserName { get; }

		/// <summary>
		/// Checks the validaity of the key
		/// </summary>
		/// <returns>True if the key is valid</returns>
		internal protected abstract bool IsKeyValid( string key ); 

		/// <summary>
		/// Returns the product type
		/// </summary>
		public ProductType Type { get { return this.productType; } }
    
      // MRS 6/12/07 - BR23802
      #region GetStatusText

      /// <summary>
      /// Returns a string to be displayed in the about box for the status.
      /// </summary>
      /// <returns>The string to display in the about box for the status.</returns>
      public string GetStatusText()
      {
      switch (this.Status)
      {
      case LicenseStatus.ActivationGracePeriodExpired:
      return SR.GetUncustomizedString("LDR_About_GraceExpired");

      case LicenseStatus.ActivationGracePeriodValid:
      return SR.GetUncustomizedString("LDR_About_InGrace");

      case LicenseStatus.BetaExpired:
      return SR.GetUncustomizedString("AboutDialogLicenseDescriptionBetaExpired");

      case LicenseStatus.BetaValid:
      return SR.GetUncustomizedString("AboutDialogLicenseDescriptionBeta");

      case LicenseStatus.TrialExpired:
      return SR.GetUncustomizedString("AboutDialogLicenseDescriptionTrialExpired");

      case LicenseStatus.TrialValid:
      return SR.GetUncustomizedString("AboutDialogLicenseDescriptionTrial");

      case LicenseStatus.CDKeyValid:
      case LicenseStatus.Licensed:
      return SR.GetUncustomizedString("AboutDialogLicenseDescriptionLicensed");

      case LicenseStatus.CDKeyInvalid:
      case LicenseStatus.RegKeyNotFound:
      case LicenseStatus.Unchecked:
      return SR.GetUncustomizedString("AboutDialogLicenseDescriptionUnknown");

      default:
      Debug.Fail("Unknown Status");
      return String.Empty;
      }
      }
      #endregion //GetStatusText

// MBS 2/4/09 - TFS12883
        #region GetKeyText

        /// <summary>
        /// Returns the string used to represent the product key in a UI, taking into account localized values.
        /// </summary>
        /// <returns>A string used to represent the product key.</returns>
        public string GetKeyText()
        {
            string key = String.Empty;
            if (String.IsNullOrEmpty(this.Key) == false)
            {
                if (this.Key.Length != 18)
                {
                    key = this.Key;
                    if (key == UltraProductInfo.trialKeyText)
                        key = SR.GetString("TrialKey_AboutDialogLabel");
                }
                else
                {
                    key = this.Key.Substring(0, 4) + "-" +
                          this.Key.Substring(4, 7) + "-" +
                          this.Key.Substring(11, 7);
                }
            }
            return key;
        }
        #endregion //GetKeyText
    
      }
      #endregion UltraProductInfoBase
      #region UltraProductInfo
      /// <summary>
	/// ProductInfo used when after a product has been released.
	/// </summary>
	[ EditorBrowsable( EditorBrowsableState.Never ) ] 
	public sealed class UltraProductInfo : UltraProductInfoBase
	{
		#region Member Variables

		private DateTime expirationDate = DateTime.MaxValue;
		private string  cdkey = null;
		private string  companyName = null;
		private string  userName = null;
		private string	regKeyProductName;
		private string	codePrefix;
		private Guid	id;
		private bool	requiresActivation;
		
		
		private byte	majorVersion;
		private byte	minorVersion;

		private LicenseStatus 
						status = LicenseStatus.Unchecked;

		private int							badCount = 0;

		private const int					MIN_USAGE_DAYS_PER_WEEK = 2;
		private const int					TRIAL_MAX_USAGE_DAYS = 20;
		private const int					GRACE_MAX_USAGE_DAYS = 40;

		private const int					SSCDKEYTYPE_INVALID = 0;
		private const int					SSCDKEYTYPE_3RDGEN = 3;

		
		
		

		
		internal const string				trialKeyText = "{Trial}";
		private const string				errorKeyText = "{Error}";
		private const int					SIZEOFCDKEY = 18;

		private static System.Collections.ArrayList
											verifiedProducts = null;

		
		private bool						promptDisplayed = false;

		
		private int							daysRemaining = 0;

		#endregion Member Variables

		#region Constructor

		/// <summary>
		/// Contructor
		/// </summary>
		/// <param name="productName">The product name for the about dialog</param>
		/// <param name="regKeyProductName">The reg key root product name</param>
		/// <param name="codePrefix">must be 4 characters long</param>
		/// <param name="id">the id</param>
		/// <param name="requiresActivation">True if activation is required.</param>
		/// <param name="majorVersion">Major version of the product.</param>
		/// <param name="minorVersion">Minor version of the product.</param>
		public UltraProductInfo( string productName,
							 	 string regKeyProductName,
								 string codePrefix,
 								 Guid   id,
								 bool   requiresActivation,
								 byte	majorVersion,
								 byte   minorVersion) : base ( productName )
		{
			if ( codePrefix == null ||
				 codePrefix.Length != 4 )
				throw new ArgumentException(SR.GetString("LE_ArgumentException_18") );

			this.regKeyProductName	= regKeyProductName;
			this.codePrefix			= codePrefix;
			this.id					= id;
			this.requiresActivation = requiresActivation;

			
			this.majorVersion = majorVersion;
			this.minorVersion = minorVersion;
		}

		
		/// <summary>
		/// Contructor
		/// </summary>
		/// <param name="productName">The product name for the about dialog</param>
		/// <param name="regKeyProductName">The reg key root product name</param>
		/// <param name="codePrefix">must be 4 characters long</param>
		/// <param name="id">the id</param>
		/// <param name="requiresActivation">True if activation is required.</param>
		/// <param name="productType">The type of product</param>
		/// <param name="majorVersion">Major version of the product.</param>
		/// <param name="minorVersion">Minor version of the product.</param>
		public UltraProductInfo( string		 productName,
								 string		 regKeyProductName,
								 string		 codePrefix,
								 Guid		 id,
								 bool		 requiresActivation,
 								 ProductType productType,
								 byte		 majorVersion,
								 byte		 minorVersion) : base ( productName,	productType )
		{
			if ( codePrefix == null ||
				codePrefix.Length != 4 )
				throw new ArgumentException(SR.GetString("LE_ArgumentException_19") );

			this.regKeyProductName	= regKeyProductName;
			this.codePrefix			= codePrefix;
			this.id					= id;
			this.requiresActivation = requiresActivation;

			
			this.minorVersion = minorVersion;
			this.majorVersion = majorVersion;
		}

		#endregion Constructor

		#region Public Properties

		/// <summary>
		/// Returns the expiration description.
		/// </summary>
		
		
		public override string Expiration 
		{ 
			get 
			{ 
				switch (this.Status)
				{
					case LicenseStatus.ActivationGracePeriodExpired:
						return SR.GetString("LDR_UltraProdInfo_Expiration1");

					case LicenseStatus.TrialExpired:
						return SR.GetString("LDR_UltraProdInfo_Expiration2");

					case LicenseStatus.BetaExpired:
						return SR.GetString("LDR_UltraProdInfo_Expiration3");

					case LicenseStatus.TrialValid:
					case LicenseStatus.ActivationGracePeriodValid:
						return SR.GetString("LDR_UltraProdInfo_Expiration4", this.daysRemaining, this.GetCalendarWeeksLeft());

					case LicenseStatus.BetaValid:
					case LicenseStatus.Licensed:
						return SR.GetString("LDR_UltraProdInfo_Expiration5");

					case LicenseStatus.CDKeyInvalid:
						return SR.GetString("LDR_UltraProdInfo_Expiration6");

					case LicenseStatus.CDKeyValid:
						return SR.GetString("LDR_UltraProdInfo_Expiration7");

					default:
					case LicenseStatus.Unchecked:
						return SR.GetString("LDR_UltraProdInfo_Expiration8");

					case LicenseStatus.RegKeyNotFound:
					case LicenseStatus.UnableToLocateLicenseAssembly:
						return SR.GetString("LDR_UltraProdInfo_Expiration9");

					case LicenseStatus.InsufficientSecurityPermissions:
						return SR.GetString("LDR_UltraProdInfo_Expiration10");
				}
			} 
		}

		/// <summary>
		/// Returns the expiration date.
		/// </summary>
		public override bool RequiresActivation { get{ return this.requiresActivation; } }

		/// <summary>
		/// Returns the statuc of the license
		/// </summary>
		public override LicenseStatus Status 
		{ 
			get
			{
				if ( this.status == LicenseStatus.Unchecked )
					this.CheckLicenseStatus();

				return this.status;
			}
		}

		/// <summary>
		/// Returns the Key 
		/// </summary>
		public override string Key { get { return this.cdkey; } }

		/// <summary>
		/// Returns the company name
		/// </summary>
		public override string CompanyName { get { return this.companyName; } }

		/// <summary>
		/// Returns the user name
		/// </summary>
		public override string UserName { get { return this.userName; } }

		#endregion Public Properties

		#region CheckLicenseStatus

		
		private void CheckLicenseStatus()
		{
			
			
			if (this.status != LicenseStatus.Unchecked)
				return;

			
			
			
			if (UltraProductInfo.CheckProductStatus(this,false) != LicenseStatus.Unchecked)
			{
				UltraProductInfo.CheckProductStatus(this,true);
				return;
			}

			string cdKey = string.Empty;

			
			

			
			string assemblyName = this.GetLicenseAssemblyInfo();

			try
			{
				
				System.Runtime.Remoting.ObjectHandle objectHandle = System.Activator.CreateInstance( assemblyName, "Infragistics.License.LicenseValidator" );
				object validator = objectHandle.Unwrap();

				
				System.Type validatorType = validator.GetType();

				
				object[] args = new object[] {this.regKeyProductName, this.majorVersion, this.minorVersion, cdKey, this.userName, this.companyName };

				System.Reflection.MethodInfo getRegInfo = validatorType.GetMethod("GetRegistryInfo");

				object retVal = getRegInfo.Invoke(validator, args);

				if ( retVal == null || !(bool)retVal || (string)args[3] == UltraProductInfo.errorKeyText )
				{
					
					
					this.UpdateProductStatus( LicenseStatus.RegKeyNotFound, DateTime.MinValue );

					return;
				}

				
				cdkey = args[3] as string;
				this.userName = args[4] as string;
				this.companyName = args[5] as string;

				if (cdkey == UltraProductInfo.trialKeyText)
				{
					
					this.IsDemoValid(true);
				}
				else
				{
					this.IsLicenseValid( cdkey, true );
				}
			}
			catch (System.IO.FileNotFoundException)
			{
				
				
				
				if (this.status == LicenseStatus.Unchecked)
					this.UpdateProductStatus(LicenseStatus.UnableToLocateLicenseAssembly, DateTime.MinValue);
			}
			catch (System.Security.SecurityException)
			{
				
				
				
				if (this.status == LicenseStatus.Unchecked)
					this.UpdateProductStatus(LicenseStatus.InsufficientSecurityPermissions, DateTime.MinValue);
			}
			catch (Exception)
			{
				if (this.status == LicenseStatus.Unchecked)
					this.UpdateProductStatus(LicenseStatus.RegKeyNotFound, DateTime.MinValue);
			}
		}

		#endregion CheckLicenseStatus

		#region GetLicenseAssemblyInfo
		private string GetLicenseAssemblyInfo()
		{
			string assemblyInfoBase = @"Infragistics.License, Culture="""", Version={0}, PublicKeyToken=7dd5c3163f2cd0cb";

			Microsoft.Win32.RegistryKey prodKey, verKey;

			verKey = prodKey = null;

			try
			{
				prodKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey( "Infragistics.License" );

				verKey = prodKey.OpenSubKey( "Version1" );

				string assemblyInfo = verKey.GetValue( null ) as string;

				return string.Format( assemblyInfoBase, assemblyInfo );
			}
			catch( Exception )
			{
				return @"Infragistics.License, Culture="""", PublicKeyToken=7dd5c3163f2cd0cb";
			}
			finally
			{
				if (verKey != null)
					verKey.Close();

				if (prodKey != null)
					prodKey.Close();
			}

		}
		#endregion GetLicenseAssemblyInfo

		#region IsKeyValid
		/// <summary>
		/// Checks the validity of the key
		/// </summary>
		/// <returns>True if the key is valid</returns>
		internal protected override bool IsKeyValid( string key )
		{
			if (key == UltraProductInfo.trialKeyText)
			{
				
				
				
				
				
				return true;
			}
			else if (key == null || key == string.Empty)
				return false;
			else
			{
				return this.IsLicenseValid(key, false);
			}
		}
		#endregion IsKeyValid

		#region IsDemoValid
		private bool IsDemoValid(bool updateProductInfo)
		{
			
			string assemblyName = this.GetLicenseAssemblyInfo();

			object validator = null;

			try
			{
				
				System.Runtime.Remoting.ObjectHandle objectHandle = System.Activator.CreateInstance( assemblyName, "Infragistics.License.LicenseValidator" );
				validator = objectHandle.Unwrap();
			}
			catch (System.IO.FileNotFoundException)
			{
				
				
				
				if (updateProductInfo)
					this.UpdateProductStatus(LicenseStatus.UnableToLocateLicenseAssembly, DateTime.MinValue);

				return false;
			}
			catch (System.Security.SecurityException)
			{
				
				
				
				if (updateProductInfo)
					this.UpdateProductStatus(LicenseStatus.InsufficientSecurityPermissions, DateTime.MinValue);

				return false;
			}
			catch (Exception)
			{
				if (updateProductInfo)
					this.UpdateProductStatus(LicenseStatus.RegKeyNotFound, DateTime.MinValue);
				
				return false;
			}


			try
			{
				
				System.Type validatorType = validator.GetType();

				
				System.Reflection.MethodInfo daysRemaining = validatorType.GetMethod("DaysRemainingBeforeTimeOut");

				

				
				object[] args = new object[] { UltraProductInfo.TRIAL_MAX_USAGE_DAYS, this.regKeyProductName, this.majorVersion, this.minorVersion, this.id };

				try
				{
					
					object retVal = daysRemaining.Invoke( validator, args );

					int days = (int)retVal;

					
					this.daysRemaining = days;

					if (updateProductInfo)
					{
						
						if (days >= 0)
							this.UpdateProductStatus(LicenseStatus.TrialValid, this.GetExpirationDate(days));
						else
							this.UpdateProductStatus(LicenseStatus.TrialExpired, this.GetExpirationDate(days));
					}

					return days >= 0;
				}
				catch (Exception)
				{
					if (updateProductInfo)
					{
						
						this.UpdateProductStatus( LicenseStatus.RegKeyNotFound, DateTime.MinValue );
					}
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
		#endregion IsDemoValid

		#region UpdateProductStatus
		private void UpdateProductStatus( LicenseStatus newStatus, DateTime expirationDate )
		{
			this.status = newStatus;
			this.expirationDate = expirationDate;

			if ((int)newStatus > (int)LicenseStatus.RegKeyNotFound && UltraProductInfo.CheckProductStatus(this,false) == LicenseStatus.Unchecked)
			{
				
				
				UltraProductInfo.ProductVerified( this );
			}
		}
		#endregion UpdateProductStatus

		#region IsLicenseValid
		private bool IsLicenseValid(string key, bool updateProductInfo)
		{
			
			if (!this.VerifyCheckSum(key, false))
			{
				if (updateProductInfo)
				{
					
					
					this.UpdateProductStatus(LicenseStatus.CDKeyInvalid, DateTime.MinValue );
				}

				return false;
			}

			
			
			
			
			
			if (!updateProductInfo)
				return true;

			
			
			
			
			
			if (updateProductInfo)
			{
				this.cdkey = key;
				this.UpdateProductStatus( LicenseStatus.Licensed, DateTime.MaxValue );
			}

			return true;
			
		}
		#endregion IsLicenseValid

		#region ProductVerified (static)
#if DEBUG
		/// <summary>
		/// Adds a product to the cached list of verified products.
		/// </summary>
		/// <param name="product">Product to add.</param>
#endif
		private static void ProductVerified( UltraProductInfo product )
		{
			if (product == null)
				return;

			if (UltraProductInfo.verifiedProducts == null)
				UltraProductInfo.verifiedProducts = new System.Collections.ArrayList();

			UltraProductInfo.verifiedProducts.Add( product );
		}
		#endregion ProductVerified

		
		#region HasPromptBeenDisplayed / UpdatePromptStatus
		private static bool HasPromptBeenDisplayed( UltraProductInfo product )
		{
			
			if (UltraProductInfo.verifiedProducts == null || product == null)
				return false;

			foreach (UltraProductInfo prod in UltraProductInfo.verifiedProducts)
			{
				if (prod.codePrefix == product.codePrefix &&
					prod.majorVersion == product.majorVersion &&
					prod.minorVersion == product.minorVersion)
				{
					return prod.promptDisplayed;
				}
			}

			return false;
		}

		private static void UpdatePromptStatus( UltraProductInfo product, bool hasBeenDisplayed )
		{
			
			if (UltraProductInfo.verifiedProducts == null || product == null)
				return;

			foreach (UltraProductInfo prod in UltraProductInfo.verifiedProducts)
			{
				if (prod.codePrefix == product.codePrefix &&
					prod.majorVersion == product.majorVersion &&
					prod.minorVersion == product.minorVersion)
				{
					prod.promptDisplayed = hasBeenDisplayed;
				}
			}
		}
		#endregion HasPromptBeenDisplayed

		#region CheckProductStatus (static)
#if DEBUG
		/// <summary>
		/// Checks to see if a product has already been verified.
		/// </summary>
		/// <param name="product">Product to checked.</param>
		/// <param name="updateProductInfoIfFound">Boolean indicating if the supplied product's members should be updated if a match is found.</param>
		/// <returns>Status of the product. If the product has not already been
		/// verified, Unchecked will be returned.</returns>
#endif
		private static LicenseStatus CheckProductStatus(UltraProductInfo product, bool updateProductInfoIfFound)
		{
			if (UltraProductInfo.verifiedProducts == null || product == null)
				return LicenseStatus.Unchecked;

			foreach (UltraProductInfo prod in UltraProductInfo.verifiedProducts)
			{
				if (prod.codePrefix == product.codePrefix &&
					prod.majorVersion == product.majorVersion &&
					prod.minorVersion == product.minorVersion)
				{
					if (updateProductInfoIfFound)
					{
						
						if (prod.cdkey != null && prod.cdkey.Length > 0)
							product.cdkey = prod.cdkey;

						product.companyName = prod.companyName;
						product.expirationDate = prod.expirationDate;
						product.status = prod.status;
						product.userName = prod.userName;

						
						product.promptDisplayed = prod.promptDisplayed;
					}

					return prod.Status;
				}
			}

			return LicenseStatus.Unchecked;
		}
		#endregion ProductStatus


		#region Key Validation Related

		private bool VerifyCheckSum( string str, bool isInternal )
		{
			return true;
		}


		#endregion Key Validation Related

		#region GetCalendarWeeksLeft
		
		private int GetCalendarWeeksLeft()
		{
			return (this.daysRemaining + UltraProductInfo.MIN_USAGE_DAYS_PER_WEEK - 1) / UltraProductInfo.MIN_USAGE_DAYS_PER_WEEK;
		}
		#endregion GetCalendarWeeksLeft

		#region GetExpirationDate
		/// <summary>
		/// Returns the expiration date based on the number of usage days remaining.
		/// </summary>
		/// <param name="usageDaysRemaining">Number of usage days remaining.</param>
		/// <returns>Expiration date for the product.</returns>
		private DateTime GetExpirationDate( int usageDaysRemaining )
		{
			int calendarWeeksLeft = (usageDaysRemaining + UltraProductInfo.MIN_USAGE_DAYS_PER_WEEK - 1) / UltraProductInfo.MIN_USAGE_DAYS_PER_WEEK;

			return DateTime.Today.AddDays( 7.0 * calendarWeeksLeft );
		}
		#endregion GetExpirationDate

		
		private UltraProductInfo[] allNewerProducts = null;
        
		#region AllNewerProducts
		internal UltraProductInfo[] AllNewerProducts
		{
			get
			{
				if (allNewerProducts == null)
					this.LoadAllNewerProductInfo();

				return this.allNewerProducts;
			}
		}
		#endregion AllNewerProducts

		#region LoadAllNewerProductInfo (private)
		private void LoadAllNewerProductInfo()
		{
			
			if (this.allNewerProducts != null)
				return;

			
			string assemblyName = this.GetLicenseAssemblyInfo();

			System.Collections.ArrayList alMajors = new System.Collections.ArrayList(3);
			System.Collections.ArrayList alMinors = new System.Collections.ArrayList(3);

			try
			{
				
				System.Runtime.Remoting.ObjectHandle objectHandle = System.Activator.CreateInstance( assemblyName, "Infragistics.License.LicenseValidator" );
				object validator = objectHandle.Unwrap();

				
				System.Type validatorType = validator.GetType();

				
				
				
				System.Reflection.MethodInfo getVersions = validatorType.GetMethod("GetNewerVersions");

				
				object[] args = new object[] {this.regKeyProductName, this.majorVersion, this.minorVersion, alMajors, alMinors };

				bool hasNewVersions = (bool)getVersions.Invoke( validator, args );

				byte[] majorBytes = alMajors.ToArray( typeof(byte) ) as byte[];
				byte[] minorBytes = alMinors.ToArray( typeof(byte) ) as byte[];

				if (majorBytes != null && minorBytes != null && majorBytes.Length == minorBytes.Length)
				{
					this.allNewerProducts = new UltraProductInfo[majorBytes.Length];

					for (int i = 0; i < majorBytes.Length; i++)
					{
						byte major = majorBytes[i];
						byte minor = minorBytes[i];

						
						UltraProductInfo product = new UltraProductInfo( this.ProductName, 
							this.regKeyProductName,
							this.codePrefix,		
							this.id,
							this.requiresActivation,
							this.Type,
							major,
							minor);

						this.allNewerProducts[i] = product;
					}
				}
				else
				{
					this.allNewerProducts = new UltraProductInfo[] {};
				}
			}
			catch {}
		}
		#endregion LoadAllNewerProductInfo

		
		internal void DisplayPrompt()
		{
			
			if (this.promptDisplayed)
				return;

			
			
			if (UltraProductInfo.HasPromptBeenDisplayed(this))
			{
				this.promptDisplayed = true;
				return;
			}

			string message = null;

			
			switch (this.status)
			{
				case LicenseStatus.ActivationGracePeriodValid:
					
					message = string.Format(SR.GetString("LicensedProductActivationGracePrompt"), this.daysRemaining, this.GetCalendarWeeksLeft(), this.ProductName);
					break;
				case LicenseStatus.TrialValid:
					message = string.Format(SR.GetString("LicensedProductTrialPeriodPrompt"), this.daysRemaining, this.GetCalendarWeeksLeft(), this.ProductName);
					break;

				default:
					return;
			}

			this.promptDisplayed = true;
			UltraProductInfo.UpdatePromptStatus( this, true );

			
			try
			{
				string title = SR.GetString("LicensedProductPromptDialogTitle", this.ProductName);
				MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning );}
			catch {}
		}

		
		/// <summary>
		/// Returns true if the license has expired (read-only)
		/// </summary>
		public override bool Expired 
		{ 
			get { return (System.DateTime.Today > this.expirationDate); } 
		}
	}
	#endregion UltraProductInfo
}