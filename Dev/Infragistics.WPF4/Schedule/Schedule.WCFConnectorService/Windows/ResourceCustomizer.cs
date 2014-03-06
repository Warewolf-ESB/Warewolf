using System;
using System.Resources;
using System.Globalization;
using System.Threading;
using System.ComponentModel;
using System.Collections;


	using System.Collections.Generic;
using System.Text;



#pragma warning disable 1574
namespace Infragistics.Shared.Services



{
	/// <summary>
	/// Class used to provide the ability to customize resource strings.
	/// </summary>
	/// <remarks>
	/// <p>There is an instance of this class exposed by the <see cref="Infragistics.Shared.ResourceCustomizer"/> property of the Resources object exposed by each Infragistics.Win assembly.</p>
	/// <p>It allows for customization/substitution of strings that would normally be loaded from resources.</p>
	/// </remarks>
	// JJD 12/05/02 - FxCop
	// Added ComVisible attribute to avoid fxcop violation
	[System.Runtime.InteropServices.ComVisible(false)]
	sealed public class ResourceCustomizer
	{
		#region Private Members

		private Hashtable customizedStrings;

		// SSP 10/31/05 BR07291
		// 
		private int versionNumber;
 
		#endregion Private Members

		#region Properties

			#region CustomizedStrings

		private Hashtable CustomizedStrings
		{
			get 
			{
				if ( this.customizedStrings == null )
					this.customizedStrings = new Hashtable();

				return this.customizedStrings;
			}
		}

			#endregion CustomizedStrings

			#region CustomizedStringsVersion

		// SSP 10/31/05 BR07291
		// Added CustomizedStringsVersion property.
		// 
		/// <summary>
		/// For internal infrastructure use. This number is incremented every time a 
		/// customized string is modified.
		/// </summary>
		[ EditorBrowsable( EditorBrowsableState.Never ) ]
		public int CustomizedStringsVersion 
		{
			get
			{
				return this.versionNumber;
			}
		}

			#endregion // CustomizedStringsVersion
 
		#endregion Properties

		#region Methods

			#region GetCustomizedString

		/// <summary>
		/// Gets the customized string identified by the specified string resource name.
		/// </summary>
		/// <param name="name">Name of the string resource that was customized.</param>
		/// <returns>The customized string or nulll if the resoource wasn't customized.</returns>
		/// <seealso cref="ResetAllCustomizedStrings"/>
		/// <seealso cref="ResetCustomizedString"/>
		/// <seealso cref="SetCustomizedString"/>
		public string GetCustomizedString( string name )
		{
			if ( this.CustomizedStrings.Contains(name) )
				return this.CustomizedStrings[name] as string;

			return null;
		}

			#endregion GetCustomizedString


			#region RaisePropertyChangedEvent

		private void RaisePropertyChangedEvent(string resourceName)
		{
			if ( this.ResourceCustomizationChanged != null )
				this.ResourceCustomizationChanged(this, new PropertyChangedEventArgs(resourceName));
		}

			#endregion //RaisePropertyChangedEvent


			#region ResetAllCustomizedStrings

		/// <summary>
		/// Clears all strings customized by calls to <see cref="SetCustomizedString"/> method.
		/// </summary>
		/// <seealso cref="SetCustomizedString"/>
		/// <seealso cref="ResetCustomizedString"/>
		public void ResetAllCustomizedStrings( )
		{
			if ( this.customizedStrings != null )
			{
				this.customizedStrings.Clear();


				// JJD 3/07/07
				// Added support for DynamicResourceStrings
				this.RaisePropertyChangedEvent(null);


				// SSP 10/31/05 BR07291
				// 
				this.versionNumber++;
			}

		}

			#endregion ResetAllCustomizedStrings

			#region ResetCustomizedString

		/// <summary>
		/// Resets a customized string identified by the specified string resource name so that it will load from the resource file.
		/// </summary>
		/// <param name="name">Name of the string resource to customize.</param>
		/// <seealso cref="SetCustomizedString"/>
		public void ResetCustomizedString( string name )
		{
			this.SetCustomizedString( name, null );
		}

			#endregion ResetCustomizedString

			#region SetCustomizedString

		/// <summary>
		/// Sets a customized string identified by the specified string resource name.
		/// </summary>
		/// <param name="name">Name of the string resource to customize.</param>
		/// <param name="customizedText">The customized string. If null this has the same effect as calling <see cref="ResetCustomizedString"/></param>
		/// <seealso cref="ResetAllCustomizedStrings"/>
		/// <seealso cref="ResetCustomizedString"/>
		public void SetCustomizedString( string name, string customizedText )
		{
			if ( this.CustomizedStrings.Contains(name) )
				this.CustomizedStrings.Remove(name);

			if ( customizedText != null )
				this.CustomizedStrings.Add( name, customizedText );


			// JJD 3/07/07
			// Added support for DynamicResourceStrings
			this.RaisePropertyChangedEvent(name);

			// SSP 10/31/05 BR07291
			// 
			this.versionNumber++;
		}

			#endregion SetCustomizedString

		#endregion Methods


		/// <summary>
		/// This event is raised when a string is customized. The property name in the event arguments refers to the resource name. 
		/// </summary>
		/// <remarks>If PropertyName is null then all customizations have been reset.</remarks>
		public event PropertyChangedEventHandler ResourceCustomizationChanged;


	}


	// JJD 3/07/07
	// Added support for DynamicResourceStrings
	#region DynamicResourceString class

	/// <summary>
	/// A class that tracks customization changes to a resource and raises an event when the resource value is changed.
	/// </summary>
	/// <seealso cref="ResourceCustomizer"/>
	/// <seealso cref="ResourceCustomizer.SetCustomizedString"/>
	/// <seealso cref="ResourceCustomizer.ResetCustomizedString"/>
	/// <seealso cref="ResourceCustomizer.ResetAllCustomizedStrings"/>
	public abstract class DynamicResourceString : INotifyPropertyChanged
	{
		#region Private Members

		private ResourceCustomizer _customizer;
		private CultureInfo _cultureInfo;
		private string _currentFormattedString;
		private string _currentUnformattedString;
		private string _resourceName;
		private object[] _args;

		#endregion //Private Members

		#region Constructor

		/// <summary>
		/// This constructor is for internal use only.
		/// </summary>
		protected DynamicResourceString(ResourceCustomizer customizer, CultureInfo cultureInfo, string resourceName, object[] args)
		{
			if (resourceName == null)
				throw new ArgumentNullException("resourceName");

			if (customizer == null)
				throw new ArgumentNullException("customizer");

			this._customizer			= customizer;
			this._cultureInfo			= cultureInfo;
			this._resourceName			= resourceName;
			this._args					= args;

			this._customizer.ResourceCustomizationChanged += new PropertyChangedEventHandler(OnResourceChanged);
		}


		#endregion //Constructor

        #region Base class overrides

            #region ToString

        // JJD 4/10/08 added ToString oveerride
        /// <summary>
        /// Returns a string representation of the object
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            // Note: we don't want to just return the Value since that
            // would let someone bind to the object and not its Value property.
            // If they did that they wouldn't get refresehed when the value
            // changed. Therefore, pre-pend "DynamicResourceString:" so they
            // know not to do that.
            sb.Append("DynamicResourceString: ");
            sb.Append(this.Value);

            return sb.ToString();

        }

            #endregion //ToString

        #endregion //Base class overrides	
        
		#region Properties

			#region Value

		/// <summary>
		/// Returns the converted string resource
		/// </summary>
		public string Value 
		{ 
			get 
			{
				string baseString = this._customizer.GetCustomizedString(this._resourceName);

				if (baseString == null)
					baseString = this.GetStringFromResource(this._resourceName, this._cultureInfo);

				if (baseString == this._currentUnformattedString)
					return this._currentFormattedString;

				this._currentUnformattedString	= baseString;
				this._currentFormattedString	= baseString;

				if (this._args != null)
				{
					try
					{
						this._currentFormattedString = string.Format(this._cultureInfo, baseString, this._args);
					}
					catch
					{
					}
				}

				return this._currentFormattedString; 
			} 
		}

			#endregion //Value

		#endregion //Properties

		#region Methods

			#region GetStringFromResource

		/// <summary>
		/// For internal use only.
		/// </summary>
		abstract protected string GetStringFromResource(string resourceName, CultureInfo culture);

			#endregion //GetStringFromResource	
    
			#region DoPropertiesMatch

		/// <summary>
		/// For internal use only.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool DoPropertiesMatch(CultureInfo cultureInfo, string resourceName, object[] args)
		{
			if (this._cultureInfo != cultureInfo)
				return false;

			if (resourceName != this._resourceName)
				return false;

			if (args != this._args && (args == null || this._args == null))
				return false;

			if (args == null)
				return true;

			if (args.Length != this._args.Length)
				return false;

			int count = args.Length;

			for (int i = 0; i < count; i++)
			{
				if (!Object.Equals( args[i], this._args[i]))
					return false;
			}

			return true;
		}

			#endregion //DoPropertiesMatch

			#region OnResourceChanged

		private void OnResourceChanged(object sender, PropertyChangedEventArgs e)
		{
			string name = e.PropertyName;

			if (name == null || name == this._resourceName)
			{
				string oldFormattedString = this._currentFormattedString;

				if (oldFormattedString != this.Value)
				{
					if (this.PropertyChanged != null)
						this.PropertyChanged(this, new PropertyChangedEventArgs( "Value" ));
				}
			}
		}

			#endregion //OnResourceChanged	
    
		#endregion //Methods

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Occurs when the underlying resource string changes via the ResourceCustomizer
		/// </summary>
		/// <seealso cref="ResourceCustomizer"/>
		/// <seealso cref="ResourceCustomizer.SetCustomizedString"/>
		/// <seealso cref="ResourceCustomizer.ResetCustomizedString"/>
		/// <seealso cref="ResourceCustomizer.ResetAllCustomizedStrings"/>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

        // JJD 4/15/08 - optiimization
        // Use dictionary to search DynamicResourceStrings
        #region ID struct

        /// <summary>
        /// For internal use only
        /// </summary>
        public struct ID
        {
            private CultureInfo _cultureInfo;
            private string _resourceName;
            private object[] _args;
            private int _hashCode;

            #region Constructor

            /// <summary>
            /// Creates an instance of the struct
            /// </summary>
            public ID(CultureInfo cultureInfo, string resourceName, object[] args)
            {
                this._cultureInfo = cultureInfo;
                this._resourceName = resourceName;
                this._args = args;

                this._hashCode = 0;

                if (this._cultureInfo != null)
                    this._hashCode = this._cultureInfo.GetHashCode() / 3;

                if (this._resourceName != null)
                    this._hashCode += this._resourceName.GetHashCode() / 3;

                if (this._args != null)
                {
                    int count = this._args.Length;

                    for (int i = 0; i < count; i++)
                    {
                        object arg = this._args[i];

                        if (arg != null)
                            this._hashCode += arg.GetHashCode() / (3 * count);
                    }
                }

            }

            #endregion //Constructor

            #region Base class overrides

            #region Equals

            /// <summary>
            /// Returns true if the passed in object is equal
            /// </summary>
            public override bool Equals(object obj)
            {
                if (!(obj is ID))
                    return false;

                ID id = (ID)obj;

                if (this._cultureInfo != id._cultureInfo)
                    return false;

                if (id._resourceName != this._resourceName)
                    return false;

                if (id._args != this._args && (id._args == null || this._args == null))
                    return false;

                if (id._args == null)
                    return true;

                if (id._args.Length != this._args.Length)
                    return false;

                int count = id._args.Length;

                for (int i = 0; i < count; i++)
                {
                    if (!Object.Equals(id._args[i], this._args[i]))
                        return false;
                }

                return true;
            }

            #endregion //Equals

            #region GetHashCode

            /// <summary>
            /// Caclulates a value used for hashing
            /// </summary>
            public override int GetHashCode()
            {
                return this._hashCode;
            }

            #endregion //GetHashCode

            #endregion //Base class overrides
        }

        #endregion //ID struct
    }

	#endregion //DynamicResourceString class

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