/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;


namespace Unlimited.Applications.BusinessDesignStudio.Activities
{


    public enum enDev2HTMLType { FORM, PAGETITLE, META, IMAGE, TEXT, MENU }

    public class Util
    {
        public static bool ValueIsNumber(string value)
        {
            return double.TryParse(value, out double val);
        }

        public static bool ValueIsDate(string value)
        {
            return DateTime.TryParse(value, out DateTime date);
        }

    
        public static bool Eq(string value, object comparisonValue)
        {
            if(string.IsNullOrEmpty(value))
            {
                return false;
            }

            if(comparisonValue == null)
            {
                return false;
            }

            if(ValueIsDate(value))
            {
                if(ValueIsDate(comparisonValue.ToString()))
                {
                    return DateTime.Parse(value) == DateTime.Parse(comparisonValue.ToString());
                }
            }

            if(ValueIsNumber(value))
            {
                if(ValueIsNumber(comparisonValue.ToString()))
                {
                    
                    return double.Parse(value) == double.Parse(comparisonValue.ToString());
                    
                }
            }

            
            return string.Equals(value, comparisonValue.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }

    
        public static bool NtEq(string value, object comparisonValue)
        {
            if(string.IsNullOrEmpty(value))
            {
                return false;
            }

            if(comparisonValue == null)
            {
                return false;
            }

            if(ValueIsDate(value))
            {
                if(ValueIsDate(comparisonValue.ToString()))
                {
                    return DateTime.Parse(value) != DateTime.Parse(comparisonValue.ToString());
                }
            }

            if(ValueIsNumber(value))
            {
                if(ValueIsNumber(comparisonValue.ToString()))
                {
                    
                    return double.Parse(value) != double.Parse(comparisonValue.ToString());
                    
                }
            }

            return value != comparisonValue.ToString();
        }

    
        public static bool LsTh(string value, object comparisonValue)
        {

            if(string.IsNullOrEmpty(value))
            {
                return false;
            }

            if(comparisonValue == null)
            {
                return false;
            }

            if(ValueIsDate(value))
            {
                if(ValueIsDate(comparisonValue.ToString()))
                {
                    return DateTime.Parse(value) < DateTime.Parse(comparisonValue.ToString());
                }
            }

            if(ValueIsNumber(value))
            {
                if(ValueIsNumber(comparisonValue.ToString()))
                {
                    return double.Parse(value) < double.Parse(comparisonValue.ToString());
                }
            }

            return false;
        }

    
        public static bool LsThEq(string value, object comparisonValue)
        {
            if(string.IsNullOrEmpty(value))
            {
                return false;
            }

            if(comparisonValue == null)
            {
                return false;
            }

            if(ValueIsDate(value))
            {
                if(ValueIsDate(comparisonValue.ToString()))
                {
                    return DateTime.Parse(value) <= DateTime.Parse(comparisonValue.ToString());
                }
            }

            if(ValueIsNumber(value))
            {
                if(ValueIsNumber(comparisonValue.ToString()))
                {
                    return double.Parse(value) <= double.Parse(comparisonValue.ToString());
                }
            }

            return false;
        }

    
        public static bool GrTh(string value, object comparisonValue)
        {

            if(string.IsNullOrEmpty(value))
            {
                return false;
            }

            if(comparisonValue == null)
            {
                return false;
            }

            if(ValueIsDate(value))
            {
                if(ValueIsDate(comparisonValue.ToString()))
                {
                    return DateTime.Parse(value) > DateTime.Parse(comparisonValue.ToString());
                }
            }

            if(ValueIsNumber(value))
            {
                if(ValueIsNumber(comparisonValue.ToString()))
                {
                    return double.Parse(value) > double.Parse(comparisonValue.ToString());
                }
            }


            return false;
        }


    
        public static bool GrThEq(string value, object comparisonValue)
        {
            if(string.IsNullOrEmpty(value))
            {
                return false;
            }

            if(comparisonValue == null)
            {
                return false;
            }

            if(ValueIsDate(value))
            {
                if(ValueIsDate(comparisonValue.ToString()))
                {
                    return DateTime.Parse(value) >= DateTime.Parse(comparisonValue.ToString());
                }
            }

            if(ValueIsNumber(value))
            {
                if(ValueIsNumber(comparisonValue.ToString()))
                {
                    return double.Parse(value) >= double.Parse(comparisonValue.ToString());
                }
            }

            return false;
        }

    
        public static bool Btw(string value, object comparisonValueStart, object comparisonValueEnd)
        {
            if(string.IsNullOrEmpty(value))
            {
                return false;
            }

            if(comparisonValueStart == null)
            {
                return false;
            }

            if(comparisonValueEnd == null)
            {
                return false;
            }



            if(ValueIsDate(value))
            {
                if(ValueIsDate(comparisonValueStart.ToString()))
                {
                    if(ValueIsDate(comparisonValueEnd.ToString()))
                    {
                        return DateTime.Parse(value) >= DateTime.Parse(comparisonValueStart.ToString())
                            && DateTime.Parse(value) <= DateTime.Parse(comparisonValueEnd.ToString());
                    }
                    
                }
            }

            if(ValueIsNumber(value))
            {
                if(ValueIsNumber(comparisonValueStart.ToString()))
                {
                    if(ValueIsNumber(comparisonValueEnd.ToString()))
                    {
                        return double.Parse(value) >= double.Parse(comparisonValueStart.ToString())
                            && double.Parse(value) <= double.Parse(comparisonValueEnd.ToString());
                    }

                }
            }

            return false;
        }
    }
}
