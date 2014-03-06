using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Navigation;
using System.Windows;


namespace Infragistics
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    internal interface INavigation
    {
        Uri NavigationUri
        {
            get;
            set;
        }
        
        string NavigationParameter
        {
            get;
            set;
        }



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        FrameworkElement NavigationElement
        {
            get;
            set;
        }


        void NavigateTo();
    }

    internal class MenuNavigation : INavigation
    {
        #region INavigation Members

        public void NavigateTo()
        {
            if (NavigationUri == null)
                throw new InvalidOperationException(Resources.GetString("Navigation_UriIsNotSet"));

            Uri navUri = null;
            if (string.IsNullOrEmpty(NavigationParameter))
            {
                navUri = this.NavigationUri;
            }
            else
            {
                List<string> parameters = ParseParameters(NavigationParameter);
                navUri = BuildNavigationString(parameters, this.NavigationUri.OriginalString);
            }

            if (NavigationElement is Frame)
            {
                (NavigationElement as Frame).Navigate(navUri);
            }
            else if (NavigationElement is Page)
            {
                NavigationService ns = (NavigationElement as Page).NavigationService;
                if (ns == null)
                {
					throw new InvalidOperationException(Resources.GetString("Navigation_PageIsNotSet"));
                }
                ns.Navigate(navUri);
            }
            else
            {
				throw new InvalidOperationException(Resources.GetString("Navigation_ElementIsNotSet"));
            }
        }

        public Uri NavigationUri
        {
            get;
            set;
        }
        public string NavigationParameter
        {
            get;
            set;
        }


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        public FrameworkElement NavigationElement
        {
            get;
            set;
        }

       
        private static Uri BuildNavigationString(List<string> parameters, string sourceUri)
        {
            int indexRight =0;
            int indexLeft =0;
            int prevIndex=0;
            int paramIndex = 0;
            string navUriBuilder = "";

            while (true)
            {
                indexLeft = sourceUri.IndexOf("{", prevIndex, StringComparison.Ordinal);
                indexRight = sourceUri.IndexOf("}", prevIndex, StringComparison.Ordinal);
                if (indexRight == -1 && indexLeft == -1)
                    break;
                else if (indexRight == -1 || indexLeft == -1)
                {
					throw new InvalidOperationException(Resources.GetString("Navigation_WrongFormat"));
                }

                navUriBuilder += sourceUri.Substring(prevIndex, indexLeft - prevIndex);
                navUriBuilder += parameters[paramIndex];

                paramIndex++;
                prevIndex = indexRight + 1;
            }

            if (prevIndex >= 0 && prevIndex < sourceUri.Length)
            {
                
                
                navUriBuilder += sourceUri.Substring(prevIndex);
            }

            return new Uri(navUriBuilder, UriKind.RelativeOrAbsolute);

        }

        private static List<string> ParseParameters(string parameterString)
        {
            int index = 0;
            int prevIndex = 0;
            List<string> ret = new List<string>();

            // If the first sing in not '/' then we assume all string is one parameter
            if (parameterString[0] != '/')
            {
                ret.Add(parameterString);
                return ret;
            }

            index = parameterString.IndexOf('/');
            while (index != -1)
            {
                prevIndex = index + 1;
                if (prevIndex >= parameterString.Length)
                {
                    ret.Add("");
                    break;
                }

                index = parameterString.IndexOf('/', prevIndex);
                if (index == -1)
                {
                    ret.Add(parameterString.Substring(prevIndex, parameterString.Length - prevIndex));
                    break;
                }
                else
                {
                    ret.Add(parameterString.Substring(prevIndex, index-prevIndex));
                }
            }
            return ret;
        }

        #endregion
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