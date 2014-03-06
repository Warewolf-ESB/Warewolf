using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Windows.Input;
using System.Security;
using Infragistics.Shared;

namespace Infragistics.Windows.Controls.Markup
{
    /// <summary>
    /// Implements a markup extension that creates a <see cref="Cursor"/>
    /// </summary>
    /// <remarks>
    /// <p class="body">The CursorExtension is useful for two main scenarios. First, 
    /// custom cursors are not supported by WPF when run in a low trust scenario. The 
    /// extension will return the <see cref="FallbackValue"/> in that case. Second, 
    /// the extension may also be used to build a uri where the name of the assembly 
    /// may change.</p>
    /// </remarks>
	[MarkupExtensionReturnType(typeof(Cursor))]
    public class CursorExtension : MarkupExtension
    {
        #region Member Variables

        private string _path;
        private Type _typeInAssembly;
        private Cursor _fallBack;

        private static bool _failedToLoad;

	    #endregion //Member Variables

        #region Constructor
		/// <summary>
		/// Initializes a new <see cref="CursorExtension"/>
		/// </summary>
		public CursorExtension()
		{
		}

		/// <summary>
		/// Initializes a new <see cref="CursorExtension"/> with the specified relative path
		/// </summary>
        /// <param name="path">A path relative to the uri context where the resource can be located.</param>
        public CursorExtension(string path) : this()
        {
            _path = path;
        }

		/// <summary>
		/// Initializes a new <see cref="CursorExtension"/> with the specified path in the specified assembly.
		/// </summary>
        /// <param name="path">The path in the assembly of the <paramref name="typeInAssembly"/> where the cursor resource can be located.</param>
        /// <param name="typeInAssembly">A type in the assembly that contains the cursor resource</param>
        public CursorExtension(string path, Type typeInAssembly) : this(path)
        {
            _typeInAssembly = typeInAssembly;
        }
		#endregion //Constructor

        #region Properties
        /// <summary>
        /// Returns or sets the string path for the cursor resource within the assembly that includes the <see cref="TypeInAssembly"/>
        /// </summary>
        [ConstructorArgument("path")]
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        /// <summary>
        /// Returns or sets a type whose assembly contains the cursor resource specified by the <see cref="Path"/>
        /// </summary>
        [ConstructorArgument("typeInAssembly")]
        public Type TypeInAssembly
        {
            get { return _typeInAssembly; }
            set { _typeInAssembly = value; }
        }

        /// <summary>
        /// Returns or sets the cursor that should be returned if the cursor resource could not be loaded.
        /// </summary>
        public Cursor FallbackValue
        {
            get { return _fallBack; }
            set { _fallBack = value; }
        } 
        #endregion //Properties

        #region Base class overrides
        /// <summary>
        /// Returns an <see cref="Cursor"/> based on the properties of the markup extension.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>A <see cref="Cursor"/> based on the <see cref="Path"/> specified or the <see cref="FallbackValue"/> if one could not be created.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_path == null)
                throw new InvalidOperationException(SR.GetString("LE_MissingCursorExtensionPath"));

            Uri uri;

            if (_typeInAssembly == null)
            {
                IUriContext service = serviceProvider.GetService(typeof(IUriContext)) as IUriContext;
                if (service == null)
                    throw new InvalidOperationException(SR.GetString("LE_MarkupExtensionServiceRequired", this.GetType(), typeof(IUriContext)));

                uri = new Uri(service.BaseUri, _path);
            }
            else
                uri = Utilities.BuildEmbeddedResourceUri(_typeInAssembly.Assembly, _path);

            Cursor cursor;

            if (_failedToLoad)
                cursor = _fallBack;
            else
            {
                try
                {
                    cursor = Utilities.LoadCursor(uri);
                }
                catch (SecurityException)
                {
                    _failedToLoad = true;
                    cursor = _fallBack;
                }
            }

            return cursor;
        }
        #endregion //Base class overrides
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