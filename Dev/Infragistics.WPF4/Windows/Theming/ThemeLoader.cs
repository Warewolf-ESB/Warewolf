using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Markup;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Infragistics.Windows;

namespace Infragistics.Windows.Themes
{
	/// <summary>
	/// Class used to merge resource set(s) into a ResourceDictionary 
	/// </summary>
	public class ThemeLoader : ResourceDictionary,
		ISupportInitialize // AS 3/14/07
	{
		#region Member Variables

		private string _theme;
		private string _grouping;
		private bool _dirty;
		private ResourceDictionary _mergedResourceSet;
		private Type _typeInThemeAssembly;
		private bool _initializing;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="ThemeLoader"/>
		/// </summary>
		public ThemeLoader()
		{
			this._mergedResourceSet = new ResourceHolder();

			this.MergedDictionaries.Add(this._mergedResourceSet);
		}

		#endregion //Constructor	
		
		#region Properties

			#region Grouping

		/// <summary>
		/// Gets/sets the name of the grouping
		/// </summary>
		[DependsOn("TypeInThemeAssembly")] // AS 3/14/07
		public string Grouping
		{
			get { return this._grouping; }
			set
			{
				if (value != this._grouping)
				{
					this._grouping = value;
					this._dirty = true;
					this.VerifyMergeDictionaries();
				}
			}
		}

			#endregion //Grouping	

			#region Theme

		/// <summary>
		/// Gets/sets the name of the theme
		/// </summary>
		[DependsOn("TypeInThemeAssembly")] // AS 3/14/07
		public string Theme
		{
			get { return this._theme; }
			set
			{
				if (value != this._theme)
				{
					// AS 3/14/07
					//if (!ThemeManager.IsThemeValid(value))
					//	throw new ArgumentException(SR.GetString("LE_ArgumentException_6", value));
					this.VerifyIsValidTheme(value);

					this._theme = value;
					this._dirty = true;
					this.VerifyMergeDictionaries();
				}
			}
		}

			#endregion //Theme	
    
			// AS 3/14/07
			// To allow this type to be used from xaml and reference a theme in an external
			// assembly, we have added a type property so they can use that to reference a type
			// within the theme assembly thereby forcing the assembly to be loaded without having
			// to go to the code behind.
			//
			#region TypeInThemeAssembly
		/// <summary>
		/// A type from the assembly that contains the <see cref="Theme"/>
		/// </summary>
		/// <remarks>
		/// <b class="body">This property is meant to be used from Xaml to force a theme assembly to 
		/// be loaded and should be set to a type within the theme assembly that contains the associated 
		/// <see cref="Theme"/>. Setting this property in code will not impact the information in the 
		/// <see cref="ResourceDictionary"/></b>
		/// </remarks>
		public Type TypeInThemeAssembly
		{
			get { return this._typeInThemeAssembly; }
			set { this._typeInThemeAssembly = value; }
		} 
			#endregion //TypeInThemeAssembly

		#endregion //Properties

		#region Methods

			#region Public Methods

				#region SetThemeAndGrouping

		/// <summary>
		/// Sets the theme and grouping properties in one atomic call.
		/// </summary>
		/// <param name="theme">The name of the theme.</param>
		/// <param name="grouping">The name of the grouping</param>
		public void SetThemeAndGrouping(string theme, string grouping)
		{
			if (theme != this._theme)
			{
				// AS 3/14/07
				this.VerifyIsValidTheme(theme);

				this._theme = theme;
				this._dirty = true;
			}

			if (grouping != this._grouping)
			{
				this._grouping = grouping;
				this._dirty = true;
			}

			this.VerifyMergeDictionaries();
		}

				#endregion //SetThemeAndGrouping

			#endregion //Public Methods	

			#region Private Methods

			#region ClearMergedDictionaries

		private void ClearMergedDictionaries()
		{
			// we need to logically clear the merged dictionaries thru
			// the follwing hack since both the Clear and Remove methods
			// of the collection have bugs. 
			
			for (int i = 0; i < this.MergedDictionaries.Count; i++)
			{
				if (this.MergedDictionaries[i] == this._mergedResourceSet)
				{
					// allocate a new dictionary and replace the old one
					this._mergedResourceSet = new ResourceHolder();
					this.MergedDictionaries[i] = this._mergedResourceSet;
				}
			}
		}

			#endregion //ClearMergedDictionaries
    
			#region VerifyMergeDictionaries

		private void VerifyMergeDictionaries()
		{
			if (this.IsReadOnly)
				return;

			if (!this._dirty)
				return;

			// AS 3/14/07
			if (this._initializing)
				return;

			this._dirty = false;

			if (this._theme == null ||
				 this._theme.Length == 0)
			{
				this.ClearMergedDictionaries();
				return;
			}

			if (this._grouping == null ||
				 this._grouping.Length == 0)
			{
				this.ClearMergedDictionaries();
				return;
			}

			ResourceDictionary resourceSetToMerge = ThemeManager.GetResourceSet(this._theme, this._grouping);

			// if not foundclear and return
			if (resourceSetToMerge == null)
			{
				this.ClearMergedDictionaries();
				return;
			}

			for (int i = 0; i < this.MergedDictionaries.Count; i++)
			{
				// look for a match with the old resourseSet holder we had
				// added
				if (this.MergedDictionaries[i] == this._mergedResourceSet)
				{
					// replace the old one
					this._mergedResourceSet = resourceSetToMerge;
					this.MergedDictionaries[i] = this._mergedResourceSet;
					return;
				}
			}

			// normally we would never get here unless someone had
			// explicitly removed the old resource set
			this._mergedResourceSet = resourceSetToMerge;
			this.MergedDictionaries.Add(this._mergedResourceSet);

		}

				#endregion //VerifyMergeDictionaries

			// AS 3/14/07
			#region VerifyIsValidTheme
		private void VerifyIsValidTheme(string theme)
		{
			if (false == this._initializing && false == ThemeManager.IsThemeValid(theme))
				throw new ArgumentException(SR.GetString("LE_ArgumentException_6", theme));
		} 
			#endregion //VerifyIsValidTheme

			#endregion //Private Methods	
        
		#endregion //Methods

		// AS 3/14/07
		// Even though we put the DependsOn attribute, the order of the property
		// setters was still based on the order it was written in the xaml so we
		// implemented ISupportInitialize to ensure that 
		#region ISupportInitialize Members

		void ISupportInitialize.BeginInit()
		{
			// ResourceDictionary implements ISupportInitialize so call its public methods.
			this.BeginInit();

			this._initializing = true;
		}

		void ISupportInitialize.EndInit()
		{
			this._initializing = false;

			if (null != this._theme)
				this.VerifyIsValidTheme(this._theme);

			this.VerifyMergeDictionaries();

			// ResourceDictionary implements ISupportInitialize so call its public methods.
			this.EndInit();
		}

		#endregion //ISupportInitialize
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