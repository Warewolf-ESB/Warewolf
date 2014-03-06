using System;
using System.Resources;
using System.Threading;

namespace PerCederberg.Grammatica.Parser.Resources 
{
	internal sealed class SRCalc
	{
		#region Member Variables

		private static SRCalc							loader = null;
		private ResourceManager						resources = null;
		private static StringResourceCallback		callback = null;

		#endregion Member Variables

		#region Contructor
		private SRCalc()
		{
			// this needs to be a string constant that points to 
			// the fully qualified name of the assembly resource with
			// the string resources
			// e.g.
			//
#pragma warning disable 0436
			string baseName = AssemblyRef.GrammaticaResourceName;
#pragma warning restore 0436

			this.resources = new ResourceManager( baseName, this.GetType().Assembly );
		}
		#endregion Contructor

		#region Properties

		#region ResourceCustomizer
		internal static StringResourceCallback ResourceCustomizer
		{
			get { return SRCalc.callback; }
			set { SRCalc.callback = value; }
		}
		#endregion //ResourceCustomizer

		#endregion //Properties

		#region Methods

		#region GetLoader
		private static SRCalc GetLoader()
		{
			if (SRCalc.loader == null)
			{
				Type type = typeof(SRCalc);

				Monitor.Enter( type );

				try
				{
					if (SRCalc.loader == null)
						SRCalc.loader = new SRCalc();
				}
				finally
				{
					Monitor.Exit( type );
				}
			}
			return SRCalc.loader;
		}
		#endregion GetLoader

		#region GetString methods



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		public static string GetString( string name )
		{
			return SRCalc.GetString( name, null );
		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public static string GetString( string name, params object[] args )
		{
			string str = null;

			// ask the callback to customize the string first
			if (SRCalc.callback != null)
				str = SRCalc.callback( name );

			// if it didn't customize it, get it from the resource
			if ( str == null )
			{
				SRCalc sr = SRCalc.GetLoader();

				// Never return null. Instead return an empty string
				if (sr != null)
					str = sr.resources.GetString( name, null );

				// Never return null. Instead return an empty string
				if (str == null )
					str = string.Empty;
			}

			// Only call string.Format if the base string is at least
			// 3 characters long which is the minimum to contain a 
			// substition string (e.g. '{0}')
			if (args != null && args.Length > 0 && str.Length > 2)
			{
				// Wrap the string.Format in a try/catch so that we will
				// return the original unformatted resource string in the case 
				// where there were more arguments than their were substition
				// literals in the string
				try
				{
					str = string.Format( null, str, args ); 
				}
				catch
				{
				}
			}

			return str;
		}
		#endregion GetString methods

		#endregion Methods

		#region Delegate

		internal delegate string StringResourceCallback(string resourceName);

		#endregion //Delegate
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