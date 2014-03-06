using System.Resources;
using Infragistics;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
internal static class SR
{
#pragma warning disable
    private static List<ResourceManager> _resourceManagers;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    static SR()
    {
        object[] stringResourceLocationAttributes = typeof(SR).Assembly.GetCustomAttributes(typeof(StringResourceLocationAttribute), true);
        if (stringResourceLocationAttributes == null || stringResourceLocationAttributes.Length == 0)
        {
            throw new InvalidOperationException("Assembly is missing the StringResourceLocation attribute.");
        }
        StringResourceLocationAttribute stringResourceLocationAttribute = stringResourceLocationAttributes[0] as StringResourceLocationAttribute;
        if (stringResourceLocationAttribute == null)    
        {
            throw new InvalidOperationException("Error retrieving the StringResourceLocation attribute.");
        }
        string[] resourceNames = stringResourceLocationAttribute.Location.Split(';');

        int resourceCount = 0;

        // count them up since someone could have empty items in the list
        // or have the string End with a semi-colon
        foreach (string resource in resourceNames)
        {
            if (resource != null && resource.Length > 0)
                resourceCount++;
        }

        System.Diagnostics.Debug.Assert(resourceCount > 0, "No valid resource string names listed in the 'AssemblyRef.BaseResource'");

        // create the array
        SR._resourceManagers = new List<ResourceManager>();

        System.Reflection.Assembly assembly = typeof(SR).Assembly;

        // count them up since someone could have empty items in the list
        foreach (string resource in resourceNames)
        {
            if (resource != null && resource.Length > 0)
                SR._resourceManagers.Add(new ResourceManager(resource, assembly));
        }
    }

    internal static string GetString(string resourceName)
    {
        foreach (ResourceManager rm in SR._resourceManagers)
        {
            string value = rm.GetString(resourceName, CultureInfo.CurrentCulture);

            if (value != null)
                return value;
        }

        return null;
    }

    internal static void AddResource(string name, Assembly assembly)
    {
        ResourceManager manager = new ResourceManager(name, assembly);

        // SZ 3/6/10
        // Yes i know this looks weird
        // But by doing this, we'll force an exception to be thrown if the 
        // name passed in is an invalid path to a resource file, thus notifying the developer.
        string value = manager.GetString("DummyValue");

        SR._resourceManagers.Insert(0, manager);




		// JJD 9/16/11 - TFS87912
		// Invalidate any cached resource strings
		Infragistics.Controls.Primitives.ResourceStringBase.InvalidateCachedResources();



	}

    internal static void RemoveResource(string name)
    {
        ResourceManager toRemove = null;
        foreach (ResourceManager manager in _resourceManagers)
        {
            if (manager.BaseName == name)
            {
                toRemove = manager;
                break;
            }
        }

		if (toRemove != null)
		{
			_resourceManagers.Remove(toRemove);




			// JJD 9/16/11 - TFS87912
			// Invalidate any cached resource strings
			Infragistics.Controls.Primitives.ResourceStringBase.InvalidateCachedResources();




		}
	}

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    internal static string GetString(string resourceName, params object[] args)
    {
        string str = GetString(resourceName);

        if (str != null)
        {
            // JJD 5/24/04
            // Only call string.Format if the base string is at least
            // 3 characters long which is the minimum to contain a 
            // substition string (e.g. '{0}')
            if (args != null && args.Length > 0 && str.Length > 2)
            {
                // AS 4/30/03 FxCop Change
                // Explicitly call the overload that takes an IFormatProvider
                //
                //str = string.Format( str, args );

                // JJD 5/24/04
                // Wrap the string.Format in a try/catch so that we will
                // return the original unformatted resource string in the case 
                // where there were more arguments than their were substition
                // literals in the string
                try
                {
                    str = string.Format(null, str, args);
                }
                catch
                {
                }
            }
        }
        return str;
    }


#pragma warning restore 
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